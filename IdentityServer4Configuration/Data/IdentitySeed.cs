using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4Configuration.Models;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4Configuration.Data
{
    public class IdentitySeed
    {
        private readonly IdentityDB _identityDB;
        public IdentitySeed(IdentityDB identityDB)
        {
            this._identityDB = identityDB;
            SeedData();
        }

        public void SeedData()
        {
            Dictionary<string, string> apis = new Dictionary<string, string>();
            apis.Add("Crypto", "Cryptography");

            foreach (KeyValuePair<string, string> key in apis)
            {
                if (!_identityDB.SysApiResources.Any(a => a.ApiResourceName == key.Key))
                {
                    SysApiResourceEntity apiResourceEntity = new SysApiResourceEntity();
                    apiResourceEntity.ApiResource = new ApiResource(key.Key, key.Value);
                    apiResourceEntity.ApiResourceName = apiResourceEntity.ApiResource.Name;
                    apiResourceEntity.AddDataToEntity();
                    _identityDB.SysApiResources.Add(apiResourceEntity);
                }
            }
            _identityDB.SaveChanges();

            foreach (KeyValuePair<string, string> key in apis)
            {
                if (!_identityDB.SysApiScopes.Any(a => a.ApiScopeName == key.Key))
                {
                    SysApiScopeEntity apiScopeEntity = new SysApiScopeEntity();
                    apiScopeEntity.ApiScope = new ApiScope(key.Key, key.Value);
                    apiScopeEntity.ApiScopeName = apiScopeEntity.ApiScope.Name;
                    apiScopeEntity.AddDataToEntity();
                    _identityDB.SysApiScopes.Add(apiScopeEntity);
                }
            }
            _identityDB.SaveChanges();

            var client = new Client();
            client.ClientUri = "https://localhost";
            client.ClientId = "Crypto";
            client.ClientName = "Crypto";
            client.RedirectUris = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000" };
            client.PostLogoutRedirectUris = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000" };
            client.AllowedCorsOrigins = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000" };
            client.AllowAccessTokensViaBrowser = true;
            client.AllowedScopes = new List<string>() { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile };
            foreach(var scope in apis)
                client.AllowedScopes.Add(scope.Key);
            client.AllowedGrantTypes = new List<string>() { GrantType.ResourceOwnerPassword };
            client.RequireClientSecret = false;
            client.RequireConsent = false;
            client.AccessTokenLifetime = 1200;

            var webClient = new SysClientEntity
            {
                ClientId = client.ClientId,
                Client = client
            };
            webClient.AddDataToEntity();
            _identityDB.SysClients.Add(webClient);

            _identityDB.SaveChanges();
        }
    }
}
