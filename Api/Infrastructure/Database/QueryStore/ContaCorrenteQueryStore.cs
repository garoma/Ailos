using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Database.QueryStore
{
    public class ContaCorrenteQueryStore : IContaCorrenteQueryStore
    {
        private readonly IDbConnection _connection;

        public ContaCorrenteQueryStore(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ContaCorrente?> ObterPorCpfAsync(string cpf)
        {
            var sql = "SELECT * FROM contacorrente WHERE cpf = @Cpf";
            return await _connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { Cpf = cpf });
        }

        public async Task<ContaCorrente?> ObterPorNumeroAsync(string numConta)
        {
            const string sql = "SELECT * FROM ContaCorrente WHERE numero = @Numero";
            return await _connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { Numero = numConta });
        }

        public async Task<decimal> ObterSaldoAsync(string id)
        {
            const string sql = @"
                        SELECT
                            IFNULL(SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE 0 END), 0) -
                            IFNULL(SUM(CASE WHEN tipomovimento = 'D' THEN valor ELSE 0 END), 0)
                        FROM movimento
                        WHERE idcontacorrente = @Id";

            var resultado = await _connection.ExecuteScalarAsync<decimal?>(sql, new { Id = id });
            return resultado ?? 0m;
        }
    }
}