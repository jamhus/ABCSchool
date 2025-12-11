using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands
{
    public class UpdateTenantSubscriptionCommand : IRequest<IResponseWrapper>
    {
        public UpdateTenantSubscriptionRequest UpdateTenantSubscription { get; set; }
    }

    public class UpdateTenantSubscriptionCommandHandler(ITenantService tenantService) : IRequestHandler<UpdateTenantSubscriptionCommand, IResponseWrapper>
    {
        private readonly ITenantService _tenantService = tenantService;

        public async Task<IResponseWrapper> Handle(UpdateTenantSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var tenantId = await _tenantService.UpdateSubscriptionAsync(request.UpdateTenantSubscription);

            return await ResponseWrapper<string>.SuccessAsync(tenantId, "Tenant subscription updated successfully");
        }
    }
}
