CREATE TABLE [dbo].[SubCompanies] (
    [Id] INT PRIMARY KEY IDENTITY(1, 1),
    [Name] NVARCHAR(MAX) NOT NULL,
    [DateCreate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [CompanyId] INT NOT NULL,
    CONSTRAINT FK_SubCompanies_Company FOREIGN KEY (CompanyId) REFERENCES [dbo].[Companies]([Id]) ON DELETE CASCADE
);