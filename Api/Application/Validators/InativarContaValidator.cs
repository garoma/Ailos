using Application.Requests;
using FluentValidation;

namespace Application.Validators;

public class InativarContaValidator : AbstractValidator<InativarContaRequest>
{
    public InativarContaValidator()
    {
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
