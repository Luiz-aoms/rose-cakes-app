# Rose Cakes e Fatias

Sistema de apoio à rotina de uma confeitaria autônoma, desenvolvido em React/TypeScript e ASP.NET Core com SQL Server.

## Execução local

1. Requisitos: Node compatível com o Vite instalado, SDK .NET 10 e SQL Server.
2. Ajuste `ConnectionStrings:DefaultConnection` em `backend/RoseCakes.Api/appsettings.json` para sua instância. Prefira variáveis de ambiente para credenciais.
3. Crie o banco `RoseCakesDB` caso não exista. Faça backup se já houver dados. Execute `database/001_gestao.sql` no banco. O script cria tabelas ausentes, sem remover registros; estruturas preexistentes diferentes precisam de revisão.
4. Inicie a API: `dotnet run --project backend/RoseCakes.Api --launch-profile http`.
5. Em outro terminal: `cd frontend`, `npm install` e `npm run dev`.
6. Abra a URL indicada pelo Vite. O proxy `/api` aponta para `http://localhost:5106`; reinicie o Vite após alterar sua configuração.

Em uma hospedagem diferente, configure `VITE_API_URL` no build para a URL da API, incluindo `/api`, ou configure um proxy de mesma origem. Para distribuição, execute `npm run build` em frontend. O preview não herda o proxy de desenvolvimento; configure API/proxy para esse ambiente.

## Recursos

- Catálogo pesquisável, categorias, edição e inativação de produtos.
- Cadastro e edição de clientes, telefone, endereço e observações.
- Vendas com vários itens, consumidor avulso ou cliente, data e pagamento.
- Preços, custos e nomes preservados no histórico da venda.
- Despesas por data, categoria e valor.
- Painel e relatório por período com receita, custo, despesas e resultado estimado.
- Exportação CSV dos registros do período, compatível com planilhas.
- Interface responsiva, formulários com rótulos, diálogo acessível e guia de primeiros passos.

A forma de pagamento registra o meio usado; não integra pagamentos nem confirma recebimento. Despesas são registros de gastos, sem contas a pagar. Não há controle de estoque, autenticação, recuperação de senha nem edição/cancelamento de vendas e despesas nesta versão. Restrinja o protótipo ao ambiente local até implementar controle de acesso para hospedagem pública. CSV é um relatório, não um backup completo do banco.

## Validação técnica

- Frontend: `npm run build` e `npm run lint`.
- Backend: `dotnet build backend/RoseCakes.Api`.
- Se a API estiver aberta e bloquear o executável, valide em saída separada: `dotnet build backend/RoseCakes.Api -o backend/validation-build`.
- Integração: conferir com o banco configurado, seguindo `docs/validacao-e-capacitacao.md`.

O resultado estimado usa receita menos custo unitário histórico e despesas. Não substitui contabilidade. Para evitar duplicação, não lance nas despesas os valores já incluídos nos custos dos produtos.

## Materiais de extensão

`docs/processos-e-escopo.md`: processos previstos e pontos para levantamento.
`docs/validacao-e-capacitacao.md`: roteiro de teste com a parceira e capacitação.
`docs/relatorio-tecnico.md`: base de relatório e conteúdo para banner, com resultados de campo ainda a preencher.

## Compatibilidade com o banco inicial

O script mantém `Vendas.ValorTotal`, `Vendas.MetodoPagamento`, `Vendas.Status` e a tabela `ItensVenda`. Acrescenta nomes históricos e custo unitário, e permite consumidor avulso. Custos de vendas anteriores à atualização permanecem zero, pois não existia histórico: o resultado de períodos antigos pode ficar superestimado. Os nomes antigos são preenchidos com os nomes atuais dos cadastros. Novas vendas preservam seus valores e nomes corretamente.
