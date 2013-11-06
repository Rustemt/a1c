using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using A1fxCrm.Web.Framework.Model;

namespace A1fxCrm.Web.Framework.Security
{
    public static class UserMapper
    {

        public static System.Web.Security.MembershipUser Map(string pname, User user, bool EFMembership)
        {

            if (EFMembership)
                return new MembershipUser(pname, user.UserName, user.Id, user.Email, string.Empty, user.Comment, user.IsApproved,
                                          false, user.CreatedDate, user.LastLoginDate ?? DateTime.Now, user.LastActivityDate ?? DateTime.Now, user.LastPasswordChangedDate,
                                          DateTime.Now, user.FirstName, user.LastName, user.TimeZone ?? 0);
            else
                return new System.Web.Security.MembershipUser(pname, user.UserName, user.Id, user.Email, string.Empty, user.Comment, user.IsApproved,
                                                              false, user.CreatedDate, user.LastLoginDate ?? DateTime.Now, user.LastActivityDate ?? DateTime.Now, user.LastPasswordChangedDate,
                                                              user.LastLockoutDate);
        }

    }
}
