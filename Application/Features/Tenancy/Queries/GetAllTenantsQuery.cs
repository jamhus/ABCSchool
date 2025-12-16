using Shared.Wrappers;
using MediatR;
using Shared.RequestModels.Tenancy;

namespace Application.Features.Tenancy.Queries
{
    public class GetAllTenantsQuery : IRequest<IResponseWrapper>
    {}

    public class GetAllTenantsQueryHandler(ITenantService tenantService) : IRequestHandler<GetAllTenantsQuery, IResponseWrapper>
    {
        private readonly ITenantService _tenantService = tenantService;

        public async Task<IResponseWrapper> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
        {
            var tenants = await _tenantService.GetTenantsAsync();

            return await ResponseWrapper<List<TenantResponse>>.SuccessAsync(tenants);
        }
    }
}
