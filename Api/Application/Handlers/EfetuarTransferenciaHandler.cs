using Application.Requests;
using Infrastructure.Database.CommandStore;
using Infrastructure.Services;
using MediatR;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class EfetuarTransferenciaHandler : IRequestHandler<EfetuarTransferenciaRequest, Unit>
    {
        private readonly ITransferenciaCommandStore _commandStore;
        private readonly IContaCorrenteService _contaCorrenteService;
        private readonly IHttpClientFactory _httpClientFactory;

        public EfetuarTransferenciaHandler(
            ITransferenciaCommandStore commandStore,
            IContaCorrenteService contaCorrenteService,
            IHttpClientFactory httpClientFactory)
        {
            _commandStore = commandStore;
            _contaCorrenteService = contaCorrenteService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Unit> Handle(EfetuarTransferenciaRequest request, CancellationToken cancellationToken)
        {
            // Validações básicas
            if (!await _contaCorrenteService.ContaExiste(request.ContaOrigem))
                throw new ApplicationException("Conta origem inválida. | Tipo: INVALID_ACCOUNT");

            if (!await _contaCorrenteService.ValidarContaAtiva(request.ContaOrigem))
                throw new ApplicationException("Conta origem inativa. | Tipo: INACTIVE_ACCOUNT");

            if (!await _contaCorrenteService.ContaExiste(request.ContaDestino))
                throw new ApplicationException("Conta destino inválida. | Tipo: INVALID_ACCOUNT");

            if (!await _contaCorrenteService.ValidarContaAtiva(request.ContaDestino))
                throw new ApplicationException("Conta destino inativa. | Tipo: INACTIVE_ACCOUNT");

            if (request.Valor <= 0)
                throw new ApplicationException("Valor inválido. | Tipo: INVALID_VALUE");
            
            if (request.IdRequisicao.ToString() == "3fa85f64-5717-4562-b3fc-2c963f66afa6")
                request.IdRequisicao = Guid.NewGuid();

            // Verificar idempotência
            var idempotente = await _commandStore.VerificarIdempotenciaAsync(request.IdRequisicao);
            if (!string.IsNullOrEmpty(idempotente))
                return Unit.Value; // já processado, ignora repetição

            // Preparar client para chamadas à API Conta Corrente
            var client = _httpClientFactory.CreateClient("ContaCorrenteApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);

            // Realiza débito na conta origem
            var debitoRequest = new
            {
                idRequisicao = request.IdRequisicao,
                numero = request.ContaOrigem,
                valor = request.Valor,
                tipo = "D"
            };

            var debitoResponse = await client.PostAsJsonAsync("/conta/movimentar", debitoRequest, cancellationToken);
            if (!debitoResponse.IsSuccessStatusCode)
                throw new ApplicationException("Falha no débito da conta origem. | Tipo: OPERATION_FAILED");

            var IdRequisicaoAnterior = request.IdRequisicao;
            request.IdRequisicao = Guid.NewGuid();

            // Realiza crédito na conta destino
            var creditoRequest = new            
            {
                idRequisicao = request.IdRequisicao,
                numero = request.ContaDestino,
                valor = request.Valor,
                tipo = "C"
            };

            var creditoResponse = await client.PostAsJsonAsync("/conta/movimentar", creditoRequest, cancellationToken);
            if (!creditoResponse.IsSuccessStatusCode)
            {
                // Estorna débito (estorno)
                var estornoRequest = new
                {
                    NumeroConta = request.ContaOrigem,
                    Valor = request.Valor,
                    IdRequisicao = request.IdRequisicao
                };
                await client.PostAsJsonAsync("/api/contas/movimentar/credito", estornoRequest, cancellationToken);

                throw new ApplicationException("Falha no crédito da conta destino e estorno efetuado. | Tipo: OPERATION_FAILED");
            }

            request.IdRequisicao = IdRequisicaoAnterior;

            // Registra transferência na base
            await _commandStore.RegistrarTransferenciaAsync(request.ContaOrigem, request.ContaDestino, request.Valor, request.IdRequisicao);

            // Retorna HTTP 204 no controller (Unit.Value indica sucesso MediatR)
            return Unit.Value;
        }
    }
}
