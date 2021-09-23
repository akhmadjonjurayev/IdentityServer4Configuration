using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4Configuration.Data;
using IdentityServer4Configuration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Service
{
    public class ResourceStore : IResourceStore
    {
        private readonly IdentityDB _context;
        private readonly ILogger _logger;

        public ResourceStore(IdentityDB context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("ResourceStore");
        }

        public Task<ApiResource> FindApiResourceAsync(string name)
        {
            var apiResource = _context.SysApiResources.First(t => t.ApiResourceName == name);
            apiResource.MapDataFromEntity();
            return Task.FromResult(apiResource.ApiResource);
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            List<SysApiResourceEntity> ress = _context.SysApiResources.ToList();
            foreach (SysApiResourceEntity res in ress)
            {
                res.MapDataFromEntity();
            }
            return Task.FromResult(ress.Where(ar => apiResourceNames.Contains(ar.ApiResourceName)).Select(a => a.ApiResource).AsEnumerable());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));


            var apiResources = new List<ApiResource>();
            var apiResourcesEntities = from i in _context.SysApiResources
                                       where scopeNames.Contains(i.ApiResourceName)
                                       select i;

            foreach (var apiResourceEntity in apiResourcesEntities)
            {
                apiResourceEntity.MapDataFromEntity();
                apiResources.Add(apiResourceEntity.ApiResource);
            }

            return Task.FromResult(apiResources.AsEnumerable());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            List<SysApiResourceEntity> ress = _context.SysApiResources.ToList();
            foreach (SysApiResourceEntity res in ress)
            {
                res.MapDataFromEntity();
            }
            List<ApiResource> apis = ress.Select(a => a.ApiResource).ToList();
            List<ApiResource> result = new List<ApiResource>();
            foreach (string scope in scopeNames)
            {
                result.AddRange(apis.Where(a => a.Scopes.Contains(scope)).AsEnumerable());
            }
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            List<SysApiScopeEntity> apis = _context.SysApiScopes.ToList();
            foreach (SysApiScopeEntity api in apis)
            {
                api.MapDataFromEntity();
            }
            List<ApiScope> apiscopes = apis.Where(s => scopeNames.Contains(s.ApiScopeName)).Select(asc => asc.ApiScope).ToList();
            return Task.FromResult(apiscopes.AsEnumerable());
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var identityResources = new List<IdentityResource>();
            var identityResourcesEntities = from i in _context.SysIdentityResources
                                            where scopeNames.Contains(i.IdentityResourceName)
                                            select i;

            foreach (var identityResourceEntity in identityResourcesEntities)
            {
                identityResourceEntity.MapDataFromEntity();
                identityResources.Add(identityResourceEntity.IdentityResource);
            }

            return Task.FromResult(identityResources.AsEnumerable());
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var identityResources = new List<IdentityResource>();
            var identityResourcesEntities = from i in _context.SysIdentityResources
                                            where scopeNames.Contains(i.IdentityResourceName)
                                            select i;

            foreach (var identityResourceEntity in identityResourcesEntities)
            {
                identityResourceEntity.MapDataFromEntity();
                identityResources.Add(identityResourceEntity.IdentityResource);
            }

            return Task.FromResult(identityResources.AsEnumerable());
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            var apiResourcesEntities = _context.SysApiResources.ToList();
            var identityResourcesEntities = _context.SysIdentityResources.ToList();
            var identityApiScopes = _context.SysApiScopes.ToList();

            var apiResources = new List<ApiResource>();
            var identityResources = new List<IdentityResource>();
            var apiScopes = new List<ApiScope>();

            foreach (var apiResourceEntity in apiResourcesEntities)
            {
                apiResourceEntity.MapDataFromEntity();
                apiResources.Add(apiResourceEntity.ApiResource);
            }

            foreach (var identityResourceEntity in identityResourcesEntities)
            {
                identityResourceEntity.MapDataFromEntity();
                identityResources.Add(identityResourceEntity.IdentityResource);
            }

            foreach (var identityApiScopeEntity in identityApiScopes)
            {
                identityApiScopeEntity.MapDataFromEntity();
                apiScopes.Add(identityApiScopeEntity.ApiScope);
            }

            var result = new Resources(identityResources, apiResources, apiScopes);
            return Task.FromResult(result);
        }
    }
}
