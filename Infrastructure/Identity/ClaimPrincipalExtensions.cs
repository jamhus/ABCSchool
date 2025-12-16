using Shared.Constants;
using System.Security.Claims;

namespace Infrastructure.Identity
{
    public static class ClaimPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier);
        public static string GetUserEmail(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email);
        public static string GetTenantId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimConstants.Tenant);
        public static string GetFirstName(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name);

    }
}
