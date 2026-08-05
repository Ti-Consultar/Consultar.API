using _2___Application._2_Dto_s.AccountingPanel.V2;

namespace _2___Application._1_Services.AccountingPanel.V2;

public sealed class AccountingPanelV2Service
{
    private readonly ClassificationService _classificationService;

    public AccountingPanelV2Service(ClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    public async Task<AccountingPanelV2Response> GetAsync(
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
        var assets = await _classificationService.GetAccountingPanelLegacyResultAsync(
            accountPlanId,
            year,
            typeClassification: 1,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        var liabilities = await _classificationService.GetAccountingPanelLegacyResultAsync(
            accountPlanId,
            year,
            typeClassification: 2,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return AccountingPanelV2Mapper.Map(assets, liabilities, year);
    }
}
