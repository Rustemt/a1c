using A1fxCrm.Web.Framework.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1fxCrm.Web.Framework.DataContexts
{
    public class CommonContext : DbContext
    {
        #region [ ObjectSet Propert(ies) ]
 
        public DbSet<PermissionItem> PermissionItems { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
 

        #endregion

        public CommonContext()
            : base("Default")
        {

            ////DropAndReCreate if in debug and model is changed. 
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
             
          //  }
                 Database.SetInitializer<CommonContext>(null);
        }

        public ObjectContext ObjectContext()
        {
            return ((IObjectContextAdapter)this).ObjectContext;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            modelBuilder.Configurations.Add(new UserMaps());
            modelBuilder.Configurations.Add(new RolePermissionItemMap());


            base.OnModelCreating(modelBuilder);
        }

        
    }

}
