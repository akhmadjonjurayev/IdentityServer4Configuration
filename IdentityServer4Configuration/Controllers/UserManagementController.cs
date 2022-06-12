using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4Configuration.Models;
using IdentityServer4Configuration.Service;
using IdentityServer4Configuration.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IBaseIdentityService _baseIdentityService;

        public UserManagementController(IBaseIdentityService baseIdentityService)
        {
            this._baseIdentityService = baseIdentityService;
        }
        #region CreateUser
        [HttpPost]
        public Task<JsonResponce> CreateUser([FromBody] CreateUserViewModel model)
        {
            return _baseIdentityService.CreateUserAsync(model);
        }
        #endregion

        #region SignIn
        [HttpPost]
        public Task<JsonResponce> SignIn([FromBody] SignInViewModel model)
        {
            return _baseIdentityService.SignInAsync(model);
        }
        #endregion

        #region Update
        [HttpPost]
        public Task<JsonResponce> UpdateUser([FromBody] UpdateUserViewModel model)
        {
            return _baseIdentityService.UpdateUserAsync(model);
        }
        #endregion

        #region Delete
        [HttpDelete]
        public Task<JsonResponce> RemoveUser([FromQuery]string userId)
        {
            return _baseIdentityService.RemoveUserAsync(userId);
        }


        #endregion

        [HttpGet]
        public JsonResponce GetUsers()
        {
            return _baseIdentityService.GetUsers();
        }

        [HttpPost]
        public async Task<JsonResponce> SignInWithCertificate([FromBody] LoginViewModel model)
        {
            return await _baseIdentityService.SignInWithCertificate(model);
        }

        [HttpPost]
        public async Task<JsonResponce> Check([FromBody]SignInViewModel viewModel)
        {
            return await _baseIdentityService.CheckMethod(viewModel);
        }

        [HttpPost]
        public async Task<JsonResponce> LoginAs(System.Guid id, [FromServices] ITokenService TS,
    [FromServices] IUserClaimsPrincipalFactory<SysUsers> principalFactory,
    [FromServices] IdentityServerOptions options)
        {
            return await _baseIdentityService.LoginAs(TS, principalFactory, options, id);
        }
    }
}
