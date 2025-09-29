using Infrastructure.Database.CommandStore;
using Infrastructure.Database.QueryStore;
using Infrastructure.Services;
using Domain.Entities;
using Domain.Language;

namespace Infrastructure.Services
{
    public class ContaCorrenteService : IContaCorrenteService
    {
        private readonly IContaCorrenteQueryStore _queryStore;
        private readonly IMovimentoCommandStore _commandStore;

        public ContaCorrenteService(
            IContaCorrenteQueryStore queryStore,
            IMovimentoCommandStore commandStore)
        {
            _queryStore = queryStore;
            _commandStore = commandStore;
        }

        public async Task<bool> ContaExiste(string numeroConta)
        {
            var conta = await _queryStore.ObterPorNumeroAsync(numeroConta);
            return conta != null;
        }

        public async Task<bool> ValidarContaAtiva(string numeroConta)
        {
            var conta = await _queryStore.ObterPorNumeroAsync(numeroConta);
            return conta != null && conta.Ativo;
        }

        public async Task<bool> RealizarMovimentacao(string numeroConta, string idRequisicao, decimal valor)
        {
            if (valor <= 0)
                throw new ApplicationException($"{Mensagens.ValorInvalido} | Tipo: INVALID_VALUE");

            var conta = await _queryStore.ObterPorNumeroAsync(numeroConta);
            if (conta == null)
                throw new ApplicationException($"{Mensagens.ContaInvalida} | Tipo: INVALID_ACCOUNT");

            if (!conta.Ativo)
                throw new ApplicationException($"{Mensagens.ContaInativa} | Tipo: INACTIVE_ACCOUNT");

            var idempotente = await _commandStore.VerificarIdempotenciaAsync(Guid.Parse(idRequisicao));
            if (!string.IsNullOrEmpty(idempotente))
                return true; // já processado com sucesso

            // se valor negativo, assume débito. Se positivo, crédito
            var tipo = valor >= 0 ? "C" : "D";

            // regra: para débito, verifica saldo
            if (tipo == "D")
            {
                var saldo = await _queryStore.ObterSaldoAsync(conta.IdContaCorrente);
                if (saldo < Math.Abs(valor))
                    throw new ApplicationException($"{Mensagens.SaldoInsuficiente} | Tipo: INSUFFICIENT_FUNDS");
            }

            // executa movimentação (armazenando valor positivo)
            var movimentoId = await _commandStore.InserirMovimentoAsync(
                conta.IdContaCorrente,
                Math.Abs(valor),
                tipo
            );

            // registra idempotência
            await _commandStore.RegistrarIdempotenciaAsync(
                Guid.Parse(idRequisicao),
                movimentoId,
                new { numeroConta, valor }
            );

            return true;
        }
    }
}
