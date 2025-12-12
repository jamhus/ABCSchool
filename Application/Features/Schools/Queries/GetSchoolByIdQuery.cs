using Application.Wrappers;
using Mapster;
using MediatR;

namespace Application.Features.Schools.Queries
{
    public class GetSchoolByIdQuery : IRequest<IResponseWrapper>
    {
        public int SchoolId { get; set; }
    }

    public class GetSchoolByIdQueryHandler(ISchoolService schoolService) : IRequestHandler<GetSchoolByIdQuery, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService = schoolService;

        public async Task<IResponseWrapper> Handle(GetSchoolByIdQuery request, CancellationToken cancellationToken)
        {
            var school = await _schoolService.GetByIdAsync(request.SchoolId);
            if(school is not null)
            {
                return await ResponseWrapper<SchoolResponse>.SuccessAsync(school.Adapt<SchoolResponse>());
            }
            return await ResponseWrapper.FailAsync("School not found");
        }
    }
}
