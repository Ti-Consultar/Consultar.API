using _2___Application._2_Dto_s.DRE.V2;

namespace _2___Application._1_Services.DRE.V2;

public sealed class DreV2Service
{
    private readonly ClassificationService _classificationService;

    public DreV2Service(ClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    public async Task<DreV2Response> GetAsync(
        int accountPlanId,
        int year,
        CancellationToken cancellationToken)
    {
        ValidateRequest(accountPlanId, year);
        cancellationToken.ThrowIfCancellationRequested();

        var legacy = await _classificationService
            .GetDreComparativeLegacyResultAsync(accountPlanId, year, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return DreV2Mapper.Map(legacy, year);
    }

    public async Task<DreRowDetailsV2Response> GetDetailsAsync(
        int accountPlanId,
        int year,
        string rowCode,
        string scenario,
        string period,
        CancellationToken cancellationToken)
    {
        ValidateRequest(accountPlanId, year);
        if (string.IsNullOrWhiteSpace(rowCode))
            throw new ArgumentException("rowCode é obrigatório.", nameof(rowCode));
        if (string.IsNullOrWhiteSpace(scenario))
            throw new ArgumentException("scenario é obrigatório.", nameof(scenario));
        if (string.IsNullOrWhiteSpace(period))
            throw new ArgumentException("period é obrigatório.", nameof(period));

        cancellationToken.ThrowIfCancellationRequested();
        var legacy = await _classificationService
            .GetDreComparativeLegacyResultAsync(accountPlanId, year, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return DreV2Mapper.MapDetails(legacy, year, rowCode, scenario, period);
    }

    private static void ValidateRequest(int accountPlanId, int year)
    {
        if (accountPlanId <= 0)
            throw new ArgumentException("accountPlanId deve ser maior que zero.", nameof(accountPlanId));
        if (year <= 0)
            throw new ArgumentException("year deve ser maior que zero.", nameof(year));
    }
}
