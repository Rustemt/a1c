using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.DataAccess;
using System.Web.Management;
using System.Web.Security;
using System.Web.Util;
 
using A1fxCrm.Web.Framework.DataContexts;
using A1fxCrm.Web.Framework.Model;

//

namespace A1fxCrm.Web.Framework.Security
{
    public class EFRoleProvider : System.Web.Security.RoleProvider
    {


        CommonContext db = new CommonContext();


        public override void Initialize(string name, NameValueCollection config)
        {
            //HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
            if (config == null)
                throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name))
                name = "SqlRoleProvider";
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "RoleSqlProvider_description");
            }
            base.Initialize(name, config);



            string temp = config["connectionStringName"];
            if (temp == null || temp.Length < 1)
                throw new ProviderException("Connection_name_not_specified");


            config.Remove("connectionStringName");
            config.Remove("commandTimeout");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException("Provider_unrecognized_attribute, " + attribUnrecognized);
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 256, "usernames");

            var users = db.Users.Include("Roles").Where(c=>usernames.Contains(c.UserName)).ToList();

            var roles = db.Roles.Where(c=>roleNames.Contains(c.RoleName)).ToList();

            foreach (var user in users)
                foreach (var role in roles)
                {
                    if (!user.Roles.Contains(role))
                        user.Roles.Add(role);
                }

            db.SaveChanges();

        }



        public override void CreateRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");


            try
            {
                var query = from r in db.Roles
                            where r.RoleName == roleName
                            select r;
                if (query.Any())
                    throw new ProviderException("Provider_role_already_exists, " + roleName);

                db.Roles.Add(new Role()
                {
                    RoleName = roleName,

                });

                db.SaveChanges();
            }
            catch
            {
                throw;
            }

        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");

            var query = from r in db.Roles
                        where r.RoleName == roleName
                        select new { r, child = r.Users.Count() };
            if (query.Any())
            {
                var r = query.First();
                if (r.child > 0 && throwOnPopulatedRole)
                    throw new ProviderException("Role_is_not_empty");

                db.Roles.Remove(r.r);
                db.SaveChanges();

                return true;

            }

            return false;

        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 256, "usernameToMatch");


            var query = from r in db.Roles
                        from u in r.Users
                        where r.RoleName == roleName && u.UserName.Contains(usernameToMatch)
                        select u.UserName;

            return query.ToArray();

        }

        public override string[] GetAllRoles()
        {

            return db.Roles.Select(r => r.RoleName).ToArray();

        }

        public override string[] GetRolesForUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 256, "username");

            var query = from u in db.Users
                        from r in u.Roles
                        where u.UserName == username
                        select r.RoleName;

            return query.ToArray();

        }

        public override string[] GetUsersInRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");

            var query = from u in db.Users
                        from r in db.Roles
                        where r.RoleName == roleName
                        select u.UserName;

            return query.ToArray();

        }

        public override bool IsUserInRole(string username, string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            SecUtility.CheckParameter(ref username, true, false, true, 256, "username");

            if (username.Length < 1)
                return false;


            var query = from u in db.Users
                        from r in u.Roles
                        where r.RoleName == roleName
                           && u.UserName == username

                        select u.UserName;

            return query.Any();

        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 256, "usernames");


            foreach (var username in usernames)
            {
                var user = db.Users.Include("Roles").Where(u => u.UserName == username).FirstOrDefault();
                if (user != null)
                    foreach (var rolename in roleNames)
                    {
                        var role = user.Roles.FirstOrDefault(r => r.RoleName == rolename);
                        if (role != null)
                            user.Roles.Remove(role);
                    }
            }
            db.SaveChanges();

        }

        public override bool RoleExists(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");

            return db.Roles.Where(r => r.RoleName == roleName).Any();

        }


        public override string ApplicationName
        {
            get
            {
                return string.Empty;
            }
            set { }
        }
    }
}
