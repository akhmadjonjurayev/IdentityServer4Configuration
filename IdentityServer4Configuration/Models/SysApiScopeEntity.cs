using IdentityServer4.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Models
{
    public class SysApiScopeEntity
    {
        public string ApiScopeData { get; set; }

        [Key]
        public string ApiScopeName { get; set; }

        [NotMapped]
        public ApiScope ApiScope { get; set; }

        public void AddDataToEntity()
        {
            ApiScopeData = JsonConvert.SerializeObject(ApiScope);
            ApiScopeName = ApiScope.Name;
        }

        public void MapDataFromEntity()
        {
            ApiScope = JsonConvert.DeserializeObject<ApiScope>(ApiScopeData);
            ApiScopeName = ApiScope.Name;
        }
    }
}
