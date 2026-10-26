using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoseCakes.Api.Data;
using RoseCakes.Api.Models;
namespace RoseCakes.Api.Controllers;
[ApiController, Route("api/produtos")]
public class ProdutosController(AppDbContext db) : ControllerBase
{
    private static bool Valido(Produto p) => !string.IsNullOrWhiteSpace(p.Nome) && p.PrecoCusto >= 0 && p.PrecoVenda > 0 && p.PrecoCusto <= 99999999 && p.PrecoVenda <= 99999999;
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await db.Produtos.AsNoTracking().OrderBy(p => p.Nome).ToListAsync());
    [HttpPost]
    public async Task<IActionResult> Criar(Produto p)
    {
        if (!Valido(p)) return BadRequest("Informe nome, custo não negativo e preço de venda positivo.");
        p.Id = 0; p.Nome = p.Nome.Trim(); db.Produtos.Add(p); await db.SaveChangesAsync(); return Ok(p);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, Produto p)
    {
        if (!Valido(p)) return BadRequest("Verifique nome e preços.");
        var atual = await db.Produtos.FindAsync(id); if (atual is null) return NotFound();
        atual.Nome = p.Nome.Trim(); atual.Descricao = p.Descricao; atual.Categoria = p.Categoria; atual.PrecoCusto = p.PrecoCusto; atual.PrecoVenda = p.PrecoVenda; atual.Ativo = p.Ativo;
        await db.SaveChangesAsync(); return Ok(atual);
    }
}
