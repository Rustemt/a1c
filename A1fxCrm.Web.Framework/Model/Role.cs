using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1fxCrm.Web.Framework.Model
{
    [Table("Role", Schema = "dbo")]
    public partial class Role
    {
        #region Primitive Properties
        [ScaffoldColumn(false)]
        public virtual int Id { get; set; }
        [Display(Name = "Role Name")]
        [Required(ErrorMessage = "Role Name is required")]
        [MaxLength(100)]
        public string RoleName { get; set; }

        public virtual string DisplayName { get; set; }

        #endregion

        #region Navigation Properties

        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<PermissionItem> PermissionItems { get; set; }

        #endregion

        #region [ Custom Propert(ies) ]

        [NotMapped]
        public virtual bool UserHasThisRole { get; set; }

        #endregion


    }
    #region [ Map Classes ]

    public class RolePermissionItemMap : EntityTypeConfiguration<Role>
    {
        public RolePermissionItemMap()
        {
            this.HasMany(u => u.PermissionItems)
             .WithMany(r => r.Roles)
             .Map(m =>
             {
                 m.ToTable("RolePermissionItem");
                 m.MapLeftKey("RoleId");
                 m.MapRightKey("PermissionItemId");
             });
        }
    }
    #endregion
}
