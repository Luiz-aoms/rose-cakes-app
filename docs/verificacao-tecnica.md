# Verificação técnica — melhorias locais

## Verificações executadas

- Build de produção do frontend e lint concluídos.
- Cinco testes automatizados de cálculos e CSV: exclusão de cancelamentos, distinção das datas de venda e recebimento, saldos antigos a conferir, períodos vazios, escape de fórmulas/aspas e recebimento de venda de outro período.
- Compilação da API concluída.
- `scripts/Testar-Integracao.ps1` executado com sucesso em bancos temporários exclusivos, removidos ao concluir.
- Fluxos de integração: cadastro de cliente e produto; encomenda com sinal; alteração de produto preservando histórico da venda; quitação posterior; rejeição de valor acima do saldo; etapa de produção; cancelamento; rejeição de pagamento/cancelamento duplicado em venda cancelada; edição e cancelamento de despesa; rejeição de edição da despesa cancelada; rejeição de imagem SVG; persistência de foto PNG e rendimento.
- Backup com checksum e VERIFYONLY, seguido de restauração real em um segundo banco de teste. Conferidos recebimentos e foto após restaurar.
- Banco local de uso: backup verificado antes da aplicação dos scripts 002 a 005. Não foram criados dados fictícios nesse banco.
- Navegador: conferidos painel em desktop, menu hambúrguer, navegação até Pedidos e Vendas, formulário móvel e cartões em viewport de 375 px; Voltar e recarregamento mantiveram a página esperada.

## Limitações

A validação automatizada não substitui uma sessão com a empreendedora. Ainda não houve capacitação nem avaliação de impacto social. As conferências móveis usaram viewport de navegador, não aparelhos físicos de diferentes fabricantes.

O build da API continua apontando os avisos NU1902/NU1903 na dependência transitiva `System.Security.Cryptography.Pkcs` 9.0.11. A atualização dessas dependências precisa de validação própria antes de hospedagem pública. Não há autenticação nesta etapa local.

Consulte README para executar novamente as verificações e para entender as regras de receitas, recebimentos, custos e cancelamentos.

## Correção de pagamentos e gerenciamento de registros

- Regressão integrada: após quitar uma venda, criar outra sem informar recebimento mantém recebido zero e saldo total. O teste unitário distingue Pendente, Parcial, Pago, Cancelado e A conferir.
- Correção e exclusão de recebimentos recalculam o saldo. Exclusões sem confirmação são rejeitadas.
- Exclusão confirmada de venda remove também seus itens e recebimentos, validada por restauração e contagem final dos recebimentos do banco de teste.
- Clientes e produtos vinculados retornam conflito ao tentar excluir; cadastros sem vínculos podem ser excluídos. Despesas podem ser excluídas após confirmação.
- Edição de pedido preserva recebimentos e nome/custo históricos. Foram testados inclusão/remoção de itens, alteração de preço e rejeição de total menor que o recebido.
- Conferência de interface: nova venda inicia em zero, alterar um valor e fechar sem salvar não o transporta para o cadastro seguinte; confirmação de exclusão e formulário de edição foram abertos e fechados sem alterar os registros reais.
- Não foi necessário mudar o esquema do banco nem corrigir automaticamente recebimentos anteriores.
