CREATE TABLE [dbo].[SubCompanies] (
    [Id] INT PRIMARY KEY IDENTITY(1, 1),
    [Name] NVARCHAR(MAX) NOT NULL,
    [DateCreate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [CompanyId] INT NOT NULL,
     BusinessEntityId INT NOT NULL,
     Deleted bit DEFAULT 0 NOT NULL,

    CONSTRAINT FK_SubCompanies_BusinessEntity FOREIGN KEY (BusinessEntityId) REFERENCES BusinessEntity(Id),
    CONSTRAINT FK_SubCompanies_Company FOREIGN KEY (CompanyId) REFERENCES [dbo].[Companies]([Id]) ON DELETE CASCADE
);