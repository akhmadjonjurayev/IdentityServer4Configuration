using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4Configuration.Models;
using IdentityServer4Configuration.ViewModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Service
{
    public interface IBaseIdentityService
    {
        public Task<JsonResponce> CreateUserAsync(CreateUserViewModel model);

        public Task<JsonResponce> SignInAsync(SignInViewModel model);

        public Task<JsonResponce> UpdateUserAsync(UpdateUserViewModel model);

        public Task<JsonResponce> RemoveUserAsync(string userId);

        public JsonResponce GetUsers();

        public Task<JsonResponce> SignInWithCertificate(LoginViewModel model);

        public Task<JsonResponce> CheckMethod(SignInViewModel viewModel);

        public Task<JsonResponce> LoginAs(ITokenService TS, IUserClaimsPrincipalFactory<SysUsers> principalFactory,
            IdentityServerOptions options, Guid id);
    }
}
