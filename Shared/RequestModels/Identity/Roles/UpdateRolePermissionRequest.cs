namespace Shared.RequestModels.Identity.Roles
{
    public class UpdateRolePermissionRequest
    {
        public string RoleId { get; set; }
        public List<string> NewPermissions { get; set; } = []; 
    }
}
