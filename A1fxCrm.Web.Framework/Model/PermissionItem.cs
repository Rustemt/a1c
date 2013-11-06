using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1fxCrm.Web.Framework.Model
{
    [Table("PermissionItem", Schema = "dbo")]
    public partial class PermissionItem  
    {
        #region Primitive Properties
        [ScaffoldColumn(false)]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string AreaName { get; set; }
        public virtual string Group { get; set; }
        public virtual bool RequiredAdministrator { get; set; }

        #endregion

        #region Navigation Properties

        public virtual ICollection<Role> Roles { get; set; }

        #endregion

        #region [ Custom Propert(ies) ]

        [NotMapped]
        public bool RoleHasThisPermission { get; set; }

        #endregion
    }
}
