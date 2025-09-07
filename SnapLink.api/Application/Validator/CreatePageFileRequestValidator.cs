using FluentValidation;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.Enum;

namespace SnapLink.api.Application.Validator
{
    public class CreatePageFileRequestValidator : AbstractValidator<CreatePageFileRequest>
    {
        private const long MaxFileSizeBytes = 50 * 1024 * 1024; 

        public CreatePageFileRequestValidator()
        {
            RuleFor(x => x.FileName)
                .MaximumLength(55)
                .WithMessage("The file name cannot exceed 55 characters.");

            RuleFor(x => x.Data)
                .NotNull().WithMessage("The file is required.")
                .Must(ValideSizeData).WithMessage("The file cannot exceed 50 MB.");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("ContentType is required.");

            RuleFor(x => x.PageId)
                .NotEmpty().WithMessage("PageId is required.")
                .MaximumLength(36);

            RuleFor(x => x.TimeToExpire)
                .Must(val => Enum.IsDefined(typeof(TimeToExpire), val) && val != (int)TimeToExpire.UNDEFINED)
                .WithMessage("TimeToExpire must be a valid value.");
        }

        private bool ValideSizeData(IFormFile file)
        {
            if (file == null) return false;
            return file.Length <= MaxFileSizeBytes;
        }
    }
}
