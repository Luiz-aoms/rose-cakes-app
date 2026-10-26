
namespace RoseCakes.Api.Models;
public class Venda
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public string ClienteNome { get; set; } = "Consumidor avulso";
    public DateTime DataVenda { get; set; }
    public string Pagamento { get; set; } = "Pix";
    public DateTime? DataEntrega { get; set; }
    public string Etapa { get; set; } = "Entregue";
    public string? TipoEntrega { get; set; }
    public string? EnderecoEntrega { get; set; }
    public string? Observacoes { get; set; }
    public List<ItemVenda> Itens { get; set; } = [];
    public decimal ValorTotal { get; set; }
    public string? MotivoCancelamento { get; set; }
    public DateTime? CanceladoEm { get; set; }
    public decimal Total => Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
    public decimal Custo => Itens.Sum(i => i.Quantidade * i.CustoUnitario);
}
public class ItemVenda
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal CustoUnitario { get; set; }
}
public record ItemPedido(int ProdutoId, int Quantidade);
public record NovaVenda(int? ClienteId, DateTime DataVenda, string Pagamento, List<ItemPedido> Itens,
    DateTime? DataEntrega = null, string? TipoEntrega = null, string? EnderecoEntrega = null, string? Observacoes = null);
public record AlterarEtapa(string Etapa);

public record Cancelamento(string Motivo);
