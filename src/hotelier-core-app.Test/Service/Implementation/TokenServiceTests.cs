using hotelier_core_app.Model.Configs;
using hotelier_core_app.Service.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Collections.Generic;
using Xunit;
using Microsoft.IdentityModel.Tokens;

namespace Service.Implementation
{
    public class TokenServiceTests
    {
        private readonly IOptions<JwtConfig> _jwtConfig = Substitute.For<IOptions<JwtConfig>>();
        private readonly JwtConfig _config = new() { TokenKey = "supersecretkey1234567890abcdef12345678", TokenIssuer = "issuer", TokenExpiryPeriod = "60" };

        private TokenService CreateService()
        {
            _jwtConfig.Value.Returns(_config);
            return new TokenService(_jwtConfig);
        }

        [Fact]
        public void GenerateJSONWebToken_ShouldReturnToken()
        {
            var service = CreateService();
            var token = service.GenerateJSONWebToken("John Doe", "john@example.com", new() { "Admin", "User" });
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GetUserFullName_ShouldReturnName_WhenClaimExists()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            var token = service.GenerateJSONWebToken("John Doe", "john@example.com", new() { "Admin" });
            httpContext.Request.Headers.Authorization = $"Bearer {token}";
            var name = service.GetUserFullName(httpContext.Request);
            Assert.Equal("John Doe", name);
        }

        [Fact]
        public void GetUserEmail_ShouldReturnEmail_WhenClaimExists()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            var token = service.GenerateJSONWebToken("John Doe", "john@example.com", new List<string> { "Admin" });
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            var email = service.GetUserEmail(httpContext.Request);
            Assert.Equal("john@example.com", email);
        }

        [Fact]
        public void GetUserRoles_ShouldReturnRoles_WhenClaimsExist()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            var token = service.GenerateJSONWebToken("John Doe", "john@example.com", new List<string> { "Admin", "User" });
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            var roles = service.GetUserRoles(httpContext.Request);
            Assert.Contains("Admin", roles);
            Assert.Contains("User", roles);
        }

        [Fact]
        public void GenerateJSONWebToken_ShouldHandleEmptyRoles()
        {
            var service = CreateService();
            var token = service.GenerateJSONWebToken("Jane Doe", "jane@example.com", new());
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GetUserFullName_ShouldReturnNull_WhenNoAuthorizationHeader()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            var name = service.GetUserFullName(httpContext.Request);
            Assert.Empty(name);
        }

        [Fact]
        public void GetUserEmail_ShouldReturnNull_WhenNoAuthorizationHeader()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            var email = service.GetUserEmail(httpContext.Request);
            Assert.Empty(email);
        }

        [Fact]
        public void GetUserRoles_ShouldReturnEmpty_WhenNoAuthorizationHeader()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            var roles = service.GetUserRoles(httpContext.Request);
            Assert.Empty(roles);
        }

        [Fact]
        public void GetUserFullName_ShouldThrowException_WhenTokenMalformed()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer malformed.token";
            Assert.Throws<SecurityTokenException>(() => service.GetUserFullName(httpContext.Request));
        }

        [Fact]
        public void GetUserEmail_ShouldThrowException_WhenTokenMalformed()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer malformed.token";
            Assert.Throws<SecurityTokenException>(() => service.GetUserEmail(httpContext.Request));
        }

        [Fact]
        public void GetUserRoles_ShouldThrowException_WhenTokenMalformed()
        {
            var service = CreateService();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer malformed.token";
            Assert.Throws<SecurityTokenException>(() => service.GetUserRoles(httpContext.Request));
        }
    }
}
