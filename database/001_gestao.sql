-- Banco existente RoseCakesDB. Fazer backup antes da aplicação.
-- As colunas originais de vendas e itens são preservadas.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.Produtos', N'U') IS NULL
CREATE TABLE dbo.Produtos (Id int IDENTITY PRIMARY KEY, Nome nvarchar(max) NOT NULL, Descricao nvarchar(max) NULL, PrecoCusto decimal(18,2) NOT NULL, PrecoVenda decimal(18,2) NOT NULL, Categoria nvarchar(max) NULL, Ativo bit NOT NULL DEFAULT 1);
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
CREATE TABLE dbo.Clientes (Id int IDENTITY PRIMARY KEY, Nome nvarchar(max) NOT NULL, Telefone nvarchar(max) NOT NULL, Endereco nvarchar(max) NULL, DataNascimento date NULL, Observacoes nvarchar(max) NULL);
IF OBJECT_ID(N'dbo.Despesas', N'U') IS NULL
CREATE TABLE dbo.Despesas (Id int IDENTITY PRIMARY KEY, Descricao nvarchar(max) NOT NULL, DataDespesa date NOT NULL, Valor decimal(18,2) NOT NULL, Categoria nvarchar(max) NULL);
IF OBJECT_ID(N'dbo.Vendas', N'U') IS NULL
CREATE TABLE dbo.Vendas (Id int IDENTITY PRIMARY KEY, ClienteId int NULL REFERENCES dbo.Clientes(Id), DataVenda datetime NOT NULL, ValorTotal decimal(18,2) NOT NULL, MetodoPagamento nvarchar(50) NULL, Status nvarchar(50) NULL DEFAULT N'Pendente');
IF OBJECT_ID(N'dbo.ItensVenda', N'U') IS NULL
CREATE TABLE dbo.ItensVenda (Id int IDENTITY PRIMARY KEY, ProdutoId int NOT NULL REFERENCES dbo.Produtos(Id), Quantidade int NOT NULL, PrecoUnitario decimal(18,2) NOT NULL, VendaId int NOT NULL REFERENCES dbo.Vendas(Id) ON DELETE CASCADE);
ALTER TABLE dbo.Vendas ALTER COLUMN ClienteId int NULL;
IF COL_LENGTH('dbo.Vendas','ClienteNome') IS NULL
BEGIN
 ALTER TABLE dbo.Vendas ADD ClienteNome nvarchar(200) NOT NULL CONSTRAINT DF_Vendas_ClienteNome DEFAULT N'Consumidor avulso';
 EXEC(N'UPDATE v SET ClienteNome = c.Nome FROM dbo.Vendas v JOIN dbo.Clientes c ON v.ClienteId = c.Id');
END;
IF COL_LENGTH('dbo.ItensVenda','ProdutoNome') IS NULL
BEGIN
 ALTER TABLE dbo.ItensVenda ADD ProdutoNome nvarchar(200) NOT NULL CONSTRAINT DF_ItensVenda_Nome DEFAULT N'Produto anterior';
 EXEC(N'UPDATE i SET ProdutoNome = p.Nome FROM dbo.ItensVenda i JOIN dbo.Produtos p ON i.ProdutoId = p.Id');
END;
IF COL_LENGTH('dbo.ItensVenda','CustoUnitario') IS NULL
BEGIN
 -- Custos anteriores não foram registrados: ficam zero para não inventar histórico.
 ALTER TABLE dbo.ItensVenda ADD CustoUnitario decimal(18,2) NOT NULL CONSTRAINT DF_ItensVenda_Custo DEFAULT 0;
END;
COMMIT;
