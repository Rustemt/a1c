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

        public static string ToRelativeDate(this DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return string.Format("{0} saniye önce", timeSpan.Seconds);

            if (timeSpan <= TimeSpan.FromMinutes(60))
                return timeSpan.Minutes > 1 ? String.Format("about {0} dakika önce", timeSpan.Minutes) :  "1 dk önce";

            if (timeSpan <= TimeSpan.FromHours(24))
                return timeSpan.Hours > 1 ? String.Format("about {0} saat önce", timeSpan.Hours) : "1 saat önce";

            if (timeSpan <= TimeSpan.FromDays(30))
                return timeSpan.Days > 1 ? String.Format("about {0} gün önce", timeSpan.Days) : "dün";

            if (timeSpan <= TimeSpan.FromDays(365))
                return timeSpan.Days > 30 ? String.Format("about {0} ay önce", timeSpan.Days / 30) : "1 ay once";

            return timeSpan.Days > 365 ? String.Format("about {0} yıl önce", timeSpan.Days / 365) : "about a year ago";
        }
    }
}