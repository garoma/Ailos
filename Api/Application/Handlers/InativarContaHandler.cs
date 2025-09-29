using Application.Requests;
using FluentValidation;
using Infrastructure.Database.CommandStore;
using Infrastructure.Database.QueryStore;
using MediatR;

namespace Application.Handlers;

public class InativarContaHandler : IRequestHandler<InativarContaRequest, Unit>
{
    private readonly IContaCorrenteQueryStore _queryStore;
    private readonly IContaCorrenteCommandStore _commandStore;
    private readonly IValidator<InativarContaRequest> _validator;

    public InativarContaHandler(
        IContaCorrenteQueryStore queryStore,
        IContaCorrenteCommandStore commandStore,
        IValidator<InativarContaRequest> validator)
    {
        _queryStore = queryStore;
        _commandStore = commandStore;
        _validator = validator;
    }

    public async Task<Unit> Handle(InativarContaRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new Exception(validation.Errors.First().ErrorMessage);

        var conta = await _queryStore.ObterPorNumeroAsync(request.NumeroConta);
        if (conta == null)
            throw new Exception("INVALID_ACCOUNT: Conta corrente não cadastrada.");

        if (conta.Senha != request.Senha)  // Ajuste conforme hash/salting se usar
            throw new Exception("INVALID_PASSWORD: Senha incorreta.");

        await _commandStore.InativarContaAsync(conta.Numero);

        return Unit.Value;
    }
}
