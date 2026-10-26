import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import type { Venda } from './tipos'
import { api } from './api'
import { data, dinheiro, hoje } from './formatacao'
export function Recebimentos({ venda, fechar, atualizar }: { venda: Venda | null; fechar: () => void; atualizar: () => Promise<void> }) {
  const dialog = useRef<HTMLDialogElement>(null)
  const [erro, setErro] = useState(''), [salvando, setSalvando] = useState(false)
  useEffect(() => { if (venda) dialog.current?.showModal(); else dialog.current?.close() }, [venda])
  async function salvar(e: FormEvent<HTMLFormElement>) {
    e.preventDefault(); if (!venda) return
    const f = new FormData(e.currentTarget); setSalvando(true); setErro('')
    try { await api(`/gestao/vendas/${venda.id}/recebimentos`, 'POST', { valor: Number(f.get('valor')), dataPagamento: f.get('data'), forma: f.get('forma') }); fechar(); await atualizar() }
    catch(e) { setErro(e instanceof Error ? e.message : 'Não foi possível registrar.') }
    finally { setSalvando(false) }
  }
  function voltar() { setErro(''); fechar() }
  return <dialog ref={dialog} aria-labelledby="recebimentos-titulo" onCancel={e=>{if(salvando)e.preventDefault();else voltar()}}><form key={venda?.id} onSubmit={salvar}><h2 id="recebimentos-titulo">Recebimentos · #{venda?.id}</h2><p className="subtitle">{venda?.clienteNome}</p>{venda && !venda.recebimentosConferidos && <p className="form-error">Venda anterior ao controle de recebimentos. Confira os comprovantes e registre os valores efetivamente recebidos; o sistema não presume que ela esteja paga.</p>}<div className="report-line"><span>Total</span><strong>{dinheiro(venda?.total ?? 0)}</strong></div><div className="report-line"><span>Recebido registrado</span><strong>{dinheiro(venda?.recebido ?? 0)}</strong></div><div className="report-line"><span>Saldo pelos registros</span><strong>{dinheiro(venda?.saldo ?? 0)}</strong></div><ul className="payment-list">{venda?.recebimentos.map(r=><li key={r.id}>{data(r.dataPagamento)} · {r.forma}<strong>{dinheiro(r.valor)}</strong></li>)}</ul>{venda && !venda.canceladoEm && venda.saldo > 0 && <div className="form-grid"><label>Valor recebido<input name="valor" type="number" min="0.01" max={venda.saldo.toFixed(2)} step="0.01" required/></label><label>Data<input name="data" type="date" defaultValue={hoje()} required/></label><label className="full">Forma<select name="forma"><option>Pix</option><option>Dinheiro</option><option>Cartão</option></select></label></div>}{venda?.canceladoEm && <p className="form-error">Venda cancelada. Os recebimentos ficam preservados; eventuais devoluções devem ser conferidas fora do sistema.</p>}{erro && <p className="form-error" role="alert">{erro}</p>}<div className="modal-actions"><button type="button" disabled={salvando} onClick={voltar}>Fechar</button>{venda && !venda.canceladoEm && venda.saldo > 0 && <button className="primary" disabled={salvando}>{salvando?'Salvando…':'Registrar recebimento'}</button>}</div></form></dialog>
}
