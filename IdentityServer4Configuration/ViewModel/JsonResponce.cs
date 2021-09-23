using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.ViewModel
{
    public class JsonResponce
    {
        public bool Success { get; set; } = false;

        public string Code { get; set; } = "error";

        public string Message { get; set; }

        public object Data { get; set; } = null;
    }
}
