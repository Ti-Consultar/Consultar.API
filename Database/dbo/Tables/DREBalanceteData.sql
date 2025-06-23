CREATE TABLE DREBalanceteData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    DREId INT NOT NULL,
    BalanceteId INT NOT NULL,
    BalanceteDataId INT NOT NULL,

    -- Foreign Keys
    CONSTRAINT FK_DREBalanceteData_DRE FOREIGN KEY (DREId)
        REFERENCES DRE (Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_DREBalanceteData_Balancete FOREIGN KEY (BalanceteId)
        REFERENCES Balancete (Id)
        ON DELETE NO ACTION,

    CONSTRAINT FK_DREBalanceteData_BalanceteData FOREIGN KEY (BalanceteDataId)
        REFERENCES BalanceteData (Id)
        ON DELETE NO ACTION
);
