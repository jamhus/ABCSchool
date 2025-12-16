using Application.Pipelines;
using Shared.Wrappers;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.RequestModels.Schools;
using Application.Validators;

namespace Application.Features.Schools.Commands
{
    public class CreateSchoolCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public CreateSchoolRequest CreateSchool { get; set; }
    }

    public class CreateSchoolCommandHandler(ISchoolService schoolService) : IRequestHandler<CreateSchoolCommand, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService = schoolService;

        public async Task<IResponseWrapper> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = request.CreateSchool.Adapt<School>();
            var schoolId = await _schoolService.CreateAsync(school);
            return await ResponseWrapper<int>.SuccessAsync(schoolId, "School created successfully");
        }
    }

    public class CreateSchoolCommandValidator : AbstractValidator<CreateSchoolCommand>
    {
        public CreateSchoolCommandValidator()
        {
            RuleFor(r => r.CreateSchool)
                .SetValidator(new CreateSchoolRequestValidator());
        }
    }
}
