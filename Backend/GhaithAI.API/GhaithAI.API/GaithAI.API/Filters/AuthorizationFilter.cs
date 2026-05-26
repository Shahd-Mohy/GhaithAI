using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace GhaithAI.API.Filters
{
    public class AuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(
            AuthorizationFilterContext context)
        {
            var user =
                context.HttpContext.User;

            if (user.Identity == null ||
                !user.Identity.IsAuthenticated)
            {
                context.Result =
                    new UnauthorizedResult();
            }
        }
    }
}