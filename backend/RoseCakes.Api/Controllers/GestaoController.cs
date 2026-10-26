using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoseCakes.Api.Data;
using RoseCakes.Api.Models;
namespace RoseCakes.Api.Controllers;
[ApiController, Route("api/gestao")]
public class GestaoController(AppDbContext db) : ControllerBase
{
    [HttpGet("clientes")]
    public async Task<IActionResult> Clientes() => Ok(await db.Clientes.AsNoTracking().OrderBy(c => c.Nome).ToListAsync());
    [HttpPost("clientes")]
    public async Task<IActionResult> CriarCliente(Cliente c)
    {
        if (string.IsNullOrWhiteSpace(c.Nome) || string.IsNullOrWhiteSpace(c.Telefone)) return BadRequest("Informe nome e telefone.");
        c.Id = 0; c.Nome = c.Nome.Trim(); db.Clientes.Add(c); await db.SaveChangesAsync(); return Ok(c);
    }
    [HttpPut("clientes/{id:int}")]
    public async Task<IActionResult> EditarCliente(int id, Cliente c)
    {
        var atual = await db.Clientes.FindAsync(id);
        if (atual is null) return NotFound();
        if (string.IsNullOrWhiteSpace(c.Nome) || string.IsNullOrWhiteSpace(c.Telefone)) return BadRequest("Informe nome e telefone.");
        atual.Nome = c.Nome.Trim(); atual.Telefone = c.Telefone; atual.Endereco = c.Endereco; atual.Observacoes = c.Observacoes; atual.DataNascimento = c.DataNascimento;
        await db.SaveChangesAsync(); return Ok(atual);
    }
    [HttpGet("despesas")]
    public async Task<IActionResult> Despesas() => Ok(await db.Despesas.AsNoTracking().OrderByDescending(d => d.DataDespesa).ToListAsync());
    [HttpPost("despesas")]
    public async Task<IActionResult> CriarDespesa(Despesa d)
    {
        if (string.IsNullOrWhiteSpace(d.Descricao) || d.Valor <= 0 || d.Valor > 99999999 || d.DataDespesa == default) return BadRequest("Informe descrição, data e valor positivo.");
        d.Id = 0; db.Despesas.Add(d); await db.SaveChangesAsync(); return Ok(d);
    }
    [HttpPut("despesas/{id:int}")]
    public async Task<IActionResult> EditarDespesa(int id, Despesa d)
    {
        if (string.IsNullOrWhiteSpace(d.Descricao) || d.Valor <= 0 || d.Valor > 99999999 || d.DataDespesa == default)
            return BadRequest("Informe descrição, data e valor positivo.");

        var atual = await db.Despesas.FindAsync(id);
        if (atual is null) return NotFound();
        if (atual.CanceladoEm.HasValue) return BadRequest("Uma despesa cancelada não pode ser editada.");

        atual.Descricao = d.Descricao.Trim();
        atual.Categoria = d.Categoria?.Trim();
        atual.Valor = d.Valor;
        atual.DataDespesa = d.DataDespesa;
        await db.SaveChangesAsync();
        return Ok(atual);
    }
    [HttpPost("despesas/{id:int}/cancelamento")]
    public async Task<IActionResult> CancelarDespesa(int id, Cancelamento pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.Motivo) || pedido.Motivo.Trim().Length > 500) return BadRequest("Informe um motivo com até 500 caracteres.");
        var despesa = await db.Despesas.FindAsync(id);
        if (despesa is null) return NotFound();
        if (despesa.CanceladoEm.HasValue) return BadRequest("Esta despesa já foi cancelada.");
        despesa.MotivoCancelamento = pedido.Motivo.Trim();
        despesa.CanceladoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(despesa);
    }
    [HttpPost("vendas/{id:int}/cancelamento")]
    public async Task<IActionResult> CancelarVenda(int id, Cancelamento pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.Motivo) || pedido.Motivo.Trim().Length > 500) return BadRequest("Informe um motivo com até 500 caracteres.");
        var venda = await db.Vendas.Include(v => v.Itens).Include(v => v.Recebimentos).FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();
        if (venda.CanceladoEm.HasValue) return BadRequest("Esta venda já foi cancelada.");
        venda.MotivoCancelamento = pedido.Motivo.Trim();
        venda.CanceladoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(venda);
    }
    [HttpGet("vendas")]
    public async Task<IActionResult> Vendas() => Ok(await db.Vendas.AsNoTracking().Include(v => v.Itens).Include(v => v.Recebimentos).OrderByDescending(v => v.DataVenda).ToListAsync());
    [HttpPost("vendas")]
    public async Task<IActionResult> CriarVenda(NovaVenda pedido)
    {
        if (pedido.DataEntrega.HasValue && (!pedido.ClienteId.HasValue || !new[] { "Retirada", "Entrega" }.Contains(pedido.TipoEntrega) || (pedido.TipoEntrega == "Entrega" && string.IsNullOrWhiteSpace(pedido.EnderecoEntrega))))
            return BadRequest("Encomendas precisam de cliente, prazo e modalidade; para entrega, informe o endereço.");
        if (pedido.Itens is null || pedido.Itens.Count == 0 || pedido.Itens.Any(i => i.Quantidade <= 0 || i.Quantidade > 10000) || pedido.DataVenda == default || !new[] { "Pix", "Dinheiro", "Cartão" }.Contains(pedido.Pagamento)) return BadRequest("Verifique os itens, a data e a forma de pagamento.");
        var cliente = pedido.ClienteId.HasValue ? await db.Clientes.FindAsync(pedido.ClienteId.Value) : null;
        if (pedido.ClienteId.HasValue && cliente is null) return BadRequest("Cliente não encontrado.");
        var ids = pedido.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos.Where(p => ids.Contains(p.Id) && p.Ativo).ToDictionaryAsync(p => p.Id);
        if (produtos.Count != ids.Count) return BadRequest("Um dos produtos não está disponível.");
        var venda = new Venda { ClienteId = cliente?.Id, ClienteNome = cliente?.Nome ?? "Consumidor avulso", DataVenda = pedido.DataVenda, Pagamento = pedido.Pagamento,
            Itens = pedido.Itens.Select(i => new ItemVenda { ProdutoId = i.ProdutoId, ProdutoNome = produtos[i.ProdutoId].Nome, Quantidade = i.Quantidade, PrecoUnitario = produtos[i.ProdutoId].PrecoVenda, CustoUnitario = produtos[i.ProdutoId].PrecoCusto }).ToList() };
        venda.DataEntrega = pedido.DataEntrega;
        venda.Etapa = pedido.DataEntrega.HasValue ? "Recebido" : "Entregue";
        venda.TipoEntrega = pedido.TipoEntrega;
        venda.EnderecoEntrega = pedido.EnderecoEntrega?.Trim();
        venda.Observacoes = pedido.Observacoes?.Trim();
        var recebido = pedido.ValorRecebido ?? 0;
        if (recebido < 0 || recebido > venda.Total || decimal.Round(recebido, 2) != recebido) return BadRequest("O sinal deve estar entre zero e o total, com até duas casas decimais.");
        venda.RecebimentosConferidos = true;
        if (recebido > 0) venda.Recebimentos.Add(new Recebimento { Valor = recebido, DataPagamento = pedido.DataVenda, Forma = pedido.Pagamento });
        venda.ValorTotal = venda.Total; db.Vendas.Add(venda); await db.SaveChangesAsync(); return Ok(venda);
    }
    [HttpPost("vendas/{id:int}/recebimentos")]
    public async Task<IActionResult> Receber(int id, NovoRecebimento pedido)
    {
        if (pedido.Valor <= 0 || decimal.Round(pedido.Valor, 2) != pedido.Valor || pedido.DataPagamento == default || !new[] { "Pix", "Dinheiro", "Cartão" }.Contains(pedido.Forma)) return BadRequest("Informe valor positivo com até duas casas decimais, data e forma de pagamento.");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var venda = await db.Vendas.Include(v => v.Itens).Include(v => v.Recebimentos).FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();
        if (venda.CanceladoEm.HasValue) return BadRequest("Não é possível receber em uma venda cancelada.");
        if (pedido.Valor > venda.Saldo) return BadRequest("O pagamento supera o saldo. Atualize o registro e confira os recebimentos.");
        venda.Recebimentos.Add(new Recebimento { Valor = pedido.Valor, DataPagamento = pedido.DataPagamento, Forma = pedido.Forma });
        venda.RecebimentosConferidos = true;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(venda);
    }
    [HttpPut("vendas/{id:int}/etapa")]
    public async Task<IActionResult> MudarEtapa(int id, AlterarEtapa pedido)
    {
        if (!new[] { "Recebido", "Em produção", "Pronto", "Entregue" }.Contains(pedido.Etapa)) return BadRequest("Etapa inválida.");
        var venda = await db.Vendas.Include(v => v.Itens).Include(v => v.Recebimentos).FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();
        if (!venda.DataEntrega.HasValue || venda.CanceladoEm.HasValue) return BadRequest("Apenas encomendas ativas podem mudar de etapa.");
        venda.Etapa = pedido.Etapa;
        await db.SaveChangesAsync();
        return Ok(venda);
    }
}



