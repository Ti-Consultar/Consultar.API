DECLARE @AccountPlanId INT = NULL; -- Informe o AccountPlanId aqui
DECLARE @CompanyId INT = NULL; -- Opcional: informe para localizar planos da empresa
DECLARE @SubCompanyId INT = NULL; -- Opcional: informe para filtrar por subempresa
DECLARE @OnlyInvalidEncoding BIT = 1; -- 1 = somente nomes com caractere de substituicao; 0 = todas as contas

SELECT
    ap.Id AS AccountPlanId,
    ap.GroupId,
    g.Name AS GroupName,
    ap.CompanyId,
    c.Name AS CompanyName,
    ap.SubCompanyId,
    sc.Name AS SubCompanyName,
    ap.SourceMode
FROM dbo.AccountPlans ap
INNER JOIN dbo.Groups g ON g.Id = ap.GroupId
LEFT JOIN dbo.Companies c ON c.Id = ap.CompanyId
LEFT JOIN dbo.SubCompanies sc ON sc.Id = ap.SubCompanyId
WHERE (@AccountPlanId IS NULL OR ap.Id = @AccountPlanId)
  AND (@CompanyId IS NULL OR ap.CompanyId = @CompanyId)
  AND (@SubCompanyId IS NULL OR ap.SubCompanyId = @SubCompanyId)
ORDER BY ap.Id;

SELECT
    apa.Id,
    apa.AccountPlanId,
    apa.CostCenter,
    apa.Name,
    apa.AccountPlanClassificationId,
    CASE apa.Status
        WHEN 1 THEN 'PendingClassification'
        WHEN 2 THEN 'Classified'
        ELSE CONCAT('Unknown(', apa.Status, ')')
    END AS ClassificationStatus,
    CASE apa.Origin
        WHEN 1 THEN 'BalanceteImport'
        WHEN 2 THEN 'ExcelUpload'
        WHEN 3 THEN 'Manual'
        WHEN 4 THEN 'LegacyBalanceteBackfill'
        ELSE CONCAT('Unknown(', apa.Origin, ')')
    END AS Origin,
    apa.CreatedAt,
    apa.UpdatedAt
FROM dbo.AccountPlanAccount apa
WHERE apa.AccountPlanId = @AccountPlanId
  AND (@OnlyInvalidEncoding = 0 OR apa.Name LIKE N'%' + NCHAR(65533) + N'%')
ORDER BY apa.CostCenter;

SELECT
    b.Id AS BalanceteId,
    b.AccountPlansId AS AccountPlanId,
    b.DateYear,
    b.DateMonth,
    bd.Id AS BalanceteDataId,
    bd.CostCenter,
    bd.Name,
    bd.InitialValue,
    bd.Debit,
    bd.Credit,
    bd.FinalValue,
    bd.CreatedAt
FROM dbo.BalanceteData bd
INNER JOIN dbo.Balancete b ON b.Id = bd.BalanceteId
WHERE b.AccountPlansId = @AccountPlanId
  AND (@OnlyInvalidEncoding = 0 OR bd.Name LIKE N'%' + NCHAR(65533) + N'%')
ORDER BY b.DateYear DESC, b.DateMonth DESC, bd.CostCenter;
