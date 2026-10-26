-- Aplicar depois de 001_gestao.sql. Não remove registros.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.Vendas','MotivoCancelamento') IS NULL
 ALTER TABLE dbo.Vendas ADD MotivoCancelamento nvarchar(max) NULL, CanceladoEm datetime2 NULL;
IF COL_LENGTH('dbo.Despesas','MotivoCancelamento') IS NULL
 ALTER TABLE dbo.Despesas ADD MotivoCancelamento nvarchar(max) NULL, CanceladoEm datetime2 NULL;
COMMIT;
