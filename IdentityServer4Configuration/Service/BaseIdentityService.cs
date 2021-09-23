using IdentityServer4Configuration.Data;
using IdentityServer4Configuration.Models;
using IdentityServer4Configuration.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Service
{
    public class BaseIdentityService : IBaseIdentityService
    {
        public UserManager<SysUsers> UserManager { get; }
        public IdentityDB Database { get; }

        public SignInManager<SysUsers> SignInManager { get; }
        public RoleManager<SysRole> RoleManager { get; }
        public ILogger<BaseIdentityService> Logger { get; }
        public IConfiguration Configuration { get; }
        public BaseIdentityService(IdentityDB database, UserManager<SysUsers> userManager,
            SignInManager<SysUsers> signInManager,
            RoleManager<SysRole> roleManager, ILogger<BaseIdentityService> logger, IConfiguration configuration)
        {
            UserManager = userManager;
            Database = database;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Logger = logger;
            Configuration = configuration;
        }
        public async Task<JsonResponce> CreateUserAsync(CreateUserViewModel model)
        {
            try
            {
                if (model == null)
                    return new JsonResponce { Success = false, Code = "error", Message = "error-invalid-data Parameter is null" };

                Logger.LogInformation("Model state is ok, trying to create new user...");

                // check userName for dublicate
                var isUserNameTaken = Database.Users.Any(user => user.UserName.Equals(model.Username));
                if (isUserNameTaken)
                {
                    return new JsonResponce { Success = false, Code = "error", Message = "error-dublicate-data" };
                }

                // check user for update

                //var isUserNameUsed = _userRepository.Entities.Any(user => user.UserName.Equals(model.Username) && user.PersonId.Equals(Guid.Parse(model.PersonId)) && user.TimelineEnd != null);
                //
                //if (isUserNameUsed)
                //{
                //    return JsonResponse.ErrorResponseWithCode("error-dublicate-account", "This person already has an account!");
                //}


                // Handle personId unique error manually
                var existUser = await Database.Users.FirstOrDefaultAsync(user => user.UserName.ToLower() == model.Username.ToLower());

                bool isUserExist = existUser != null;

                if (isUserExist)
                {
                    Logger.LogWarning("Username is already taken!");
                    return new JsonResponce { Success = false, Code = "error", Message = "error-dublicate-data" };
                }

                if (string.IsNullOrEmpty(model.RoleName))
                    model.RoleName = "User";

                var user = new SysUsers
                {
                    UserName = model.Username,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var identityResult = await UserManager.CreateAsync(user, model.Password);

                if (identityResult.Succeeded)
                {
                    await UserManager.SetLockoutEnabledAsync(user, false);

                    Logger.LogInformation("User is created successfully, now adding role to the user");

                    var createRoleResult = await CheckAndCreateRoleAsync(model.RoleName);
                    SysUsers newCreatedUser;

                    if (!createRoleResult)
                    {
                        newCreatedUser = await UserManager.FindByNameAsync(model.Username);
                        Logger.LogWarning("User isn't created, There is a problem with creating role...");
                        Logger.LogWarning("Now trying to delete created user...");

                        identityResult = await UserManager.DeleteAsync(newCreatedUser);

                        if (identityResult.Succeeded)
                        {
                            Logger.LogWarning("User is deleted!");
                        }
                        else
                            Logger.LogWarning("User couldn't be deleted, there are some issues on deleting user...");

                        return new JsonResponce
                        {
                            Message = "User isn't created.",
                            Success = false,
                        };
                    }

                    identityResult = await UserManager.AddToRoleAsync(user, model.RoleName);

                    if (identityResult.Succeeded)
                    {
                        Logger.LogInformation("Role is assigned to the user succesfully!");

                        return new JsonResponce
                        {
                            Message = "User is created successfully!",
                            Data = user.Id.ToString(),
                            Success = true,
                        };
                    }

                    newCreatedUser = await UserManager.FindByNameAsync(model.Username);

                    Logger.LogWarning("User isn't created, There is a problem with creating role...");
                    Logger.LogWarning("Now trying to delete created user...");

                    identityResult = await UserManager.DeleteAsync(newCreatedUser);

                    if (identityResult.Succeeded)
                    {
                        Logger.LogWarning("User is deleted!");
                    }
                    else
                        Logger.LogWarning("User couldn't be deleted, there are some issues on deleting user...");

                    Logger.LogWarning("User isn't created.");

                    return new JsonResponce
                    {
                        Message = "User isn't created.",
                        Success = false,
                    };
                }
                else
                {
                    Logger.LogWarning("User isn't created.");

                    return new JsonResponce
                    {
                        Message = "It couldn't be saved or no changes!",
                        Success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task<bool> CheckAndCreateRoleAsync(string roleName)
        {
            // Create User Role
            var exists = await RoleManager.RoleExistsAsync(roleName);

            if (!exists)
            {
                var identityResult = await RoleManager.CreateAsync(new SysRole { Name = roleName });

                if (identityResult.Succeeded)
                {
                    return true;
                }
                // If it fails...
                return false;
            }

            return true;
        }

        public async Task<JsonResponce> SignInAsync(SignInViewModel model)
        {
            try
            {
                if (model == null)
                    return new JsonResponce
                    {
                        Message = "Model state is not correct",
                        Success = false,
                    };

                Logger.LogInformation("Model state is ok, attempting to sign in...");

                // Sign in
                var signInResult = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RemeberMe, lockoutOnFailure: false);

                // If succceeded...
                if (signInResult.Succeeded)
                {
                    Logger.LogInformation("User successfully logged in!");

                    return new JsonResponce
                    {
                        Message = "User successfully logged in!",
                        Success = true,
                    };
                }
                // If failed...
                else
                {
                    Logger.LogWarning("User couldn't log in.");

                    return new JsonResponce
                    {
                        Message = "Invalid username or password",
                        Success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<JsonResponce> UpdateUserAsync(UpdateUserViewModel model)
        {
            try
            {
                if (model == null)
                    return new JsonResponce
                    {
                        Message = "Model state is not correct",
                        Success = false,
                    };

                Logger.LogInformation("Model state is ok, trying to update the user...");

                var user = await UserManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    Logger.LogWarning("User couldn't be found!");

                    return new JsonResponce
                    {
                        Message = "User couldn't be found!",
                        Success = false,
                    };
                }

                user.UserName = model.Username;
                user.LockoutEnabled = model.Blocked;

                if (model.Blocked)
                    user.LockoutEnd = DateTime.MaxValue;
                else
                    user.LockoutEnd = DateTime.UtcNow;

                // Update Username or email...
                var identityResult = await UserManager.UpdateAsync(user);

                // Try to change password
                if (identityResult.Succeeded)
                {
                    user = await UserManager.FindByIdAsync(model.UserId);


                    // Get password reset token
                    var passwordResetToken = await UserManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);

                    //identityResult = await UserManager.ResetPasswordAsync(user, passwordResetToken, model.Password);

                    if (identityResult.Succeeded)
                    {
                        identityResult = await UserManager.UpdateAsync(user);

                        Logger.LogInformation("User's password is updated successfully!");
                    }
                    else
                    {
                        Logger.LogWarning("User's password isn't updated!");

                        return new JsonResponce
                        {
                            Message = "User's password isn't updated.",
                            Success = false,
                        };
                    }


                    Logger.LogInformation("User is updated successfully!");

                    return new JsonResponce
                    {
                        Message = "User is updated successfully!",
                        Success = true,
                    };
                }
                else
                {
                    Logger.LogWarning("User isn't updated!");

                    return new JsonResponce
                    {
                        Message = "User isn't updated.",
                        Success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<JsonResponce> RemoveUserAsync(string userId)
        {
            try
            {
                if (userId == null)
                    return new JsonResponce
                    {
                        Message = "Model state is not correct",
                        Success = false,
                    };

                Logger.LogInformation("Model state is ok, trying to remove the user...");

                var user = await UserManager.FindByIdAsync(userId);

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                //Update user to set its state to removed
                var identityResult = await UserManager.UpdateAsync(user);

                if (identityResult.Succeeded)
                {
                    Logger.LogInformation("User is removed successfully");

                    return new JsonResponce
                    {
                        Message = "User is removed successfully.",
                        Success = true,
                    };
                }
                else
                {
                    Logger.LogWarning("User isn't removed.");

                    return new JsonResponce
                    {
                        Message = "User couldn't be removed!",
                        Success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public JsonResponce GetUsers()
        {
            return new JsonResponce
            {
                Success = true,
                Code = "success",
                Data = Database.Users
            };
        }
    }
}
