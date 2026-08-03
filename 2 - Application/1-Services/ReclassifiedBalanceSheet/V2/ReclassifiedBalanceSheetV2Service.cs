using _2___Application._2_Dto_s.ReclassifiedBalanceSheet.V2;

namespace _2___Application._1_Services.ReclassifiedBalanceSheet.V2;

public sealed class ReclassifiedBalanceSheetV2Service
{
    private readonly ClassificationService _classificationService;

    public ReclassifiedBalanceSheetV2Service(ClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    public async Task<ReclassifiedBalanceSheetV2Response> GetAsync(
        int accountPlanId,
        int year,
        CancellationToken cancellationToken)
    {
        if (accountPlanId <= 0)
            throw new ArgumentException("accountPlanId deve ser maior que zero.", nameof(accountPlanId));
        if (year <= 0)
            throw new ArgumentException("year deve ser maior que zero.", nameof(year));

        cancellationToken.ThrowIfCancellationRequested();

        // Os dois fluxos compartilham o mesmo serviço e DbContext scoped.
        // Por isso são executados sequencialmente.
        var assets = await _classificationService
            .GetReclassifiedBalanceSheetComparativeLegacyResultAsync(
                accountPlanId,
                year,
                typeClassification: 1,
                cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        var liabilities = await _classificationService
            .GetReclassifiedBalanceSheetComparativeLegacyResultAsync(
                accountPlanId,
                year,
                typeClassification: 2,
                cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return ReclassifiedBalanceSheetV2Mapper.Map(assets, liabilities, year);
    }
}
