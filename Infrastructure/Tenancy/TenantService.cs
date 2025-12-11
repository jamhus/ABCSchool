using Application.Features.Tenancy;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy
{
    public class TenantService(
        IMultiTenantStore<ABCSchoolTenantInfo> tenantStore,
        ApplicationDbSeeder dbSeeder,
        IServiceProvider service) : ITenantService
    {
        private readonly IMultiTenantStore<ABCSchoolTenantInfo> _tenantStore = tenantStore;
        private readonly ApplicationDbSeeder _dbSeeder = dbSeeder;
        private readonly IServiceProvider _service = service;

        public async Task<string> ActivateAsync(string id)
        {
            var tenant = await _tenantStore.TryGetAsync(id);
            tenant.IsActive = true;

            await _tenantStore.TryUpdateAsync(tenant);
            return tenant.Identifier;
        }

        public async Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken ct)
        {
            var newTenant = new ABCSchoolTenantInfo
            {
                Id = createTenant.Identifier,
                Identifier = createTenant.Identifier,
                IsActive = createTenant.IsActive,
                ConnectionString = createTenant.ConnectionString,
                Email = createTenant.Email,
                FirstName = createTenant.FirstName,
                LastName = createTenant.LastName,
                Name = createTenant.Name,
                ValidTo = createTenant.ValidTo,
            };

            await _tenantStore.TryAddAsync(newTenant);

            // seeding tenant
            // create a scope for the seeder where the new tenant is the tenant to go
            using var scope = _service.CreateScope();
            _service.GetRequiredService<IMultiTenantContextSetter>()
                .MultiTenantContext = new MultiTenantContext<ABCSchoolTenantInfo>()
                {
                    TenantInfo = newTenant,
                };

            await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
                .InitializeDatabaseAsync(ct);

            return newTenant.Identifier;

        }

        public async Task<string> DeactivateAsync(string id)
        {
            var tenant = await _tenantStore.TryGetAsync(id);
            tenant.IsActive = false;

            await _tenantStore.TryUpdateAsync(tenant);
            return tenant.Identifier;
        }

        public async Task<TenantResponse> GetTenantByIdAsync(string id)
        {
            var tenant = await _tenantStore.TryGetAsync(id);

            return tenant.Adapt<TenantResponse>();
        }

        public async Task<List<TenantResponse>> GetTenantsAsync()
        {
            var tenants = await _tenantStore.GetAllAsync();

            return tenants.Adapt<List<TenantResponse>>();
        }

        public async Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscription)
        {
            var tenant = await _tenantStore.TryGetAsync(updateTenantSubscription.TenantId);
            tenant.ValidTo = updateTenantSubscription.NewExpiryDate;
            await _tenantStore.TryUpdateAsync(tenant);
            return tenant.Identifier;
        }
    }
}
