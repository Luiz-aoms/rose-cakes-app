import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
export type AlvoCancelamento = { id: number; tipo: 'vendas' | 'despesas'; descricao: string }
export function CancelarRegistro({ alvo, cancelar, fechar }: { alvo: AlvoCancelamento | null; cancelar: (motivo: string) => Promise<void>; fechar: () => void }) {
  const dialog = useRef<HTMLDialogElement>(null)
  const [erro, setErro] = useState(''), [salvando, setSalvando] = useState(false)
  useEffect(() => { if (alvo) dialog.current?.showModal(); else dialog.current?.close() }, [alvo])
  async function salvar(e: FormEvent<HTMLFormElement>) {
    e.preventDefault(); setErro(''); setSalvando(true)
    try { const motivo = String(new FormData(e.currentTarget).get('motivo') || '').trim(); if (!motivo) throw new Error('Informe o motivo do cancelamento.'); await cancelar(motivo) }
    catch (e) { setErro(e instanceof Error ? e.message : 'Não foi possível cancelar.') }
    finally { setSalvando(false) }
  }
  return <dialog ref={dialog} aria-labelledby="cancelamento-titulo" onCancel={e => { if (salvando) e.preventDefault(); else { setErro(''); fechar() } }}><form key={alvo?.id + '-' + alvo?.tipo} onSubmit={salvar}><h2 id="cancelamento-titulo">Cancelar registro</h2><p className="subtitle">{alvo?.descricao}. Cancelar mantém o registro no histórico, marcado como cancelado, e o retira dos totais de vendas ou despesas. Para apagar de vez, use Excluir. Recebimentos permanecem registrados; este comando não faz devoluções de dinheiro. O cancelamento não pode ser desfeito nesta versão.</p><label className="field">Motivo<textarea name="motivo" required maxLength={500} autoFocus/></label>{erro && <p className="form-error" role="alert">{erro}</p>}<div className="modal-actions"><button type="button" disabled={salvando} onClick={() => { setErro(''); fechar() }}>Manter registro</button><button className="primary" disabled={salvando}>{salvando ? 'Cancelando…' : 'Confirmar cancelamento'}</button></div></form></dialog>
}


