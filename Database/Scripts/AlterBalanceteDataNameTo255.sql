IF COL_LENGTH('dbo.BalanceteData', 'Name') IS NOT NULL
BEGIN
    ALTER TABLE dbo.BalanceteData
    ALTER COLUMN Name VARCHAR(255) NULL;
END
