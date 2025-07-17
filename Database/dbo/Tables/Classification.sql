CREATE TABLE Classification
(
	Id INT IDENTITY(1,1) PRIMARY KEY,
	TotalizerClassificationTemplateId INT  NULL,
	Name NVARCHAR(255) NOT NULL,
	TypeOrder INT NOT NULL,
	TypeClassification int NOT NULL,

	 CONSTRAINT FK_AccountPlanCassification_TotalizerClassificationTemplate 
        FOREIGN KEY (TotalizerClassificationTemplateId) REFERENCES TotalizerClassificationTemplate(Id)
);