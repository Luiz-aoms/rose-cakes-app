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
    [HttpGet("vendas")]
    public async Task<IActionResult> Vendas() => Ok(await db.Vendas.AsNoTracking().Include(v => v.Itens).OrderByDescending(v => v.DataVenda).ToListAsync());
    [HttpPost("vendas")]
    public async Task<IActionResult> CriarVenda(NovaVenda pedido)
    {
        if (pedido.Itens is null || pedido.Itens.Count == 0 || pedido.Itens.Any(i => i.Quantidade <= 0 || i.Quantidade > 10000) || pedido.DataVenda == default || !new[] { "Pix", "Dinheiro", "Cartão" }.Contains(pedido.Pagamento)) return BadRequest("Verifique os itens, a data e a forma de pagamento.");
        var cliente = pedido.ClienteId.HasValue ? await db.Clientes.FindAsync(pedido.ClienteId.Value) : null;
        if (pedido.ClienteId.HasValue && cliente is null) return BadRequest("Cliente não encontrado.");
        var ids = pedido.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos.Where(p => ids.Contains(p.Id) && p.Ativo).ToDictionaryAsync(p => p.Id);
        if (produtos.Count != ids.Count) return BadRequest("Um dos produtos não está disponível.");
        var venda = new Venda { ClienteId = cliente?.Id, ClienteNome = cliente?.Nome ?? "Consumidor avulso", DataVenda = pedido.DataVenda, Pagamento = pedido.Pagamento,
            Itens = pedido.Itens.Select(i => new ItemVenda { ProdutoId = i.ProdutoId, ProdutoNome = produtos[i.ProdutoId].Nome, Quantidade = i.Quantidade, PrecoUnitario = produtos[i.ProdutoId].PrecoVenda, CustoUnitario = produtos[i.ProdutoId].PrecoCusto }).ToList() };
        venda.ValorTotal = venda.Total; db.Vendas.Add(venda); await db.SaveChangesAsync(); return Ok(venda);
    }
}

