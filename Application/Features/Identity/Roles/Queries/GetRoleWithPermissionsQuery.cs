using Application.Features.Identity.Roles;
using Application.Wrappers;
using MediatR;

namespace Application.Features.Identity.Roles.Queries
{
    public class GetRoleWithPermissionsQuery : IRequest<IResponseWrapper>
    {
        public string RoleId { get; set; }
    }

    public class GetRoleByNameQueryHandler(IRoleService roleService) : IRequestHandler<GetRoleWithPermissionsQuery, IResponseWrapper>
    {
        private readonly IRoleService _roleService = roleService;

        public async Task<IResponseWrapper> Handle(GetRoleWithPermissionsQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetWithPermissionsAsync(request.RoleId, cancellationToken);
            return await ResponseWrapper<RoleResponse>.SuccessAsync(roles);
        }
    }
}
