CREATE TABLE Interaction (
    Id INT PRIMARY KEY,
    UserId INT NOT NULL,
    Action INT NOT NULL,
    DateAction DATE NOT NULL,
    BalanceteId INT,
    BalanceteDataId INT,

	    CONSTRAINT FK_Interaction_Balancete FOREIGN KEY (BalanceteId) REFERENCES Balancete(Id) ON DELETE CASCADE,
		CONSTRAINT FK_Interaction_BalanceteData FOREIGN KEY (BalanceteDataId) REFERENCES BalanceteData(Id) ON DELETE NO ACTION
);