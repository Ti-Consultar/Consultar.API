/*
    Cria/atualiza as classificacoes de passivo abaixo para um plano de contas:
    - Recursos p/ Aumento de Capital
    - Antecipacoes de Dividendos
    - Dividendos Recebidos

    Regras:
    - BP Contabil (/painel, typeClassification = 2):
      vincula em "Total Passivo Nao Circulante".
    - BP Reclassificado (/painel-reclassificado/comparativo, typeClassification = 2):
      vincula em "Passivo Nao Circulante Financeiro".

    Preferencialmente preencha @AccountPlanId.
    Use @CompanyId apenas se quiser que o script tente localizar o plano da empresa.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CompanyId INT;
DECLARE @AccountPlanId INT;
DECLARE @TotalPassivoNaoCirculanteId INT;
DECLARE @PassivoNaoCirculanteFinanceiroId INT;
DECLARE @RowsAffected INT;

SET @CompanyId = NULL;
SET @AccountPlanId = NULL;
SET @RowsAffected = 0;

DECLARE @Classificacoes TABLE
(
    Name NVARCHAR(200) NOT NULL,
    TypeOrder INT NOT NULL
);

INSERT INTO @Classificacoes (Name, TypeOrder)
VALUES
    (N'Recursos p/ Aumento de Capital', 31),
    (N'Antecipa' + NCHAR(231) + NCHAR(245) + N'es de Dividendos', 32),
    (N'Dividendos Recebidos', 33);

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

SELECT @TotalPassivoNaoCirculanteId = Id
FROM dbo.TotalizerClassification
WHERE AccountPlanId = @AccountPlanId
  AND Name = N'Total Passivo N' + NCHAR(227) + N'o Circulante';

IF @TotalPassivoNaoCirculanteId IS NULL
BEGIN
    RAISERROR('Totalizador "Total Passivo Nao Circulante" nao encontrado para o plano informado.', 16, 1);
    RETURN;
END;

SELECT @PassivoNaoCirculanteFinanceiroId = Id
FROM dbo.BalancoReclassificado
WHERE AccountPlanId = @AccountPlanId
  AND Name = N'Passivo N' + NCHAR(227) + N'o Circulante Financeiro';

IF @PassivoNaoCirculanteFinanceiroId IS NULL
BEGIN
    RAISERROR('Balanco reclassificado "Passivo Nao Circulante Financeiro" nao encontrado para o plano informado.', 16, 1);
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE apc
    SET TotalizerClassificationId = @TotalPassivoNaoCirculanteId,
        BalancoReclassificadoId = @PassivoNaoCirculanteFinanceiroId,
        TypeOrder = c.TypeOrder
    FROM dbo.AccountPlanClassification apc
    INNER JOIN @Classificacoes c
        ON c.Name = apc.Name
    WHERE apc.AccountPlanId = @AccountPlanId
      AND apc.TypeClassification = 2;

    SET @RowsAffected = @RowsAffected + @@ROWCOUNT;

    INSERT INTO dbo.AccountPlanClassification (
        AccountPlanId,
        TotalizerClassificationId,
        Name,
        TypeOrder,
        TypeClassification,
        BalancoReclassificadoId
    )
    SELECT
        @AccountPlanId,
        @TotalPassivoNaoCirculanteId,
        c.Name,
        c.TypeOrder,
        2,
        @PassivoNaoCirculanteFinanceiroId
    FROM @Classificacoes c
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.AccountPlanClassification apc
        WHERE apc.AccountPlanId = @AccountPlanId
          AND apc.TypeClassification = 2
          AND apc.Name = c.Name
    );

    SET @RowsAffected = @RowsAffected + @@ROWCOUNT;

    IF (
        SELECT COUNT(1)
        FROM dbo.AccountPlanClassification apc
        INNER JOIN @Classificacoes c
            ON c.Name = apc.Name
        WHERE apc.AccountPlanId = @AccountPlanId
          AND apc.TypeClassification = 2
          AND apc.TotalizerClassificationId = @TotalPassivoNaoCirculanteId
          AND apc.BalancoReclassificadoId = @PassivoNaoCirculanteFinanceiroId
    ) <> 3
    BEGIN
        RAISERROR('Validacao falhou: as tres classificacoes nao ficaram vinculadas aos destinos esperados.', 16, 1);
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
    @TotalPassivoNaoCirculanteId AS TotalPassivoNaoCirculanteId,
    @PassivoNaoCirculanteFinanceiroId AS PassivoNaoCirculanteFinanceiroId,
    @RowsAffected AS RowsAffected;

SELECT
    apc.Id AS AccountPlanClassificationId,
    apc.AccountPlanId,
    apc.Name AS ClassificationName,
    apc.TypeOrder,
    apc.TypeClassification,
    apc.TotalizerClassificationId,
    tc.Name AS TotalizerName,
    apc.BalancoReclassificadoId,
    br.Name AS BalancoReclassificadoName
FROM dbo.AccountPlanClassification apc
INNER JOIN dbo.TotalizerClassification tc
    ON tc.Id = apc.TotalizerClassificationId
INNER JOIN dbo.BalancoReclassificado br
    ON br.Id = apc.BalancoReclassificadoId
INNER JOIN @Classificacoes c
    ON c.Name = apc.Name
WHERE apc.AccountPlanId = @AccountPlanId
  AND apc.TypeClassification = 2
ORDER BY apc.TypeOrder;
