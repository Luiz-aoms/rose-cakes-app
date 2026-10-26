const base = import.meta.env.VITE_API_URL || '/api'
export async function api<T>(path: string, method = 'GET', body?: unknown): Promise<T> {
  const res = await fetch(base + path, { method, headers: { 'Content-Type': 'application/json' }, body: body === undefined ? undefined : JSON.stringify(body), signal: AbortSignal.timeout(15000) })
  if (res.status >= 500) throw new Error("Serviço indisponível. Tente novamente em instantes.")
  if (!res.ok) { const text = await res.text(); throw new Error(text.startsWith('{') ? 'Não foi possível concluir. Verifique os dados e tente novamente.' : text || 'Serviço indisponível.') }
  if (res.status === 204) return undefined as T
  return res.json()
}


