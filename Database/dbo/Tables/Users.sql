CREATE TABLE [dbo].[Users] (
    [Id]  INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (MAX) NOT NULL,
    [Email]    NVARCHAR (255) NOT NULL,  
    [Password] NVARCHAR (MAX) NOT NULL,
    [Role]     NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC)   
);
