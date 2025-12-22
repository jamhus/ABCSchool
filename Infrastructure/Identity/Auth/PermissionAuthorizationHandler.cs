using Microsoft.AspNetCore.Authorization;
using Shared.Constants;
using System.Security.Claims;

namespace Infrastructure.Identity.Auth
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissionClaim = context.User.Claims.Where(c => c.Type == ClaimConstants.Permission && c.Value == requirement.Permission);

            if (permissionClaim.Any())
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
            }

        }
    }
}
