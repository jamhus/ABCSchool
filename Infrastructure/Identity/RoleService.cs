using Application.Exceptions;
using Application.Features.Identity.Roles;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Shared.RequestModels.Identity.Roles;

namespace Infrastructure.Identity
{
    public class RoleService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IMultiTenantContextAccessor<ABCSchoolTenantInfo> contextAccessor) : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ApplicationDbContext _context = context;
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _contextAccessor = contextAccessor;

        public async Task<string> CreateAsync(CreateRoleRequest request)
        {
            var role = request.Adapt<ApplicationRole>();

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors.Select(e => e.Description).ToList());
            }

            return role.Name;
        }

        public async Task<string> DeleteAsync(string id)
        {
            ApplicationRole role = await getDbRole(id);

            if (RoleConstants.IsDefaultRole(role.Name))
            {
                throw new ConflictException([$"Not allowed to delete {role.Name} role as it's a system role."]);
            }

            if ((await _userManager.GetUsersInRoleAsync(role.Name)).Any())
            {
                throw new ConflictException([$"Not allowed to delete {role.Name} role as it's currently used."]);
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors.Select(e => e.Description).ToList());
            }

            return role.Name;
        }

        public async Task<bool> DoesExistAsync(string name)
        {
            return await _roleManager.RoleExistsAsync(name);
        }

        public async Task<List<RoleResponse>> GetAllAsync(CancellationToken ct)
        {
            var roles = await _roleManager.Roles.ToListAsync(ct);
            return roles.Adapt<List<RoleResponse>>();
        }

        public async Task<RoleResponse> GetByIdAsync(string id)
        {
            ApplicationRole role = await getDbRole(id);
            return role.Adapt<RoleResponse>();
        }

        public async Task<RoleResponse> GetWithPermissionsAsync(string id, CancellationToken ct)
        {
            var role = await GetByIdAsync(id);

            role.Permissions = await _context.RoleClaims.Where(rc
                => rc.RoleId == role.Id
                && rc.ClaimType == ClaimConstants.Permission)
                .Select(rc => rc.ClaimValue)
                .ToListAsync(ct);

            return role.Adapt<RoleResponse>();
        }

        public async Task<string> UpdateAsync(UpdateRoleRequest request)
        {
            ApplicationRole role = await getDbRole(request.Id);

            if (RoleConstants.IsDefaultRole(role.Name))
            {
                throw new ConflictException([$"Not allowed to update {role.Name} role as it's a system role."]);
            }

            role.Name = request.Name;
            role.Description = request.Description;
            role.NormalizedName = request.Name.ToUpper();

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors.Select(e => e.Description).ToList());
            }

            return role.Name;
        }

        public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionRequest request)
        {
            ApplicationRole role = await getDbRole(request.RoleId);
            if (RoleConstants.IsAdmin(role.Name))
            {
                throw new ConflictException([$"Not allowed to update {role.Name} role permissions"]);
            }

            // tenant roles shall not be assigned to customers
            if (_contextAccessor.MultiTenantContext.TenantInfo.Id != TenancyConstants.Root.Id)
            {
                request.NewPermissions.RemoveAll(p => p.StartsWith("Permission.Tenants."));
            }

            // Drop and lift : delete the currently assigned roles to the role 
            var currentClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var claim in currentClaims.Where(c => !request.NewPermissions.Any(p => p == c.Value)))
            {
                var result = await _roleManager.RemoveClaimAsync(role, claim);

                if (!result.Succeeded)
                {
                    throw new IdentityException(result.Errors.Select(e => e.Description).ToList());
                }
            }

            foreach (var permission in request.NewPermissions.Where(p => currentClaims.Any(c => c.Value == p)))
            {
                await _context.RoleClaims.AddAsync(
                    new ApplicationRoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = ClaimConstants.Permission,
                        ClaimValue = permission,
                        Description = "",
                        Group = ""
                    });

            }
            await _context.SaveChangesAsync();

            return "Permissions updated successfully";
        }

        private async Task<ApplicationRole> getDbRole(string id)
        {
            return await _roleManager.FindByIdAsync(id)
                ?? throw new NotFoundException(["Role does not exist"]);
        }
    }
}
