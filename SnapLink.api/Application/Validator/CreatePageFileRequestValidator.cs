using FluentValidation;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.Enum;

namespace SnapLink.api.Application.Validator
{
    public class CreatePageFileRequestValidator : AbstractValidator<CreatePageFileRequest>
    {
        private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

        public CreatePageFileRequestValidator()
        {
            RuleFor(x => x.FileName)
                .MaximumLength(55)
                .WithMessage("O nome do arquivo não pode ultrapassar 55 caracteres.");

            RuleFor(x => x.Data)
                .NotNull().WithMessage("O arquivo é obrigatório.")
                .Must(ValidarTamanhoArquivo).WithMessage("O arquivo não pode ultrapassar 50 MB.");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("O tipo de conteúdo (ContentType) é obrigatório.");

            RuleFor(x => x.PageId)
                .NotEmpty().WithMessage("O identificador da página (PageId) é obrigatório.")
                .MaximumLength(36).WithMessage("O identificador da página deve ter no máximo 36 caracteres.");

            RuleFor(x => x.TimeToExpire)
                .Must(val => Enum.IsDefined(typeof(TimeToExpire), val) && val != (int)TimeToExpire.UNDEFINED)
                .WithMessage("O tempo de expiração (TimeToExpire) deve ser um valor válido.");
        }

        private bool ValidarTamanhoArquivo(IFormFile file)
        {
            if (file == null) return false;
            return file.Length <= MaxFileSizeBytes;
        }
    }
}
