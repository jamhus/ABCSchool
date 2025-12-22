using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Portal.Infrastructure;
using Portal.Infrastructure.Extenstions;

namespace ABCSchoolPortal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddSingleton(builder.Configuration.GetSection(nameof(ApiSettings)).Get<ApiSettings>());
            builder.AddClientServices();

            await builder.Build().RunAsync();
        }
    }
}
