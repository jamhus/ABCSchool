using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Portal.Infrastructure.Constants;
using Shared.Constants;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Portal.Infrastructure.Services.Auth
{
    public class ApplicationStateProvider
        (
        ILocalStorageService localStorage,
        HttpClient httpClient
        ) : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly HttpClient _httpClient = httpClient;

        public ClaimsPrincipal AuthenticationStateUser { get; set; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>(StorageConstants.AuthToken);

            if (string.IsNullOrEmpty(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);

            var state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(GetClaimsFromJwt(savedToken), StorageConstants.AuthToken)));

            AuthenticationStateUser = state.User;

            return state;
        }

        public void MarkUserAuthenticated(string username)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Email,username)
                },"apiauth"));

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(authenticatedUser)));

            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkUserAsLoggedOut()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(user)));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task<ClaimsPrincipal> GetAuthenticationStateProviderUserAsync()
        {
            var state = await GetAuthenticationStateAsync();

            return state.User;
        }

        #region helpers
        private IEnumerable<Claim> GetClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs is not null)
            {
                keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles);

                if (roles is not null)
                {
                    if (roles.ToString().Trim().StartsWith('[')) // multiple roles
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                        claims.AddRange(parsedRoles.Select(r => new Claim(ClaimTypes.Role, r)));
                    }
                    else // single role
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                    }

                    keyValuePairs.Remove(ClaimTypes.Role);
                }

                keyValuePairs.TryGetValue(ClaimConstants.Permission, out var permissions);

                if (permissions is not null)
                {
                    if (permissions.ToString().Trim().StartsWith('['))
                    {
                        var parsedPermissions = JsonSerializer.Deserialize<string[]>(permissions.ToString());
                        claims.AddRange(parsedPermissions.Select(p => new Claim(ClaimConstants.Permission, p)));
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimConstants.Permission, permissions.ToString()));
                    }

                    keyValuePairs.Remove(ClaimConstants.Permission);
                }

                claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64Payload)
        {
            switch (base64Payload.Length % 4)
            {
                case 2:
                    base64Payload += "==";
                    break;
                case 3:
                    base64Payload += "=";
                    break;
            }

            return Convert.FromBase64String(base64Payload);
        }
        #endregion
    }
}
