using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hotelier_core_app.API.Attributes
{
    /// <summary>
    /// An authorization filter that enforces a specific policy for controller actions.
    /// </summary>
    public class PolicyAuthorizeAttribute : IAuthorizationFilter
    {
        /// <summary>
        /// The name of the policy to enforce.
        /// </summary>
        private readonly string _policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyAuthorizeAttribute"/> class with the specified policy.
        /// </summary>
        /// <param name="policy">The name of the policy to enforce.</param>
        public PolicyAuthorizeAttribute(string policy) => _policy = policy;

        /// <summary>
        /// Called during authorization to enforce the specified policy.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if the user is authenticated
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Resolve the authorization service
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            // Authorize the user against the specified policy
            var policyResult = authorizationService.AuthorizeAsync(user, null, _policy).Result;

            // If authorization fails, forbid access
            if (!policyResult.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
