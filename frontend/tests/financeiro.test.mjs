import { test } from 'node:test'
import assert from 'node:assert/strict'
import { resumir } from '../src/financeiro.ts'
const venda = (props={}) => ({dataVenda:'2026-10-20',total:120,custo:40,saldo:80,recebimentosConferidos:true,itens:[{produtoNome:'Bolo',quantidade:1}],recebimentos:[{valor:40,dataPagamento:'2026-10-21'}],...props})
test('exclui cancelamentos e separa data de pagamento da data de venda',()=>{
  const r=resumir([venda(),venda({canceladoEm:'2026-10-21'})],[{dataDespesa:'2026-10-20',categoria:'Transporte',valor:10},{dataDespesa:'2026-10-20',categoria:'Transporte',valor:99,canceladoEm:'2026-10-20'}],'2026-10-20','2026-10-20')
  assert.equal(r.receita,120);assert.equal(r.custo,40);assert.equal(r.despesa,10);assert.equal(r.resultado,70);assert.equal(r.recebido,0);assert.equal(r.saldo,80);assert.equal(r.produtos[0][1],1)
  assert.equal(resumir([venda()],[],'2026-10-21','2026-10-21').recebido,40)
})
test('não presume saldo de vendas antigas e não divide por zero',()=>{
  const r=resumir([venda({recebimentosConferidos:false})],[],'2026-10-01','2026-10-31');assert.equal(r.saldo,0);assert.equal(r.aConferir,1)
  const vazio=resumir([],[],'2026-10-01','2026-10-31');assert.equal(vazio.ticket,0);assert.equal(vazio.resultado,0)
})
