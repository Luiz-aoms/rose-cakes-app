import { useEffect, useState } from 'react'

/** URLs por hash funcionam também ao abrir uma rota local diretamente. */
export function useNavegacao<T extends string>(rotas: Record<T, string>, inicial: T) {
  const lerTela = () => (Object.keys(rotas) as T[]).find(t => '#/' + rotas[t] === window.location.hash) ?? inicial
  const [tela, definirTela] = useState<T>(lerTela)
  useEffect(() => {
    const atualizar = () => definirTela(lerTela())
    window.addEventListener('hashchange', atualizar)
    return () => window.removeEventListener('hashchange', atualizar)
  })
  function navegar(t: T) {
    window.location.hash = '/' + rotas[t]
    definirTela(t)
  }
  return [tela, navegar] as const
}
