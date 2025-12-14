using Application.Features.Identity.Roles;
using Application.Wrappers;
using MediatR;

public class UpdateRoleCommand : IRequest<IResponseWrapper>
{
    public UpdateRoleRequest UpdateRole { get; set; }
}

public class UpdateRoleCommandHandler(IRoleService roleService) : IRequestHandler<UpdateRoleCommand, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = await _roleService.UpdateAsync(request.UpdateRole);

        return await ResponseWrapper<string>.SuccessAsync($"Role {roleName} was updated successfully");
    }
}