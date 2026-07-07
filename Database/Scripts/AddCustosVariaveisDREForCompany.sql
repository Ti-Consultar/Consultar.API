/*
    Cria a classification "(-) Custos Variaveis" dentro do totalizador
    "(=) Receita Liquida de Vendas" na DRE de um plano de contas.

    Preferencialmente preencha @AccountPlanId.
    Use @CompanyId apenas se quiser que o script tente localizar o plano
    principal da empresa.

    Observacao: os nomes gravados no banco preservam acentos via NCHAR(...)
    para evitar problema de encoding ao executar o script.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CompanyId INT;
DECLARE @AccountPlanId INT;
DECLARE @ReceitaLiquidaTotalizerId INT;
DECLARE @CustosVariaveisClassificationId INT;
DECLARE @CustosVariaveisTypeOrder INT;
DECLARE @RowsAffected INT;
DECLARE @ReceitaLiquidaName NVARCHAR(200);
DECLARE @CustosVariaveisName NVARCHAR(200);

SET @CompanyId = NULL;
SET @AccountPlanId = NULL;
SET @CustosVariaveisTypeOrder = NULL;
SET @RowsAffected = 0;
SET @ReceitaLiquidaName = N'(=) Receita L' + NCHAR(237) + N'quida de Vendas';
SET @CustosVariaveisName = N'(-) Custos Vari' + NCHAR(225) + N'veis';

IF @AccountPlanId IS NULL AND @CompanyId IS NOT NULL
BEGIN
    SELECT TOP (1) @AccountPlanId = Id
    FROM dbo.AccountPlans
    WHERE CompanyId = @CompanyId
      AND SubCompanyId IS NULL
    ORDER BY Id;
END;

IF @AccountPlanId IS NULL
BEGIN
    RAISERROR('Informe @AccountPlanId ou um @CompanyId que possua plano de contas.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.AccountPlans
    WHERE Id = @AccountPlanId
)
BEGIN
    RAISERROR('AccountPlanId informado nao existe em dbo.AccountPlans.', 16, 1);
    RETURN;
END;

SELECT @ReceitaLiquidaTotalizerId = Id
FROM dbo.TotalizerClassification
WHERE AccountPlanId = @AccountPlanId
  AND Name = @ReceitaLiquidaName;

IF @ReceitaLiquidaTotalizerId IS NULL
BEGIN
    RAISERROR('Nao foi possivel localizar o totalizador (=) Receita Liquida de Vendas para o plano informado.', 16, 1);
    RETURN;
END;

IF @CustosVariaveisTypeOrder IS NULL
BEGIN
    SELECT @CustosVariaveisTypeOrder = ISNULL(MAX(TypeOrder), 0) + 1
    FROM dbo.AccountPlanClassification
    WHERE AccountPlanId = @AccountPlanId
      AND TypeClassification = 3
      AND TotalizerClassificationId = @ReceitaLiquidaTotalizerId;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.AccountPlanClassification
        WHERE AccountPlanId = @AccountPlanId
          AND TypeClassification = 3
          AND Name = @CustosVariaveisName
    )
    BEGIN
        INSERT INTO dbo.AccountPlanClassification
        (
            AccountPlanId,
            TotalizerClassificationId,
            Name,
            TypeOrder,
            TypeClassification
        )
        VALUES
        (
            @AccountPlanId,
            @ReceitaLiquidaTotalizerId,
            @CustosVariaveisName,
            @CustosVariaveisTypeOrder,
            3
        );

        SET @RowsAffected = @RowsAffected + @@ROWCOUNT;
    END
    ELSE
    BEGIN
        UPDATE dbo.AccountPlanClassification
        SET TotalizerClassificationId = @ReceitaLiquidaTotalizerId,
            TypeOrder = @CustosVariaveisTypeOrder
        WHERE AccountPlanId = @AccountPlanId
          AND TypeClassification = 3
          AND Name = @CustosVariaveisName
          AND (
                ISNULL(TotalizerClassificationId, 0) <> @ReceitaLiquidaTotalizerId
                OR TypeOrder <> @CustosVariaveisTypeOrder
              );

        SET @RowsAffected = @RowsAffected + @@ROWCOUNT;
    END;

    SELECT @CustosVariaveisClassificationId = Id
    FROM dbo.AccountPlanClassification
    WHERE AccountPlanId = @AccountPlanId
      AND TypeClassification = 3
      AND Name = @CustosVariaveisName
      AND TotalizerClassificationId = @ReceitaLiquidaTotalizerId;

    IF @CustosVariaveisClassificationId IS NULL
    BEGIN
        RAISERROR('Validacao falhou: (-) Custos Variaveis nao ficou vinculado a (=) Receita Liquida de Vendas.', 16, 1);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000);
    SET @ErrorMessage = ERROR_MESSAGE();

    RAISERROR(@ErrorMessage, 16, 1);
    RETURN;
END CATCH;

SELECT
    DB_NAME() AS DatabaseName,
    @AccountPlanId AS AccountPlanId,
    @ReceitaLiquidaTotalizerId AS ReceitaLiquidaTotalizerId,
    @CustosVariaveisClassificationId AS CustosVariaveisClassificationId,
    @RowsAffected AS RowsAffected;

SELECT
    tc.Id AS TotalizerClassificationId,
    tc.AccountPlanId,
    tc.Name AS TotalizerName,
    tc.TypeOrder AS TotalizerTypeOrder,
    apc.Id AS AccountPlanClassificationId,
    apc.Name AS ClassificationName,
    apc.TypeOrder AS ClassificationTypeOrder,
    apc.TypeClassification
FROM dbo.TotalizerClassification tc
INNER JOIN dbo.AccountPlanClassification apc
    ON apc.TotalizerClassificationId = tc.Id
WHERE tc.AccountPlanId = @AccountPlanId
  AND tc.Id = @ReceitaLiquidaTotalizerId
  AND apc.Id = @CustosVariaveisClassificationId;
