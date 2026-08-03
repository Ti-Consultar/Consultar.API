namespace _2___Application._1_Services.DRE.V2;

public sealed record DreRowDefinition(
    string Code,
    string Name,
    string RowType,
    string ValueType,
    int DisplayOrder,
    int Level,
    string? ParentCode,
    string SourceType,
    string LegacySourceName,
    string? LegacyParentTotalizerName,
    bool Required);

public static class DreRowCatalog
{
    public const string Totalizer = "totalizer";
    public const string Classification = "classification";
    public const string Calculated = "calculated";

    public static IReadOnlyList<DreRowDefinition> All { get; } = new[]
    {
        Total("GROSS_REVENUE", "Receita Operacional Bruta", "section", 10),
        Class("PRODUCT_SALES", "Vendas de Produtos", 20, "GROSS_REVENUE", "Receita Operacional Bruta"),
        Class("MERCHANDISE_SALES", "Vendas de Mercadorias", 30, "GROSS_REVENUE", "Receita Operacional Bruta"),
        Class("SERVICE_REVENUE", "Prestação de Serviço", 40, "GROSS_REVENUE", "Receita Operacional Bruta"),
        Class("RENTAL_REVENUE", "Receita Com Locação", 50, "GROSS_REVENUE", "Receita Operacional Bruta",
            name: "Receita com Locação"),

        Total("GROSS_REVENUE_DEDUCTIONS", "(-) Deduções da Receita Bruta", "section", 60),
        Class("SALES_RETURNS", "(-) Devoluções de Vendas", 70, "GROSS_REVENUE_DEDUCTIONS", "(-) Deduções da Receita Bruta"),
        Class("SALES_ALLOWANCES", "(-) Abatimentos", 80, "GROSS_REVENUE_DEDUCTIONS", "(-) Deduções da Receita Bruta"),
        Class("TAXES_AND_CONTRIBUTIONS", "(-) Impostos e Contribuições", 90, "GROSS_REVENUE_DEDUCTIONS", "(-) Deduções da Receita Bruta"),

        Total("NET_REVENUE", "(=) Receita Líquida de Vendas", "subtotal", 100),
        Class("COST_OF_GOODS", "(-) Custos das Mercadorias", 110, null, "(=) Receita Líquida de Vendas"),
        Class("COST_OF_SERVICES", "(-) Custos dos Serviços Prestados", 120, null, "(=) Receita Líquida de Vendas",
            name: "Custos Operacionais"),
        Class("VARIABLE_COSTS", "(-) Custos Variáveis", 130, null, "(=) Receita Líquida de Vendas",
            required: false),
        Total("GROSS_PROFIT", "Lucro Bruto", "subtotal", 140),
        Total("GROSS_MARGIN_PERCENT", "Margem Bruta %", "percentage", 150, "percentage"),
        Class("VARIABLE_EXPENSES", "Despesas Variáveis", 160, null, "Margem Bruta %"),
        Total("CONTRIBUTION_MARGIN", "Margem Contribuição", "subtotal", 170,
            name: "Margem de Contribuição"),
        Total("CONTRIBUTION_MARGIN_PERCENT", "Margem Contribuição %", "percentage", 180, "percentage",
            "Margem de Contribuição %"),

        Total("OPERATING_EXPENSES", "(-) Despesas Operacionais", "section", 190),
        Class("DEPRECIATION_EXPENSE", "Despesas com Depreciação", 200, "OPERATING_EXPENSES", "(-) Despesas Operacionais"),
        Class("SALES_EXPENSES", "Despesas com Vendas", 210, "OPERATING_EXPENSES", "(-) Despesas Operacionais"),
        Class("PERSONNEL_EXPENSES", "Despesas com Pessoal e Encargos", 220, "OPERATING_EXPENSES", "(-) Despesas Operacionais"),
        Class("ADMINISTRATIVE_GENERAL_EXPENSES", "Despesas Administrativas e Gerais", 230, "OPERATING_EXPENSES", "(-) Despesas Operacionais"),
        Class("OTHER_OPERATING_RESULTS", "Outros Resultados Operacionais", 240, "OPERATING_EXPENSES", "(-) Despesas Operacionais"),
        Total("OPERATING_PROFIT", "Lucro Operacional", "subtotal", 250),
        Total("OPERATING_MARGIN_PERCENT", "Margem Operacional %", "percentage", 260, "percentage"),

        Total("OTHER_RESULTS", "Outros Resultados", "section", 270, required: false),
        Class("OTHER_INCOME", "Outras Receitas", 280, "OTHER_RESULTS", "Outros Resultados",
            required: false),
        Class("OTHER_EXPENSES", "Outras Despesas", 290, "OTHER_RESULTS", "Outros Resultados",
            required: false),
        Total("EBIT", "Lucro Antes do Resultado Financeiro", "subtotal", 300),
        Total("EBIT_MARGIN_PERCENT", "Margem LAJIR %", "percentage", 310, "percentage"),
        Class("CAPITAL_GAINS_AND_LOSSES", "Ganhos e Perdas de Capital", 320, null, "Margem Operacional %"),
        Class("OTHER_NON_OPERATING_INCOME", "Outras Receitas não Operacionais", 330, null, "Margem Operacional %"),
        CalculatedTotal("FINANCIAL_RESULT", "Resultado Financeiro", "subtotal", 340),
        Class("FINANCIAL_INCOME", "Receitas Financeiras", 350, "FINANCIAL_RESULT", "Margem LAJIR %"),
        Class("FINANCIAL_EXPENSES", "Despesas Financeiras", 360, "FINANCIAL_RESULT", "Margem LAJIR %"),

        Total("EBT", "Resultado do Exercício Antes do Imposto", "subtotal", 370),
        Class("IRPJ_PROVISION", "Provisão para IRPJ", 380, "EBT", "Margem LAIR %"),
        Class("CSLL_PROVISION", "Provisão para CSLL", 390, "EBT", "Margem LAIR %"),
        Total("EBT_MARGIN_PERCENT", "Margem LAIR %", "percentage", 400, "percentage"),
        Total("NET_INCOME", "Lucro Líquido do Periodo", "subtotal", 410,
            name: "Lucro Líquido do Período"),
        Total("NET_MARGIN_PERCENT", "Margem Líquida %", "percentage", 420, "percentage"),
        Class("EBITDA_DEPRECIATION_ADDBACK", "Despesas com Depreciação", 430, null, "Margem Líquida %",
            "adjustment"),
        Total("EBITDA", "EBITDA", "subtotal", 440),
        Total("EBITDA_MARGIN_PERCENT", "Margem EBITDA %", "percentage", 450, "percentage"),
        Total("NOPAT", "NOPAT", "subtotal", 460),
        Total("NOPAT_MARGIN_PERCENT", "Margem NOPAT %", "percentage", 470, "percentage")
    };

    private static DreRowDefinition Total(
        string code,
        string legacyName,
        string rowType,
        int displayOrder,
        string valueType = "currency",
        string? name = null,
        bool required = true) =>
        new(
            code,
            name ?? legacyName,
            rowType,
            valueType,
            displayOrder,
            0,
            null,
            Totalizer,
            legacyName,
            null,
            required);

    private static DreRowDefinition CalculatedTotal(
        string code,
        string name,
        string rowType,
        int displayOrder) =>
        new(
            code,
            name,
            rowType,
            "currency",
            displayOrder,
            0,
            null,
            Calculated,
            name,
            null,
            true);

    private static DreRowDefinition Class(
        string code,
        string legacyName,
        int displayOrder,
        string? parentCode,
        string legacyParent,
        string rowType = "classification",
        string? name = null,
        bool required = true) =>
        new(
            code,
            name ?? legacyName,
            rowType,
            "currency",
            displayOrder,
            parentCode is null ? 0 : 1,
            parentCode,
            Classification,
            legacyName,
            legacyParent,
            required);
}
