using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hotelier_core_app.API.Attributes
{
    public class PolicyAuthorizeAttribute : IAsyncAuthorizationFilter
    {
        private readonly string _policy;

        public PolicyAuthorizeAttribute(string policy) => _policy = policy;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new ForbidResult();
                return;
            }

            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var policyResult = await authorizationService.AuthorizeAsync(user, null, _policy);

            if (!policyResult.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
