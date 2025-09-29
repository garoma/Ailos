using System;
using System.Threading.Tasks;

namespace Infrastructure.Database.CommandStore
{
    public interface ITransferenciaCommandStore
    {
        Task<string> RegistrarTransferenciaAsync(string contaOrigem, string contaDestino, decimal valor, Guid idRequisicao);
        Task<string> VerificarIdempotenciaAsync(Guid idRequisicao);
        Task RegistrarIdempotenciaAsync(Guid idRequisicao, string transferenciaId, object request);
    }
}
