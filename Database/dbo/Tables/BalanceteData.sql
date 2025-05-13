CREATE TABLE BalanceteData (
    Id INT PRIMARY KEY,
    BalanceteId INT NOT NULL,
    CostCenter VARCHAR(100),
    Name VARCHAR(100),
    InitialValue DECIMAL(18, 2),
    Credit DECIMAL(18, 2),
    Debit DECIMAL(18, 2),
    FinalValue DECIMAL(18, 2),
    BudgetedAmount BIT,
     CONSTRAINT FK_BalanceteData_Balancete FOREIGN KEY (BalanceteId) REFERENCES Balancete(Id) ON DELETE CASCADE
);