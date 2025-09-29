using FluentValidation;
using SnapLink.api.Crosscutting.DTO.Request;

namespace SnapLink.api.Application.Validator
{
    public class ValideAcessCodeRequestValidator : AbstractValidator<ValideAcessCodeRequest>
    {
        public ValideAcessCodeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome da página é obrigatório.")
                .MaximumLength(30).WithMessage("O nome da página não pode ter mais que 30 caracteres.");

            RuleFor(x => x.AccessCode)
                .NotEmpty().WithMessage("A senha é obrigatória para páginas privadas.")
                .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
                .Matches("[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
                .Matches("[a-z]").WithMessage("A senha deve conter pelo menos uma letra minúscula.")
                .Matches(@"\d").WithMessage("A senha deve conter pelo menos um número.");
        }
    }
}
