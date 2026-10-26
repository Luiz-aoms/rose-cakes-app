import { test } from 'node:test'
import assert from 'node:assert/strict'
import { situacaoPagamento } from '../src/pagamento.ts'
test('nova venda não herda pagamento da anterior',()=>{
 const base={recebimentosConferidos:true,total:100,recebido:0}
 assert.equal(situacaoPagamento({...base,recebido:100}),'Pago')
 assert.equal(situacaoPagamento(base),'Pendente')
 assert.equal(situacaoPagamento({...base,recebido:40}),'Parcial')
 assert.equal(situacaoPagamento({...base,recebimentosConferidos:false}),'A conferir')
 assert.equal(situacaoPagamento({...base,total:0}),'Pendente')
 assert.equal(situacaoPagamento({...base,recebido:100,canceladoEm:'2026-10-26'}),'Cancelado')
})
