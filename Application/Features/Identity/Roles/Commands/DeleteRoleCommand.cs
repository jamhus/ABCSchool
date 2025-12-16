using Application.Features.Identity.Roles;
using MediatR;
using Shared.Wrappers;

public class DeleteRoleCommand : IRequest<IResponseWrapper>
{
    public string RoleId { get; set; }
}

public class DeleteRoleCommandHandler(IRoleService roleService) : IRequestHandler<DeleteRoleCommand, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = await _roleService.DeleteAsync(request.RoleId);

        return await ResponseWrapper<string>.SuccessAsync($"Role {roleName} was deleted successfully");
    }
}