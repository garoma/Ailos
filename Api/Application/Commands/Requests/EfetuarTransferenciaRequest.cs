using MediatR;

namespace Application.Requests
{
    public class EfetuarTransferenciaRequest : IRequest<Unit>
    {
        public Guid IdRequisicao { get; set; }
        public string ContaOrigem { get; set; }
        public string ContaDestino { get; set; }
        public decimal Valor { get; set; }
        public string Token { get; set; }  // opcional, pode ser passado separadamente no header da API
    }
}
