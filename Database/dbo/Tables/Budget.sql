CREATE TABLE Budget (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AccountPlansId INT NOT NULL,
    DateMonth INT NOT NULL,
    DateYear INT NOT NULL,
    DateCreate DATE NOT NULL,
    CONSTRAINT FK_Budget_Accountplan 
        FOREIGN KEY (AccountPlansId) REFERENCES AccountPlans(Id) ON DELETE CASCADE
);