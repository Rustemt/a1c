using System.Security.Principal;
using System.Web.Routing;
using A1fxCrm.Web.Framework.Authorizations;


namespace A1fxCrm.Web.Framework.Security.Attributes
{
    public static class AuthorizationHelpers
    {
        public static bool Authorize(this RequestContext requestContext, Permission permission)
        {
            IPrincipal user = requestContext.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }
            return A1fxCrmAuthorization.Authorize(user.Identity.Name, permission);

        }

    }
}