using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4Configuration.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4Configuration.Data
{
    public class IdentitySeed
    {
        private readonly IdentityDB _identityDB;
        private readonly UserManager<SysUsers> _userManager;
        private readonly RoleManager<SysRole> _roleManager;
        public IdentitySeed(IdentityDB identityDB,
            UserManager<SysUsers> userManager,
            RoleManager<SysRole> roleManager)
        {
            this._identityDB = identityDB;
            this._roleManager = roleManager;
            this._userManager = userManager;
            SeedData();
        }

        public void SeedData()
        {
            Dictionary<string, string> apis = new Dictionary<string, string>();
            apis.Add("crypto", "cryptography");

            if(_identityDB.Users.Count() == 0)
            {
                var user = new SysUsers
                {
                    UserName = "kalinus",
                    PersonId = System.Guid.Parse("5d09de05-2cf2-4037-a870-08da326b7122"),
                    Email = "formyproject1999@gmail.com",
                    PhoneNumber = "+998946644275"
                };
                _userManager.CreateAsync(user, "burgut").Wait();
                _userManager.SetLockoutEnabledAsync(user, false).Wait();
                _roleManager.CreateAsync(new SysRole { Name = "Administrator" });
                _userManager.AddToRoleAsync(user, "Administrator");
                _identityDB.SaveChanges();
            }

            List<IdentityResource> identityResources = new List<IdentityResource>();
            identityResources.Add(new IdentityResources.Address());
            identityResources.Add(new IdentityResources.Email());
            identityResources.Add(new IdentityResources.OpenId());
            identityResources.Add(new IdentityResources.Phone());
            identityResources.Add(new IdentityResources.Profile());

            foreach (var resource in identityResources)
            {
                if (!_identityDB.SysIdentityResources.Any(r => r.IdentityResourceName == resource.Name))
                {
                    resource.UserClaims.Add("name");
                    resource.UserClaims.Add("email");
                    SysIdentityResourceEntity identityResourceEntity = new SysIdentityResourceEntity();
                    identityResourceEntity.IdentityResource = resource;
                    identityResourceEntity.IdentityResourceName = identityResourceEntity.IdentityResource.Name;
                    identityResourceEntity.AddDataToEntity();
                    _identityDB.SysIdentityResources.Add(identityResourceEntity);
                }
            }

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

            if(_identityDB.SysClients.Count() == 0)
            {
                var client = new Client();
                client.ClientUri = "http://localhost:5000";
                client.ClientId = "crypto";
                client.ClientName = "Crypto";
                client.RedirectUris = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000", "http://localhost:5000/wwwroot/callback.html" };
                client.PostLogoutRedirectUris = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000" };
                client.AllowedCorsOrigins = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000", "https://localhost:5042" };
                client.AllowAccessTokensViaBrowser = true;
                client.AllowedScopes = new List<string>() { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile };
                foreach (var scope in apis)
                    client.AllowedScopes.Add(scope.Key);
                client.AllowedGrantTypes = new List<string>() { GrantType.AuthorizationCode, GrantType.ClientCredentials, GrantType.ResourceOwnerPassword };
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
            }

            _identityDB.SaveChanges();
        }
    }
}
