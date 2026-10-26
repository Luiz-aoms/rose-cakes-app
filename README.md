# Rose Cakes e Fatias

Sistema local de gestão em React/TypeScript, ASP.NET Core e SQL Server.

## Preparar e executar

1. Requisitos: Node.js 24 (usado na validação), SDK .NET 10, SQL Server e `sqlcmd` para os scripts operacionais.
2. Configure `ConnectionStrings:DefaultConnection` em `backend/RoseCakes.Api/appsettings.json` para a sua instância. Credenciais devem ficar em variável de ambiente, sem serem enviadas ao Git.
3. Crie `RoseCakesDB` se necessário. Faça backup se já houver dados. Execute os arquivos de `database/` **em ordem numérica, de 001 a 005**, no mesmo banco. Os scripts são aditivos e podem ser reaplicados no esquema esperado. Estruturas diferentes precisam de revisão.
4. Inicie a API: `dotnet run --project backend/RoseCakes.Api --launch-profile http`.
5. Em outro terminal: `cd frontend`, `npm install`, `npm run dev`.
6. Abra o endereço mostrado pelo Vite. O proxy `/api` aponta para `http://localhost:5106`. Reinicie a API e o frontend depois de atualizar o projeto.

Para conferir uma segunda instância da API, o Vite aceita `API_PROXY_TARGET`, por exemplo `http://127.0.0.1:5120`. Para hospedagens futuras, `VITE_API_URL` define a URL da API com `/api` no build. O preview não herda o proxy do servidor de desenvolvimento.

## Uso

- **Visão geral:** entregas pendentes ordenadas pelo prazo, recebimentos de hoje, saldo conferido e últimas vendas.
- **Produtos:** nome, categoria livre com sugestões, unidade de venda, rendimento informativo, custo unitário, preço, foto e inativação. Fotos PNG/JPEG/WebP de até 750 KB são guardadas no banco, incluídas no backup.
- **Clientes:** cadastro e edição, contatos, endereço, observações e ficha com histórico de compras/encomendas.
- **Vendas:** registro imediato, pagamento pendente por padrão, valor recebido informado explicitamente, edição de cliente/data/itens/preços e cancelamento com motivo.
- **Pedidos:** encomenda com cliente, data/horário, retirada ou entrega com endereço, itens, detalhes e sinal. Etapas: Recebido, Em produção, Pronto e Entregue. A mudança de etapa não altera a situação financeira.
- **Recebimentos:** registre pagamentos posteriores até o saldo. É possível alterar um lançamento incorreto ou excluí-lo com confirmação; o saldo e a situação de pagamento são recalculados. O histórico registra data, valor e forma. Não integra bancos nem processa Pix ou cartão.
- **Despesas:** criar, editar e cancelar com motivo. Registros cancelados ficam no histórico e não podem ser editados.
- **Relatórios:** período, receita, custo, despesas, resultado estimado, ticket médio, ranking de produtos, gastos por categoria, vendas por dia e recebimentos.
- **CSV:** exporta registros válidos de vendas/despesas e recebimentos pela data do pagamento. Recebimentos de vendas canceladas ficam identificados para conferir eventuais devoluções.

O menu hambúrguer é mantido no celular, as listas principais se adaptam em cartões e formulários longos ocupam a tela. URLs como `#/pedidos` preservam a página ao recarregar e permitem usar Voltar/Avançar.

## Regras dos números

Receita e custo seguem a data do registro da venda ou encomenda; recebimentos seguem a data do pagamento. Resultado estimado = receita − custo histórico − despesas adicionais. Não lance novamente em despesas os custos já incluídos no custo unitário.

Cancelamentos não apagam o histórico e não fazem devoluções financeiras. Os recebimentos permanecem registrados; confira e realize devoluções fora do sistema. Cancelamentos não podem ser desfeitos nesta versão. **Excluir** é diferente: apaga definitivamente o registro após a confirmação. Ao excluir uma venda ou pedido, os itens e recebimentos também são apagados. Clientes e produtos com vínculos históricos não podem ser excluídos; produtos podem ser inativados.

Vendas anteriores ao novo controle de recebimentos ficam **a conferir**. Não presumimos pagamentos antigos: confira comprovantes e registre os valores efetivamente recebidos. Até a conferência, elas não entram no saldo consolidado. Custos de vendas anteriores à primeira atualização podem estar zerados porque não havia histórico; resultados desses períodos podem ficar superestimados. O sistema não substitui contabilidade.

## Verificações

```powershell
npm --prefix frontend run build
npm --prefix frontend run lint
npm --prefix frontend test
dotnet build backend/RoseCakes.Api --no-restore -o backend/validation-build
.\scripts\Testar-Integracao.ps1
```

O teste de integração precisa de PowerShell 7, SQL Server com permissão para criar/remover bancos de teste, backup e restauração, e da API compilada no caminho acima. Ele cria bancos com nomes aleatórios, valida os fluxos e os remove ao terminar. Não grava no banco de uso real. Backups de teste ficam na pasta de backups da instância SQL Server. A porta padrão é 5119; use `-Porta` para escolher outra livre.

## Backup

Execute `.\scripts\Backup-Local.ps1` para criar e verificar uma cópia completa na pasta padrão do SQL Server. Consulte `docs/backup-local.md` para recuperar em outro banco com segurança. O CSV não substitui o backup.

## Limites atuais

Uso local, sem autenticação, controle de estoque, apuração fiscal ou devoluções automatizadas. Vendas e encomendas começam pendentes quando nenhum valor recebido é informado. Escolher Pix, dinheiro ou cartão não confirma recebimento. Pagamentos podem ser parciais ou integrais em ambos os casos. O rendimento do produto não calcula ingredientes automaticamente. Vendas e pedidos ativos podem ser editados, preservando os recebimentos e o nome/custo histórico dos itens existentes. Novos itens usam o custo atual do produto. O novo total não pode ficar menor que o já recebido. Registros cancelados não podem ser editados; podem ser excluídos após confirmação. Nenhuma atualização é enviada automaticamente ao GitHub.

## Extensão universitária

`docs/processos-e-escopo.md`: levantamento inicial a confirmar com a parceira.
`docs/validacao-e-capacitacao.md`: roteiro de sessão com a empreendedora.
`docs/relatorio-tecnico.md`: base de relatório e conteúdo de banner.
`docs/verificacao-tecnica.md`: evidências técnicas e limitações da validação.

Entrevista, capacitação e medição de impacto social ainda dependem das atividades reais com a parceira.

## Corrigir uma venda que ficou paga por engano

Abra **Vendas → Ver recebimentos** na linha desejada. Use **Alterar** para corrigir valor, data ou forma. Se o recebimento inteiro foi lançado por engano, use **Excluir recebimento**, confira o aviso e confirme. O saldo volta a ficar em aberto e a situação é recalculada. Valores de registros antigos não são apagados automaticamente, pois o sistema não consegue distinguir sozinho pagamentos reais de lançamentos indevidos.

Nos formulários, **Fechar sem salvar** apenas abandona a edição; **Cancelar registro/pedido** mantém o histórico e o retira dos totais; **Excluir** apaga definitivamente após confirmação. Os comandos não movimentam dinheiro nem realizam devoluções.
