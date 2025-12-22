using Microsoft.AspNetCore.Authorization;
using Shared.Constants;

namespace Portal.Infrastructure.Services.Auth
{
    public class ShouldHavePermissionAttribute : AuthorizeAttribute
    {
        public ShouldHavePermissionAttribute(string action, string feature)
        {
            Policy = SchoolPermission.NameForPermission(action, feature);
        }
    }
}
