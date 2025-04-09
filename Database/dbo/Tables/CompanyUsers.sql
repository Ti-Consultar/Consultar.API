CREATE TABLE [dbo].[CompanyUsers] (
    [Id] INT PRIMARY KEY IDENTITY(1, 1),
    [UserId] INT NOT NULL,
    [GroupId] INT NOT NULL,
    [CompanyId] INT NULL,
    [SubCompanyId] INT NULL,
    [PermissionId] INT NOT NULL,
    
    CONSTRAINT FK_EmpresaVinculo_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_EmpresaVinculo_Group FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Groups] ([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_EmpresaVinculo_Company FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Companies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_EmpresaVinculo_SubCompany FOREIGN KEY ([SubCompanyId]) REFERENCES [dbo].[SubCompanies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT FK_EmpresaVinculo_Permission FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] ([Id]) ON DELETE CASCADE
);