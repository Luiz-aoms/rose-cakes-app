using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoseCakes.Api.Data;
using RoseCakes.Api.Models;
namespace RoseCakes.Api.Controllers;
[ApiController, Route("api/produtos")]
public class ProdutosController(AppDbContext db) : ControllerBase
{
    private static bool Valido(Produto p) => !string.IsNullOrWhiteSpace(p.Nome) && p.PrecoCusto >= 0 && p.PrecoVenda > 0 && p.PrecoCusto <= 99999999 && p.PrecoVenda <= 99999999 && !string.IsNullOrWhiteSpace(p.Categoria) && !string.IsNullOrWhiteSpace(p.UnidadeVenda) && p.Rendimento > 0 && p.Rendimento <= 10000 && ImagemProduto.Valida(p.Foto);
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await db.Produtos.AsNoTracking().OrderBy(p => p.Nome).ToListAsync());
    [HttpPost]
    public async Task<IActionResult> Criar(Produto p)
    {
        if (!Valido(p)) return BadRequest("Informe nome, categoria, preços e rendimento válidos. A foto deve ser PNG, JPEG ou WebP até 750 KB.");
        p.Id = 0; p.Nome = p.Nome.Trim(); db.Produtos.Add(p); await db.SaveChangesAsync(); return Ok(p);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, Produto p)
    {
        if (!Valido(p)) return BadRequest("Verifique nome, categoria, preços, rendimento e foto (PNG, JPEG ou WebP até 750 KB).");
        var atual = await db.Produtos.FindAsync(id); if (atual is null) return NotFound();
        atual.Foto = p.Foto; atual.UnidadeVenda = p.UnidadeVenda.Trim(); atual.Rendimento = p.Rendimento;
        atual.Nome = p.Nome.Trim(); atual.Descricao = p.Descricao; atual.Categoria = p.Categoria; atual.PrecoCusto = p.PrecoCusto; atual.PrecoVenda = p.PrecoVenda; atual.Ativo = p.Ativo;
        await db.SaveChangesAsync(); return Ok(atual);
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, ConfirmarExclusao pedido)
    {
        if (!pedido.Confirmar) return BadRequest("Confirme a exclusão definitiva.");
        var registro = await db.Produtos.FindAsync(id);
        if (registro is null) return NotFound();
        if (await db.Set<ItemVenda>().AnyAsync(i => i.ProdutoId == id)) return Conflict("Este produto faz parte do histórico de vendas. Use a opção Produto ativo para inativá-lo, preservando os registros.");
        db.Produtos.Remove(registro);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict("O produto passou a ter vínculos e não foi excluído. Atualize a lista."); }
        return NoContent();
    }
}


