using _2___Application._1_Services.DRE.V2;
using _2___Application._2_Dto_s.DRE.V2;
using _2___Application._2_Dto_s.TotalizerClassification;

namespace _2___Application._1_Services.BreakEven.V2;

public sealed record BreakEvenSourceRow(
    string Code,
    string Name,
    string RowType,
    string ValueType,
    int DisplayOrder,
    int Level,
    string? ParentCode,
    bool Expandable,
    DreRowSourceDto Source,
    decimal Value);

public interface IBreakEvenDreProvider
{
    Task<IReadOnlyList<BreakEvenSourceRow>> GetMonthlyRowsAsync(
        int accountPlanId,
        int year,
        int month,
        CancellationToken cancellationToken);
}

public sealed class BreakEvenDreProvider : IBreakEvenDreProvider
{
    private readonly ClassificationService _classificationService;

    public BreakEvenDreProvider(ClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    public async Task<IReadOnlyList<BreakEvenSourceRow>> GetMonthlyRowsAsync(
        int accountPlanId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var realized = await _classificationService.GetDreRealizedLegacyResultAsync(
            accountPlanId,
            year,
            cancellationToken);

        var response = DreV2Mapper.Map(new PainelBalancoComparativoResponse
        {
            Realizado = realized
        }, year);

        return BreakEvenMonthlyRowMapper.Map(response, year, month);
    }
}

public static class BreakEvenMonthlyRowMapper
{
    public static IReadOnlyList<BreakEvenSourceRow> Map(
        DreV2Response response,
        int year,
        int month)
    {
        ArgumentNullException.ThrowIfNull(response);
        var periodKey = $"{year:D4}-{month:D2}";
        var data = response.Data;

        if (!data.Scenarios.Any(scenario => scenario.Key == "realizado") ||
            !data.Periods.Any(period => period.Key == periodKey && period.Type == "month"))
        {
            throw new BreakEvenNotFoundException(
                $"Não há DRE realizada para o período {month:D2}/{year:D4}.");
        }

        var rows = data.Rows
            .Where(row => row.Values.TryGetValue("realizado", out var periods) &&
                          periods.TryGetValue(periodKey, out var value) &&
                          value.HasValue)
            .Select(row => new BreakEvenSourceRow(
                row.Code,
                row.Name,
                row.RowType,
                row.ValueType,
                row.DisplayOrder,
                row.Level,
                row.ParentCode,
                row.Expandable,
                row.Source,
                row.Values["realizado"][periodKey]!.Value))
            .ToArray();

        if (rows.Length == 0)
        {
            throw new BreakEvenNotFoundException(
                $"A DRE de {month:D2}/{year:D4} não possui valores suficientes para o cálculo.");
        }

        return rows;
    }
}
