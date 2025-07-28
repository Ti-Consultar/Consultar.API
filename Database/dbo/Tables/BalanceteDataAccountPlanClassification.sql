Create Table BalanceteDataAccountPlanClassification
(
	Id int Identity (1, 1) primary key NOT NULL,
	AccountPlanClassificationId int NOT NULL,
	CostCenter Varchar(100) NOT NULL 


	    CONSTRAINT FK_BalanceteDataAccountPlanClassification_AccountPlanClassification
        FOREIGN KEY (AccountPlanClassificationId) REFERENCES AccountPlanClassification(Id) ON DELETE CASCADE,
);