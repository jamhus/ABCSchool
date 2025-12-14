namespace Application.Features.Identity.Roles
{
    public interface IRoleService
    {
        Task<string> CreateAsync(CreateRoleRequest request);
        Task<string> UpdateAsync(UpdateRoleRequest request);
        Task<string> DeleteAsync(string id);
        Task<string> UpdatePermissionsAsync(UpdateRolePermissionRequest request);
        Task<bool> DoesExistAsync(string name);
        Task<List<RoleResponse>> GetAllAsync(CancellationToken ct);
        Task<RoleResponse> GetByIdAsync(string id);
        Task<RoleResponse> GetWithPermissionsAsync(string id, CancellationToken ct);
    }
}
