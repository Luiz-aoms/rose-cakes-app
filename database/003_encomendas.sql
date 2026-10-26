SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.Vendas','DataEntrega') IS NULL
 ALTER TABLE dbo.Vendas ADD DataEntrega datetime2 NULL, Etapa nvarchar(max) NOT NULL CONSTRAINT DF_Vendas_Etapa DEFAULT N'Entregue', TipoEntrega nvarchar(max) NULL, EnderecoEntrega nvarchar(max) NULL, Observacoes nvarchar(max) NULL;
COMMIT;
