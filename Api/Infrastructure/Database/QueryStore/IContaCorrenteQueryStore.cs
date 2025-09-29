using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.Database.QueryStore
{
    public interface IContaCorrenteQueryStore
    {
        Task<ContaCorrente?> ObterPorNumeroAsync(string id);
        Task<decimal> ObterSaldoAsync(string id);
        Task<ContaCorrente?> ObterPorCpfAsync(string cpf);
    }
}
