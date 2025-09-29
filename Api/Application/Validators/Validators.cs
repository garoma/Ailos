using FluentValidation;
using Application.Requests;

namespace Api.Application.Validators;

public class EfetuarTransferenciaValidator : AbstractValidator<EfetuarTransferenciaRequest>
{
    public EfetuarTransferenciaValidator()
    {
        RuleFor(x => x.IdRequisicao)
            .NotEmpty().WithMessage("Identificação da requisição é obrigatória.");

        RuleFor(x => x.ContaDestino)
            .NotEmpty().WithMessage("Número da conta de destino é obrigatório.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor da transferência deve ser positivo.");
    }
}
