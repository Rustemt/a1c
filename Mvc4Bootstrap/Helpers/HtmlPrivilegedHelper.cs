using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace A1fxCrm.Web.Helpers
{
    public static class HtmlPrivilegedHelper
    {
        public static bool IsAdmin(this HtmlHelper html)
        {
            
            IPrincipal currentUser = HttpContext.Current.User;
            bool writeEnable = currentUser.IsInRole("admin");
            return writeEnable;
        }

        public static bool IsAdmin()
        {

            IPrincipal currentUser = HttpContext.Current.User;
            bool writeEnable = currentUser.IsInRole("admin");
            return writeEnable;
        }
        public static string UserName()
        {

            return HttpContext.Current.User.Identity.Name;
          
        }
    }
}