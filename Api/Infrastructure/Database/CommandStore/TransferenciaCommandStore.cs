using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Database.CommandStore
{
    public class TransferenciaCommandStore : ITransferenciaCommandStore
    {
        private readonly IDbConnection _connection;

        public TransferenciaCommandStore(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<string> RegistrarTransferenciaAsync(string contaOrigem, string contaDestino, decimal valor, Guid idRequisicao)
        {
            var id = Guid.NewGuid().ToString();

            var sql = @"INSERT INTO transferencia (id, identificacao_requisicao, conta_origem, conta_destino, valor, datahora)
                        VALUES (@Id, @IdRequisicao, @ContaOrigem, @ContaDestino, @Valor, @DataHora);";

            await _connection.ExecuteAsync(sql, new
            {
                Id = id,
                IdRequisicao = idRequisicao.ToString(),
                ContaOrigem = contaOrigem,
                ContaDestino = contaDestino,
                Valor = valor,
                DataHora = DateTime.UtcNow
            });

            return id;
        }

        public async Task<string?> VerificarIdempotenciaAsync(Guid idRequisicao)
        {
            const string sql = @"
                    SELECT resultado 
                    FROM idempotencia 
                    WHERE chave_idempotencia = @Id";

            return await _connection.QueryFirstOrDefaultAsync<string>(sql, new { Id = idRequisicao.ToString() });
        }
      
        public Task RegistrarIdempotenciaAsync(Guid idRequisicao, string transferenciaId, object request)
        {
            // Como já salvamos o registro na transferência, não precisa fazer nada extra aqui
            return Task.CompletedTask;
        }
    }
}
