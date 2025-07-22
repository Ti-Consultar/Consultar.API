CREATE TABLE AccountPlanClassification
(
    Id INT IDENTITY (1, 1) PRIMARY KEY NOT NULL,
    AccountPlanId INT NOT NULL,
    TotalizerClassificationId INT NULL,
    Name NVARCHAR(200) NOT NULL,
    TypeOrder INT NOT NULL,
    TypeClassification INT NULL,
    BalancoReclassificadoId INT NULL,
    
    CONSTRAINT FK_AccountPlanClassification_Accountplan 
        FOREIGN KEY (AccountPlanId) REFERENCES AccountPlans(Id) ON DELETE CASCADE,

    CONSTRAINT FK_AccountPlanClassification_TotalizerClassification 
        FOREIGN KEY (TotalizerClassificationId) REFERENCES TotalizerClassification(Id) ON DELETE CASCADE,

        
    CONSTRAINT FK_AccountPlanClassification_BalancoReclassificado
        FOREIGN KEY (BalancoReclassificadoId) REFERENCES BalancoReclassificado(Id) ON DELETE CASCADE,
);
