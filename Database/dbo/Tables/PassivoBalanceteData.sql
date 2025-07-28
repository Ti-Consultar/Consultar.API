CREATE TABLE PassivoBalanceteData
(
	Id INT IDENTITY(1,1) PRIMARY KEY,
	AccountPlansId INT NOT NULL,
	BalanceteId INT NOT NULL,
	PassivoId INT NOT NULL,
	BalanceteDataId INT NOT NULL,

	 -- Foreign Keys
    CONSTRAINT FK_PassivoBalanceteData_AccountPlans FOREIGN KEY (AccountPlansId)
        REFERENCES AccountPlans (Id)
        ON DELETE CASCADE,
    
          CONSTRAINT FK_PassivoBalanceteData_Balancete FOREIGN KEY (BalanceteId)
        REFERENCES Balancete (Id)
        ON DELETE NO ACTION,

        CONSTRAINT FK_PassivoBalanceteData_Passivo FOREIGN KEY (PassivoId)
        REFERENCES ClassificationPassivo (Id)
        ON DELETE NO ACTION,

    CONSTRAINT FK_PassivoBalanceteData_BalanceteData FOREIGN KEY (BalanceteDataId)
        REFERENCES BalanceteData (Id)
        ON DELETE NO ACTION

);
