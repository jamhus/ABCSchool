using Application.Wrappers;
using Mapster;
using MediatR;

namespace Application.Features.Schools.Queries
{
    public class GetAllSchoolsQuery : IRequest<IResponseWrapper>
    {
    }

    public class GetAllSchoolsQueryHandler(ISchoolService schoolService) : IRequestHandler<GetAllSchoolsQuery, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService = schoolService;

        public async Task<IResponseWrapper> Handle(GetAllSchoolsQuery request, CancellationToken cancellationToken)
        {
            var schools = await _schoolService.GetAllAsync();
            if (schools.Any())
            {
                return await ResponseWrapper<List<SchoolResponse>>.SuccessAsync(schools.Adapt<List<SchoolResponse>>());
            }
            return await ResponseWrapper.FailAsync("Schools not found");
        }
    }
}
