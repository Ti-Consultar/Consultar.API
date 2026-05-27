IF OBJECT_ID('dbo.AccountPlanAccount', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AccountPlanAccount
    (
        Id INT IDENTITY (1, 1) PRIMARY KEY NOT NULL,
        AccountPlanId INT NOT NULL,
        CostCenter VARCHAR(100) NOT NULL,
        Name NVARCHAR(255) NOT NULL,
        AccountPlanClassificationId INT NULL,
        Status INT NOT NULL,
        Origin INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AccountPlanAccount_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,

        CONSTRAINT FK_AccountPlanAccount_AccountPlans
            FOREIGN KEY (AccountPlanId) REFERENCES dbo.AccountPlans(Id) ON DELETE CASCADE,

        CONSTRAINT FK_AccountPlanAccount_AccountPlanClassification
            FOREIGN KEY (AccountPlanClassificationId) REFERENCES dbo.AccountPlanClassification(Id) ON DELETE NO ACTION,

        CONSTRAINT UQ_AccountPlanAccount_AccountPlan_CostCenter
            UNIQUE (AccountPlanId, CostCenter)
    );
END

INSERT INTO dbo.AccountPlanAccount
(
    AccountPlanId,
    CostCenter,
    Name,
    AccountPlanClassificationId,
    Status,
    Origin,
    CreatedAt,
    UpdatedAt
)
SELECT
    source.AccountPlansId,
    source.CostCenter,
    COALESCE(source.Name, ''),
    bond.AccountPlanClassificationId,
    CASE WHEN bond.AccountPlanClassificationId IS NULL THEN 1 ELSE 2 END,
    1,
    SYSUTCDATETIME(),
    SYSUTCDATETIME()
FROM
(
    SELECT
        b.AccountPlansId,
        LTRIM(RTRIM(bd.CostCenter)) AS CostCenter,
        MAX(NULLIF(LTRIM(RTRIM(bd.Name)), '')) AS Name
    FROM dbo.BalanceteData bd
    INNER JOIN dbo.Balancete b ON b.Id = bd.BalanceteId
    WHERE bd.CostCenter IS NOT NULL
      AND LTRIM(RTRIM(bd.CostCenter)) <> ''
    GROUP BY b.AccountPlansId, LTRIM(RTRIM(bd.CostCenter))
) source
OUTER APPLY
(
    SELECT TOP 1 bdapc.AccountPlanClassificationId
    FROM dbo.BalanceteDataAccountPlanClassification bdapc
    INNER JOIN dbo.AccountPlanClassification apc ON apc.Id = bdapc.AccountPlanClassificationId
    WHERE apc.AccountPlanId = source.AccountPlansId
      AND LTRIM(RTRIM(bdapc.CostCenter)) = source.CostCenter
    ORDER BY bdapc.Id
) bond
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.AccountPlanAccount apa
    WHERE apa.AccountPlanId = source.AccountPlansId
      AND apa.CostCenter = source.CostCenter
);
