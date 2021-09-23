using IdentityServer4Configuration.Service;
using IdentityServer4Configuration.ViewModel;
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
    }
}
