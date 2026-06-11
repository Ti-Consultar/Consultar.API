IF COL_LENGTH('dbo.BalanceteData', 'Name') IS NOT NULL
BEGIN
    ALTER TABLE dbo.BalanceteData
    ALTER COLUMN Name NVARCHAR(255) NULL;
END;

IF COL_LENGTH('dbo.BudgetData', 'Name') IS NOT NULL
BEGIN
    ALTER TABLE dbo.BudgetData
    ALTER COLUMN Name NVARCHAR(255) NULL;
END;

IF COL_LENGTH('dbo.AccountPlanAccount', 'Name') IS NOT NULL
BEGIN
    ALTER TABLE dbo.AccountPlanAccount
    ALTER COLUMN Name NVARCHAR(255) NOT NULL;
END;
