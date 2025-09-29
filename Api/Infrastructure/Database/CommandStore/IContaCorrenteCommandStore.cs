using Domain.Entities;

public interface IContaCorrenteCommandStore
{
    Task CriarAsync(ContaCorrente conta);
    Task AtualizarStatusAsync(string idContaCorrente, bool ativo);
    Task InativarContaAsync(string numero);
}
