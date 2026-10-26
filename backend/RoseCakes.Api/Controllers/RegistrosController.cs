using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoseCakes.Api.Data;
using RoseCakes.Api.Models;
namespace RoseCakes.Api.Controllers;

public record ConfirmarExclusao(bool Confirmar);

[ApiController, Route("api/gestao")]
public class RegistrosController(AppDbContext db) : ControllerBase
{
    [HttpDelete("despesas/{id:int}")]
    public async Task<IActionResult> ExcluirDespesa(int id, ConfirmarExclusao pedido)
    {
        if (!pedido.Confirmar) return BadRequest("Confirme a exclusão definitiva.");
        var registro = await db.Despesas.FindAsync(id);
        if (registro is null) return NotFound();
        db.Despesas.Remove(registro);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("vendas/{id:int}")]
    public async Task<IActionResult> ExcluirVenda(int id, ConfirmarExclusao pedido)
    {
        if (!pedido.Confirmar) return BadRequest("Confirme a exclusão definitiva da venda, itens e recebimentos.");
        await using var transacao = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var registro = await db.Vendas.Include(v => v.Itens).Include(v => v.Recebimentos).FirstOrDefaultAsync(v => v.Id == id);
        if (registro is null) return NotFound();
        db.Vendas.Remove(registro);
        await db.SaveChangesAsync();
        await transacao.CommitAsync();
        return NoContent();
    }

    [HttpDelete("clientes/{id:int}")]
    public async Task<IActionResult> ExcluirCliente(int id, ConfirmarExclusao pedido)
    {
        if (!pedido.Confirmar) return BadRequest("Confirme a exclusão definitiva.");
        var registro = await db.Clientes.FindAsync(id);
        if (registro is null) return NotFound();
        if (await db.Vendas.AnyAsync(v => v.ClienteId == id)) return Conflict("Este cliente possui vendas ou pedidos no histórico. Não é possível excluí-lo enquanto houver vínculos.");
        db.Clientes.Remove(registro);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict("O cliente passou a ter vínculos e não foi excluído. Atualize a lista."); }
        return NoContent();
    }

    [HttpPut("vendas/{id:int}/recebimentos/{recebimentoId:int}")]
    public async Task<IActionResult> EditarRecebimento(int id, int recebimentoId, NovoRecebimento pedido)
    {
        if (pedido.Valor <= 0 || decimal.Round(pedido.Valor, 2) != pedido.Valor || pedido.DataPagamento == default || !new[] { "Pix", "Dinheiro", "Cartão" }.Contains(pedido.Forma)) return BadRequest("Informe valor positivo com até duas casas decimais, data e forma de pagamento.");
        await using var transacao = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var venda = await db.Vendas.Include(v => v.Itens).Include(v => v.Recebimentos).FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();
        if (venda.CanceladoEm.HasValue) return BadRequest("Não é possível alterar recebimentos de uma venda cancelada.");
        var registro = venda.Recebimentos.FirstOrDefault(r => r.Id == recebimentoId);
        if (registro is null) return NotFound();
        if (venda.Recebido - registro.Valor + pedido.Valor > venda.Total) return BadRequest("A alteração supera o total da venda. Confira os outros recebimentos.");
        registro.Valor = pedido.Valor;
        registro.DataPagamento = pedido.DataPagamento;
        registro.Forma = pedido.Forma;
        await db.SaveChangesAsync();
        await transacao.CommitAsync();
        return Ok(venda);
    }

    [HttpDelete("vendas/{id:int}/recebimentos/{recebimentoId:int}")]
    public async Task<IActionResult> ExcluirRecebimento(int id, int recebimentoId, ConfirmarExclusao pedido)
    {
        if (!pedido.Confirmar) return BadRequest("Confirme a exclusão definitiva do recebimento.");
        await using var transacao = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var venda = await db.Vendas.Include(v => v.Recebimentos).FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();
        if (venda.CanceladoEm.HasValue) return BadRequest("Não é possível alterar recebimentos de uma venda cancelada.");
        var registro = venda.Recebimentos.FirstOrDefault(r => r.Id == recebimentoId);
        if (registro is null) return NotFound();
        db.Remove(registro);
        await db.SaveChangesAsync();
        await transacao.CommitAsync();
        return NoContent();
    }
}
