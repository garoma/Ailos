using MediatR;

namespace Application.Requests;

public class InativarContaRequest : IRequest<Unit>
{
    public string Senha { get; set; } = null!;
    public string NumeroConta { get; set; } = null!; // será preenchido pelo token no handler/endpoint
}
