namespace _2___Application._2_Dto_s.AccountPlan.Balancete.V2;

public sealed class TrialBalanceViewerRowsRequest
{
    public int Offset { get; set; } = 0;
    public int Limit { get; set; } = 200;
    public string? Search { get; set; }
    public string? Levels { get; set; }
    public bool OnlyWithMovement { get; set; } = false;
    public string Sort { get; set; } = "sourceOrder";
    public string Direction { get; set; } = "asc";
}

public sealed class TrialBalanceViewerRowsResponse
{
    public int TrialBalanceId { get; init; }
    public IReadOnlyList<TrialBalanceViewerRowDto> Items { get; init; } = [];
    public TrialBalanceViewerPaginationDto Pagination { get; init; } = new();
}

public sealed class TrialBalanceViewerRowDto
{
    public int Id { get; init; }
    public int SourceOrder { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Level { get; init; }
    public bool HasChildren { get; init; }
    public decimal PreviousBalance { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal FinalBalance { get; init; }
}

public sealed class TrialBalanceViewerPaginationDto
{
    public int Offset { get; init; }
    public int Limit { get; init; }
    public int Returned { get; init; }
    public int Total { get; init; }
    public bool HasMore { get; init; }
}
