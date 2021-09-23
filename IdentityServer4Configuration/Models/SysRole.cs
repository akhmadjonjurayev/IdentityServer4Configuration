using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Models
{
    public class SysRole : IdentityRole<Guid>
    {
        public string DisplayName { get; set; }
        public virtual ICollection<SysUserRole> UserRoles { get; set; }
        public virtual ICollection<SysRoleClaim> RoleClaims { get; set; }
    }
}
