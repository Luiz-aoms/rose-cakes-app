export const dinheiro = (n: number) => n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
export const hoje = () => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}` }
export const data = (v: string) => new Date(v.slice(0,10)+'T12:00:00').toLocaleDateString('pt-BR')
