/*
    Migra um Group existente para dentro de outro Group como Company,
    transformando as Companies antigas desse Group em SubCompanies da nova Company.

    Cenario:
      - O Group de origem foi cadastrado no nivel errado.
      - Ele ja possui Companies.
      - Os dados consolidados/financeiros devem ser preservados.

    Estrategia:
      - Copia a BusinessEntity do Group de origem para uma nova BusinessEntity.
      - Cria uma nova Company dentro do Group destino.
      - Copia a BusinessEntity de cada Company antiga para novas BusinessEntities.
      - Cria uma SubCompany para cada Company antiga.
      - Move o escopo dos AccountPlans existentes:
          Group antigo                      -> Company nova
          Company antiga dentro do Group    -> SubCompany nova
      - Migra CompanyUsers e InvitationToCompany para os novos escopos.
      - Remove fisicamente o Group antigo e suas Companies antigas.
      - Remove as BusinessEntities antigas quando nao estiverem mais referenciadas.

    Importante:
      - Este script aborta se as Companies antigas ja tiverem SubCompanies.
      - Rode primeiro com @DryRun = 1.
      - Faca backup antes de rodar com @DryRun = 0.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @DryRun BIT = 1;
DECLARE @TargetGroupId INT = 13011;
DECLARE @SourceGroupId INT = 0; -- TODO: informe aqui o Group que sera transformado em Company
DECLARE @MigrateSourceGroupUsersToNewCompany BIT = 0;

DECLARE @NewCompanyBusinessEntityId INT;
DECLARE @NewCompanyId INT;
DECLARE @SourceGroupBusinessEntityId INT;

DECLARE @CompanyMap TABLE
(
    SourceCompanyId INT NOT NULL PRIMARY KEY,
    SourceCompanyName NVARCHAR(MAX) NOT NULL,
    SourceBusinessEntityId INT NOT NULL,
    SourceCompanyDeleted BIT NOT NULL,
    NewBusinessEntityId INT NULL,
    NewSubCompanyId INT NULL
);

/* Validacoes */

IF @SourceGroupId IS NULL OR @SourceGroupId = 0
BEGIN
    THROW 51000, 'Informe @SourceGroupId antes de executar o script.', 1;
END;

IF @SourceGroupId = @TargetGroupId
BEGIN
    THROW 51001, 'O Group de origem nao pode ser igual ao Group destino.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM Groups
    WHERE Id = @SourceGroupId
      AND Deleted = 0
)
BEGIN
    THROW 51002, 'Group de origem inexistente ou deletado.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM Groups
    WHERE Id = @TargetGroupId
      AND Deleted = 0
)
BEGIN
    THROW 51003, 'Group destino inexistente ou deletado.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM Companies
    WHERE GroupId = @SourceGroupId
      AND Deleted = 0
)
BEGIN
    THROW 51004, 'O Group de origem nao possui Companies ativas para converter em SubCompanies.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM Companies c
    JOIN SubCompanies sc ON sc.CompanyId = c.Id
    WHERE c.GroupId = @SourceGroupId
)
BEGIN
    THROW 51005, 'Uma ou mais Companies de origem ja possuem SubCompanies. Este script nao trata quarto nivel.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM AccountPlans ap
    WHERE ap.GroupId = @SourceGroupId
      AND ap.SubCompanyId IS NOT NULL
)
BEGIN
    THROW 51006, 'Existem AccountPlans no nivel SubCompany dentro do Group de origem. Este script nao trata quarto nivel.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM CompanyUsers cu
    WHERE cu.GroupId = @SourceGroupId
      AND cu.SubCompanyId IS NOT NULL
)
BEGIN
    THROW 51007, 'Existem CompanyUsers no nivel SubCompany dentro do Group de origem. Este script nao trata quarto nivel.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM InvitationToCompany i
    WHERE i.GroupId = @SourceGroupId
      AND i.SubCompanyId IS NOT NULL
)
BEGIN
    THROW 51008, 'Existem convites no nivel SubCompany dentro do Group de origem. Este script nao trata quarto nivel.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM Groups sourceGroup
    JOIN BusinessEntity sourceBe ON sourceBe.Id = sourceGroup.BusinessEntityId
    JOIN Companies targetCompany ON targetCompany.GroupId = @TargetGroupId
    JOIN BusinessEntity targetBe ON targetBe.Id = targetCompany.BusinessEntityId
    WHERE sourceGroup.Id = @SourceGroupId
      AND targetCompany.Deleted = 0
      AND targetBe.Cnpj = sourceBe.Cnpj
)
BEGIN
    THROW 51009, 'Ja existe Company ativa no Group destino com o CNPJ do Group de origem.', 1;
END;

SELECT @SourceGroupBusinessEntityId = BusinessEntityId
FROM Groups
WHERE Id = @SourceGroupId;

INSERT INTO @CompanyMap
(
    SourceCompanyId,
    SourceCompanyName,
    SourceBusinessEntityId,
    SourceCompanyDeleted
)
SELECT
    c.Id,
    c.Name,
    c.BusinessEntityId,
    c.Deleted
FROM Companies c
WHERE c.GroupId = @SourceGroupId;

IF EXISTS
(
    SELECT 1
    FROM @CompanyMap cm
    JOIN BusinessEntity sourceBe ON sourceBe.Id = cm.SourceBusinessEntityId
    JOIN Companies targetCompany ON targetCompany.GroupId = @TargetGroupId
    JOIN SubCompanies targetSubCompany ON targetSubCompany.CompanyId = targetCompany.Id
    JOIN BusinessEntity targetBe ON targetBe.Id = targetSubCompany.BusinessEntityId
    WHERE targetCompany.Deleted = 0
      AND targetSubCompany.Deleted = 0
      AND targetBe.Cnpj = sourceBe.Cnpj
)
BEGIN
    THROW 51010, 'Ja existe SubCompany ativa no Group destino com CNPJ de alguma Company de origem.', 1;
END;

/* Cria a nova Company a partir do Group de origem */

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
    be.NomeFantasia,
    be.RazaoSocial,
    be.Cnpj,
    be.Logradouro,
    be.Numero,
    be.Bairro,
    be.Municipio,
    be.Uf,
    be.Cep,
    be.Telefone,
    be.Email,
    0
FROM Groups g
JOIN BusinessEntity be ON be.Id = g.BusinessEntityId
WHERE g.Id = @SourceGroupId;

SET @NewCompanyBusinessEntityId = CONVERT(INT, SCOPE_IDENTITY());

INSERT INTO Companies
(
    Name,
    DateCreate,
    GroupId,
    BusinessEntityId,
    Deleted
)
SELECT
    g.Name,
    GETDATE(),
    @TargetGroupId,
    @NewCompanyBusinessEntityId,
    0
FROM Groups g
WHERE g.Id = @SourceGroupId;

SET @NewCompanyId = CONVERT(INT, SCOPE_IDENTITY());

/* Cria novas SubCompanies a partir das Companies antigas */

DECLARE
    @SourceCompanyId INT,
    @SourceCompanyName NVARCHAR(MAX),
    @SourceBusinessEntityId INT,
    @SourceCompanyDeleted BIT,
    @NewSubCompanyBusinessEntityId INT,
    @NewSubCompanyId INT;

DECLARE source_company_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT
    SourceCompanyId,
    SourceCompanyName,
    SourceBusinessEntityId,
    SourceCompanyDeleted
FROM @CompanyMap
ORDER BY SourceCompanyId;

OPEN source_company_cursor;

FETCH NEXT FROM source_company_cursor
INTO @SourceCompanyId, @SourceCompanyName, @SourceBusinessEntityId, @SourceCompanyDeleted;

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

    SET @NewSubCompanyBusinessEntityId = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO SubCompanies
    (
        Name,
        DateCreate,
        CompanyId,
        BusinessEntityId,
        Deleted
    )
    VALUES
    (
        @SourceCompanyName,
        GETDATE(),
        @NewCompanyId,
        @NewSubCompanyBusinessEntityId,
        @SourceCompanyDeleted
    );

    SET @NewSubCompanyId = CONVERT(INT, SCOPE_IDENTITY());

    UPDATE @CompanyMap
    SET
        NewBusinessEntityId = @NewSubCompanyBusinessEntityId,
        NewSubCompanyId = @NewSubCompanyId
    WHERE SourceCompanyId = @SourceCompanyId;

    FETCH NEXT FROM source_company_cursor
    INTO @SourceCompanyId, @SourceCompanyName, @SourceBusinessEntityId, @SourceCompanyDeleted;
END;

CLOSE source_company_cursor;
DEALLOCATE source_company_cursor;

/* Move AccountPlans existentes para os novos escopos.
   As tabelas financeiras continuam apontando para os mesmos AccountPlanIds. */

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

UPDATE ap
SET
    ap.GroupId = @TargetGroupId,
    ap.CompanyId = @NewCompanyId,
    ap.SubCompanyId = cm.NewSubCompanyId
FROM AccountPlans ap
JOIN @CompanyMap cm ON cm.SourceCompanyId = ap.CompanyId
WHERE ap.SubCompanyId IS NULL;

INSERT INTO AccountPlans
(
    GroupId,
    CompanyId,
    SubCompanyId
)
SELECT
    @TargetGroupId,
    @NewCompanyId,
    cm.NewSubCompanyId
FROM @CompanyMap cm
WHERE NOT EXISTS
(
    SELECT 1
    FROM AccountPlans ap
    WHERE ap.GroupId = @TargetGroupId
      AND ap.CompanyId = @NewCompanyId
      AND ap.SubCompanyId = cm.NewSubCompanyId
);

/* Migra usuarios.

   Por padrao, usuarios que tinham permissao somente no Group de origem
   NAO sao promovidos para a nova Company. Isso evita que um acesso de nivel
   grupo antigo vire acesso amplo ao novo nivel Company.

   Os usuarios vinculados as Companies antigas sao migrados para as novas
   SubCompanies correspondentes.

   Se a regra de negocio desejar que usuarios do Group antigo tambem tenham
   acesso direto a nova Company, altere @MigrateSourceGroupUsersToNewCompany
   para 1.
*/

IF @MigrateSourceGroupUsersToNewCompany = 1
BEGIN
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
END;

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
    cm.NewSubCompanyId,
    cu.PermissionId
FROM CompanyUsers cu
JOIN @CompanyMap cm ON cm.SourceCompanyId = cu.CompanyId
WHERE cu.GroupId = @SourceGroupId
  AND cu.SubCompanyId IS NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM CompanyUsers existing
      WHERE existing.UserId = cu.UserId
        AND existing.GroupId = @TargetGroupId
        AND existing.CompanyId = @NewCompanyId
        AND existing.SubCompanyId = cm.NewSubCompanyId
  );

/* Migra convites */

UPDATE InvitationToCompany
SET
    GroupId = @TargetGroupId,
    CompanyId = @NewCompanyId,
    SubCompanyId = NULL,
    UpdatedAt = GETDATE()
WHERE GroupId = @SourceGroupId
  AND CompanyId IS NULL
  AND SubCompanyId IS NULL;

UPDATE i
SET
    i.GroupId = @TargetGroupId,
    i.CompanyId = @NewCompanyId,
    i.SubCompanyId = cm.NewSubCompanyId,
    i.UpdatedAt = GETDATE()
FROM InvitationToCompany i
JOIN @CompanyMap cm ON cm.SourceCompanyId = i.CompanyId
WHERE i.GroupId = @SourceGroupId
  AND i.SubCompanyId IS NULL;

/* Remove a estrutura antiga.
   AccountPlans, CompanyUsers e convites ja foram migrados para os novos escopos.
   Ao deletar as Companies antigas, os CompanyUsers antigos delas caem por ON DELETE CASCADE.
   Ao deletar o Group antigo, os CompanyUsers/convites remanescentes do Group caem por ON DELETE CASCADE.
*/

IF EXISTS
(
    SELECT 1
    FROM AccountPlans ap
    WHERE ap.GroupId = @SourceGroupId
       OR ap.CompanyId IN (SELECT SourceCompanyId FROM @CompanyMap)
)
BEGIN
    SELECT
        ap.Id AS AccountPlanId,
        ap.GroupId,
        ap.CompanyId,
        ap.SubCompanyId,
        c.Name AS OldCompanyName,
        c.Deleted AS OldCompanyDeleted
    FROM AccountPlans ap
    LEFT JOIN Companies c ON c.Id = ap.CompanyId
    WHERE ap.GroupId = @SourceGroupId
       OR ap.CompanyId IN (SELECT SourceCompanyId FROM @CompanyMap);

    THROW 51011, 'Ainda existem AccountPlans apontando para a estrutura antiga. Migracao abortada.', 1;
END;

DELETE c
FROM Companies c
JOIN @CompanyMap cm ON cm.SourceCompanyId = c.Id;

DELETE FROM Groups
WHERE Id = @SourceGroupId;

DELETE be
FROM BusinessEntity be
WHERE be.Id IN
(
    SELECT @SourceGroupBusinessEntityId
    UNION
    SELECT SourceBusinessEntityId
    FROM @CompanyMap
)
AND NOT EXISTS
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

/* Resultado para conferencia */

SELECT
    @SourceGroupId AS SourceGroupId,
    @TargetGroupId AS TargetGroupId,
    @NewCompanyId AS NewCompanyId,
    @NewCompanyBusinessEntityId AS NewCompanyBusinessEntityId;

SELECT
    cm.SourceCompanyId,
    cm.SourceCompanyName,
    cm.SourceBusinessEntityId,
    cm.SourceCompanyDeleted,
    cm.NewSubCompanyId,
    cm.NewBusinessEntityId
FROM @CompanyMap cm
ORDER BY cm.SourceCompanyId;

SELECT
    ap.Id AS AccountPlanId,
    ap.GroupId,
    ap.CompanyId,
    ap.SubCompanyId
FROM AccountPlans ap
WHERE ap.GroupId = @TargetGroupId
  AND ap.CompanyId = @NewCompanyId
ORDER BY ap.CompanyId, ap.SubCompanyId, ap.Id;

IF @DryRun = 1
BEGIN
    ROLLBACK;
    PRINT 'DRY RUN concluido. Nenhuma alteracao foi persistida. Altere @DryRun para 0 para aplicar.';
END
ELSE
BEGIN
    COMMIT;
    PRINT 'Migracao aplicada com sucesso.';
END;
