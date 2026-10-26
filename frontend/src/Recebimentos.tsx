import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import type { Venda } from './tipos'
import { api } from './api'
import { data, dinheiro, hoje } from './formatacao'
import { situacaoPagamento } from './pagamento'
import { ExcluirRegistro } from './ExcluirRegistro'
import type { AlvoExclusao } from './ExcluirRegistro'
export function Recebimentos({ venda, fechar, atualizar }: { venda: Venda | null; fechar: () => void; atualizar: () => Promise<void> }) {
  const dialog = useRef<HTMLDialogElement>(null)
  const [erro, setErro] = useState(''), [salvando, setSalvando] = useState(false), [editando, setEditando] = useState<number | null>(null)
  const [exclusao, setExclusao] = useState<AlvoExclusao | null>(null)
  useEffect(() => { if (venda) dialog.current?.showModal(); else dialog.current?.close() }, [venda])
  const registro = venda?.recebimentos.find(r=>r.id===editando)
  const limite = (venda?.saldo ?? 0) + (registro?.valor ?? 0)
  async function salvar(e: FormEvent<HTMLFormElement>) {
    e.preventDefault(); if (!venda || salvando) return
    const f = new FormData(e.currentTarget); setSalvando(true); setErro('')
    try {
      await api(`/gestao/vendas/${venda.id}/recebimentos${registro?`/${registro.id}`:''}`, registro?'PUT':'POST', { valor: Number(f.get('valor')), dataPagamento: f.get('data'), forma: f.get('forma') })
      setEditando(null); await atualizar()
    }
    catch(e) { setErro(e instanceof Error ? e.message : 'Não foi possível registrar.') }
    finally { setSalvando(false) }
  }
  function voltar() { setErro(''); setEditando(null); fechar() }
  return <><dialog ref={dialog} aria-labelledby="recebimentos-titulo" onCancel={e=>{if(salvando)e.preventDefault();else voltar()}}><h2 id="recebimentos-titulo">Recebimentos · #{venda?.id}</h2><p className="subtitle">{venda?.clienteNome} · {venda && situacaoPagamento(venda)}</p>{venda && !venda.recebimentosConferidos && <p className="form-error">Venda anterior ao controle de recebimentos. Confira os comprovantes e registre os valores efetivamente recebidos.</p>}<div className="report-line"><span>Total</span><strong>{dinheiro(venda?.total ?? 0)}</strong></div><div className="report-line"><span>Recebido registrado</span><strong>{dinheiro(venda?.recebido ?? 0)}</strong></div><div className="report-line"><span>Saldo a receber</span><strong>{dinheiro(venda?.saldo ?? 0)}</strong></div><ul className="payment-list">{venda?.recebimentos.map(r=><li key={r.id}><div>{data(r.dataPagamento)} · {r.forma}<strong className="payment-amount">{dinheiro(r.valor)}</strong></div>{!venda.canceladoEm && <div className="actions"><button type="button" className="text-button" disabled={salvando} onClick={()=>{setErro('');setEditando(r.id)}}>Alterar</button><button type="button" className="text-button danger-text" disabled={salvando} onClick={()=>setExclusao({rota:`/gestao/vendas/${venda.id}/recebimentos/${r.id}`,titulo:`Recebimento de ${dinheiro(r.valor)}`,impacto:'O lançamento de recebimento será apagado. O saldo voltará a ficar em aberto e a situação de pagamento será recalculada. Isso corrige o registro, sem movimentar dinheiro.'})}>Excluir recebimento</button></div>}</li>)}</ul>{venda && !venda.canceladoEm && (venda.saldo > 0 || registro) && <form key={`${venda.id}-${editando ?? 'novo'}`} onSubmit={salvar}><h3>{registro?'Alterar recebimento':'Registrar novo recebimento'}</h3><div className="form-grid"><label>Valor (R$)<input name="valor" type="number" min="0.01" max={limite.toFixed(2)} step="0.01" required defaultValue={registro?.valor}/></label><label>Data<input name="data" type="date" defaultValue={registro?.dataPagamento.slice(0,10)||hoje()} required/></label><label className="full">Forma<select name="forma" defaultValue={registro?.forma||'Pix'}><option>Pix</option><option>Dinheiro</option><option>Cartão</option></select></label></div>{erro && <p className="form-error" role="alert">{erro}</p>}<div className="modal-actions">{registro && <button type="button" disabled={salvando} onClick={()=>{setErro('');setEditando(null)}}>Não alterar</button>}<button className="primary" disabled={salvando}>{salvando?'Salvando…':registro?'Salvar alteração':'Registrar recebimento'}</button></div></form>}{venda?.canceladoEm && <p className="form-error">Venda cancelada. Os recebimentos estão preservados; este sistema não realiza devoluções.</p>}<div className="modal-actions"><button type="button" disabled={salvando} onClick={voltar}>Fechar</button></div></dialog><ExcluirRegistro alvo={exclusao} fechar={()=>setExclusao(null)} atualizar={async()=>{setEditando(null);await atualizar()}}/></>
}
