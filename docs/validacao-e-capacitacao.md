# Validação com a parceira e capacitação

Status: roteiro preparado; sessão com a parceira ainda não realizada. Use banco de teste. Não misture os exemplos abaixo com registros reais.

## Cenário de aceitação

1. Cadastrar cliente com nome e telefone; buscar pelo telefone e editar uma observação.
2. Cadastrar Fatia de chocolate com custo de R$ 6,00 e preço de R$ 15,00.
3. Registrar venda de duas unidades por Pix para o cliente: total esperado R$ 30,00 e custo R$ 12,00.
4. Alterar preço para R$ 18,00 e custo para R$ 7,00. A venda anterior deve continuar em R$ 30,00 e R$ 12,00.
5. Registrar R$ 5,00 de transporte na mesma data. No período, sem outros registros, o resultado deve ser R$ 13,00.
6. Exportar CSV e conferir venda e despesa. Escolher um período sem registros e conferir totais zerados.
7. Inativar o produto: deve continuar no catálogo e histórico, mas não pode ser selecionado em nova venda.
8. Tentar salvar com valores negativos, campos obrigatórios vazios e quantidade zero. O sistema deve impedir o envio.
9. Testar no celular, navegação por teclado, foco nos campos, fechamento do diálogo e leitura dos erros.
10. Parar a API: deve aparecer mensagem de conexão e opção para tentar novamente, sem indicar sucesso em operações não salvas.

## Capacitação sugerida — 45 minutos

- 10 minutos: acesso, catálogo, custo e preço.
- 10 minutos: cadastro de clientes e cuidados com dados pessoais.
- 10 minutos: venda com mais de um produto e conferência do pagamento.
- 10 minutos: despesas, período e exportação.
- 5 minutos: exercício autônomo e dúvidas.

## Registro da sessão

Data: a preencher. Participantes: a preencher. Dispositivo: a preencher.

Para cada tarefa registre: concluída sem ajuda / com ajuda / não concluída; tempo; erro observado; sugestão; prioridade de correção. Registre autorização da parceira para fotos ou depoimentos antes de utilizá-los no relatório/banner.

Critério proposto de aprovação: concluir autonomamente cadastro, venda e leitura do relatório, com valores corretos. A aprovação depende da sessão real e dos ajustes identificados.

## Novos cenários do protótipo local

- Criar encomenda para cliente com prazo, retirada/entrega e sinal; conferir total, recebido e saldo.
- Alterar a etapa de produção sem alterar os recebimentos.
- Registrar pagamento posterior e conferir saldo zerado; tentar ultrapassar o total.
- Cancelar com motivo e conferir histórico preservado e exclusão dos totais de vendas/despesas.
- Abrir a ficha do cliente e conferir a encomenda e a situação financeira.
- Cadastrar foto e unidade do produto, mantendo o custo por unidade.
- Usar Voltar/Avançar e recarregar uma página interna.
- Criar backup, restaurar em outro banco e conferir os registros antes de qualquer troca do banco em uso.
