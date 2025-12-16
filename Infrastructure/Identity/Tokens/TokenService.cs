using Application;
using Application.Exceptions;
using Application.Features.Identity.Tokens;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants;
using Shared.RequestModels.Identity.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _multiTenant;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JwtSettings _jwtSettings;

        public TokenService(UserManager<ApplicationUser> userManager,
            IMultiTenantContextAccessor<ABCSchoolTenantInfo> multiTenant,
            RoleManager<ApplicationRole> roleManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _multiTenant = multiTenant;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
        }
        public async Task<TokenResponse> LoginAsync(TokenRequest request)
        {
            #region Validation
            // Insure that user belongs to active tenant
            if (!(bool)_multiTenant.MultiTenantContext?.TenantInfo.IsActive)
            {
                throw new UnauthorizedException(["Tenant subscription is not active. Contact Administrator"]);
            }

            var user = await _userManager.FindByNameAsync(request.Username) ?? throw new UnauthorizedException(["Authentication not successful"]);

            if (!user.IsActive)
            {
                throw new UnauthorizedException(["User is not active. Contact Administrator"]);
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedException(["Incorrect username or password"]);
            }

            if (!(bool)(_multiTenant.MultiTenantContext?.TenantInfo.Id is not TenancyConstants.Root.Id))
            {
                if (_multiTenant.MultiTenantContext?.TenantInfo.ValidTo < DateTime.UtcNow)
                {
                    throw new UnauthorizedException(["Tenant subscription has expired. Contact Administrator"]);
                }
            }
            #endregion

            return await GenerateTokenAndUpdateUserAsync(user);
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = GetClaimsPrincipalFromExpiringToken(request.CurrentJwt);
            var userId = principal.GetUserId();
            var user = await _userManager.FindByIdAsync(userId) ?? throw new UnauthorizedException(["Authentication failed"]);

            if (user.RefreshToken != request.CurrentRefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                throw new UnauthorizedException(["Invalid token."]);
            }

            return await GenerateTokenAndUpdateUserAsync(user);
        }

        private ClaimsPrincipal GetClaimsPrincipalFromExpiringToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret)),
                ValidateIssuerSigningKey = true,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedException(["Invalid token. Failed to generate new token"]);
            }
            return principal;
        }

        private async Task<TokenResponse> GenerateTokenAndUpdateUserAsync(ApplicationUser user)
        {
            // Generate JWT and Refresh Token
            var newJwt = await GenerateJwtTokenAsync(user);
            var newRefreshToken = generateRefreshToken();

            // Update user with new refresh token and expiry date
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);

            await _userManager.UpdateAsync(user);

            return new TokenResponse
            {
                Jwt = newJwt,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryDate = user.RefreshTokenExpiryTime
            };
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            return GenerateEncryptedToken(GetSigningCredentials(), await GetUserClaimsAsync(user));
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryTimeInMinutes),
                signingCredentials: signingCredentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private SigningCredentials GetSigningCredentials()
        {
            byte[] secretKey = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            return new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256);
        }

        private async Task<IEnumerable<Claim>> GetUserClaimsAsync(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();

            foreach (var roleName in userRoles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, roleName));

                var role = await _roleManager.FindByNameAsync(roleName);

                var rolePermissions = await _roleManager.GetClaimsAsync(role);

                permissionClaims.AddRange(rolePermissions);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Name, user.FirstName),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimConstants.Tenant, _multiTenant.MultiTenantContext.TenantInfo.Id),
            }
            .Union(roleClaims)
            .Union(userClaims)
            .Union(permissionClaims);

            return claims;
        }

        private string generateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
