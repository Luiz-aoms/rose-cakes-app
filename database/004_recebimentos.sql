SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.Vendas','RecebimentosConferidos') IS NULL
 ALTER TABLE dbo.Vendas ADD RecebimentosConferidos bit NOT NULL CONSTRAINT DF_Vendas_RecebimentosConferidos DEFAULT 0;
IF OBJECT_ID('dbo.Recebimentos','U') IS NULL
 CREATE TABLE dbo.Recebimentos (Id int IDENTITY PRIMARY KEY, VendaId int NOT NULL REFERENCES dbo.Vendas(Id) ON DELETE CASCADE, Valor decimal(18,2) NOT NULL, DataPagamento datetime2 NOT NULL, Forma nvarchar(max) NOT NULL);
-- Vendas anteriores ficam a conferir. Não presumir recebimentos inexistentes no histórico.
COMMIT;
