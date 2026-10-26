import type { Venda, Despesa } from './tipos'
export function resumir(vendas: Venda[], despesas: Despesa[], inicio: string, fim: string) {
  const noPeriodo = (d: string) => d.slice(0,10) >= inicio && d.slice(0,10) <= fim
  const ativas = vendas.filter(v=>!v.canceladoEm && noPeriodo(v.dataVenda)), gastos = despesas.filter(d=>!d.canceladoEm && noPeriodo(d.dataDespesa))
  const receita = ativas.reduce((s,v)=>s+v.total,0), custo = ativas.reduce((s,v)=>s+v.custo,0), despesa = gastos.reduce((s,d)=>s+d.valor,0)
  const produtos = new Map<string,number>(), categorias = new Map<string,number>(), dias = new Map<string,number>()
  for (const v of ativas) { dias.set(v.dataVenda.slice(0,10), (dias.get(v.dataVenda.slice(0,10))??0)+v.total); for (const i of v.itens) produtos.set(i.produtoNome,(produtos.get(i.produtoNome)??0)+i.quantidade) }
  for (const d of gastos) categorias.set(d.categoria || 'Sem categoria',(categorias.get(d.categoria || 'Sem categoria')??0)+d.valor)
  const ranking = (m: Map<string,number>) => [...m].sort((a,b)=>b[1]-a[1])
  return { receita, custo, despesa, resultado: receita-custo-despesa, quantidade: ativas.length, ticket: ativas.length?receita/ativas.length:0, produtos: ranking(produtos), categorias: ranking(categorias), dias: [...dias].sort((a,b)=>a[0].localeCompare(b[0])), saldo: ativas.filter(v=>v.recebimentosConferidos).reduce((s,v)=>s+v.saldo,0), aConferir: ativas.filter(v=>!v.recebimentosConferidos).length,
    recebido: vendas.flatMap(v=>v.recebimentos).filter(r=>noPeriodo(r.dataPagamento)).reduce((s,r)=>s+r.valor,0) }
}
