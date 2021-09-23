using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Models
{
    public class SysUsers : IdentityUser<Guid>
    {
        public virtual ICollection<SysUserClaim> Claims { get; set; }

        public virtual ICollection<SysUserLogin> Logins { get; set; }

        public virtual ICollection<SysUserToken> Tokens { get; set; }

        public virtual ICollection<SysUserRole> Roles { get; set; }
    }
}
