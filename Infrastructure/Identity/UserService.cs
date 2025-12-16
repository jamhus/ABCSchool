using Application.Exceptions;
using Application.Features.Identity.Users;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Shared.RequestModels.Identity.Users;

namespace Infrastructure.Identity
{
    public class UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            IMultiTenantContextAccessor<ABCSchoolTenantInfo> contextAccessor
        ) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly ApplicationDbContext _context = context;
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _contextAccessor = contextAccessor;

        public async Task<string> ActivateOrDeactivateAsync(string userId, bool activation)
        {
            var user = await GetUserAsync(userId);
            user.IsActive = activation;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors.Select(e => e.Description).ToList());
            }
            return user.Id;
        }

        public async Task<string> AssignRolesAsync(string userId, UserRolesRequest request)
        {
            var user = await GetUserAsync(userId);

            if (await _userManager.IsInRoleAsync(user, RoleConstants.Admin)
                && request.UserRoles.Any(ur => !ur.IsAssigned && ur.Name == RoleConstants.Admin))
            {
                var adminsCount = (await _userManager.GetUsersInRoleAsync(RoleConstants.Admin)).Count();

                if (user.Email == TenancyConstants.Root.Email)
                {
                    if (_contextAccessor.MultiTenantContext.TenantInfo.Id == TenancyConstants.Root.Id)
                    {
                        throw new ConflictException(["You are not allowed to remove admin role for a root tenant user"]);
                    }
                }

                else if (adminsCount <= 2)
                {
                    throw new ConflictException(["Tenant should have atleast two admin users"]);
                }
            }

            foreach (var userRole in request.UserRoles)
            {
                if (userRole.IsAssigned)
                {
                    if (!await _userManager.IsInRoleAsync(user, userRole.Name))
                    {
                        await _userManager.AddToRoleAsync(user, userRole.Name);
                    }
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, userRole.Name);
                }
            }

            return userId;
        }

        public async Task<string> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await GetUserAsync(request.UserId);

            if (!request.NewPassword.Equals(request.ConfirmNewPassword))
            {
                throw new ConflictException(["Passwords does not match"]);
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors.Select(e => e.Description).ToList());
            }
            return user.Id;
        }

        public async Task<string> CreateAsync(CreateUserRequest request)
        {
            if (!request.Password.Equals(request.ConfirmPassword))
            {
                throw new ConflictException(["Passwords does not match"]);
            }

            if (await IsEmailTakenAsync(request.Email))
            {
                throw new ConflictException(["Email must be unique"]);
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                NormalizedEmail = request.Email.ToUpper(),
                UserName = request.Email,
                IsActive = request.IsActive,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors.Select(e => e.Description).ToList());
            }
            return user.Id;
        }

        public async Task<string> DeleteAsync(string userId)
        {
            var user = await GetUserAsync(userId);

            if (user.Email == TenancyConstants.Root.Email)
            {
                throw new ConflictException(["Not allowed to remove Admin User for a Root Tenant."]);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return userId;
        }

        public async Task<List<UserResponse>> GetAllAsync(CancellationToken ct)
        {
            var users = await _userManager.Users.ToListAsync(ct);

            return users.Adapt<List<UserResponse>>();
        }

        public async Task<UserResponse> GetByIdAsync(string userId, CancellationToken ct)
        {
            var user = await GetUserAsync(userId);

            return user.Adapt<UserResponse>();
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken ct)
        {
            var user = await GetUserAsync(userId);

            var userRolesNames = await _userManager.GetRolesAsync(user);

            var permissions = new List<string>();

            foreach (var role in await _roleManager
                .Roles
                .Where(r => userRolesNames.Contains(r.Name))
                .ToListAsync(ct))
            {
                permissions.AddRange(await _context
                    .RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == ClaimConstants.Permission)
                    .Select(rc => rc.ClaimValue)
                    .ToListAsync(ct));
            }

            return permissions.Distinct().ToList();
        }

        public async Task<List<UserRoleResponse>> GetUserRolesAsync(string userId, CancellationToken ct)
        {
            var user = await GetUserAsync(userId);

            var userRoles = new List<UserRoleResponse>();

            var rolesInDb = await _roleManager.Roles.ToListAsync(ct);

            foreach (var role in rolesInDb)
            {
                userRoles.Add(new UserRoleResponse
                {
                    RoleId = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsAssigned = await _userManager.IsInRoleAsync(user, role.Name),
                });
            }

            return userRoles;
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        public async Task<bool> IsPermissionAssigedAsync(string userId, string permission, CancellationToken ct = default)
        {
            return (await GetUserPermissionsAsync(userId, ct)).Contains(permission);
        }

        public async Task<string> UpdateAsync(UpdateUserRequest request)
        {
            var user = await GetUserAsync(request.Id);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors.Select(e => e.Description).ToList());

            }

            return user.Id;
        }

        private async Task<ApplicationUser> GetUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId)
                            ?? throw new NotFoundException(["User does not exist"]);
        }
    }
}
