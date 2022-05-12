using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4Configuration.ViewModel;
using IdentityServer4Configuration.Models;
using IdentityServer4Configuration.Data;
using Microsoft.EntityFrameworkCore;
using IdentityServer4;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4Configuration.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiScopeController : ControllerBase
    {
        private readonly IdentityDB _dB;
        public UserManager<SysUsers> UserManager { get; set; }
        public RoleManager<SysRole> RoleManager { get; set; }

        public ApiScopeController(IdentityDB dB, UserManager<SysUsers> userManager, RoleManager<SysRole> roleManager)
        {
            this.UserManager = userManager;
            this.RoleManager = roleManager;
            this._dB = dB;
        }
        [HttpGet]
        public async Task<JsonResponce> GetApiScopesAsync()
        {
            return new JsonResponce
            {
                Success = true,
                Code = "success",
                Data = await _dB.SysApiScopes.AsNoTracking().ToListAsync()
            };
        }

        [HttpPost]
        public async Task<JsonResponce> CreateApiScopeAsync([FromBody]ApiScopeViewModel viewModel)
        {
            try
            {
                var scope = new ApiScope(viewModel.Name, viewModel.DisplayName);
                var apiScope = new SysApiScopeEntity()
                {
                    ApiScopeName = viewModel.ApiScopeName,
                    ApiScope = scope
                };
                apiScope.AddDataToEntity();
                await _dB.SysApiScopes.AddAsync(apiScope);
                if (await _dB.SaveChangesAsync() > 0)
                    return new JsonResponce
                    {
                        Success = true,
                        Code = "success-add-data"
                    };
                return new JsonResponce
                {
                    Success = false,
                    Code = "error-add-data"
                };
            }
            catch(Exception ex)
            {
                return new JsonResponce
                {
                    Success = false,
                    Code = "error",
                    Message = ex.Message
                };
            }
        }

        [HttpGet]
        public async Task<JsonResponce> GetApiResourceAsync()
        {
            return new JsonResponce
            {
                Success = true,
                Code = "success",
                Data = await _dB.SysApiResources.AsNoTracking().ToListAsync()
            };
        }

        [HttpPost]
        public async Task<JsonResponce> CreateApiResourceAsync([FromBody]ApiResourceViewModel viewModel)
        {
            try 
            {
                var apiresource = new ApiResource(viewModel.Name, viewModel.DisplayName);
                var apiResourceEntity = new SysApiResourceEntity
                {
                    ApiResourceName = viewModel.ApiResourceName,
                    ApiResource = apiresource
                };
                apiResourceEntity.AddDataToEntity();
                await _dB.SysApiResources.AddAsync(apiResourceEntity);
                if(await _dB.SaveChangesAsync() > 0)
                {
                    return new JsonResponce
                    {
                        Success = true,
                        Code = "success-add-data"
                    };
                }
                return new JsonResponce
                {
                    Success = false,
                    Code = "error-add-data"
                };
            }
            catch(Exception ex)
            {
                return new JsonResponce
                {
                    Success = false,
                    Code = "error",
                    Message = ex.Message
                };
            }
        }

        [HttpGet]
        public async Task<JsonResponce> GetApiClientAsync()
        {
            return new JsonResponce
            {
                Success = true,
                Code = "success",
                Data = await _dB.SysClients.AsNoTracking().ToListAsync()
            };
        }

        [HttpPost]
        public async Task<JsonResponce> CreateApiClientAsync([FromBody] ClientViewModel viewModel)
        {
            try
            {
                var client = new Client
                {
                    Enabled = viewModel.Enable,
                    ClientId = viewModel.ClientId,
                    ProtocolType = viewModel.ProtocolType,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(viewModel.Value.Sha256())
                    },
                    AllowedGrantTypes = new List<string>() { GrantType.ResourceOwnerPassword, GrantType.ClientCredentials, GrantType.Implicit }
                };
                var clientEntity = new SysClientEntity
                {
                    ClientId = viewModel.ClientId,
                    Client = client
                };
                clientEntity.AddDataToEntity();
                await _dB.SysClients.AddAsync(clientEntity);
                if (await _dB.SaveChangesAsync() > 0)
                    return new JsonResponce
                    {
                        Success = true,
                        Code = "success-add-data"
                    };
                return new JsonResponce
                {
                    Success = false,
                    Code = "error-add-data"
                };
            }
            catch(Exception ex)
            {
                return new JsonResponce
                {
                    Success = false,
                    Code = "error-add-data",
                    Message = ex.Message
                };
            }
        }

        [HttpGet]
        public async Task<JsonResponce> GetIdentityResourceAsync()
        {
            return new JsonResponce
            {
                Success = true,
                Code = "success",
                Data = await _dB.SysIdentityResources.AsNoTracking().ToListAsync()
            };
        }

        [HttpPost]
        public async Task<JsonResponce> CreateIdentityResourceAsync([FromBody] IdentityResourceViewModel viewModel)
        {
            try
            {
                #region old way
                /*var identityresource = new IdentityResource
                {
                    Name = viewModel.Name,
                    DisplayName = viewModel.DisplayName,
                    Required = viewModel.Required,
                    Emphasize = viewModel.Emphasize
                };
                var identityResourceEntity = new SysIdentityResourceEntity
                {
                    IdentityResourceName = viewModel.IdentityResourceName,
                    IdentityResource = identityresource
                };
                identityResourceEntity.AddDataToEntity();
                await __dB.SysIdentityResources.AddAsync(identityResourceEntity);
                if (await __dB.SaveChangesAsync() > 0)
                    return new JsonResponce
                    {
                        Success = true,
                        Code = "success-add-data"
                    };
                return new JsonResponce
                {
                    Success = false,
                    Code = "error-add-data"
                };*/
                #endregion
                List<IdentityResource> identityResources = new List<IdentityResource>();
                identityResources.Add(new IdentityResources.Address());
                identityResources.Add(new IdentityResources.Email());
                identityResources.Add(new IdentityResources.OpenId());
                identityResources.Add(new IdentityResources.Phone());
                identityResources.Add(new IdentityResources.Profile());

                foreach (var resource in identityResources)
                {
                    if (!_dB.SysIdentityResources.Any(r => r.IdentityResourceName == resource.Name))
                    {
                        resource.UserClaims.Add("name");
                        resource.UserClaims.Add("email");
                        SysIdentityResourceEntity identityResourceEntity = new SysIdentityResourceEntity();
                        identityResourceEntity.IdentityResource = resource;
                        identityResourceEntity.IdentityResourceName = identityResourceEntity.IdentityResource.Name;
                        identityResourceEntity.AddDataToEntity();
                        _dB.SysIdentityResources.Add(identityResourceEntity);
                    }
                }
                if(await _dB.SaveChangesAsync() > 0)
                {
                    return new JsonResponce
                    {
                        Success = true,
                        Code = "success-add-data"
                    };
                }
                return new JsonResponce
                {
                    Success = false,
                    Code = "error-add-data"
                };
            }
            catch (Exception ex)
            {
                return new JsonResponce
                {
                    Success = false,
                    Code = "error",
                    Message = ex.Message
                };
            }
        }

        [HttpGet]
        public async Task<JsonResponce> RunSeedData()
        {
            Secret secret = new Secret("gsbeearcimrkeest".Sha256(), "Germes Secret Key");

            if (!_dB.SysRoles.Any())
            {
                var roleAdministrator = new SysRole()
                {
                    Name = "administrator",
                    DisplayName = "Administrator",
                    NormalizedName = "ADMINISTRATOR"
                };
                await RoleManager.CreateAsync(roleAdministrator);

                var roleUser = new SysRole()
                {
                    Name = "user",
                    DisplayName = "User",
                    NormalizedName = "USER"
                };
                await RoleManager.CreateAsync(roleUser);
            }

            if (!_dB.SysUsers.Any())
            {
                var roleAdministrator = _dB.SysRoles.FirstOrDefault(r => r.Name == "administrator");
                var roleUser = _dB.SysRoles.FirstOrDefault(r => r.Name == "user");

                var user = new SysUsers
                {
                    UserName = "admin",
                    Email = "admin@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user, "1");
                await UserManager.SetLockoutEnabledAsync(user, false);
                if (roleAdministrator != null)
                {
                    await UserManager.AddToRoleAsync(user, roleAdministrator.Name);
                }

                var user2 = new SysUsers
                {
                    UserName = "chancellery",
                    Email = "chancellery@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user2, "1");
                await UserManager.SetLockoutEnabledAsync(user2, false);
                if (roleUser != null)
                {
                    await UserManager.AddToRoleAsync(user2, roleUser.Name);
                }

                var user3 = new SysUsers
                {
                    UserName = "leader",
                    Email = "leader@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user3, "1");
                await UserManager.SetLockoutEnabledAsync(user3, false);
                if (roleUser != null)
                {
                    await UserManager.AddToRoleAsync(user3, roleUser.Name);
                }

                var user4 = new SysUsers
                {
                    UserName = "assistant",
                    Email = "assistant@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user4, "1");
                await UserManager.SetLockoutEnabledAsync(user4, false);
                if (roleUser != null)
                {
                    await UserManager.AddToRoleAsync(user4, roleUser.Name);
                }

                var user5 = new SysUsers
                {
                    UserName = "controller",
                    Email = "controller@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user5, "1");
                await UserManager.SetLockoutEnabledAsync(user5, false);
                if (roleUser != null)
                {
                    await UserManager.AddToRoleAsync(user5, roleUser.Name);
                }

                var user6 = new SysUsers
                {
                    UserName = "staff",
                    Email = "staff@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user6, "1");
                await UserManager.SetLockoutEnabledAsync(user6, false);
                if (roleUser != null)
                {
                    await UserManager.AddToRoleAsync(user6, roleUser.Name);
                }

                var user7 = new SysUsers
                {
                    UserName = "head",
                    Email = "head@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user7, "1");
                await UserManager.SetLockoutEnabledAsync(user7, false);
                if (roleUser != null)
                {
                    await UserManager.AddToRoleAsync(user7, roleUser.Name);
                }

                var user8 = new SysUsers
                {
                    UserName = "assistant2",
                    Email = "assistant2@baik.uz",
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(user8, "1");
                await UserManager.SetLockoutEnabledAsync(user8, false);
                if (roleUser != null)
                {
                    await UserManager.AddToRoleAsync(user8, roleUser.Name);
                }
            }
            #region IdentityServer

            #region IdentityResources

            List<IdentityResource> identityResources = new List<IdentityResource>();
            identityResources.Add(new IdentityResources.Address());
            identityResources.Add(new IdentityResources.Email());
            identityResources.Add(new IdentityResources.OpenId());
            identityResources.Add(new IdentityResources.Phone());
            identityResources.Add(new IdentityResources.Profile());

            foreach (var resource in identityResources)
            {
                if (!_dB.SysIdentityResources.Any(r => r.IdentityResourceName == resource.Name))
                {
                    resource.UserClaims.Add("name");
                    resource.UserClaims.Add("email");
                    SysIdentityResourceEntity identityResourceEntity = new SysIdentityResourceEntity();
                    identityResourceEntity.IdentityResource = resource;
                    identityResourceEntity.IdentityResourceName = identityResourceEntity.IdentityResource.Name;
                    identityResourceEntity.AddDataToEntity();
                    _dB.SysIdentityResources.Add(identityResourceEntity);
                }
            }
            _dB.SaveChanges();

            #endregion

            #region ApiResources

            Dictionary<string, string> apis = new Dictionary<string, string>();
            apis.Add("GermesApiGateway", "Germes API Gateway");
            apis.Add("OrganizationService", "Organization Service");
            apis.Add("StaffManagementService", "Staff Management Service");
            apis.Add("ModuleService", "Module Service");
            apis.Add("DictionaryService", "Dictionary Service");
            apis.Add("EnvironmentService", "Environment Service");
            apis.Add("FormService", "Form Service");
            apis.Add("GridService", "Grid Service");
            apis.Add("EditorService", "Editor Service");
            apis.Add("FileService", "File Service");
            apis.Add("FilterService", "Filter Service");
            apis.Add("NotificationService", "Notification Service");
            apis.Add("ChatService", "Chat Service");
            apis.Add("DigitalSignService", "DigitalSign Service");
            apis.Add("NewsService", "News Service");
            apis.Add("PollService", "Poll Service");
            apis.Add("AttendenceScheduleService", "AttendenceSchedule Service");
            apis.Add("TaskService", "Task Service");
            apis.Add("DocumentService", "Document Service");
            apis.Add("DocumentMessageService", "Document Message Service");

            foreach (KeyValuePair<string, string> key in apis)
            {
                if (!_dB.SysApiResources.Any(a => a.ApiResourceName == key.Key))
                {
                    SysApiResourceEntity apiResourceEntity = new SysApiResourceEntity();
                    apiResourceEntity.ApiResource = new ApiResource(key.Key, key.Value);
                    apiResourceEntity.ApiResourceName = apiResourceEntity.ApiResource.Name;
                    apiResourceEntity.AddDataToEntity();
                    _dB.SysApiResources.Add(apiResourceEntity);
                }
            }
            _dB.SaveChanges();

            #endregion

            #region ApiScopes

            foreach (KeyValuePair<string, string> key in apis)
            {
                if (!_dB.SysApiScopes.Any(a => a.ApiScopeName == key.Key))
                {
                    SysApiScopeEntity apiScopeEntity = new SysApiScopeEntity();
                    apiScopeEntity.ApiScope = new ApiScope(key.Key, key.Value);
                    apiScopeEntity.ApiScopeName = apiScopeEntity.ApiScope.Name;
                    apiScopeEntity.AddDataToEntity();
                    _dB.SysApiScopes.Add(apiScopeEntity);
                }
            }
            _dB.SaveChanges();
            #endregion

            #region Clients
            //if (_dB.SysClients.FirstOrDefault(c => c.ClientId == "GermesApiGateway") == null)
            //{
            //    Client mainApiClient = new Client()
            //    {
            //        ClientId = "GermesApiGateway",
            //        ClientName = "Germes API Gateway",
            //        ClientSecrets = { secret },
            //        RequireClientSecret = false,
            //        AllowedGrantTypes = new List<string>() { GrantType.ClientCredentials, GrantType.AuthorizationCode },
            //        AllowedScopes = new List<string>() {
            //                IdentityServerConstants.StandardScopes.Profile,
            //                IdentityServerConstants.StandardScopes.Phone,
            //                IdentityServerConstants.StandardScopes.OpenId,
            //                IdentityServerConstants.StandardScopes.OfflineAccess,
            //                IdentityServerConstants.StandardScopes.Email,
            //                IdentityServerConstants.StandardScopes.Address
            //            },
            //        RedirectUris = new List<string>() { "" }
            //    };
            //    foreach (KeyValuePair<string, string> api in apis.Skip(1))
            //    {
            //        mainApiClient.AllowedScopes.Add(api.Key);
            //    }
            //    var sysApiGatewayClient = new SysClientEntity
            //    {
            //        ClientId = mainApiClient.ClientId,
            //        Client = mainApiClient
            //    };
            //    sysApiGatewayClient.AddDataToEntity();
            //    _dB.SysClients.Add(sysApiGatewayClient);
            //    _dB.SaveChanges();
            //}

            List<string> redirectUrls = new List<string>();
            redirectUrls.Add("http://localhost/callback");
            redirectUrls.Add("https://localhost/callback");
            redirectUrls.Add("http://localhost/silentrenew");
            redirectUrls.Add("https://localhost/silentrenew");
            redirectUrls.Add("https://localhost/silent_renew.html");
            redirectUrls.Add("http://localhost:7000/callback");
            redirectUrls.Add("https://localhost:7001/callback");
            redirectUrls.Add("http://localhost:7000/silentrenew");
            redirectUrls.Add("https://localhost:7001/silentrenew");
            redirectUrls.Add("http://localhost:7000/signin-oidc");
            redirectUrls.Add("https://localhost/signin-oidc");
            redirectUrls.Add("https://localhost:7001/callback");
            redirectUrls.Add("https://localhost:9090/silent_renew.html");
            List<string> postLogoutRedirectUris = new List<string>() {
                "http://localhost/", "https://localhost/",
                "http://localhost:7000/", "https://localhost:7001/",
                "http://localhost/logout", "https://localhost/logout",
                "http://localhost:7000/signout-oidc",
                "https://localhost/signout-oidc"
            };
            List<string> allowedCorsOrigins = new List<string> {
                "http://localhost:7000", "https://localhost:7001",
                "http://localhost", "https://localhost"
            };

            if (_dB.SysClients.FirstOrDefault(c => c.ClientId == "GermesWeb") == null)
            {
                Client webClient = new Client();
                webClient.ClientUri = "https://localhost";
                webClient.ClientId = "GermesWeb";
                webClient.ClientName = "Germes Web";
                webClient.ClientSecrets = new List<Secret>() { secret };
                webClient.RedirectUris = redirectUrls;
                webClient.PostLogoutRedirectUris = postLogoutRedirectUris;
                webClient.AllowedCorsOrigins = allowedCorsOrigins;
                webClient.AllowAccessTokensViaBrowser = true;
                webClient.AllowOfflineAccess = true;
                webClient.AllowedScopes = new List<string>() {
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Phone,
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.Address,
                    "GermesApiGateway"
                };
                foreach (KeyValuePair<string, string> api in apis.Skip(1))
                {
                    webClient.AllowedScopes.Add(api.Key);
                }
                webClient.AllowedGrantTypes = new List<string>() { GrantType.ResourceOwnerPassword, GrantType.ClientCredentials, GrantType.Implicit };
                webClient.RequireClientSecret = false;
                webClient.RequireConsent = false;
                //webClient.RefreshTokenUsage = TokenUsage.ReUse;
                //webClient.RefreshTokenExpiration = TokenExpiration.Sliding;
                //webClient.SlidingRefreshTokenLifetime = 120;
                webClient.AccessTokenLifetime = 3600;
                var sysWebClient = new SysClientEntity
                {
                    ClientId = webClient.ClientId,
                    Client = webClient
                };
                sysWebClient.AddDataToEntity();
                _dB.SysClients.Add(sysWebClient);
                _dB.SaveChanges();
                #endregion
                #endregion
            }
            return new JsonResponce
            {
                Success = true,
                Code = "success"
            };
        }

        [HttpPost]
        public async Task<JsonResponce> CreateSeedApiClient()
        {
            try
            {
                var client = new Client();
                client.ClientUri = "https://localhost";
                client.ClientId = "Crypto";
                client.ClientName = "Crypto";
                client.RedirectUris = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000" };
                client.PostLogoutRedirectUris = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000" };
                client.AllowedCorsOrigins = new List<string>() { "https://localhost:7001", "http://localhost:7000", "https://localhost:5001", "http://localhost:5000" };
                client.AllowAccessTokensViaBrowser = true;
                client.AllowedScopes = new List<string>() { "Crypto", IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile };
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
                await _dB.SysClients.AddAsync(webClient);
                _dB.SaveChanges();
                return new JsonResponce { Success = true, Code = "success" };

            }
            catch(Exception ex)
            {
                return new JsonResponce() { Code = "error", Success = false, Message = ex.Message };
            }
        }

        [HttpDelete]
        public async Task<JsonResponce> DeleteApiCilent(string clientId)
        {
            try
            {
                var client = new SysClientEntity { ClientId = clientId };
                _dB.Entry(client).State = EntityState.Deleted;
                await _dB.SaveChangesAsync();
                return new JsonResponce { Success = true, Code = "success" };
            }
            catch (Exception ex)
            {
                return new JsonResponce() { Code = "error", Success = false, Message = ex.Message };
            }
        }
    }
}
