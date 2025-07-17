CREATE TABLE AccountPlanClassification
(
    Id INT IDENTITY (1, 1) PRIMARY KEY NOT NULL,
    AccountPlanId INT NOT NULL,
    TotalizerClassificationId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    TypeOrder INT NOT NULL,
    TypeClassification INT NULL,
    
    CONSTRAINT FK_AccountPlanCassification_Accountplan 
        FOREIGN KEY (AccountPlanId) REFERENCES AccountPlans(Id) ON DELETE CASCADE,

    CONSTRAINT FK_AccountPlanCassification_TotalizerClassification 
        FOREIGN KEY (TotalizerClassificationId) REFERENCES TotalizerClassification(Id) ON DELETE CASCADE
);
