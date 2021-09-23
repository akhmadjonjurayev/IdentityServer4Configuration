using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Models
{
    public class SysUserClaim : IdentityUserClaim<Guid>
    {
        public virtual SysUsers User { get; set; }
    }
}
