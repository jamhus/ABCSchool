using Application.Features.Tenancy;
using Application.Features.Tenancy.Commands;
using Application.Features.Tenancy.Queries;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.RequestModels.Tenancy;

namespace WebApi.Controllers
{
    [Route("api/tenants")]
    [ApiController]
    public class TenantsController : BaseApiController
    {

        [HttpPost("add")]
        [ShouldHavePermission(action: SchoolAction.Create, feature: SchoolFeature.Tenants)]
        public async Task<IActionResult> CreateTenantAsync([FromBody] CreateTenantRequest request)
        {
            var response = await Sender.Send(new CreateTenantCommand { CreateTenantRequest = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("{tenantId}/activate")]
        [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.Tenants)]
        public async Task<IActionResult> ActivateTenantAsync(string tenantId)
        {
            var response = await Sender.Send(new ActivateTenantCommand { TenantId = tenantId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("{tenantId}/deactivate")]
        [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.Tenants)]
        public async Task<IActionResult> DectivateTenantAsync(string tenantId)
        {
            var response = await Sender.Send(new DeactivateTenantCommand { TenantId = tenantId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("upgrade")]
        [ShouldHavePermission(action: SchoolAction.UpgrateSubscription, feature: SchoolFeature.Tenants)]
        public async Task<IActionResult> UpgrateTenantSubscriptionAsync([FromBody] UpdateTenantSubscriptionRequest request)
        {
            var response = await Sender.Send(new UpdateTenantSubscriptionCommand { UpdateTenantSubscription = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("{tenantId}")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Tenants)]
        public async Task<IActionResult> GetTenantByIdAsync(string tenantId)
        {
            var response = await Sender.Send(new GetTenantByIdQuery{ TenantId = tenantId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("all")]
        [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Tenants)]
        public async Task<IActionResult> GetAllTenantsAsync()
        {
            var response = await Sender.Send(new GetAllTenantsQuery{ });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
