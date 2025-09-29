using Application.Commands.Requests;
using Application.Commands.Responses;
using Domain.Entities;
using Infrastructure.Database.CommandStore;
using Infrastructure.Database.QueryStore;
using MediatR;
using System.Security.Cryptography;
using System.Text;

public class CriarContaHandler : IRequestHandler<CriarContaRequest, CriarContaResponse>
{
    private readonly IContaCorrenteCommandStore _commandStore;
    private readonly IContaCorrenteQueryStore _queryStore;

    public CriarContaHandler(IContaCorrenteCommandStore commandStore, IContaCorrenteQueryStore queryStore)
    {
        _commandStore = commandStore;
        _queryStore = queryStore;
    }

    public async Task<CriarContaResponse> Handle(CriarContaRequest request, CancellationToken cancellationToken)
    {
        // Verifica se CPF já existe
        var contaExistente = await _queryStore.ObterPorCpfAsync(request.Cpf);
        if (contaExistente != null)
            throw new InvalidOperationException("Já existe uma conta cadastrada com este CPF.");

        // Gera número da conta (exemplo: aleatório de 6 dígitos)
        var numeroConta = new Random().Next(100000, 999999).ToString();

        // Gera salt e hash
        var salt = Guid.NewGuid().ToString();
        var senhaHash = HashSenha(request.Senha, salt);

        var conta = new ContaCorrente
        {
            IdContaCorrente = Guid.NewGuid().ToString(),
            Numero = numeroConta,
            Nome = request.Nome,
            Cpf = request.Cpf,
            Senha = senhaHash,
            Salt = salt,
            Ativo = true
        };

        await _commandStore.CriarAsync(conta);

        return new CriarContaResponse
        {
            NumeroConta = numeroConta,
            Nome = conta.Nome
        };
    }

    private string HashSenha(string senha, string salt)
    {
        var salted = senha + salt;
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(salted));
        return Convert.ToBase64String(hashBytes);
    }
}
