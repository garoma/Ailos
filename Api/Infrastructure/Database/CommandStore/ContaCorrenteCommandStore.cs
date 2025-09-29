using Dapper;
using Domain.Entities;
using System.Data;

public class ContaCorrenteCommandStore : IContaCorrenteCommandStore
{
    private readonly IDbConnection _connection;

    public ContaCorrenteCommandStore(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task CriarAsync(ContaCorrente conta)
    {
        var sql = @"
            INSERT INTO contacorrente (
                idcontacorrente,
                numero,
                nome,
                cpf,
                ativo,
                senha,
                salt
            ) VALUES (
                @IdContaCorrente,
                @Numero,
                @Nome,
                @Cpf,
                @Ativo,
                @Senha,
                @Salt
            );";

        await _connection.ExecuteAsync(sql, conta);
    }

    public async Task AtualizarStatusAsync(string idContaCorrente, bool ativo)
    {
        var sql = @"UPDATE contacorrente SET ativo = @Ativo WHERE idcontacorrente = @IdContaCorrente";
        await _connection.ExecuteAsync(sql, new { IdContaCorrente = idContaCorrente, Ativo = ativo });
    }

    public async Task InativarContaAsync(string numero)
    {
        var sql = "UPDATE contacorrente SET ativo = 0 WHERE numero = @Numero";
        await _connection.ExecuteAsync(sql, new { Numero = numero });
    }
}
