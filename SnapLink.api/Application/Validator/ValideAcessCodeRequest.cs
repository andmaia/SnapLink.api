using FluentValidation;
using SnapLink.api.Crosscutting.DTO.Request;

namespace SnapLink.api.Application.Validator
{
    public class ValideAcessCodeRequestValidator :AbstractValidator<ValideAcessCodeRequest>
    {
        public ValideAcessCodeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Page name is required")
                .MaximumLength(30).WithMessage("Page name cannot be longer than 100 characters");

           
                RuleFor(x => x.AccessCode)
                    .NotEmpty().WithMessage("Password is required for private pages")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                    .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                    .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                    .Matches(@"\d").WithMessage("Password must contain at least one number");
     
        }
    }
}
