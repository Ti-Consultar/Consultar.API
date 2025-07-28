Create Table TotalizerClassification
(
	Id Int identity (1, 1) primary key NOT NULL,
	AccountPlanId Int Not Null,
	Name Nvarchar (200) NOT NULL,
	typeOrder int NOT NULL,	
	    CONSTRAINT FK_TotalizerClassification_AccountPlan 
        FOREIGN KEY (AccountPlanId) REFERENCES AccountPlans(Id) 
)