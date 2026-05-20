/*
    Cria o totalizador "Outros Resultados" e as classificacoes
    "Outras Receitas" e "Outras Despesas" para um plano de contas especifico.

    Preferencialmente preencha @AccountPlanId.
    Use @CompanyId apenas se quiser que o script tente localizar o plano da empresa.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CompanyId INT;
DECLARE @AccountPlanId INT;
DECLARE @OutrosResultadosTypeOrder INT;
DECLARE @OutrasReceitasTypeOrder INT;
DECLARE @OutrasDespesasTypeOrder INT;
DECLARE @OutrosResultadosTotalizerId INT;
DECLARE @ContasCriadasOuAtualizadas INT;

SET @CompanyId = NULL;
SET @AccountPlanId = NULL;
SET @OutrosResultadosTypeOrder = 22;
SET @OutrasReceitasTypeOrder = 50;
SET @OutrasDespesasTypeOrder = 51;
SET @ContasCriadasOuAtualizadas = 0;

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

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.TotalizerClassification
        WHERE AccountPlanId = @AccountPlanId
          AND Name = N'Outros Resultados'
    )
    BEGIN
        INSERT INTO dbo.TotalizerClassification (AccountPlanId, Name, TypeOrder)
        VALUES (@AccountPlanId, N'Outros Resultados', @OutrosResultadosTypeOrder);
    END
    ELSE
    BEGIN
        UPDATE dbo.TotalizerClassification
        SET TypeOrder = @OutrosResultadosTypeOrder
        WHERE AccountPlanId = @AccountPlanId
          AND Name = N'Outros Resultados';
    END;

    SELECT @OutrosResultadosTotalizerId = Id
    FROM dbo.TotalizerClassification
    WHERE AccountPlanId = @AccountPlanId
      AND Name = N'Outros Resultados';

    IF @OutrosResultadosTotalizerId IS NULL
    BEGIN
        RAISERROR('Nao foi possivel criar/localizar o totalizador Outros Resultados.', 16, 1);
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.AccountPlanClassification
        WHERE AccountPlanId = @AccountPlanId
          AND TypeClassification = 3
          AND Name = N'Outras Receitas'
    )
    BEGIN
        INSERT INTO dbo.AccountPlanClassification (
            AccountPlanId,
            TotalizerClassificationId,
            Name,
            TypeOrder,
            TypeClassification
        )
        VALUES (
            @AccountPlanId,
            @OutrosResultadosTotalizerId,
            N'Outras Receitas',
            @OutrasReceitasTypeOrder,
            3
        );

        SET @ContasCriadasOuAtualizadas = @ContasCriadasOuAtualizadas + @@ROWCOUNT;
    END
    ELSE
    BEGIN
        UPDATE dbo.AccountPlanClassification
        SET TotalizerClassificationId = @OutrosResultadosTotalizerId,
            TypeOrder = @OutrasReceitasTypeOrder
        WHERE AccountPlanId = @AccountPlanId
          AND TypeClassification = 3
          AND Name = N'Outras Receitas';

        SET @ContasCriadasOuAtualizadas = @ContasCriadasOuAtualizadas + @@ROWCOUNT;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.AccountPlanClassification
        WHERE AccountPlanId = @AccountPlanId
          AND TypeClassification = 3
          AND Name = N'Outras Despesas'
    )
    BEGIN
        INSERT INTO dbo.AccountPlanClassification (
            AccountPlanId,
            TotalizerClassificationId,
            Name,
            TypeOrder,
            TypeClassification
        )
        VALUES (
            @AccountPlanId,
            @OutrosResultadosTotalizerId,
            N'Outras Despesas',
            @OutrasDespesasTypeOrder,
            3
        );

        SET @ContasCriadasOuAtualizadas = @ContasCriadasOuAtualizadas + @@ROWCOUNT;
    END
    ELSE
    BEGIN
        UPDATE dbo.AccountPlanClassification
        SET TotalizerClassificationId = @OutrosResultadosTotalizerId,
            TypeOrder = @OutrasDespesasTypeOrder
        WHERE AccountPlanId = @AccountPlanId
          AND TypeClassification = 3
          AND Name = N'Outras Despesas';

        SET @ContasCriadasOuAtualizadas = @ContasCriadasOuAtualizadas + @@ROWCOUNT;
    END;

    IF (
        SELECT COUNT(1)
        FROM dbo.AccountPlanClassification
        WHERE AccountPlanId = @AccountPlanId
          AND TypeClassification = 3
          AND Name IN (N'Outras Receitas', N'Outras Despesas')
          AND TotalizerClassificationId = @OutrosResultadosTotalizerId
    ) <> 2
    BEGIN
        RAISERROR('Validacao falhou: as duas contas nao ficaram vinculadas ao totalizador Outros Resultados.', 16, 1);
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
    @OutrosResultadosTotalizerId AS OutrosResultadosTotalizerId,
    @ContasCriadasOuAtualizadas AS RowsAffected;

SELECT
    tc.Id AS TotalizerClassificationId,
    tc.AccountPlanId,
    tc.Name AS TotalizerName,
    tc.TypeOrder AS TotalizerTypeOrder
FROM dbo.TotalizerClassification tc
WHERE tc.AccountPlanId = @AccountPlanId
  AND tc.Name = N'Outros Resultados';

SELECT
    apc.Id AS AccountPlanClassificationId,
    apc.AccountPlanId,
    apc.Name AS ClassificationName,
    apc.TypeOrder,
    apc.TypeClassification,
    apc.TotalizerClassificationId,
    tc.Name AS TotalizerName
FROM dbo.AccountPlanClassification apc
INNER JOIN dbo.TotalizerClassification tc
    ON tc.Id = apc.TotalizerClassificationId
WHERE apc.AccountPlanId = @AccountPlanId
  AND apc.TypeClassification = 3
  AND apc.Name IN (N'Outras Receitas', N'Outras Despesas')
ORDER BY apc.TypeOrder;
