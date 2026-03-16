/* =========================
   VIEW CONFIG
========================= */

CREATE TABLE ViewConfig
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    AccountPlanId INT NULL,
    ConfigPrincipalId INT NULL,
    SonConfigId INT NULL,

    CONSTRAINT FK_ViewConfig_AccountPlan
        FOREIGN KEY (AccountPlanId)
        REFERENCES AccountPlans(Id)
        ON DELETE SET NULL,

    CONSTRAINT FK_ViewConfig_ConfigPrincipal
        FOREIGN KEY (ConfigPrincipalId)
        REFERENCES ConfigPrincipal(Id)
        ON DELETE SET NULL,

    CONSTRAINT FK_ViewConfig_SonConfig
        FOREIGN KEY (SonConfigId)
        REFERENCES SonConfig(Id)
        ON DELETE SET NULL
);
GO

CREATE INDEX IX_ViewConfig_AccountPlanId
ON ViewConfig(AccountPlanId);
GO

CREATE INDEX IX_ViewConfig_ConfigPrincipalId
ON ViewConfig(ConfigPrincipalId);
GO

CREATE INDEX IX_ViewConfig_SonConfigId
ON ViewConfig(SonConfigId);
GO