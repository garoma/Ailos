namespace Infrastructure.Services;

public interface IContaCorrenteService
{
    Task<bool> ValidarContaAtiva(string numeroConta);
    Task<bool> ContaExiste(string numeroConta);
    Task<bool> RealizarMovimentacao(string numeroConta, string idRequisicao, decimal valor);
}
