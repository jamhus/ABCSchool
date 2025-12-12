using Application.Wrappers;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Schools.Commands
{
    public class CreateSchoolCommand : IRequest<IResponseWrapper>
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

    internal class CreateSchoolCommandValidator : AbstractValidator<CreateSchoolCommand>
    {
        public CreateSchoolCommandValidator()
        {
            RuleFor(r => r.CreateSchool)
                .SetValidator(new CreateSchoolRequestValidator());
        }
    }
}
