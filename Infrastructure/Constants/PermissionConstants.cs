namespace Infrastructure.Constants
{
    public static class SchoolAction
    {
        public const string Read = nameof(Read);
        public const string Create = nameof(Create);
        public const string Delete = nameof(Delete);
        public const string Update = nameof(Update);
        public const string RefreshToken = nameof(RefreshToken);
        public const string UpgrateSubscription = nameof(UpgrateSubscription);
    }

    public static class SchoolFeature
    {
        public const string Users = nameof(Users);
        public const string Roles = nameof(Roles);
        public const string Tokens = nameof(Tokens);
        public const string Tenants = nameof(Tenants);
        public const string Schools = nameof(Schools);
        public const string UserRoles = nameof(UserRoles);
        public const string RoleClaims = nameof(RoleClaims);
    }

    public record SchoolPermission(string Action, string Feature, string Description, string Group, bool IsBasic = false, bool IsRoot = false)
    {
        public string Name => NameForPermission(Action, Feature);
        public static string NameForPermission(string action, string feature) => $"Permission.{feature}.{action}";
    }

    public static class SchoolPermissions
    {
        public static readonly SchoolPermission[] AllPermissions =
        {
            new SchoolPermission(SchoolAction.Read, SchoolFeature.Users, "SystemAccess", "Read Users"),
            new SchoolPermission(SchoolAction.Create, SchoolFeature.Users, "SystemAccess", "Create Users"),
            new SchoolPermission(SchoolAction.Update, SchoolFeature.Users, "SystemAccess", "Update Users"),
            new SchoolPermission(SchoolAction.Delete, SchoolFeature.Users, "SystemAccess", "Delete Users"),

            new SchoolPermission(SchoolAction.Read, SchoolFeature.UserRoles, "SystemAccess", "Delete UserRoles"),
            new SchoolPermission(SchoolAction.Update, SchoolFeature.UserRoles, "SystemAccess", "Delete UserRoles"),

            new SchoolPermission(SchoolAction.Read, SchoolFeature.Roles, "SystemAccess", "Read Roles"),
            new SchoolPermission(SchoolAction.Create, SchoolFeature.Roles, "SystemAccess", "Create Roles"),
            new SchoolPermission(SchoolAction.Update, SchoolFeature.Roles, "SystemAccess", "Update Roles"),
            new SchoolPermission(SchoolAction.Delete, SchoolFeature.Roles, "SystemAccess", "Delete Roles"),
            new SchoolPermission(SchoolAction.RefreshToken, SchoolFeature.Tokens, "SystemAccess", "Generate Refresh Token", IsBasic: true),
            
            new SchoolPermission(SchoolAction.Read, SchoolFeature.RoleClaims, "SystemAccess", "Delete RoleClaims/Permissions"),
            new SchoolPermission(SchoolAction.Update, SchoolFeature.RoleClaims, "SystemAccess", "Delete RoleClaims/Permissions"),

            new SchoolPermission(SchoolAction.Read, SchoolFeature.Schools, "Academics", "Read Schools", IsBasic: true),
            new SchoolPermission(SchoolAction.Create, SchoolFeature.Schools, "Academics", "Create Schools"),
            new SchoolPermission(SchoolAction.Update, SchoolFeature.Schools, "Academics", "Update Schools"),
            new SchoolPermission(SchoolAction.Delete, SchoolFeature.Schools, "Academics", "Delete Schools"),


            new SchoolPermission(SchoolAction.Read, SchoolFeature.Tenants, "Tenancy", "Read Tenants", IsRoot: true),
            new SchoolPermission(SchoolAction.Create, SchoolFeature.Tenants, "Tenancy", "Create Tenants", IsRoot: true),
            new SchoolPermission(SchoolAction.Update, SchoolFeature.Tenants, "Tenancy", "Update Tenants", IsRoot: true),
            new SchoolPermission(SchoolAction.UpgrateSubscription, SchoolFeature.Tenants, "Tenancy", "Upgrate Tenants Subscription", IsRoot: true),
        };

        public static IReadOnlyList<SchoolPermission> All ()=> AllPermissions;
        public static IReadOnlyList<SchoolPermission> Root ()=> AllPermissions.Where(p => p.IsRoot).ToList();
        public static IReadOnlyList<SchoolPermission> Admin ()=> AllPermissions.Where(p => !p.IsRoot).ToList();
        public static IReadOnlyList<SchoolPermission> Basic ()=> AllPermissions.Where(p => p.IsBasic).ToList();
    }
}
