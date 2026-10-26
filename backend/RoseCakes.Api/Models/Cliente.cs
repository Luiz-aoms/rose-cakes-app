namespace RoseCakes.Api.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Telefone { get; set; }
        public string? Endereco { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Observacoes { get; set; }
    }
}