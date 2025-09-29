using Application.Commands.Requests;
using Application.Commands.Responses;
using Application.Queries.Responses;
using Application.Requests;
using Infrastructure.Database.QueryStore;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Database.Services.Controllers
{
    public static class ContaCorrenteController
    {
        public static void MapCriarContaEndpoint(this WebApplication app)
        {
            app.MapPost("/conta", async (
                [FromBody] CriarContaRequest command,
                IMediator mediator) =>
            {
                try
                {
                    var resultado = await mediator.Send(command);

                    return Results.Ok(new
                    {
                        NumeroConta = resultado.NumeroConta,
                        Nome = resultado.Nome
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new
                    {
                        erro = ex.Message,
                        tipo = "CpfDuplicado"
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        detail: ex.Message,
                        title: "Erro interno",
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization()
            .WithName("CriarConta")
            .WithTags("Conta Corrente")
            .WithMetadata(new SwaggerOperationAttribute(
                summary: "Cadastra uma nova conta corrente",
                description: "Cria uma conta corrente com nome, CPF e senha criptografada"
            ))
            .Accepts<CriarContaRequest>("application/json")
            .Produces(200)
            .Produces(400)
            .Produces(500);
        }
        
        public static void MapMovimentarContaEndpoint(this WebApplication app)
        {
            app.MapPost("/conta/movimentar", async (
                //string numero, 
                MovimentarContaRequest body,
                //[FromHeader(Name = "Idempotency-Key")] Guid idRequisicao,
                IMediator mediator) =>
            {
                //body.Numero = numero;
                //body.IdRequisicao = idRequisicao; // injeta no request
                var result = await mediator.Send(body);
                return Results.Ok(result);
            })
            .WithName("MovimentarConta")
            .WithTags("Conta Corrente")
            .WithMetadata(new SwaggerOperationAttribute(
                summary: "Realiza movimentação de crédito ou débito na conta",
                description: "Movimenta o saldo da conta corrente com suporte a idempotência"
            ))
            .RequireAuthorization()
            .Produces<MovimentarContaResponse>(200)
            .Produces(400);
        }

        public static void MapConsultarSaldoEndpoint(this WebApplication app)
        {
            app.MapGet("/conta/{Numero}/saldo", async (string numero, IMediator mediator) =>
            {
                var response = await mediator.Send(new ConsultarSaldoRequest(numero));
                return Results.Ok(response);
            })
            .WithName("ConsultarSaldo")
            .WithTags("Conta Corrente")
            .WithMetadata(new SwaggerOperationAttribute(
                summary: "Consulta o saldo atual da conta",
                description: "Retorna o valor atual do saldo com nome do titular e horário da consulta"
            ))
            .RequireAuthorization()
            .Produces<ConsultarSaldoResponse>(200)
            .Produces(400);
        }

        public static void MapTransferenciaEndpoints(this WebApplication app)
        {
            app.MapPost("/api/transferencias", async (
                EfetuarTransferenciaRequest request,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                try
                {
                    // Pega a conta origem a partir do token (Claim "sub" ou "nameidentifier")
                    var contaOrigem = httpContext.User.FindFirst("sub")?.Value
                                    ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(contaOrigem))
                        return Results.Forbid();

                    request.ContaOrigem = contaOrigem;

                    await mediator.Send(request);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    var tipoFalha = Enum.GetNames(typeof(Domain.Enums.TipoFalhaTransferencia))
                        .FirstOrDefault(x => ex.Message.StartsWith(x));

                    var mensagem = tipoFalha != null
                        ? ex.Message.Substring(tipoFalha.Length + 2)
                        : ex.Message;

                    return Results.BadRequest(new
                    {
                        tipo = tipoFalha ?? "UNKNOWN",
                        mensagem
                    });
                }

            })
            .RequireAuthorization()
            .WithName("EfetuarTransferencia")
            .WithTags("Transferência");
        }

        public static void MapInativarContaEndpoints(this WebApplication app)
        {
            app.MapPost("/api/contas/inativar", async (
                InativarContaRequest request,
                IMediator mediator) =>
            {
                try
                {
                    await mediator.Send(request);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message.Contains(":") ? ex.Message.Split(":")[1].Trim() : ex.Message;
                    var tipo = ex.Message.Contains(":") ? ex.Message.Split(":")[0] : "UNKNOWN";

                    return Results.BadRequest(new { tipo, mensagem = msg });
                }
            })
            .RequireAuthorization()
            .WithTags("Conta Corrente")
            .WithName("InativarConta")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest, typeof(object));
        }

        // Program.cs ou AuthEndpoints.cs
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/login", async (
                Application.Requests.LoginRequest request,
                IConfiguration config,
                IContaCorrenteQueryStore queryStore) =>
            {
                var conta = await queryStore.ObterPorCpfAsync(request.Cpf);
                if (conta == null)
                    return Results.BadRequest(new { tipo = "INVALID_CPF", mensagem = "CPF não encontrado." });

                if (conta.Senha != request.Senha) // substitua por hash/secure compare se necessário
                    return Results.BadRequest(new { tipo = "INVALID_PASSWORD", mensagem = "Senha incorreta." });

                var chaveJwt = config["Jwt:Key"];
                var token = TokenService.GerarToken(conta.Numero, conta.Cpf, chaveJwt);

                return Results.Ok(new { token });
            })
            .AllowAnonymous()
            .WithTags("Autenticação")
            .WithName("Login")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}