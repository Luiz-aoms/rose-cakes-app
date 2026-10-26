namespace RoseCakes.Api.Models
{
    public class Despesa
    {
        public int Id { get; set; }
        public string? MotivoCancelamento { get; set; }
        public DateTime? CanceladoEm { get; set; }
        public required string Descricao { get; set; }
        public DateTime DataDespesa { get; set; }
        public decimal Valor { get; set; }
        public string? Categoria { get; set; }
    }
}
