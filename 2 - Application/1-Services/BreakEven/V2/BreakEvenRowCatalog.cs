namespace _2___Application._1_Services.BreakEven.V2;

public sealed record BreakEvenRowRule(
    string Code,
    string Behavior,
    bool CanSimulate,
    int DisplayOrder,
    int? Level = null,
    string? ParentCode = null,
    bool OverrideHierarchy = false,
    string? SimulationSignRule = null);

public static class BreakEvenRowCatalog
{
    public const string Variable = "VARIABLE";
    public const string Fixed = "FIXED";
    public const string Calculated = "CALCULATED";
    public const string NegativeSimulationSign = "negative";
    public const string FreeSimulationSign = "free";

    public static IReadOnlyList<BreakEvenRowRule> All { get; } = new[]
    {
        CalculatedRow("GROSS_REVENUE", 10),
        VariableRow("PRODUCT_SALES", 20),
        VariableRow("MERCHANDISE_SALES", 30),
        VariableRow("SERVICE_REVENUE", 40),
        VariableRow("RENTAL_REVENUE", 50),

        CalculatedRow("GROSS_REVENUE_DEDUCTIONS", 60),
        VariableRow("SALES_RETURNS", 70, true),
        VariableRow("SALES_ALLOWANCES", 80, true),
        VariableRow("TAXES_AND_CONTRIBUTIONS", 90, true),
        CalculatedRow("NET_REVENUE", 100),

        VariableRow("COST_OF_GOODS", 110, true),
        VariableRow("COST_OF_SERVICES", 120, true),
        VariableRow("VARIABLE_COSTS", 130),
        CalculatedRow("GROSS_PROFIT", 140),
        CalculatedRow("GROSS_MARGIN_PERCENT", 150),
        VariableRow("VARIABLE_EXPENSES", 160, true),
        CalculatedRow("CONTRIBUTION_MARGIN", 170),
        CalculatedRow("CONTRIBUTION_MARGIN_PERCENT", 180),

        CalculatedRow("OPERATING_EXPENSES", 190),
        FixedRow("DEPRECIATION_EXPENSE", 200, true),
        FixedRow("SALES_EXPENSES", 210, true),
        FixedRow("PERSONNEL_EXPENSES", 220, true),
        FixedRow("ADMINISTRATIVE_GENERAL_EXPENSES", 230, true),
        FixedRow("OTHER_OPERATING_RESULTS", 240, true, 0, null, FreeSimulationSign),
        CalculatedRow("OPERATING_PROFIT", 250),
        CalculatedRow("OPERATING_MARGIN_PERCENT", 260),

        CalculatedRow("OTHER_RESULTS", 270),
        FixedRow("OTHER_INCOME", 280, true, simulationSignRule: FreeSimulationSign),
        FixedRow("OTHER_EXPENSES", 290, true),
        FixedRow("CAPITAL_GAINS_AND_LOSSES", 300, true, 1, "OTHER_RESULTS", FreeSimulationSign),
        FixedRow("OTHER_NON_OPERATING_INCOME", 310),
        CalculatedRow("EBIT", 320),
        CalculatedRow("EBIT_MARGIN_PERCENT", 330),

        CalculatedRow("FINANCIAL_RESULT", 340),
        FixedRow("FINANCIAL_INCOME", 350, true, simulationSignRule: FreeSimulationSign),
        FixedRow("FINANCIAL_EXPENSES", 360, true),
        CalculatedRow("EBT", 370),
        CalculatedRow("EBT_MARGIN_PERCENT", 380)
    };

    public static IReadOnlyDictionary<string, BreakEvenRowRule> ByCode { get; } =
        All.ToDictionary(rule => rule.Code, StringComparer.Ordinal);

    private static BreakEvenRowRule VariableRow(string code, int order, bool canSimulate = false) =>
        new(
            code,
            Variable,
            canSimulate,
            order,
            SimulationSignRule: canSimulate ? NegativeSimulationSign : null);

    private static BreakEvenRowRule FixedRow(
        string code,
        int order,
        bool canSimulate = false,
        int? level = null,
        string? parentCode = null,
        string? simulationSignRule = null) =>
        new(
            code,
            Fixed,
            canSimulate,
            order,
            level,
            parentCode,
            level.HasValue || parentCode is not null,
            canSimulate ? simulationSignRule ?? NegativeSimulationSign : null);

    private static BreakEvenRowRule CalculatedRow(string code, int order) =>
        new(code, Calculated, false, order);
}
