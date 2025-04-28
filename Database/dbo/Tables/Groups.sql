CREATE TABLE Groups (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    DateCreate DATETIME NOT NULL,
    BusinessEntityId INT NOT NULL,
    Deleted bit DEFAULT 0 NOT NULL,
    CONSTRAINT FK_Groups_BusinessEntity FOREIGN KEY (BusinessEntityId) REFERENCES BusinessEntity(Id)
);
