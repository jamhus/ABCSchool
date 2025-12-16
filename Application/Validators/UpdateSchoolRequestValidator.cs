using Application.Features.Schools;
using Domain.Entities;
using FluentValidation;
using Shared.RequestModels.Schools;


namespace Application.Validators
{
    public class UpdateSchoolRequestValidator : AbstractValidator<UpdateSchoolRequest>
    {
        private readonly ISchoolService _schoolService;

        public UpdateSchoolRequestValidator(ISchoolService schoolService)
        {
            _schoolService = schoolService;

            RuleFor(r => r.Id)
                .NotEmpty()
                .MustAsync(async (id, ct) => await _schoolService.GetByIdAsync(id) is School school && school.Id == id)
                    .WithMessage("School does not exist");

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
