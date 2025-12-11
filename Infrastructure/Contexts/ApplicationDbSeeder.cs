using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public class ApplicationDbSeeder
    {
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantContextAccessor;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _applicationDbContext;

        public ApplicationDbSeeder(
            IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContextAccessor,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext applicationDbContext
            )
        {
            _tenantContextAccessor = tenantContextAccessor;
            _roleManager = roleManager;
            _userManager = userManager;
            _applicationDbContext = applicationDbContext;
        }

        public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
        {
            if (_applicationDbContext.Database.GetMigrations().Any())
            {
                if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                {
                    await _applicationDbContext.Database.MigrateAsync(cancellationToken);
                }

                // seeding 
                if (await _applicationDbContext.Database.CanConnectAsync(cancellationToken))
                {
                    var tenantId = _tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;

                    // Seed Roles > Assign Permissions/claims
                    await SeedRolesAsync(cancellationToken);
                    // Seed users > Assign Roles
                    await SeedAdminAsync();
                }
            }
        }

        #region seed roles
        private async Task SeedRolesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var roleName in RoleConstants.DefaultRoles)
            {
                // if role does not exist, incomingRole will be created as new ApplicationRole 
                if (await _roleManager.Roles.SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken) is not ApplicationRole incomingRole)
                {
                    incomingRole = new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} Role",
                    };

                    await _roleManager.CreateAsync(incomingRole);
                }

                // Assign Claims/Permissions to Role
                if (roleName == RoleConstants.Admin)
                {
                    // assign all permissions/claims to Admin role
                    var adminClaims = SchoolPermissions.Admin();
                    await AssignClaimsToRoleAsync(incomingRole, adminClaims, cancellationToken);

                    if (_tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id == TenancyConstants.Root.Id)
                    {
                        // if Root tenant, assign all permissions/claims to Admin role
                        var rootClaims = SchoolPermissions.Root();
                        await AssignClaimsToRoleAsync(incomingRole, rootClaims, cancellationToken);
                    }

                }
                else if (roleName == RoleConstants.Basic)
                {
                    // assign basic permissions/claims to Basic role
                    var basicClaims = SchoolPermissions.Basic();
                    await AssignClaimsToRoleAsync(incomingRole, basicClaims, cancellationToken);
                }
            }
        }

        private async Task AssignClaimsToRoleAsync(ApplicationRole role, IEnumerable<SchoolPermission> permissions, CancellationToken cancellationToken = default)
        {
            var existingRoleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var permission in permissions)
            {
                if (!existingRoleClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == permission.Name))
                {
                    await _applicationDbContext.RoleClaims.AddAsync(new ApplicationRoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = ClaimConstants.Permission,
                        ClaimValue = permission.Name,
                        Description = permission.Description,
                        Group = permission.Group

                    }, cancellationToken);

                    await _applicationDbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }
        #endregion

        #region seed admin
        private async Task SeedAdminAsync()
        {
            if(string.IsNullOrEmpty(_tenantContextAccessor.MultiTenantContext.TenantInfo.Email)) return;

            if(await _userManager.Users.SingleOrDefaultAsync(user => user.Email == _tenantContextAccessor.MultiTenantContext.TenantInfo.Email) is not ApplicationUser incomingUser)
            {
                incomingUser = new ApplicationUser
                {
                    IsActive = true,
                    LastName = _tenantContextAccessor.MultiTenantContext.TenantInfo.LastName,
                    EmailConfirmed = true,
                    FirstName = _tenantContextAccessor.MultiTenantContext.TenantInfo.FirstName,
                    PhoneNumberConfirmed = true,
                    Email = _tenantContextAccessor.MultiTenantContext.TenantInfo.Email,
                    UserName = _tenantContextAccessor.MultiTenantContext.TenantInfo.Email,
                    NormalizedEmail = _tenantContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpper(),
                    NormalizedUserName = _tenantContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpper(),
                };

                var passwordHash = new PasswordHasher<ApplicationUser>();
                incomingUser.PasswordHash = passwordHash.HashPassword(incomingUser, "Admin123!");
                await _userManager.CreateAsync(incomingUser);
            }
            // Assign Admin role to user
            if (!await _userManager.IsInRoleAsync(incomingUser, RoleConstants.Admin))
            {
                await _userManager.AddToRoleAsync(incomingUser, RoleConstants.Admin);
            }

        }
        #endregion
    }
}
