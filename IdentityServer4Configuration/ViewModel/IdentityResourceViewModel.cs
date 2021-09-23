using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.ViewModel
{
    public class IdentityResourceViewModel
    {
        public string IdentityResourceName { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public bool Required { get; set; }

        public bool Emphasize { get; set; }
    }
}
