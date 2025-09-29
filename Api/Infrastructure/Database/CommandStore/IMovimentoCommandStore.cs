using System;
using System.Threading.Tasks;

namespace Infrastructure.Database.CommandStore
{
    public interface IMovimentoCommandStore
    {
        Task<string> InserirMovimentoAsync(string contaId, decimal valor, string tipo);
        Task<string?> VerificarIdempotenciaAsync(Guid idRequisicao);
        Task RegistrarIdempotenciaAsync(Guid idRequisicao, string movimentoId, object requisicao);
    }
}
