import type { Venda } from './tipos'
export function situacaoPagamento(v: Pick<Venda, 'canceladoEm' | 'recebimentosConferidos' | 'total' | 'recebido'>) {
  if (v.canceladoEm) return 'Cancelado'
  if (!v.recebimentosConferidos) return 'A conferir'
  if (v.total > 0 && Math.round(v.recebido * 100) >= Math.round(v.total * 100)) return 'Pago'
  return v.recebido > 0 ? 'Parcial' : 'Pendente'
}
