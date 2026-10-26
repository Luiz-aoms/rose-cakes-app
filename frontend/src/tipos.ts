export type Produto = { id: number; nome: string; descricao: string; categoria: string; precoCusto: number; precoVenda: number; ativo: boolean }
export type Cliente = { id: number; nome: string; telefone: string; endereco: string; observacoes: string }
export type Despesa = { canceladoEm?: string; motivoCancelamento?: string; id: number; descricao: string; categoria: string; valor: number; dataDespesa: string }
export type Venda = { clienteId?: number; dataEntrega?: string; etapa: string; tipoEntrega?: string; enderecoEntrega?: string; observacoes?: string; canceladoEm?: string; motivoCancelamento?: string; id: number; clienteNome: string; dataVenda: string; pagamento: string; total: number; custo: number; itens: { produtoNome: string; quantidade: number }[] }

