import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import type { Venda, Cliente, Produto } from './tipos'
import { api } from './api'
import { dinheiro } from './formatacao'
type Item = { id?: number; produtoId: number; produtoNome: string; quantidade: number; precoUnitario: number }
export function EditarVenda({ venda, clientes, produtos, fechar, atualizar }: { venda: Venda; clientes: Cliente[]; produtos: Produto[]; fechar: () => void; atualizar: () => Promise<void> }) {
  const dialog = useRef<HTMLDialogElement>(null)
  const [itens, setItens] = useState<Item[]>(()=>venda.itens.map(i=>({...i})))
  const [entrega, setEntrega] = useState(venda.tipoEntrega || 'Retirada'), [erro, setErro] = useState(''), [salvando, setSalvando] = useState(false)
  useEffect(()=>{ dialog.current?.showModal() },[])
  const total = itens.reduce((s,i)=>s+i.quantidade*i.precoUnitario,0)
  const pedido = !!venda.dataEntrega
  async function salvar(e: FormEvent<HTMLFormElement>) {
    e.preventDefault(); if (salvando) return
    setErro(''); setSalvando(true)
    const f = new FormData(e.currentTarget)
    try {
      await api(`/gestao/vendas/${venda.id}`, 'PUT', { clienteId: Number(f.get('cliente'))||null, dataVenda: f.get('data'), dataEntrega: pedido?f.get('prazo'):null, tipoEntrega: pedido?entrega:null, enderecoEntrega: f.get('endereco'), observacoes: f.get('observacoes'), itens: itens.map(i=>({id:i.id ?? null,produtoId:i.produtoId,quantidade:i.quantidade,precoUnitario:i.precoUnitario})) })
      fechar(); await atualizar()
    } catch(e) { setErro(e instanceof Error ? e.message : 'Não foi possível alterar.') }
    finally { setSalvando(false) }
  }
  function alterar(n: number, valores: Partial<Item>) { setItens(itens.map((i,j)=>j===n?{...i,...valores}:i)) }
  return <dialog ref={dialog} aria-labelledby="editar-venda-titulo" onCancel={e=>{if(salvando)e.preventDefault();else fechar()}}><form onSubmit={salvar}><h2 id="editar-venda-titulo">Alterar {pedido?'pedido':'venda'} #{venda.id}</h2><p className="subtitle">Os recebimentos continuam registrados. Para corrigir um pagamento, use Ver recebimentos.</p><div className="form-grid"><label className="full">Cliente<select name="cliente" required={pedido} defaultValue={venda.clienteId ?? ''}><option value="">{pedido?'Selecione…':'Consumidor avulso'}</option>{clientes.map(c=><option key={c.id} value={c.id}>{c.nome}</option>)}</select></label><label className="full">Data do registro<input name="data" type="date" required defaultValue={venda.dataVenda.slice(0,10)}/></label>{pedido && <><label>Data e horário de entrega<input type="datetime-local" name="prazo" required defaultValue={venda.dataEntrega!.slice(0,16)}/></label><label>Modalidade<select value={entrega} onChange={e=>setEntrega(e.target.value)}><option>Retirada</option><option>Entrega</option></select></label>{entrega==='Entrega' && <label className="full">Endereço<input name="endereco" required maxLength={300} defaultValue={venda.enderecoEntrega}/></label>}</>}<div className="full edit-items">{itens.map((item,n)=><section key={n}><label>Produto{item.id ? <input readOnly value={item.produtoNome}/> : <select required value={item.produtoId||''} onChange={e=>{const p=produtos.find(p=>p.id===Number(e.target.value));alterar(n,{produtoId:p?.id||0,produtoNome:p?.nome||'',precoUnitario:p?.precoVenda||0})}}><option value="">Selecione…</option>{produtos.filter(p=>p.ativo).map(p=><option key={p.id} value={p.id}>{p.nome}</option>)}</select>}</label><div className="edit-item-values"><label>Quantidade<input type="number" required min="1" max="10000" step="1" value={item.quantidade||''} onChange={e=>alterar(n,{quantidade:Number(e.target.value)})}/></label><label>Preço unitário (R$)<input type="number" required min="0.01" max="99999999" step="0.01" value={item.precoUnitario||''} onChange={e=>alterar(n,{precoUnitario:Number(e.target.value)})}/></label></div><div className="row"><small>Subtotal: {dinheiro(item.quantidade*item.precoUnitario)}</small><button className="text-button danger-text" type="button" disabled={itens.length===1} onClick={()=>setItens(itens.filter((_,j)=>j!==n))}>Remover item</button></div></section>)}<button className="text-button" type="button" disabled={itens.length>=100} onClick={()=>setItens([...itens,{produtoId:0,produtoNome:'',quantidade:1,precoUnitario:0}])}>+ Adicionar produto</button></div><label className="full">Observações<textarea name="observacoes" maxLength={1000} defaultValue={venda.observacoes}/></label></div><div className="report-line"><span>Já recebido</span><strong>{dinheiro(venda.recebido)}</strong></div><div className="sale-total">Novo total<strong>{dinheiro(total)}</strong></div>{total<venda.recebido && <p className="form-error">O total não pode ficar menor que o valor recebido. Corrija o recebimento se ele foi lançado incorretamente.</p>}{erro && <p role="alert" className="form-error">{erro}</p>}<div className="modal-actions"><button type="button" disabled={salvando} onClick={fechar}>Fechar sem salvar</button><button className="primary" disabled={salvando || total<venda.recebido}>{salvando?'Salvando…':'Salvar alterações'}</button></div></form></dialog>
}
