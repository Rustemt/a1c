using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1fxCrm.Web.Framework.Model
{
    public class Enumerations
    {
        public enum UserStatus : byte
        {
            Approved = 1,
            Locked = 2,
            Expire = 3
        }
    }
}
