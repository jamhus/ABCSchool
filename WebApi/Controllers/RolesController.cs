using Application.Features.Identity.Roles;
using Application.Features.Identity.Roles.Commands;
using Application.Features.Identity.Roles.Queries;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.RequestModels.Identity.Roles;

namespace WebApi.Controllers
{
    [Route("api/roles")]
    public class RolesController : BaseApiController
    {
        [HttpGet("all")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Roles)]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var response = await Sender.Send(new GetAllRolesQuery { });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("by-id/{roleId}")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Roles)]
        public async Task<IActionResult> GetRoleByIdAsync(string roleId)
        {
            var response = await Sender.Send(new GetRoleByIdQuery { RoleId = roleId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("with-permissions/{roleId}")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Roles)]
        public async Task<IActionResult> GetRoleWithPermissionsAsync(string roleId)
        {
            var response = await Sender.Send(new GetRoleWithPermissionsQuery { RoleId = roleId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPost("add")]
        [ShouldHavePermission(action: SchoolAction.Create, feature: SchoolFeature.Roles)]
        public async Task<IActionResult> CreateRoleAsync(CreateRoleRequest request)
        {
            var response = await Sender.Send(new CreateRoleCommand { CreateRole = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPut("update")]
        [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.Roles)]
        public async Task<IActionResult> UpdateRoleAsync(UpdateRoleRequest request)
        {
            var response = await Sender.Send(new UpdateRoleCommand { UpdateRole = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPut("update-permissions")]
        [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.RoleClaims)]
        public async Task<IActionResult> UpdateRolePermissionsAsync(UpdateRolePermissionRequest request)
        {
            var response = await Sender.Send(new UpdateRolePermissionsCommand { UpdateRolePermissions = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }


        [HttpDelete("{roleId}")]
        [ShouldHavePermission(action: SchoolAction.Delete, feature: SchoolFeature.Roles)]
        public async Task<IActionResult> DeleteRoleAsync(string roleId)
        {
            var response = await Sender.Send(new DeleteRoleCommand { RoleId = roleId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }
    }
}
