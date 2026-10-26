using Microsoft.EntityFrameworkCore;
using RoseCakes.Api.Models;
namespace RoseCakes.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Despesa> Despesas { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Produto>().Property(p => p.PrecoCusto).HasPrecision(18, 2);
        model.Entity<Produto>().Property(p => p.PrecoVenda).HasPrecision(18, 2);
        model.Entity<Despesa>().Property(p => p.Valor).HasPrecision(18, 2);
        model.Entity<ItemVenda>().ToTable("ItensVenda");
        model.Entity<Venda>().Property(v => v.ValorTotal).HasPrecision(18, 2);
        model.Entity<Venda>().Property(v => v.Pagamento).HasColumnName("MetodoPagamento");
        model.Entity<ItemVenda>().Property(p => p.PrecoUnitario).HasPrecision(18, 2);
        model.Entity<ItemVenda>().Property(p => p.CustoUnitario).HasPrecision(18, 2);
        model.Entity<Venda>().HasMany(v => v.Itens).WithOne().HasForeignKey("VendaId").IsRequired().OnDelete(DeleteBehavior.Cascade);
        model.Entity<Venda>().HasMany(v => v.Recebimentos).WithOne().HasForeignKey("VendaId").IsRequired().OnDelete(DeleteBehavior.Cascade);
        model.Entity<Recebimento>().ToTable("Recebimentos");
        model.Entity<Recebimento>().Property(r => r.Valor).HasPrecision(18, 2);
        model.Entity<Venda>().HasOne<Cliente>().WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ItemVenda>().HasOne<Produto>().WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    }
}



