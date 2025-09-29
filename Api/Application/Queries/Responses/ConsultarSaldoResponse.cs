namespace Application.Queries.Responses
{
    public class ConsultarSaldoResponse
    {
        public string ContaCorrenteId { get; set; }
        public string NumeroConta { get; set; }
        public string NomeTitular { get; set; }
        public DateTime DataConsulta { get; set; }
        public decimal Saldo { get; set; }
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
    }
}