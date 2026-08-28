using _2___Application._1_Services.FinancialReports;
using _2___Application._2_Dto_s.CashFlow;

namespace _2___Application._1_Services.CashFlow;

public static class CashFlowRollingCalculator
{
    public const int AnnualPeriodNumber = RollingContract.AnnualDisplayOrder;

    public static CashFlowAnnualGroupDto Calculate(
        IReadOnlyCollection<CashFlowResponseDto> realized,
        IReadOnlyCollection<CashFlowResponseDto> budget,
        int year)
    {
        ArgumentNullException.ThrowIfNull(realized);
        ArgumentNullException.ThrowIfNull(budget);

        var realizedByMonth = ToMonthlyDictionary(realized);
        var budgetByMonth = ToMonthlyDictionary(budget);
        var selection = RollingPeriodSelector.Select(realizedByMonth.Keys, budgetByMonth.Keys);
        var selectedMonths = selection
            .Select(item => item.Source == RollingPeriodSource.Realized
                ? realizedByMonth[item.Month]
                : budgetByMonth[item.Month])
            .ToArray();

        var annualBudget = AggregateBudget(budgetByMonth.Values, year);
        var rolling = AggregateRolling(selectedMonths, year);
        var variation = Subtract(rolling, annualBudget, year);

        return new CashFlowAnnualGroupDto
        {
            Year = year,
            Type = RollingContract.PeriodType,
            DisplayOrder = AnnualPeriodNumber,
            Columns = new List<CashFlowAnnualColumnDto>
            {
                Column(RollingContract.BudgetKey, "Orçado", "budget", 1, annualBudget),
                Column(RollingContract.RollingKey, "Rolling", RollingContract.PeriodType, 2, rolling),
                Column(RollingContract.VariationKey, "Variação", "variation", 3, variation)
            }
        };
    }

    private static Dictionary<int, CashFlowResponseDto> ToMonthlyDictionary(
        IEnumerable<CashFlowResponseDto> values) =>
        values
            .Where(value => value is not null && value.DateMonth is >= 1 and <= 12)
            .GroupBy(value => value.DateMonth)
            .ToDictionary(group => group.Key, group => group.First());

    private static CashFlowAnnualColumnDto Column(
        string key,
        string label,
        string type,
        int displayOrder,
        CashFlowResponseDto value) =>
        new()
        {
            Key = key,
            Label = label,
            Type = type,
            DisplayOrder = displayOrder,
            Value = value
        };

    private static CashFlowResponseDto AggregateBudget(
        IEnumerable<CashFlowResponseDto> months,
        int year) => Aggregate(months.OrderBy(month => month.DateMonth).ToArray(), year);

    private static CashFlowResponseDto AggregateRolling(
        IReadOnlyList<CashFlowResponseDto> months,
        int year) => Aggregate(months, year);

    private static CashFlowResponseDto Aggregate(
        IReadOnlyList<CashFlowResponseDto> months,
        int year)
    {
        var first = months.FirstOrDefault();
        var last = months.LastOrDefault();

        return new CashFlowResponseDto
        {
            Name = year.ToString(),
            DateMonth = AnnualPeriodNumber,
            Year = year,
            PeriodType = RollingContract.PeriodType,
            LucroOperacionalLiquido = months.Sum(value => value.LucroOperacionalLiquido),
            DepreciacaoAmortizacao = months.Sum(value => value.DepreciacaoAmortizacao),
            VariacaoNCG = months.Sum(value => value.VariacaoNCG),
            Clientes = months.Sum(value => value.Clientes),
            Estoques = months.Sum(value => value.Estoques),
            OutrosAtivosOperacionais = months.Sum(value => value.OutrosAtivosOperacionais),
            Fornecedores = months.Sum(value => value.Fornecedores),
            ObrigacoesTributariasTrabalhistas = months.Sum(value => value.ObrigacoesTributariasTrabalhistas),
            OutrosPassivosOperacionais = months.Sum(value => value.OutrosPassivosOperacionais),
            FluxoDeCaixaOperacional = months.Sum(value => value.FluxoDeCaixaOperacional),
            AtivoNaoCirculante = months.Sum(value => value.AtivoNaoCirculante),
            VariacaoInvestimento = months.Sum(value => value.VariacaoInvestimento),
            VariacaoImobilizado = months.Sum(value => value.VariacaoImobilizado),
            VariacaoIntangivel = months.Sum(value => value.VariacaoIntangivel),
            FluxoDeCaixaLivre = months.Sum(value => value.FluxoDeCaixaLivre),
            CaptacoesAmortizacoesFinanceira = months.Sum(value => value.CaptacoesAmortizacoesFinanceira),
            PassivoNaoCirculante = months.Sum(value => value.PassivoNaoCirculante),
            VariacaoPatrimonioLiquido = months.Sum(value => value.VariacaoPatrimonioLiquido),
            FluxoDeCaixaDaEmpresa = months.Sum(value => value.FluxoDeCaixaDaEmpresa),
            DisponibilidadeInicioDoPeriodo = first?.DisponibilidadeInicioDoPeriodo ?? 0m,
            DisponibilidadeFinalDoPeriodo = last?.DisponibilidadeFinalDoPeriodo ?? 0m
        };
    }

    private static CashFlowResponseDto Subtract(
        CashFlowResponseDto rolling,
        CashFlowResponseDto budget,
        int year) =>
        new()
        {
            Name = year.ToString(),
            DateMonth = AnnualPeriodNumber,
            Year = year,
            PeriodType = RollingContract.PeriodType,
            LucroOperacionalLiquido = rolling.LucroOperacionalLiquido - budget.LucroOperacionalLiquido,
            DepreciacaoAmortizacao = rolling.DepreciacaoAmortizacao - budget.DepreciacaoAmortizacao,
            VariacaoNCG = rolling.VariacaoNCG - budget.VariacaoNCG,
            Clientes = rolling.Clientes - budget.Clientes,
            Estoques = rolling.Estoques - budget.Estoques,
            OutrosAtivosOperacionais = rolling.OutrosAtivosOperacionais - budget.OutrosAtivosOperacionais,
            Fornecedores = rolling.Fornecedores - budget.Fornecedores,
            ObrigacoesTributariasTrabalhistas = rolling.ObrigacoesTributariasTrabalhistas - budget.ObrigacoesTributariasTrabalhistas,
            OutrosPassivosOperacionais = rolling.OutrosPassivosOperacionais - budget.OutrosPassivosOperacionais,
            FluxoDeCaixaOperacional = rolling.FluxoDeCaixaOperacional - budget.FluxoDeCaixaOperacional,
            AtivoNaoCirculante = rolling.AtivoNaoCirculante - budget.AtivoNaoCirculante,
            VariacaoInvestimento = rolling.VariacaoInvestimento - budget.VariacaoInvestimento,
            VariacaoImobilizado = rolling.VariacaoImobilizado - budget.VariacaoImobilizado,
            VariacaoIntangivel = rolling.VariacaoIntangivel - budget.VariacaoIntangivel,
            FluxoDeCaixaLivre = rolling.FluxoDeCaixaLivre - budget.FluxoDeCaixaLivre,
            CaptacoesAmortizacoesFinanceira = rolling.CaptacoesAmortizacoesFinanceira - budget.CaptacoesAmortizacoesFinanceira,
            PassivoNaoCirculante = rolling.PassivoNaoCirculante - budget.PassivoNaoCirculante,
            VariacaoPatrimonioLiquido = rolling.VariacaoPatrimonioLiquido - budget.VariacaoPatrimonioLiquido,
            FluxoDeCaixaDaEmpresa = rolling.FluxoDeCaixaDaEmpresa - budget.FluxoDeCaixaDaEmpresa,
            DisponibilidadeInicioDoPeriodo = rolling.DisponibilidadeInicioDoPeriodo - budget.DisponibilidadeInicioDoPeriodo,
            DisponibilidadeFinalDoPeriodo = rolling.DisponibilidadeFinalDoPeriodo - budget.DisponibilidadeFinalDoPeriodo
        };
}
