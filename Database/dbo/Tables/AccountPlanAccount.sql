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
