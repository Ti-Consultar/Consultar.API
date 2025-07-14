CREATE TABLE Classification
(
	Id INT IDENTITY(1,1) PRIMARY KEY,
	Name NVARCHAR(255) NOT NULL,
	TypeOrder INT NOT NULL,
	TypeClassification int NOT NULL,
);