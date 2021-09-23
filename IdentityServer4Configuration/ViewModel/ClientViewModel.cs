using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.ViewModel
{
    public class ClientViewModel
    {
        public string ClientId { get; set; }

        public bool Enable { get; set; }

        public string ProtocolType { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }
    }
}
