namespace Domain.Entities;

public class Transferencia
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IdRequisicao { get; set; } = null!;
    public string ContaOrigem { get; set; } = null!;
    public string ContaDestino { get; set; } = null!;
    public decimal Valor { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
