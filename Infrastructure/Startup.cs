using Application;
using Application.Features.Identity.Tokens;
using Application.Wrappers;
using Finbuckle.MultiTenant;
using Infrastructure.Constants;
using Infrastructure.Contexts;
using Infrastructure.Identity.Auth;
using Infrastructure.Identity.Models;
using Infrastructure.Identity.Tokens;
using Infrastructure.OpenApi;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;

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
                    .AddTransient<ITenantDbSeeder, TenantDbSeeder>()
                    .AddTransient<ApplicationDbSeeder>()
                    .AddIdentityService()
                    .AddPermissions()
                    .AddOpenApiDocumentation(config);
        }

        internal static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            // Identity services can be added here
            return services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                    options.User.RequireUniqueEmail = true;
                }).AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .Services
                .AddScoped<ITokenService, TokenService>();
        }

        public static async Task InitializeDatabasesAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var tenantDbSeeder = scope.ServiceProvider.GetRequiredService<ITenantDbSeeder>();
                await tenantDbSeeder.InitializeDatabaseAsync(ct);
            }
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            var secret = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearer =>
            {
                bearer.RequireHttpsMetadata = false;
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };

                bearer.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";

                            var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("Token has expired"));

                            return context.Response.WriteAsync(result);
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "application/json";

                            var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("Unhandled error has occured"));

                            return context.Response.WriteAsync(result);
                        }
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You are not Authorized"));
                            return context.Response.WriteAsync(result);
                        }
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You are not authorized to access this resource"));
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                foreach (var property in typeof(SchoolPermissions).GetNestedTypes()
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
                {
                    var permission = property.GetValue(null);
                    if (permission is not null)
                    {
                        options.AddPolicy(permission.ToString(), policy => policy.RequireClaim(ClaimConstants.Permission, permission.ToString()));
                    }
                }
            });

            return services;
        }
        public static JwtSettings GetJwtSettings(this IServiceCollection services, IConfiguration config)
        {
            var jwtSettingsConfig = config.GetSection(nameof(JwtSettings));
            services.Configure<JwtSettings>(jwtSettingsConfig);
            return jwtSettingsConfig.Get<JwtSettings>();
        }

        internal static IServiceCollection AddPermissions(this IServiceCollection services)
        {

            return services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration config)
        {
            var swaggerSettings = config.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();

            services.AddEndpointsApiExplorer();
            _ = services.AddOpenApiDocument((document, serviceProvider) =>
            {
                document.PostProcess = d =>
                {
                    d.Info.Title = swaggerSettings.Title;
                    d.Info.Description = swaggerSettings.Description;
                    d.Info.Contact = new OpenApiContact
                    {
                        Name = swaggerSettings.ContactName,
                        Email = swaggerSettings.ContactEmail,
                        Url = swaggerSettings.ContactUrl
                    };
                    d.Info.License = new OpenApiLicense
                    {
                        Name = swaggerSettings.LicenseName,
                        Url = swaggerSettings.LicenseUrl
                    };
                };

                document.AddSecurity(JwtBearerDefaults.AuthenticationScheme, Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter your Bearer token to attach it as a header on your requests",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                });

                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));
                document.OperationProcessors.Add(new SwaggerGlobalAuthProcessor());
                document.OperationProcessors.Add(new SwaggerHeaderAttributeProcessor());
            });
            return services;
        }
        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            return app
                .UseAuthentication()
                .UseMultiTenant()
                .UseAuthorization()
                .UseOpenApiDocumentation();
        }

        internal static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
        {
            app.UseOpenApi();
            app.UseSwaggerUi(settings =>
            {
                settings.DefaultModelExpandDepth = -1;
                settings.DocExpansion = "none";
                settings.TagsSorter = "alpha";
            });
            return app;
        }
    }
}
