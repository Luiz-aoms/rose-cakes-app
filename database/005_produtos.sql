SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.Produtos','Foto') IS NULL
 ALTER TABLE dbo.Produtos ADD Foto nvarchar(max) NULL, UnidadeVenda nvarchar(max) NOT NULL CONSTRAINT DF_Produtos_Unidade DEFAULT N'Unidade', Rendimento int NOT NULL CONSTRAINT DF_Produtos_Rendimento DEFAULT 1;
COMMIT;
