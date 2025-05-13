CREATE TABLE Balancete (
    Id INT PRIMARY KEY,
    AccountPlansId INT NOT NULL,
    DateMonth INT NOT NULL,
    DateYear INT NOT NULL,
    Status INT NOT NULL,
    DateCreate DATE NOT NULL,
    CONSTRAINT FK_Balancete_Accountplan FOREIGN KEY (AccountPlansId) REFERENCES AccountPlans(Id) ON DELETE CASCADE
);