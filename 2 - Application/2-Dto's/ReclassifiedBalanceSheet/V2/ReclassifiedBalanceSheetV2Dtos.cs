using System.Text.Json.Serialization;

namespace _2___Application._2_Dto_s.ReclassifiedBalanceSheet.V2;

public sealed class ReclassifiedBalanceSheetV2Response
{
    public ReclassifiedBalanceSheetV2DataDto Data { get; init; } = new();
}

public sealed class ReclassifiedBalanceSheetV2DataDto
{
    public IReadOnlyList<BalanceSheetPeriodDto> Periods { get; init; } = Array.Empty<BalanceSheetPeriodDto>();
    public IReadOnlyList<BalanceSheetScenarioDto> Scenarios { get; init; } = Array.Empty<BalanceSheetScenarioDto>();
    public IReadOnlyList<BalanceSheetStatementDto> Statements { get; init; } = Array.Empty<BalanceSheetStatementDto>();
    public IReadOnlyList<BalanceSheetRowDto> Rows { get; init; } = Array.Empty<BalanceSheetRowDto>();
}

public sealed class BalanceSheetPeriodDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int? Year { get; init; }
    public int? Month { get; init; }
    public string Type { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class BalanceSheetScenarioDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class BalanceSheetStatementDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public string TotalRowCode { get; init; } = string.Empty;
}

public sealed class BalanceSheetRowDto
{
    public string Code { get; init; } = string.Empty;
    public string StatementKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string RowType { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public int Level { get; init; }
    public string? ParentCode { get; init; }
    public bool Expandable { get; init; }
    public BalanceSheetRowSourceDto Source { get; init; } = new();
    public BalanceSheetRowDetailsDto Details { get; init; } = new();
    public Dictionary<string, Dictionary<string, decimal?>> Values { get; init; } = new();
}

public sealed class BalanceSheetRowSourceDto
{
    public string SourceType { get; init; } = string.Empty;
    public int? SourceId { get; init; }
}

public sealed class BalanceSheetRowDetailsDto
{
    public bool Available { get; init; }
    public Dictionary<string, Dictionary<string, int>> Counts { get; init; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Dictionary<string, IReadOnlyList<BalanceSheetTotalizerDetailDto>>>? Data { get; init; }
}

public sealed class BalanceSheetTotalizerDetailDto
{
    public int Id { get; init; }
    public int TypeOrder { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TotalValue { get; init; }
    public bool Expandable { get; init; }
    public IReadOnlyList<BalanceSheetClassificationDetailDto> Classifications { get; init; } =
        Array.Empty<BalanceSheetClassificationDetailDto>();
}

public sealed class BalanceSheetClassificationDetailDto
{
    public int Id { get; init; }
    public int TypeOrder { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public IReadOnlyList<BalanceSheetAccountingDataDto> Datas { get; init; } = Array.Empty<BalanceSheetAccountingDataDto>();
}

public sealed class BalanceSheetAccountingDataDto
{
    public int Id { get; init; }
    public int TypeOrder { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CostCenter { get; init; } = string.Empty;
    public decimal InitialValue { get; init; }
    public decimal CreditValue { get; init; }
    public decimal DebitValue { get; init; }
    public decimal Value { get; init; }
}
