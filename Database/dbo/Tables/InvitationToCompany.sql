CREATE TABLE [dbo].[InvitationToCompany] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [CompanyId] INT NOT NULL,
    [SubCompanyId] INT NULL,
    [UserId] INT NOT NULL,
    [InvitedById] INT NOT NULL,
    [PermissionId] INT NOT NULL,
    [Status] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME NULL,

    CONSTRAINT FK_Invitation_Company FOREIGN KEY ([CompanyId]) 
        REFERENCES [dbo].[Companies] ([Id]) ON DELETE CASCADE,
    
    CONSTRAINT FK_Invitation_SubCompany FOREIGN KEY ([SubCompanyId]) 
        REFERENCES [dbo].[SubCompanies] ([Id]) ON DELETE NO ACTION, 
    
    CONSTRAINT FK_Invitation_User FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
    
    CONSTRAINT FK_Invitation_InvitedBy FOREIGN KEY ([InvitedById]) 
        REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION,

    CONSTRAINT FK_Invitation_Permission FOREIGN KEY ([PermissionId]) 
        REFERENCES [dbo].[Permissions] ([Id]) ON DELETE CASCADE
);
