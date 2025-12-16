using FluentValidation;
using Shared.RequestModels.Schools;

namespace Application.Validators
{
    public class CreateSchoolRequestValidator : AbstractValidator<CreateSchoolRequest>
    {
        public CreateSchoolRequestValidator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                    .WithMessage("School name is required")
                .MaximumLength(60);

            RuleFor(r => r.EstablishedDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage("Established date can not be future date ");
        }
    }
}
