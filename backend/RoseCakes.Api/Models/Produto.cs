namespace RoseCakes.Api.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        public string? Categoria { get; set; }
        public bool Ativo { get; set; } = true;
    }
}