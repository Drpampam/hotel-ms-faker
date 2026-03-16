using hotelier_core_app.Model.Configs;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace hotelier_core_app.Service.Implementation
{
    /// <summary>
    /// Provides token generation and token claim extraction helpers.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IOptions<JwtConfig> _jwtConfig;

        public TokenService(IOptions<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig;
        }

        /// <summary>
        /// Gets the user's full name from the JWT token in the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request containing the JWT token.</param>
        /// <returns>The user's full name if present, otherwise an empty string.</returns>
        public string GetUserFullName(HttpRequest request)
        {
            return GetSingleClaimValue(ExtractSecurityToken(request), ClaimTypes.Name);
        }

        /// <summary>
        /// Gets the user's email from the JWT token in the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request containing the JWT token.</param>
        /// <returns>The user's email if present, otherwise an empty string.</returns>
        public string GetUserEmail(HttpRequest request)
        {
            return GetSingleClaimValue(ExtractSecurityToken(request), ClaimTypes.Email);
        }

        /// <summary>
        /// Gets the user's roles from the JWT token in the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request containing the JWT token.</param>
        /// <returns>A list of user roles extracted from the token.</returns>
        public List<string> GetUserRoles(HttpRequest request)
        {
            return GetMultipleClaimValues(ExtractSecurityToken(request), ClaimTypes.Role);
        }

        /// <summary>
        /// Generates a JWT token for the specified user details and roles.
        /// </summary>
        /// <param name="fullName">The user's full name.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="userRoles">A list of user roles.</param>
        /// <returns>The generated JWT token as a string.</returns>
        public string GenerateJSONWebToken(string fullName, string email, List<string> userRoles)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Value.TokenKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim> {
                new(ClaimTypes.Name, fullName),
                new(ClaimTypes.Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(_jwtConfig.Value.TokenIssuer,
                _jwtConfig.Value.TokenIssuer,
                claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_jwtConfig.Value.TokenExpiryPeriod)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Extracts the claims from a JWT token string.
        /// </summary>
        /// <param name="token">The raw JWT token string.</param>
        /// <returns>A JwtSecurityToken object containing the token's claims.</returns>
        public JwtSecurityToken GetClaims(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be null or empty.");
            }

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = token.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    token = parts[1];
                }
            }

            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal;

            try
            {
                principal = ValidateToken(handler, token);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Token validation failed.", ex);
            }

            var decodedToken = handler.ReadToken(token) as JwtSecurityToken;
            return decodedToken ?? throw new ArgumentException("Invalid JWT token format.");
        }

        /// <summary>
        /// Gets the user's MacAddress from the request header.
        /// </summary>
        /// <param name="request">The HTTP request containing the MacAddress header.</param>
        /// <returns>The MacAddress value if present, otherwise an empty string.</returns>
        public string GetMacAddress(HttpRequest request)
        {
            return GetHeaderValue(request, "MacAddress");
        }

        /// <summary>
        /// Extracts claims from the token in the HTTP request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The JWT security token or null if invalid.</returns>
        private JwtSecurityToken ExtractSecurityToken(HttpRequest request)
        {
            var token = request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(token))
            {
                return new JwtSecurityToken();
            }

            var authHeaderValue = AuthenticationHeaderValue.Parse(token.ToString());
            if (authHeaderValue.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase))
            {
                return new JwtSecurityToken();
            }

            return GetClaims(token.ToString());
        }

        /// <summary>
        /// Extracts the value of a specific claim type from the token.
        /// </summary>
        /// <param name="securityToken"></param>
        /// <param name="claimType"></param>
        /// <returns>The first claim value for the specified claim type.</returns>
        private static string GetSingleClaimValue(JwtSecurityToken securityToken, string claimType)
        {
            return securityToken?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Extracts all values of a specific claim type from the token.
        /// </summary>
        /// <param name="securityToken"></param>
        /// <param name="claimType"></param>
        /// <returns>A list of claim values for the specified claim type.</returns>
        private static List<string> GetMultipleClaimValues(JwtSecurityToken securityToken, string claimType)
        {
            return securityToken?.Claims
                .Where(c => c.Type == claimType)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
        }

        /// <summary>
        /// Extracts the value of a specific header from the HTTP request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="headerName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static string GetHeaderValue(HttpRequest request, string headerName)
        {
            request.Headers.TryGetValue(headerName, out StringValues value);

            return value.ToString();
        }

        /// <summary>
        /// Validates the JWT token against the provided validation parameters.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private ClaimsPrincipal ValidateToken(JwtSecurityTokenHandler handler, string token)
        {
            return handler.ValidateToken(token, GetTokenValidationParameter(), out _);
        }

        private TokenValidationParameters GetTokenValidationParameter()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtConfig.Value.TokenIssuer,
                ValidAudience = _jwtConfig.Value.TokenIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Value.TokenKey))
            };
        }
    }
}
