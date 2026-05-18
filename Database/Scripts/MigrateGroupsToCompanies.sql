/*
    Migra grupos cadastrados no nivel errado para empresas de um grupo destino.

    O script:
      1. Cria uma nova BusinessEntity para cada grupo de origem.
      2. Cria uma Company no grupo destino usando os dados do grupo de origem.
      3. Migra usuarios/convites do escopo grupo antigo para o escopo empresa nova.
      4. Move ou cria AccountPlans no escopo da empresa nova.
      5. Remove os grupos antigos.
      6. Remove as BusinessEntities originais quando nao estiverem mais referenciadas.

    Recomendacao:
      - Rode primeiro em homologacao.
      - Em producao, faca backup antes.
      - Para simular, altere o COMMIT no final para ROLLBACK.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @TargetGroupId INT = 123; -- TODO: informe o grupo correto que recebera as novas empresas

DECLARE @SourceGroups TABLE
(
    GroupId INT NOT NULL PRIMARY KEY
);

INSERT INTO @SourceGroups (GroupId)
VALUES
    (10),
    (11),
    (12); -- TODO: informe os grupos cadastrados por engano

DECLARE @MigrationMap TABLE
(
    SourceGroupId INT NOT NULL PRIMARY KEY,
    SourceGroupName NVARCHAR(255) NOT NULL,
    SourceBusinessEntityId INT NOT NULL,
    NewBusinessEntityId INT NULL,
    NewCompanyId INT NULL
);

INSERT INTO @MigrationMap
(
    SourceGroupId,
    SourceGroupName,
    SourceBusinessEntityId
)
SELECT
    g.Id,
    g.Name,
    g.BusinessEntityId
FROM @SourceGroups sg
JOIN Groups g ON g.Id = sg.GroupId;

/* Validacoes */

IF NOT EXISTS
(
    SELECT 1
    FROM Groups
    WHERE Id = @TargetGroupId
      AND Deleted = 0
)
BEGIN
    THROW 50000, 'Grupo destino inexistente ou deletado.', 1;
END;

IF EXISTS
(
    SELECT sg.GroupId
    FROM @SourceGroups sg
    LEFT JOIN Groups g ON g.Id = sg.GroupId
    WHERE g.Id IS NULL
)
BEGIN
    THROW 50001, 'Um ou mais grupos de origem nao existem.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM @SourceGroups sg
    WHERE sg.GroupId = @TargetGroupId
)
BEGIN
    THROW 50002, 'O grupo destino nao pode estar na lista de grupos de origem.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM @SourceGroups sg
    JOIN Groups g ON g.Id = sg.GroupId
    WHERE g.Deleted = 1
)
BEGIN
    THROW 50003, 'Um ou mais grupos de origem ja estao deletados.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM @SourceGroups sg
    JOIN Companies c ON c.GroupId = sg.GroupId
    WHERE c.Deleted = 0
)
BEGIN
    THROW 50004, 'Um ou mais grupos de origem possuem empresas ativas. Migre manualmente esse caso.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM @SourceGroups sg
    JOIN Companies c ON c.GroupId = sg.GroupId
)
BEGIN
    THROW 50007, 'Um ou mais grupos de origem possuem empresas ativas ou inativas. Este script so remove grupos sem empresas.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM @SourceGroups sg
    JOIN AccountPlans ap ON ap.GroupId = sg.GroupId
    LEFT JOIN Balancete b ON b.AccountPlansId = ap.Id
    LEFT JOIN Budget bu ON bu.AccountPlansId = ap.Id
    LEFT JOIN AccountPlanClassification apc ON apc.AccountPlanId = ap.Id
    LEFT JOIN BalanceteImportConfig bic ON bic.AccountPlanId = ap.Id
    LEFT JOIN BalancoReclassificado br ON br.AccountPlanId = ap.Id
    LEFT JOIN PlansAccountUsers pau ON pau.AccountPlansId = ap.Id
    LEFT JOIN Parameter p ON p.AccountPlansId = ap.Id
    LEFT JOIN TotalizerClassification tc ON tc.AccountPlanId = ap.Id
    LEFT JOIN ViewConfig vc ON vc.AccountPlanId = ap.Id
    WHERE b.Id IS NOT NULL
       OR bu.Id IS NOT NULL
       OR apc.Id IS NOT NULL
       OR bic.Id IS NOT NULL
       OR br.Id IS NOT NULL
       OR pau.Id IS NOT NULL
       OR p.Id IS NOT NULL
       OR tc.Id IS NOT NULL
       OR vc.Id IS NOT NULL
)
BEGIN
    THROW 50005, 'Ha dados vinculados ao plano de contas de algum grupo de origem. Migração abortada.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM @MigrationMap m
    JOIN BusinessEntity be ON be.Id = m.SourceBusinessEntityId
    JOIN Companies c ON c.GroupId = @TargetGroupId
    JOIN BusinessEntity existingBe ON existingBe.Id = c.BusinessEntityId
    WHERE c.Deleted = 0
      AND existingBe.Cnpj = be.Cnpj
)
BEGIN
    THROW 50006, 'Ja existe empresa ativa no grupo destino com o mesmo CNPJ de algum grupo de origem.', 1;
END;

/* Migracao */

DECLARE
    @SourceGroupId INT,
    @SourceGroupName NVARCHAR(255),
    @SourceBusinessEntityId INT,
    @NewBusinessEntityId INT,
    @NewCompanyId INT;

DECLARE source_group_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT
    SourceGroupId,
    SourceGroupName,
    SourceBusinessEntityId
FROM @MigrationMap
ORDER BY SourceGroupId;

OPEN source_group_cursor;

FETCH NEXT FROM source_group_cursor
INTO @SourceGroupId, @SourceGroupName, @SourceBusinessEntityId;

WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO BusinessEntity
    (
        NomeFantasia,
        RazaoSocial,
        Cnpj,
        Logradouro,
        Numero,
        Bairro,
        Municipio,
        Uf,
        Cep,
        Telefone,
        Email,
        Deleted
    )
    SELECT
        NomeFantasia,
        RazaoSocial,
        Cnpj,
        Logradouro,
        Numero,
        Bairro,
        Municipio,
        Uf,
        Cep,
        Telefone,
        Email,
        0
    FROM BusinessEntity
    WHERE Id = @SourceBusinessEntityId;

    SET @NewBusinessEntityId = SCOPE_IDENTITY();

    INSERT INTO Companies
    (
        Name,
        DateCreate,
        GroupId,
        BusinessEntityId,
        Deleted
    )
    VALUES
    (
        @SourceGroupName,
        GETDATE(),
        @TargetGroupId,
        @NewBusinessEntityId,
        0
    );

    SET @NewCompanyId = SCOPE_IDENTITY();

    UPDATE @MigrationMap
    SET
        NewBusinessEntityId = @NewBusinessEntityId,
        NewCompanyId = @NewCompanyId
    WHERE SourceGroupId = @SourceGroupId;

    INSERT INTO CompanyUsers
    (
        UserId,
        GroupId,
        CompanyId,
        SubCompanyId,
        PermissionId
    )
    SELECT DISTINCT
        cu.UserId,
        @TargetGroupId,
        @NewCompanyId,
        NULL,
        cu.PermissionId
    FROM CompanyUsers cu
    WHERE cu.GroupId = @SourceGroupId
      AND cu.CompanyId IS NULL
      AND cu.SubCompanyId IS NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM CompanyUsers existing
          WHERE existing.UserId = cu.UserId
            AND existing.GroupId = @TargetGroupId
            AND existing.CompanyId = @NewCompanyId
            AND existing.SubCompanyId IS NULL
      );

    UPDATE InvitationToCompany
    SET
        GroupId = @TargetGroupId,
        CompanyId = @NewCompanyId,
        SubCompanyId = NULL,
        UpdatedAt = GETDATE()
    WHERE GroupId = @SourceGroupId
      AND CompanyId IS NULL
      AND SubCompanyId IS NULL;

    UPDATE ap
    SET
        ap.GroupId = @TargetGroupId,
        ap.CompanyId = @NewCompanyId,
        ap.SubCompanyId = NULL
    FROM AccountPlans ap
    WHERE ap.GroupId = @SourceGroupId
      AND ap.CompanyId IS NULL
      AND ap.SubCompanyId IS NULL;

    INSERT INTO AccountPlans
    (
        GroupId,
        CompanyId,
        SubCompanyId
    )
    SELECT
        @TargetGroupId,
        @NewCompanyId,
        NULL
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM AccountPlans ap
        WHERE ap.GroupId = @TargetGroupId
          AND ap.CompanyId = @NewCompanyId
          AND ap.SubCompanyId IS NULL
    );

    FETCH NEXT FROM source_group_cursor
    INTO @SourceGroupId, @SourceGroupName, @SourceBusinessEntityId;
END;

CLOSE source_group_cursor;
DEALLOCATE source_group_cursor;

/* Resultado para conferencia */
SELECT
    m.SourceGroupId,
    m.SourceGroupName,
    m.SourceBusinessEntityId,
    m.NewBusinessEntityId,
    m.NewCompanyId,
    c.GroupId AS NewCompanyGroupId,
    c.Name AS NewCompanyName,
    be.Cnpj AS NewCompanyCnpj
FROM @MigrationMap m
JOIN Companies c ON c.Id = m.NewCompanyId
JOIN BusinessEntity be ON be.Id = m.NewBusinessEntityId
ORDER BY m.SourceGroupId;

/* Remove os grupos originais.
   CompanyUsers e convites remanescentes desses grupos caem por ON DELETE CASCADE.
   AccountPlans ja foram movidos para as novas Companies antes deste ponto. */
DELETE g
FROM Groups g
JOIN @MigrationMap m ON m.SourceGroupId = g.Id;

/* Remove as BusinessEntities originais somente se nao estiverem mais referenciadas. */
DELETE be
FROM BusinessEntity be
JOIN @MigrationMap m ON m.SourceBusinessEntityId = be.Id
WHERE NOT EXISTS
(
    SELECT 1
    FROM Groups g
    WHERE g.BusinessEntityId = be.Id
)
AND NOT EXISTS
(
    SELECT 1
    FROM Companies c
    WHERE c.BusinessEntityId = be.Id
)
AND NOT EXISTS
(
    SELECT 1
    FROM SubCompanies sc
    WHERE sc.BusinessEntityId = be.Id
);

COMMIT;
-- ROLLBACK;
