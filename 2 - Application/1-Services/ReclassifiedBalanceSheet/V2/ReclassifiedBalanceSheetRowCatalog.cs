namespace _2___Application._1_Services.ReclassifiedBalanceSheet.V2;

public sealed record ReclassifiedBalanceSheetRowDefinition(
    string Code,
    string StatementKey,
    string Name,
    string RowType,
    int DisplayOrder,
    string SourceType,
    string LegacySourceName,
    bool Expandable,
    IReadOnlyList<string> LegacyDetailTotalizerNames);

public static class ReclassifiedBalanceSheetRowCatalog
{
    public const string Asset = "asset";
    public const string Liability = "liability";
    public const string LegacyGroup = "legacyGroup";
    public const string LegacyTotalizer = "legacyTotalizer";
    public const string Calculated = "calculated";

    public static IReadOnlyList<ReclassifiedBalanceSheetRowDefinition> All { get; } = new[]
    {
        Group("FINANCIAL_ASSETS", Asset, "Ativo Financeiro", 10,
            "Caixa e Equivalente de Caixa", "Aplicação Financeira"),
        Group("OPERATING_ASSETS", Asset, "Ativo Operacional", 20,
            "Clientes", "Estoques", "Outros Ativos Operacionais Total"),
        Group("NON_CURRENT_ASSETS", Asset, "Ativo Não Circulante", 30,
            "Ativo Não Circulante Operacional"),
        Group("FIXED_ASSETS", Asset, "Ativo Fixo", 40,
            "Investimentos", "Imobilizado", "Depreciação / Amort. Acumulada", "Intangível"),
        Total("TOTAL_ASSETS", Asset, "TOTAL DO ATIVO", 50),

        Group("FINANCIAL_LIABILITIES", Liability, "Passivo Financeiro", 60,
            "Empréstimos e Financiamentos"),
        Group("OPERATING_LIABILITIES", Liability, "Passivo Operacional", 70,
            "Fornecedores", "Obrigações Tributárias e Trabalhistas", "Outros Passivos Operacionais Total"),
        Group("NON_CURRENT_LIABILITIES", Liability, "Passivo Não Circulante", 80,
            "Passivo Não Circulante Financeiro"),
        GroupWithLegacyName("EQUITY", Liability, "Patrimônio Líquido", 90, "Patrimônio Liquido",
            "Capital Social", "Reservas", "Lucros / Prejuízos Acumulados",
            "Distribuição de Lucro", "Resultado Acumulado"),
        Total("TOTAL_LIABILITIES", Liability, "TOTAL DO PASSIVO", 100),
        CalculatedRow("BALANCE_DIFFERENCE", Liability, "Diferença do Balanço", 110)
    };

    private static ReclassifiedBalanceSheetRowDefinition Group(
        string code,
        string statementKey,
        string name,
        int displayOrder,
        params string[] detailTotalizerNames) =>
        GroupWithLegacyName(code, statementKey, name, displayOrder, name, detailTotalizerNames);

    private static ReclassifiedBalanceSheetRowDefinition GroupWithLegacyName(
        string code,
        string statementKey,
        string name,
        int displayOrder,
        string legacyName,
        params string[] detailTotalizerNames) =>
        new(code, statementKey, name, "section", displayOrder, LegacyGroup, legacyName, true,
            detailTotalizerNames);

    private static ReclassifiedBalanceSheetRowDefinition Total(
        string code,
        string statementKey,
        string name,
        int displayOrder) =>
        new(code, statementKey, name, "total", displayOrder, LegacyTotalizer, name, false,
            Array.Empty<string>());

    private static ReclassifiedBalanceSheetRowDefinition CalculatedRow(
        string code,
        string statementKey,
        string name,
        int displayOrder) =>
        new(code, statementKey, name, "validation", displayOrder, Calculated, name, false,
            Array.Empty<string>());
}
