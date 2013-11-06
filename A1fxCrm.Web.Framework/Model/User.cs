using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1fxCrm.Web.Framework.Model
{
   [Table("User", Schema = "dbo")]
    public partial class User 
    {
        #region Primitive Properties
       
       [ScaffoldColumn(false)]
        public virtual int Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }

        [DefaultValue(false)]
        public virtual bool? IsAdministrator { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [Display(Name = "User Name")]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(128)]
        public string PasswordSalt { get; set; }

        public string Password { get; set; }

        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Comment { get; set; }

        [Display(Name = "Approved?")]
        public bool IsApproved { get; set; }

        [Display(Name = "Crate Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Last Activity Date")]
        public DateTime? LastActivityDate { get; set; }

        [Display(Name = "Last Password Changed Date")]
        public DateTime LastPasswordChangedDate { get; set; }

        [Display(Name = "Last Lockout Date")]
        public DateTime LastLockoutDate { get; set; }

        [Display(Name = "Failed Password Attempt Window Start")]
        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        [Display(Name = "Failed Password Answer Attempt Count")]
        public int FailedPasswordAnswerAttemptCount { get; set; }

        [Display(Name = "Failed Password Attempt Count")]
        public int FailedPasswordAttemptCount { get; set; }

        [Display(Name = "Time Zone")]
        public int? TimeZone { get; set; }

        public Boolean IsAnonymous { get; set; }


        public Byte Status { get; set; }

        public string FullName { get; set; }

        public int PasswordFormat { get; set; }

        #endregion

        #region Navigation Properties

        public virtual ICollection<Role> Roles { get; set; }
 
        #endregion

        #region [ Custom Propert(ies) ]

        #endregion

    }

    #region [ Map Classes ]

    public class UserMaps : EntityTypeConfiguration<User>
    {
        public UserMaps()
        {
            this.HasMany(u => u.Roles)
             .WithMany(r => r.Users)
             .Map(m =>
             {
                 m.ToTable("UserRole");
                 m.MapLeftKey("UserId");
                 m.MapRightKey("RoleId");
             });
 
        }
    }
 
    #endregion
 
}
