-- Execute uma vez nos bancos existentes antes de publicar o visualizador V2.
-- Id é o identity que preserva a sequência de inserção das linhas importadas.
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BalanceteData_BalanceteId_Id'
      AND object_id = OBJECT_ID(N'dbo.BalanceteData')
)
BEGIN
    CREATE INDEX IX_BalanceteData_BalanceteId_Id
        ON dbo.BalanceteData (BalanceteId, Id)
        INCLUDE (CostCenter, Name, InitialValue, Debit, Credit, FinalValue);
END;
