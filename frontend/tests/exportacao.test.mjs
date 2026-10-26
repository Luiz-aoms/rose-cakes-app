import { test } from 'node:test'
import assert from 'node:assert/strict'
import { gerarCsv } from '../src/exportacao.ts'
test('exporta recebimento pela sua data mesmo para venda de outro período',()=>{
 const csv=gerarCsv([{id:1,dataVenda:'2026-09-01',clienteNome:'Cliente',total:120,custo:40,recebimentos:[{valor:80,dataPagamento:'2026-10-20',forma:'Pix'}]}],[],'2026-10-01','2026-10-31')
 assert.ok(csv.includes('Recebimento'));assert.ok(csv.includes('80,00'));assert.ok(!csv.includes('120,00'))
})
test('preserva cancelamento de caixa, escapa fórmulas e aspas',()=>{
 const csv=gerarCsv([{id:1,dataVenda:'2026-10-01',canceladoEm:'2026-10-02',recebimentos:[{valor:20,dataPagamento:'2026-10-01',forma:'Pix'}]}],[{dataDespesa:'2026-10-01',descricao:'  =SUM(1;2) "teste"',valor:5}],'2026-10-01','2026-10-31')
 assert.ok(csv.startsWith('\uFEFF'));assert.ok(csv.includes('conferir devolução'));assert.ok(csv.includes('"\'  =SUM(1;2) ""teste"""'));assert.ok(csv.includes('5,00'))
})
