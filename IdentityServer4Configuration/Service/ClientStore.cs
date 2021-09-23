using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4Configuration.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Service
{
    public class ClientStore : IClientStore
    {
        private readonly IdentityDB _context;
        private readonly ILogger _logger;
        public ClientStore(IdentityDB context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("ClientStore");
        }
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var client = _context.SysClients.First(p => p.ClientId == clientId);
            client.MapDataFromEntity();
            return Task.FromResult(client.Client);
        }
    }
}
