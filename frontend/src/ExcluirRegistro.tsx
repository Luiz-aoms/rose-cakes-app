import { useEffect, useId, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { api } from './api'
export type AlvoExclusao = { rota: string; titulo: string; impacto: string }
export function ExcluirRegistro({ alvo, fechar, atualizar }: { alvo: AlvoExclusao | null; fechar: () => void; atualizar: () => Promise<void> }) {
  const tituloId = useId()
  const dialog = useRef<HTMLDialogElement>(null)
  const [erro, setErro] = useState(''), [salvando, setSalvando] = useState(false)
  useEffect(() => { if (alvo) dialog.current?.showModal(); else dialog.current?.close() }, [alvo])
  function voltar() { if (salvando) return; setErro(''); fechar() }
  async function excluir(e: FormEvent<HTMLFormElement>) {
    e.preventDefault(); if (!alvo || salvando) return
    setSalvando(true); setErro('')
    try { await api(alvo.rota, 'DELETE', { confirmar: true }); fechar(); await atualizar() }
    catch(e) { setErro(e instanceof Error ? e.message : 'Não foi possível excluir.') }
    finally { setSalvando(false) }
  }
  return <dialog ref={dialog} aria-labelledby={tituloId} onCancel={e=>{e.preventDefault();voltar()}}><form key={alvo?.rota} onSubmit={excluir}><h2 id={tituloId}>Excluir definitivamente?</h2><p className="subtitle"><strong>{alvo?.titulo}</strong></p><p className="deletion-warning">{alvo?.impacto} Esta ação não pode ser desfeita pelo sistema.</p><label className="confirmation-check"><input type="checkbox" required/>Entendi e quero excluir este registro.</label>{erro && <p role="alert" className="form-error">{erro}</p>}<div className="modal-actions"><button type="button" disabled={salvando} onClick={voltar}>Manter registro</button><button className="danger" disabled={salvando}>{salvando?'Excluindo…':'Excluir definitivamente'}</button></div></form></dialog>
}

