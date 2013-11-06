using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using A1fxCrm.Web.Framework.DataContexts;
using A1fxCrm.Web.Framework.Model;

namespace A1fxCrm.Web.Framework.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class RequiredLogOnAttribute : FilterAttribute, IAuthorizationFilter
    {
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            var requestContext = filterContext.RequestContext;
            var exclusiveFlag = "AuthorizationExclusive";
            var exclusive = requestContext.HttpContext.Items[exclusiveFlag] == null ? false : (bool)requestContext.HttpContext.Items[exclusiveFlag];
            if (!exclusive)
            {
                if (AuthorizeCore(filterContext.RequestContext))
                {
                    //HttpCachePolicyBase cache = filterContext.HttpContext.Response.Cache;
                    //cache.SetProxyMaxAge(new TimeSpan(0L));
                    //cache.AddValidationCallback(new HttpCacheValidateHandler(this.CacheValidateHandler), null);
                }
                else
                {
                    this.HandleUnauthorizedRequest(filterContext);
                }
            }
            requestContext.HttpContext.Items[exclusiveFlag] = Exclusive;
        }
        //private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        //{
        //    validationStatus = this.OnCacheAuthorization(new HttpContextWrapper(context));
        //}
        //protected virtual HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext)
        //{
        //    if (!this.AuthorizeCore(httpContext))
        //    {
        //        return HttpValidationStatus.IgnoreThisRequest;
        //    }
        //    return HttpValidationStatus.Valid;
        //}
        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }

        protected virtual bool AuthorizeCore(RequestContext requestContext)
        {
            var authenticated = requestContext.HttpContext.User.Identity.IsAuthenticated;
            if (authenticated && RequiredAdministrator)
            {
                using (CommonContext db = new CommonContext())
                {
                    User user = db.Users.Include("Roles.PermissionItems").FirstOrDefault(c => c.UserName == requestContext.HttpContext.User.Identity.Name);
                    if (user == null)
                    {
                        return false;
                    }
                    else
                    {
                        if (user.IsAdministrator.HasValue && user.IsAdministrator.Value)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }
            else
            {
                return authenticated;
            }
        }
        public bool RequiredAdministrator { get; set; }
        public bool Exclusive { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizationAttribute : RequiredLogOnAttribute
    {
        protected override bool AuthorizeCore(RequestContext requestContext)
        {
            var authorized = base.AuthorizeCore(requestContext);
            if (authorized)
            {
                var permission = new Permission() { Group = this.Group, Name = this.Name };

                return requestContext.Authorize(permission);
            }
            else
            {
                return authorized;
            }
        }


        public string Group { get; set; }
        public string Name { get; set; }
    }


}
