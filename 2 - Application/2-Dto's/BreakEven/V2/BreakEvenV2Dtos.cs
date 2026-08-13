using _2___Application._2_Dto_s.DRE.V2;

namespace _2___Application._2_Dto_s.BreakEven.V2;

public sealed class BreakEvenV2Response
{
    public BreakEvenV2DataDto Data { get; init; } = new();
}

public sealed class BreakEvenV2DataDto
{
    public BreakEvenScopeDto Scope { get; init; } = new();
    public BreakEvenPeriodDto Period { get; init; } = new();
    public decimal Factor { get; init; }
    public BreakEvenSummaryDto Summary { get; init; } = new();
    public IReadOnlyList<BreakEvenRowDto> Rows { get; init; } = Array.Empty<BreakEvenRowDto>();
}

public sealed class BreakEvenScopeDto
{
    public int GroupId { get; init; }
    public int? CompanyId { get; init; }
    public int? SubCompanyId { get; init; }
}

public sealed class BreakEvenPeriodDto
{
    public int Year { get; init; }
    public int Month { get; init; }
}

public sealed class BreakEvenSummaryDto
{
    public decimal ProjectedGrossOperatingRevenue { get; init; }
    public decimal ContributionMargin { get; init; }
    public decimal ContributionMarginPercentage { get; init; }
    public decimal FixedResult { get; init; }
    public decimal FixedAmountToCover { get; init; }
    public decimal BaseBreakEven { get; init; }
    public decimal FinalBreakEven { get; init; }
}

public sealed class BreakEvenRowDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string RowType { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public int Level { get; init; }
    public string? ParentCode { get; init; }
    public bool HasChildren { get; init; }
    public bool IsTotalizer { get; init; }
    public bool CanSimulate { get; init; }
    public decimal? SimulationPercentage { get; init; }
    /// <summary>Regra visual da simulação: negative, free ou null quando a linha não é simulável.</summary>
    public string? SimulationSignRule { get; init; }
    public string Behavior { get; init; } = string.Empty;
    public DreRowSourceDto Source { get; init; } = new();
    public decimal BaseValue { get; init; }
    public decimal ProjectedValue { get; init; }
    public decimal BreakEvenValue { get; init; }
}

public sealed class BreakEvenSimulationRequest
{
    public int GroupId { get; init; }
    public int? CompanyId { get; init; }
    public int? SubCompanyId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Factor { get; init; }
    public IReadOnlyList<BreakEvenLineSimulationDto> Simulations { get; init; } =
        Array.Empty<BreakEvenLineSimulationDto>();
}

public sealed class BreakEvenLineSimulationDto
{
    /// <summary>Código estável da linha retornado pelo próprio endpoint.</summary>
    public string RowCode { get; init; } = string.Empty;

    /// <summary>Percentual decimal: 0.10 representa 10% e -0.05 representa -5%.</summary>
    public decimal Percentage { get; init; }
}
