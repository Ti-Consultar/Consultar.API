/* =========================
   SON CONFIG
========================= */

CREATE TABLE SonConfig
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    ConfigPrincipalId INT NULL,

    CONSTRAINT PK_SonConfig
        PRIMARY KEY (Id),

    CONSTRAINT FK_SonConfig_ConfigPrincipal
        FOREIGN KEY (ConfigPrincipalId)
        REFERENCES ConfigPrincipal(Id)
        ON DELETE SET NULL
);
GO

CREATE INDEX IX_SonConfig_ConfigPrincipalId
ON SonConfig(ConfigPrincipalId);
GO