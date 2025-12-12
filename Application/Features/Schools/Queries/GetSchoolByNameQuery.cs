using Application.Wrappers;
using Mapster;
using MediatR;

namespace Application.Features.Schools.Queries
{
    public class GetSchoolByNameQuery : IRequest<IResponseWrapper>
    {
        public string Name { get; set; }
    }

    public class GetSchoolByNameQueryHandler(ISchoolService schoolService) : IRequestHandler<GetSchoolByNameQuery, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService = schoolService;

        public async Task<IResponseWrapper> Handle(GetSchoolByNameQuery request, CancellationToken cancellationToken)
        {
            var school = await _schoolService.GetByNameAsync(request.Name);
            if (school is not null)
            {
                return await ResponseWrapper<SchoolResponse>.SuccessAsync(school.Adapt<SchoolResponse>());
            }
            return await ResponseWrapper.FailAsync("School not found");
        }
    }
}
