using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
 
using A1fxCrm.Web.Framework.Security;
using A1fxCrm.Web.Framework.DataContexts;
using A1fxCrm.Web.Framework.Model;

namespace A1fxCrm.Web.Framework.Authorizations
{
    public class A1fxCrmAuthorization
    {
        public static bool Authorize(string userName, Permission permission)
        {
            using (CommonContext db = new CommonContext())
            {
                bool changeDbFlag = false;
                User user = db.Users.Include("Roles.PermissionItems").FirstOrDefault(c => c.UserName == userName);
                if (user == null)
                {
                    //user = new GKFX.Models.Common.User { UserName = userName };
                    //db.Users.Add(user);
                    //changeDbFlag = true;
                    return false;
                }
                else if (user.IsAdministrator.HasValue && user.IsAdministrator.Value)
                {
                    //user is admin so can se every where
                    return true;
                }

                PermissionItem permissionItem = db.PermissionItems.FirstOrDefault(c => c.Name == permission.Name);
                if (permissionItem == null)
                {
                    permissionItem = new  PermissionItem { Name = permission.Name, DisplayName = permission.DisplayName, Group = permission.Group };
                    db.PermissionItems.Add(permissionItem);
                    changeDbFlag = true;
                }
                else if (permissionItem.RequiredAdministrator)
                {
                    //Resource needs admin permission and user is not admin.
                    return false;
                }

                if (changeDbFlag)
                {
                    db.SaveChanges();
                }

                if (user.Roles != null)
                {
                    if (user.Roles.Any(c => c.PermissionItems.Any(d => d.Name == permission.Name )))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;

            }
            
        }
    }
}