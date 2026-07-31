using System.Text.Json.Serialization;

namespace _2___Application._2_Dto_s.DRE.V2;

public sealed class DreV2Response
{
    public DreV2DataDto Data { get; init; } = new();
}

public sealed class DreV2DataDto
{
    public IReadOnlyList<DrePeriodDto> Periods { get; init; } = Array.Empty<DrePeriodDto>();
    public IReadOnlyList<DreScenarioDto> Scenarios { get; init; } = Array.Empty<DreScenarioDto>();
    public IReadOnlyList<DreRowDto> Rows { get; init; } = Array.Empty<DreRowDto>();
}

public sealed class DrePeriodDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int? Year { get; init; }
    public int? Month { get; init; }
    public string Type { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class DreScenarioDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class DreRowDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string RowType { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public int Level { get; init; }
    public string? ParentCode { get; init; }
    public bool Expandable { get; init; }
    public DreRowSourceDto Source { get; init; } = new();
    public DreRowDetailsMetadataDto Details { get; init; } = new();
    public Dictionary<string, Dictionary<string, decimal?>> Values { get; init; } = new();
}

public sealed class DreRowSourceDto
{
    public string SourceType { get; init; } = string.Empty;
    public int? TotalizerId { get; init; }
    public int? ClassificationId { get; init; }
    public int? SourceParentTotalizerId { get; init; }
}

public sealed class DreRowDetailsMetadataDto
{
    public bool Available { get; init; }
    public Dictionary<string, Dictionary<string, int>> Counts { get; init; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Dictionary<string, IReadOnlyList<DreDetailEntryDto>>>? Data { get; init; }
}

public sealed class DreRowDetailsV2Response
{
    public DreRowDetailsV2DataDto Data { get; init; } = new();
}

public sealed class DreRowDetailsV2DataDto
{
    public string RowCode { get; init; } = string.Empty;
    public string Scenario { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public IReadOnlyList<DreDetailEntryDto> Entries { get; init; } = Array.Empty<DreDetailEntryDto>();
}

public sealed class DreDetailEntryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CostCenter { get; init; } = string.Empty;
    public decimal InitialValue { get; init; }
    public decimal CreditValue { get; init; }
    public decimal DebitValue { get; init; }
    public decimal Value { get; init; }
}
