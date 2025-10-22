CREATE TABLE BudgetData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BudgetId INT NOT NULL,
    CostCenter VARCHAR(100),
    Name VARCHAR(100),
    InitialValue DECIMAL(18, 2),
    Credit DECIMAL(18, 2),
    Debit DECIMAL(18, 2),
    FinalValue DECIMAL(18, 2),
    BudgetedAmount BIT,
     CONSTRAINT FK_BudgetData_Budget FOREIGN KEY (BudgetId) REFERENCES Budget(Id) ON DELETE CASCADE
);