using Microsoft.AspNetCore.Authorization;
using Shared.Constants;

namespace Infrastructure.Identity.Auth
{
    public class ShouldHavePermissionAttribute : AuthorizeAttribute
    {
        public ShouldHavePermissionAttribute(string feature, string action)
        {
            Policy = SchoolPermission.NameForPermission(action, feature);
        }
    }
}
