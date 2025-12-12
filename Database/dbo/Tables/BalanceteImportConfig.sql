CREATE TABLE BalanceteImportConfig (
    Id INT IDENTITY PRIMARY KEY,
    AccountPlanId INT NOT NULL,
    StartRow INT NOT NULL,

    CostCenterCol INT NOT NULL,
    NameCol INT NOT NULL,
    InitialValueCol INT NOT NULL,
    DebitCol INT NOT NULL,
    CreditCol INT NOT NULL,
    FinalValueCol INT NOT NULL,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

	    
    CONSTRAINT FK_BalanceteImportConfig_Accountplan 
        FOREIGN KEY (AccountPlanId) REFERENCES AccountPlans(Id) ON DELETE CASCADE,

);