using Finbuckle.MultiTenant;
using Infrastructure.Constants;
using Infrastructure.Contexts;
using Infrastructure.Identity.Auth;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            return services
                    .AddDbContext<TenantDbContext>(options => options
                        .UseSqlServer(config.GetConnectionString("DefaultConnection")))
                    .AddMultiTenant<ABCSchoolTenantInfo>()
                        .WithHeaderStrategy(TenancyConstants.TenantIdName)
                        .WithClaimStrategy(TenancyConstants.TenantIdName)
                        .WithEFCoreStore<TenantDbContext, ABCSchoolTenantInfo>()
                        .Services
                    .AddDbContext<ApplicationDbContext>(options => options
                        .UseSqlServer(config.GetConnectionString("DefaultConnection")))
                    .AddTransient<ITenantDbSeeder,TenantDbSeeder>()
                    .AddTransient<ApplicationDbSeeder>()
                    .AddIdentityService()
                    .AddPermissions();
        }

        internal static IServiceCollection AddIdentityService (this IServiceCollection services)
        {
            // Identity services can be added here
            return services
                .AddIdentity<ApplicationUser,ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                    options.User.RequireUniqueEmail = true;
                }).AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .Services;
        }

        public static async Task InitializeDatabasesAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var tenantDbSeeder = scope.ServiceProvider.GetRequiredService<ITenantDbSeeder>();
                await tenantDbSeeder.InitializeDatabaseAsync(ct);
            }
        }

        internal static IServiceCollection AddPermissions(this IServiceCollection services)
        {
            
            return services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            return app.UseMultiTenant();
        }
    }
}
