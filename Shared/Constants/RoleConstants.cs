namespace Shared.Constants
{
    public static class RoleConstants
    {
        public const string Admin = nameof(Admin);
        public const string Basic = nameof(Basic);

        public static readonly string[] DefaultRoles =
        {
            Admin,
            Basic
        };
        
        public static bool IsDefaultRole(string roleName) => DefaultRoles.Contains(roleName);
        public static bool IsAdmin(string roleName) => roleName.Equals(RoleConstants.Admin);
    }
}
