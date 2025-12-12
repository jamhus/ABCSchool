using FluentValidation;

namespace Application.Features.Schools
{
    public class CreateSchoolRequest
    {
        public string Name { get; set; }
        public DateTime EstablishedDate { get; set; }
    }

    internal class CreateSchoolRequestValidator : AbstractValidator<CreateSchoolRequest>
    {
        internal CreateSchoolRequestValidator()
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
