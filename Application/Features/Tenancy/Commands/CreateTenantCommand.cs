using Shared.Wrappers;
using MediatR;
using Shared.RequestModels.Tenancy;

namespace Application.Features.Tenancy.Commands
{
    public class CreateTenantCommand : IRequest<IResponseWrapper>
    {
        public CreateTenantRequest CreateTenantRequest { get; set; }
    }

    public class CreateTenantCommandHandler(ITenantService tenantService) : IRequestHandler<CreateTenantCommand, IResponseWrapper>
    {
        private readonly ITenantService _tenantService = tenantService;

        public async Task<IResponseWrapper> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenantId = await _tenantService.CreateTenantAsync(request.CreateTenantRequest, cancellationToken);

            return await ResponseWrapper<string>.SuccessAsync(tenantId, "Tenant created successfully");
        }
    }
}
