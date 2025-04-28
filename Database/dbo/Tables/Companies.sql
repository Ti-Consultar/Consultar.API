CREATE TABLE [dbo].[Companies] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(MAX) NOT NULL,
    [DateCreate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [GroupId] INT NOT NULL,
    BusinessEntityId INT NOT NULL,
    Deleted bit DEFAULT 0 NOT NULL,

    CONSTRAINT FK_Companies_BusinessEntity FOREIGN KEY (BusinessEntityId) REFERENCES BusinessEntity(Id),
    CONSTRAINT FK_Company_Group FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Groups] ([Id])  ON DELETE NO ACTION
);

