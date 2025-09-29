namespace Domain.Entities
{
    public class ContaCorrente
    {
        public string IdContaCorrente { get; set; } = Guid.NewGuid().ToString();
        public string Numero { get; set; } = Guid.NewGuid().ToString();
        public string Nome { get; set; }
        public bool Ativo { get; set; }
        public string Senha { get; set; }
        public string Cpf { get; set; }
        public string Salt { get; set; }
    }
}
