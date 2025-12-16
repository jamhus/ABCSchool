using Shared.Wrappers;
using MediatR;

namespace Application.Features.Schools.Commands
{
    public class DeleteSchoolCommand : IRequest<IResponseWrapper>
    {
        public int SchoolId { get; set; }
    }

    public class DeleteSchoolCommandHandler(ISchoolService schoolService) : IRequestHandler<DeleteSchoolCommand, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService = schoolService;


        public async Task<IResponseWrapper> Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = await _schoolService.GetByIdAsync(request.SchoolId);
            if (school is not null)
            {
                var deletedSchoolId = await _schoolService.DeleteAsync(school);
                return await ResponseWrapper<int>.SuccessAsync(deletedSchoolId, "School deleted successfully");
            }
            return await ResponseWrapper<int>.FailAsync("School deleting failed");
        }
    }
}
