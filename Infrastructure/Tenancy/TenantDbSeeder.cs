
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy
{
    public class TenantDbSeeder : ITenantDbSeeder
    {
        private readonly TenantDbContext _tenantDbContext;
        private readonly IServiceProvider _serviceProvider;

        public TenantDbSeeder(TenantDbContext tenantDbContext, IServiceProvider serviceProvider)
        {
            _tenantDbContext = tenantDbContext;
            _serviceProvider = serviceProvider;
        }
        public async Task InitializeDatabaseAsync(CancellationToken ct)
        {
            // Seed tenant data
            await SeedTenantAsync(ct);

            foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(ct))
            {
                await InitalizeApplicationDbForTenant(tenant, ct);
            }

        }

        private async Task SeedTenantAsync(CancellationToken ct)
        {
            if(await _tenantDbContext.TenantInfo.FindAsync([TenancyConstants.Root.Id], ct) is null)
            {
                var rootTenant = new ABCSchoolTenantInfo
                {
                    Id = TenancyConstants.Root.Id,
                    Name = TenancyConstants.Root.Name,
                    Email = TenancyConstants.Root.Email,
                    Identifier = TenancyConstants.Root.Id,
                    FullName = "Christopher Nolan",
                    IsActive = true,
                    ValidTo = DateTime.UtcNow.AddYears(100),
                };

                await _tenantDbContext.TenantInfo.AddAsync(rootTenant, ct);
                await _tenantDbContext.SaveChangesAsync(ct);
            }
        }

        private async Task InitalizeApplicationDbForTenant(ABCSchoolTenantInfo currentTenant, CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
                .MultiTenantContext = new MultiTenantContext<ABCSchoolTenantInfo>()
                {
                    TenantInfo = currentTenant,
                };

            await scope.ServiceProvider
                .GetRequiredService<ApplicationDbSeeder>()
                .SeedAsync(ct);
        }
    }
}
