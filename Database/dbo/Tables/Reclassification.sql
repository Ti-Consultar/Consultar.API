CREATE TABLE Reclassification
(
	Id INT IDENTITY(1,1) PRIMARY KEY,
	Name NVARCHAR(255) NOT NULL,
	ClassificationId INT NOT NULL,
	AccountPlanId INT NOT NULL,
	sequential Int Not Null

	 CONSTRAINT FK_Classification FOREIGN KEY (ClassificationId) REFERENCES Classification(Id) ON DELETE CASCADE,
	 CONSTRAINT FK_AccountPlan FOREIGN KEY (AccountPlanId) REFERENCES AccountPlans(Id) ON DELETE CASCADE,
);