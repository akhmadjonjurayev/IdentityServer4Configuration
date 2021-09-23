using IdentityServer4Configuration.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
