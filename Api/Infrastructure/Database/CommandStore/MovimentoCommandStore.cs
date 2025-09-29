using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace Infrastructure.Database.CommandStore
{
    public class MovimentoCommandStore : IMovimentoCommandStore
    {
        private readonly IDbConnection _connection;
        public MovimentoCommandStore(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<string?> VerificarIdempotenciaAsync(Guid idRequisicao)
        {
            const string sql = @"
                    SELECT resultado 
                    FROM idempotencia 
                    WHERE chave_idempotencia = @Id";

            return await _connection.QueryFirstOrDefaultAsync<string>(sql, new { Id = idRequisicao.ToString() });
        }

        public async Task RegistrarIdempotenciaAsync(Guid idRequisicao, string movimentoId, object requisicao)
        {
            const string sql = @"
            INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
            VALUES (@Chave, @Requisicao, @Resultado);";

            await _connection.ExecuteAsync(sql, new
            {
                Chave = idRequisicao.ToString(),
                Requisicao = JsonConvert.SerializeObject(requisicao),
                Resultado = movimentoId
            });
        }

        public async Task<string> InserirMovimentoAsync(string contaId, decimal valor, string tipo)
        {
            var id = Guid.NewGuid().ToString(); // idmovimento
            var data = DateTime.Now.ToString("dd/MM/yyyy"); // datamovimento

            const string sql = @"
                        INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
                        VALUES (@Id, @ContaId, @Data, @Tipo, @Valor);";

            await _connection.ExecuteAsync(sql, new
            {
                Id = id,
                ContaId = contaId,
                Data = data,
                Tipo = tipo,
                Valor = valor
            });

            return id;
        }
    }
}