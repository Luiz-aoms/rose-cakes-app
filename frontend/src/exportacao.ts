import type { Venda, Despesa } from './tipos'
import { data } from './formatacao.ts'
export function gerarCsv(vendas: Venda[], despesas: Despesa[], inicio: string, fim: string) {
  const noPeriodo = (d: string) => d.slice(0,10) >= inicio && d.slice(0,10) <= fim
  const valor = (n: number) => n.toFixed(2).replace('.',',')
  const linhas: { dia: string; colunas: string[] }[] = []
  for (const v of vendas) {
    if (!v.canceladoEm && noPeriodo(v.dataVenda)) linhas.push({dia:v.dataVenda,colunas:['Venda',data(v.dataVenda),`#${v.id} - ${v.clienteNome}`,valor(v.total),valor(v.custo),'','','Válida']})
    for (const r of v.recebimentos) if (noPeriodo(r.dataPagamento)) linhas.push({dia:r.dataPagamento,colunas:['Recebimento',data(r.dataPagamento),`Venda #${v.id} - ${r.forma}`,'','','',valor(r.valor),v.canceladoEm?'Venda cancelada; conferir devolução':'Registrado']})
  }
  for (const d of despesas) if (!d.canceladoEm && noPeriodo(d.dataDespesa)) linhas.push({dia:d.dataDespesa,colunas:['Despesa',data(d.dataDespesa),d.descricao,'','',valor(d.valor),'','Válida']})
  const tabela = [['Tipo','Data','Descrição','Receita','Custo de produtos','Despesa','Recebimento','Situação'], ...linhas.sort((a,b)=>a.dia.localeCompare(b.dia)).map(l=>l.colunas)]
  const escapar = (s: string) => '"'+((/^\s*[=+@-]/.test(s)||/^[\t\r\n]/.test(s))?"'":'')+s.replaceAll('"','""')+'"'
  return '\uFEFF'+tabela.map(l=>l.map(escapar).join(';')).join('\r\n')
}

