/*
    Remove permissao direta na Company criada pela migracao quando essa permissao
    veio de usuarios que estavam somente no Group de origem.

    Use este script se voce rodou a primeira versao de
    MigrateGroupWithCompaniesToCompanyWithSubCompanies.sql e percebeu usuarios
    vendo a nova Company/filiais alem do que deveriam.

    Ele nao remove permissoes em SubCompanies migradas das Companies antigas.

    Rode primeiro com @DryRun = 1.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @DryRun BIT = 1;
DECLARE @SourceGroupId INT = 0; -- TODO: Group antigo que foi migrado
DECLARE @TargetGroupId INT = 13011;
DECLARE @NewCompanyId INT = 0; -- TODO: Company criada dentro do Group 13011

IF @SourceGroupId IS NULL OR @SourceGroupId = 0
BEGIN
    THROW 52000, 'Informe @SourceGroupId.', 1;
END;

IF @NewCompanyId IS NULL OR @NewCompanyId = 0
BEGIN
    THROW 52001, 'Informe @NewCompanyId.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM Companies
    WHERE Id = @NewCompanyId
      AND GroupId = @TargetGroupId
)
BEGIN
    THROW 52002, 'A Company informada nao pertence ao Group destino.', 1;
END;

DECLARE @RowsToDelete TABLE
(
    CompanyUserId INT NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    PermissionId INT NOT NULL
);

INSERT INTO @RowsToDelete
(
    CompanyUserId,
    UserId,
    PermissionId
)
SELECT
    targetCu.Id,
    targetCu.UserId,
    targetCu.PermissionId
FROM CompanyUsers targetCu
WHERE targetCu.GroupId = @TargetGroupId
  AND targetCu.CompanyId = @NewCompanyId
  AND targetCu.SubCompanyId IS NULL
  AND EXISTS
  (
      SELECT 1
      FROM CompanyUsers sourceGroupCu
      WHERE sourceGroupCu.GroupId = @SourceGroupId
        AND sourceGroupCu.CompanyId IS NULL
        AND sourceGroupCu.SubCompanyId IS NULL
        AND sourceGroupCu.UserId = targetCu.UserId
        AND sourceGroupCu.PermissionId = targetCu.PermissionId
  )
  AND NOT EXISTS
  (
      SELECT 1
      FROM CompanyUsers targetSubCu
      WHERE targetSubCu.GroupId = @TargetGroupId
        AND targetSubCu.CompanyId = @NewCompanyId
        AND targetSubCu.SubCompanyId IS NOT NULL
        AND targetSubCu.UserId = targetCu.UserId
  );

SELECT
    r.CompanyUserId,
    r.UserId,
    u.Name,
    u.Email,
    r.PermissionId
FROM @RowsToDelete r
JOIN Users u ON u.Id = r.UserId
ORDER BY u.Name, u.Email;

DELETE cu
FROM CompanyUsers cu
JOIN @RowsToDelete r ON r.CompanyUserId = cu.Id;

IF @DryRun = 1
BEGIN
    ROLLBACK;
    PRINT 'DRY RUN concluido. Nenhuma permissao foi removida. Altere @DryRun para 0 para aplicar.';
END
ELSE
BEGIN
    COMMIT;
    PRINT 'Permissoes diretas removidas com sucesso.';
END;
