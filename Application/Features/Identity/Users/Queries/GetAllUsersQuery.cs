using Shared.Wrappers;
using MediatR;
using Shared.RequestModels.Identity.Users;

namespace Application.Features.Identity.Users.Queries
{
    public class GetAllUsersQuery : IRequest<IResponseWrapper>
    {
    }

    public class GetAllUsersQueryHandler(IUserService userService) : IRequestHandler<GetAllUsersQuery, IResponseWrapper>
    {
        private readonly IUserService _userService = userService;

        public async Task<IResponseWrapper> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userService.GetAllAsync(cancellationToken);
            return await ResponseWrapper<List<UserResponse>>.SuccessAsync(data: users);
        }
    }
}
