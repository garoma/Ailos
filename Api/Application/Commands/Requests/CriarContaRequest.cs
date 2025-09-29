using MediatR;
using Application.Commands.Responses;

public class CriarContaRequest : IRequest<CriarContaResponse>
{
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Senha { get; set; }
}
