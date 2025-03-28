﻿CREATE TABLE [dbo].[CompanyUsers] (
    [Id] INT PRIMARY KEY IDENTITY(1, 1),
    [UserId] INT NOT NULL,
    [CompanyId] INT NOT NULL,
    [SubCompanyId] INT NULL,
    [PermissionId] INT NOT NULL,
    
    CONSTRAINT FK_EmpresaVinculo_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_EmpresaVinculo_Company FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Companies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_EmpresaVinculo_SubCompany FOREIGN KEY ([SubCompanyId]) REFERENCES [dbo].[Companies] ([Id]) ON DELETE NO ACTION,  -- Alterado para NO ACTION
    CONSTRAINT FK_EmpresaVinculo_Permission FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] ([Id]) ON DELETE CASCADE
);