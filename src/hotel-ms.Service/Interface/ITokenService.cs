using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace hotelier_core_app.Service.Interface
{
    public interface ITokenService : IAutoDependencyService
    {
        string GetUserFullName(HttpRequest Request);

        string GetUserEmail(HttpRequest Request);

        List<string> GetUserRoles(HttpRequest Request);

        string GenerateJSONWebToken(string fullname, string email, List<string> userRoles);

        JwtSecurityToken GetClaims(string token);

        string GetMacAddress(HttpRequest Request);
    }
}
