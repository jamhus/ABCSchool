using Shared.Wrappers;
using MediatR;
using Shared.RequestModels.Tenancy;

namespace Application.Features.Tenancy.Queries
{
    public class GetTenantByIdQuery : IRequest<IResponseWrapper>
    {
        public string TenantId { get; set; }
    }

    public class GetTenantByIdQueryHandler(ITenantService tenantService) : IRequestHandler<GetTenantByIdQuery, IResponseWrapper>
    {
        private readonly ITenantService _tenantService = tenantService;

        public async Task<IResponseWrapper> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(request.TenantId);
            
            if (tenant is not null)
            {
                return await ResponseWrapper<TenantResponse>.SuccessAsync(tenant);
            }

            return await ResponseWrapper<TenantResponse>.FailAsync("Tenant does not exist");
        }
    }
}
