using MediatR;
using Application.Queries.Responses;

public class ConsultarSaldoRequest : IRequest<ConsultarSaldoResponse>
{
    public string NumeroConta { get; set; }

    public ConsultarSaldoRequest(string numeroConta)
    {
        NumeroConta = numeroConta;
    }
}
