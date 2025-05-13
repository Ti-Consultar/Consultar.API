CREATE TABLE PlansAccountUsers (
    Id INT PRIMARY KEY IDENTITY,
    AccountPlansId INT NOT NULL,
    BalanceteId INT NOT NULL,

     CONSTRAINT FK_PlansAccountUsers_AccountPlan FOREIGN KEY (AccountPlansId) REFERENCES AccountPlans(Id) ON DELETE CASCADE,
     CONSTRAINT FK_PlansAccountUsers_Balancete FOREIGN KEY (BalanceteId) REFERENCES Balancete(Id) ON DELETE NO ACTION,
);