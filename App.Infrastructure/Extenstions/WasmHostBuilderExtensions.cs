using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Portal.Infrastructure.Services.Auth;
using Shared.Constants;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using static MudBlazor.Defaults.Classes;

namespace Portal.Infrastructure.Extenstions
{
    public static class WasmHostBuilderExtensions
    {
        private const string _clientName = "ABC School Api";
        public static WebAssemblyHostBuilder AddClientServices(this WebAssemblyHostBuilder builder)
        {
            builder.Services
                .AddAuthorizationCore(RegisterPermissions)
                .AddBlazoredLocalStorage()
                .AddMudServices(config =>
                {
                    config.SnackbarConfiguration.PositionClass = Position.TopRight;
                    config.SnackbarConfiguration.HideTransitionDuration = 100;
                    config.SnackbarConfiguration.ShowTransitionDuration = 100;
                    config.SnackbarConfiguration.VisibleStateDuration = 3000;
                    config.SnackbarConfiguration.ShowCloseIcon = true;
                })
                .AddScoped<ApplicationStateProvider>()
                .AddScoped<AuthenticationStateProvider, ApplicationStateProvider>()
                .AddTransient<AuthenticationHeaderHandler>()
                .AddScoped(sp => sp
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient(_clientName).EnableIntercept(sp))
                .AddHttpClient(_clientName , client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration.GetSection("ApiSettings:BaseApiUrl").Get<string>());
                })
                .AddHttpMessageHandler<AuthenticationHeaderHandler>();

            builder.Services.AddHttpClientInterceptor();

            return builder;
        }

        private static void RegisterPermissions(AuthorizationOptions options)
        {
            foreach (var permission in SchoolPermissions.All())
            {
                options.AddPolicy(permission.Name, policy => policy.RequireClaim(ClaimConstants.Permission, permission.Name));
            }
        }
    }
}
