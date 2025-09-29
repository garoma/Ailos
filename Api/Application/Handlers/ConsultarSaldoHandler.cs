using Application.Queries.Responses;
using Domain.Language;
using Infrastructure.Database.QueryStore;
using MediatR;

public class ConsultarSaldoHandler : IRequestHandler<ConsultarSaldoRequest, ConsultarSaldoResponse>
{
    private readonly IContaCorrenteQueryStore _queryStore;

    public ConsultarSaldoHandler(IContaCorrenteQueryStore queryStore)
    {
        _queryStore = queryStore;
    }

    public async Task<ConsultarSaldoResponse> Handle(ConsultarSaldoRequest request, CancellationToken cancellationToken)
    {
        var conta = await _queryStore.ObterPorNumeroAsync(request.NumeroConta);
        if (conta == null)
            throw new ApplicationException($"{Mensagens.ContaInvalida} | Tipo: INVALID_ACCOUNT");
        if (!conta.Ativo)
            throw new ApplicationException($"{Mensagens.ContaInativa} | Tipo: INACTIVE_ACCOUNT");

        var saldo = await _queryStore.ObterSaldoAsync(conta.IdContaCorrente);

        return new ConsultarSaldoResponse
        {
            NumeroConta = conta.Numero,
            NomeTitular = conta.Nome,
            DataConsulta = DateTime.Now,
            Saldo = saldo,
            Sucesso = true,
            Mensagem = "Saldo consultado com sucesso."
        };

    }
}
