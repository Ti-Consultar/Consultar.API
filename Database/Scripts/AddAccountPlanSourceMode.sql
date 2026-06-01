IF COL_LENGTH('dbo.AccountPlans', 'SourceMode') IS NULL
BEGIN
    ALTER TABLE dbo.AccountPlans
    ADD SourceMode INT NOT NULL
        CONSTRAINT DF_AccountPlans_SourceMode DEFAULT 1;
END
