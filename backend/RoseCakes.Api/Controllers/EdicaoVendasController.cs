using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoseCakes.Api.Data;
using RoseCakes.Api.Models;
namespace RoseCakes.Api.Controllers;

public record ItemEdicao(int? Id, int ProdutoId, int Quantidade, decimal PrecoUnitario);
public record EdicaoVenda(int? ClienteId, DateTime DataVenda, List<ItemEdicao> Itens, DateTime? DataEntrega, string? TipoEntrega, string? EnderecoEntrega, string? Observacoes);

[ApiController, Route("api/gestao/vendas")]
public class EdicaoVendasController(AppDbContext db) : ControllerBase
{
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, EdicaoVenda pedido)
    {
        if (pedido.DataVenda == default || pedido.Itens is null || pedido.Itens.Count == 0 || pedido.Itens.Count > 100 || pedido.Itens.Any(i => i.Quantidade < 1 || i.Quantidade > 10000 || i.PrecoUnitario <= 0 || i.PrecoUnitario > 99999999 || decimal.Round(i.PrecoUnitario, 2) != i.PrecoUnitario)) return BadRequest("Informe data e itens válidos, com preço positivo e até duas casas decimais.");
        var ids = pedido.Itens.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToList();
        if (ids.Count != ids.Distinct().Count()) return BadRequest("Um mesmo item não pode ser enviado duas vezes.");
        await using var transacao = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var venda = await db.Vendas.Include(v => v.Itens).Include(v => v.Recebimentos).FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();
        if (venda.CanceladoEm.HasValue) return BadRequest("Não é possível alterar um registro cancelado. Ele pode ser excluído com confirmação.");
        if (venda.DataEntrega.HasValue != pedido.DataEntrega.HasValue) return BadRequest("Mantenha o tipo original: venda imediata ou encomenda.");
        if (pedido.DataEntrega.HasValue && (!pedido.ClienteId.HasValue || !new[] { "Retirada", "Entrega" }.Contains(pedido.TipoEntrega) || (pedido.TipoEntrega == "Entrega" && string.IsNullOrWhiteSpace(pedido.EnderecoEntrega)))) return BadRequest("Informe cliente, prazo e modalidade; entregas precisam de endereço.");
        var cliente = pedido.ClienteId.HasValue ? await db.Clientes.FindAsync(pedido.ClienteId.Value) : null;
        if (pedido.ClienteId.HasValue && cliente is null) return BadRequest("Cliente não encontrado.");
        var existentes = venda.Itens.ToDictionary(i => i.Id);
        if (pedido.Itens.Any(i => i.Id.HasValue && (!existentes.TryGetValue(i.Id.Value, out var original) || original.ProdutoId != i.ProdutoId))) return BadRequest("Um item não pertence a esta venda. Para trocar o produto, remova o item e adicione outro.");
        var novosIds = pedido.Itens.Where(i => !i.Id.HasValue).Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos.Where(p => novosIds.Contains(p.Id) && p.Ativo).ToDictionaryAsync(p => p.Id);
        if (produtos.Count != novosIds.Count) return BadRequest("Um produto adicionado não está ativo ou não existe.");
        var total = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
        if (total < venda.Recebido) return BadRequest("O novo total é menor que o valor já recebido. Corrija os recebimentos lançados por engano antes de alterar a venda.");
        foreach (var removido in venda.Itens.Where(i => !ids.Contains(i.Id)).ToList()) { db.Remove(removido); venda.Itens.Remove(removido); }
        foreach (var i in pedido.Itens)
        {
            if (i.Id.HasValue)
            {
                var atual = existentes[i.Id.Value];
                atual.Quantidade = i.Quantidade;
                atual.PrecoUnitario = i.PrecoUnitario;
                // Nome e custo histórico não mudam ao editar quantidade ou preço.
            }
            else venda.Itens.Add(new ItemVenda { ProdutoId = i.ProdutoId, ProdutoNome = produtos[i.ProdutoId].Nome, Quantidade = i.Quantidade, PrecoUnitario = i.PrecoUnitario, CustoUnitario = produtos[i.ProdutoId].PrecoCusto });
        }
        if (venda.ClienteId != pedido.ClienteId) venda.ClienteNome = cliente?.Nome ?? "Consumidor avulso";
        venda.ClienteId = pedido.ClienteId;
        venda.DataVenda = pedido.DataVenda;
        venda.DataEntrega = pedido.DataEntrega;
        venda.TipoEntrega = pedido.TipoEntrega;
        venda.EnderecoEntrega = pedido.TipoEntrega == "Entrega" ? pedido.EnderecoEntrega?.Trim() : null;
        venda.Observacoes = pedido.Observacoes?.Trim();
        venda.ValorTotal = total;
        await db.SaveChangesAsync();
        await transacao.CommitAsync();
        return Ok(venda);
    }
}
