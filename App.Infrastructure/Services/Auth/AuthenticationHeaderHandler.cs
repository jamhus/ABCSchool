using Blazored.LocalStorage;
using Portal.Infrastructure.Constants;
using System.Net.Http.Headers;

namespace Portal.Infrastructure.Services.Auth
{
    public class AuthenticationHeaderHandler(ILocalStorageService storageService) : DelegatingHandler
    {
        private readonly ILocalStorageService _storageService = storageService;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            try
            {
                if(request.Headers.Authorization?.Scheme != "Bearer")
                {
                    var savedToken = await _storageService.GetItemAsync<string>(StorageConstants.AuthToken, ct);
                    if (!string.IsNullOrEmpty(savedToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
                    }
                }

                return await base.SendAsync(request, ct);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
