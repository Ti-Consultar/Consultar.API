using System.Text.Json.Serialization;

namespace _2___Application._2_Dto_s.AccountingPanel.V2;

public sealed class AccountingPanelV2Response
{
    public AccountingPanelV2DataDto Data { get; init; } = new();
}

public sealed class AccountingPanelV2DataDto
{
    public IReadOnlyList<AccountingPanelPeriodDto> Periods { get; init; } = Array.Empty<AccountingPanelPeriodDto>();
    public IReadOnlyList<AccountingPanelScenarioDto> Scenarios { get; init; } = Array.Empty<AccountingPanelScenarioDto>();
    public IReadOnlyList<AccountingPanelStatementDto> Statements { get; init; } = Array.Empty<AccountingPanelStatementDto>();
    public IReadOnlyList<AccountingPanelRowDto> Rows { get; init; } = Array.Empty<AccountingPanelRowDto>();
}

public sealed class AccountingPanelPeriodDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int? Year { get; init; }
    public int? Month { get; init; }
    public string Type { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class AccountingPanelScenarioDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class AccountingPanelStatementDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public string TotalRowCode { get; init; } = string.Empty;
}

public sealed class AccountingPanelRowDto
{
    public string Code { get; init; } = string.Empty;
    public string StatementKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string RowType { get; init; } = string.Empty;
    public string ValueType { get; init; } = "currency";
    public int DisplayOrder { get; init; }
    public int Level { get; init; }
    public string? ParentCode { get; init; }
    public bool Expandable { get; init; }
    public AccountingPanelRowSourceDto Source { get; init; } = new();
    public AccountingPanelRowDetailsDto Details { get; init; } = new();
    public Dictionary<string, Dictionary<string, decimal?>> Values { get; init; } = new();
}

public sealed class AccountingPanelRowSourceDto
{
    public string SourceType { get; init; } = string.Empty;
    public int? TotalizerId { get; init; }
    public int? ClassificationId { get; init; }
    public int? SourceParentTotalizerId { get; init; }
}

public sealed class AccountingPanelRowDetailsDto
{
    public bool Available { get; init; }
    public Dictionary<string, Dictionary<string, int>> Counts { get; init; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Dictionary<string, IReadOnlyList<AccountingPanelEntryDto>>>? Data { get; init; }
}

public sealed class AccountingPanelEntryDto
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
