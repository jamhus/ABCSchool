using Application.Wrappers;
using FluentValidation;
using MediatR;

namespace Application.Features.Schools.Commands
{
    public class UpdateSchoolCommand : IRequest<IResponseWrapper>
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

    internal class UpdateSchoolCommandValidator : AbstractValidator<UpdateSchoolCommand>
    {
        internal UpdateSchoolCommandValidator(ISchoolService schoolService)
        {
            RuleFor(c => c.UpdateSchool)
                .SetValidator(new UpdateSchoolRequestValidator(schoolService));
        }
    }
}
