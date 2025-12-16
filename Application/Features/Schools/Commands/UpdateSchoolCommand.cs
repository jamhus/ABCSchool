using Application.Pipelines;
using Shared.Wrappers;
using FluentValidation;
using MediatR;
using Shared.RequestModels.Schools;
using Application.Validators;

namespace Application.Features.Schools.Commands
{
    public class UpdateSchoolCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public UpdateSchoolRequest UpdateSchool { get; set; }
    }

    public class UpdateSchoolCommandHandler(ISchoolService schoolService) : IRequestHandler<UpdateSchoolCommand, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService = schoolService;

        public async Task<IResponseWrapper> Handle(UpdateSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = await _schoolService.GetByIdAsync(request.UpdateSchool.Id);
            if (school is not null)
            {
                school.Name = request.UpdateSchool.Name;
                school.EstablishedDate = request.UpdateSchool.EstablishedDate;
                var updatedSchoolId = await _schoolService.UpdateAsync(school);
                return await ResponseWrapper<int>.SuccessAsync(updatedSchoolId, "School updated successfully");
            }
            return await ResponseWrapper<int>.FailAsync("School update failed");
        }
    }

    public class UpdateSchoolCommandValidator : AbstractValidator<UpdateSchoolCommand>
    {
        public UpdateSchoolCommandValidator(ISchoolService schoolService)
        {
            RuleFor(c => c.UpdateSchool)
                .SetValidator(new UpdateSchoolRequestValidator(schoolService));
        }
    }
}
