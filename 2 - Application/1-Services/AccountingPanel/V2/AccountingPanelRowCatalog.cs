namespace _2___Application._1_Services.AccountingPanel.V2;

public sealed record AccountingPanelRowDefinition(
    string Code,
    string StatementKey,
    string Name,
    string RowType,
    int DisplayOrder,
    int Level,
    string? ParentCode,
    string SourceType,
    string LegacySourceName,
    string? LegacyParentTotalizerName);

public static class AccountingPanelRowCatalog
{
    public const string Asset = "asset";
    public const string Liability = "liability";
    public const string Totalizer = "totalizer";
    public const string Classification = "classification";
    public const string GeneralTotal = "generalTotal";
    public const string Calculated = "calculated";

    public static IReadOnlyList<AccountingPanelRowDefinition> All { get; } = new[]
    {
        Group("TOTAL_CURRENT_ASSETS", Asset, "Total Ativo Circulante", 10),
        Child("CASH", Asset, "Caixas", 20, "TOTAL_CURRENT_ASSETS", "Total Ativo Circulante"),
        Child("BANKS", Asset, "Bancos", 30, "TOTAL_CURRENT_ASSETS", "Total Ativo Circulante"),
        Child("FINANCIAL_INVESTMENTS", Asset, "Aplicações Financeiras", 40, "TOTAL_CURRENT_ASSETS", "Total Ativo Circulante"),
        Child("CUSTOMERS", Asset, "Clientes", 50, "TOTAL_CURRENT_ASSETS", "Total Ativo Circulante"),
        Child("ADVANCES", Asset, "Adiantamentos", 60, "TOTAL_CURRENT_ASSETS", "Total Ativo Circulante"),
        Child("LOANS_RECEIVABLE", Asset, "Empréstimos", 70, "TOTAL_CURRENT_ASSETS", "Total Ativo Circulante"),

        Group("LONG_TERM_RECEIVABLES", Asset, "Realizavel Longo Prazo", 80),
        Child("RELATED_PARTY_LOANS_RECEIVABLE", Asset, "Empréstimos a Coligadas e Controlada", 90,
            "LONG_TERM_RECEIVABLES", "Realizavel Longo Prazo"),

        Group("TOTAL_NON_CURRENT_ASSETS", Asset, "Total Ativo Não Circulante", 100),
        Child("INVESTMENTS", Asset, "Investimentos", 110, "TOTAL_NON_CURRENT_ASSETS", "Total Ativo Não Circulante"),
        Child("FIXED_ASSETS", Asset, "Imobilizado", 120, "TOTAL_NON_CURRENT_ASSETS", "Total Ativo Não Circulante"),
        Child("INTANGIBLE_ASSETS", Asset, "Intangível", 130, "TOTAL_NON_CURRENT_ASSETS", "Total Ativo Não Circulante"),
        Child("ACCUMULATED_DEPRECIATION", Asset, "Depreciação / Amortização Acumuladas", 140,
            "TOTAL_NON_CURRENT_ASSETS", "Total Ativo Não Circulante"),
        Total("TOTAL_ASSETS", Asset, "TOTAL GERAL DO ATIVO", 150),

        Group("TOTAL_CURRENT_LIABILITIES", Liability, "Total Passivo Circulante", 160),
        Child("MISCELLANEOUS_SUPPLIERS", Liability, "Fornecedores Diversos", 170,
            "TOTAL_CURRENT_LIABILITIES", "Total Passivo Circulante"),
        Child("OTHER_ACCOUNTS_PAYABLE", Liability, "Outras Contas a Pagar", 180,
            "TOTAL_CURRENT_LIABILITIES", "Total Passivo Circulante"),
        Child("CUSTOMER_CREDITS", Liability, "Creditos de Clientes", 190,
            "TOTAL_CURRENT_LIABILITIES", "Total Passivo Circulante"),
        Child("LOANS_AND_FINANCING", Liability, "Empréstimos e Financiamentos", 200,
            "TOTAL_CURRENT_LIABILITIES", "Total Passivo Circulante"),
        Child("SOCIAL_OBLIGATIONS_PAYABLE", Liability, "Obrigações Sociais a Pagar", 210,
            "TOTAL_CURRENT_LIABILITIES", "Total Passivo Circulante"),
        Child("TAX_OBLIGATIONS_PAYABLE", Liability, "Obrigações Fiscais a Pagar", 220,
            "TOTAL_CURRENT_LIABILITIES", "Total Passivo Circulante"),

        Group("TOTAL_NON_CURRENT_LIABILITIES", Liability, "Total Passivo Não Circulante", 230),
        Child("LONG_TERM_LOANS_AND_FINANCING", Liability, "Empréstimos e Financiamentos a Longo Prazo", 240,
            "TOTAL_NON_CURRENT_LIABILITIES", "Total Passivo Não Circulante"),
        Child("INSTALLMENT_TAXES", Liability, "Impostos Parcelados", 250,
            "TOTAL_NON_CURRENT_LIABILITIES", "Total Passivo Não Circulante"),

        Group("EQUITY", Liability, "Patrimônio Liquido", 260),
        Child("SHARE_CAPITAL", Liability, "Capital Social", 270, "EQUITY", "Patrimônio Liquido"),
        Child("RESERVES", Liability, "Reservas", 280, "EQUITY", "Patrimônio Liquido"),
        Child("RETAINED_EARNINGS", Liability, "Lucros / Prejuízos Acumulados", 290,
            "EQUITY", "Patrimônio Liquido"),
        Child("PROFIT_DISTRIBUTION", Liability, "Distribuição de Lucro", 300,
            "EQUITY", "Patrimônio Liquido"),
        Child("ACCUMULATED_PERIOD_RESULT", Liability, "Resultado do Exercício Acumulado", 310,
            "EQUITY", "Patrimônio Liquido"),
        Total("TOTAL_LIABILITIES", Liability, "TOTAL GERAL DO PASSIVO", 320),
        CalculatedRow("BALANCE_DIFFERENCE", Liability, "Diferença do Balanço", 330)
    };

    private static AccountingPanelRowDefinition Group(
        string code,
        string statementKey,
        string name,
        int displayOrder) =>
        new(code, statementKey, name, "section", displayOrder, 0, null,
            Totalizer, name, null);

    private static AccountingPanelRowDefinition Child(
        string code,
        string statementKey,
        string name,
        int displayOrder,
        string parentCode,
        string legacyParentTotalizerName) =>
        new(code, statementKey, name, "classification", displayOrder, 1, parentCode,
            Classification, name, legacyParentTotalizerName);

    private static AccountingPanelRowDefinition Total(
        string code,
        string statementKey,
        string name,
        int displayOrder) =>
        new(code, statementKey, name, "total", displayOrder, 0, null,
            GeneralTotal, name, null);

    private static AccountingPanelRowDefinition CalculatedRow(
        string code,
        string statementKey,
        string name,
        int displayOrder) =>
        new(code, statementKey, name, "validation", displayOrder, 0, null,
            Calculated, name, null);
}
