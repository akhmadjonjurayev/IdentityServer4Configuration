using Grpc.Net.Client;
using IdentityPage.Properties;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using IdentityServer4Configuration.Data;
using IdentityServer4Configuration.Models;
using IdentityServer4Configuration.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Service
{
    public class BaseIdentityService : IBaseIdentityService
    {
        public UserManager<SysUsers> UserManager { get; }
        public IdentityDB Database { get; }
        private readonly IdentityServerTools _tools;
        private readonly ITokenService TS;
        private readonly IUserClaimsPrincipalFactory<SysUsers> principalFactory;
        private readonly IdentityServerOptions options;
        public SignInManager<SysUsers> SignInManager { get; }
        public RoleManager<SysRole> RoleManager { get; }
        public ILogger<BaseIdentityService> Logger { get; }
        public IConfiguration Configuration { get; }
        public BaseIdentityService(IdentityDB database, UserManager<SysUsers> userManager,
            SignInManager<SysUsers> signInManager,
            RoleManager<SysRole> roleManager, ILogger<BaseIdentityService> logger, IConfiguration configuration,
            IdentityServerTools tools, ITokenService tokenService,
            IUserClaimsPrincipalFactory<SysUsers> PrincipalFactory, IdentityServerOptions Options)
        {
            this._tools = tools;
            UserManager = userManager;
            Database = database;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Logger = logger;
            Configuration = configuration;
            this.TS = tokenService;
            this.principalFactory = PrincipalFactory;
            this.options = Options;
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
                    PersonId = model.PersonId,
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

        public async Task<JsonResponce> SignInWithCertificate(LoginViewModel viewModel)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("http://localhost:7000");
                var grpcClient = new Crypto.CryptoClient(channel);
                var result = await grpcClient.GetCertificateBySerialNumberAsync(new SerialNumber { SerialNumber_ = viewModel.SerialNumber });
                var user = await Database.Users.FirstOrDefaultAsync(l => l.PersonId == Guid.Parse(result.PersonId));
                using var certificate = new X509Certificate2(Convert.FromBase64String(result.SertificateBase64));
                using var rsa = certificate.GetRSAPublicKey();
                var verifySign = rsa.VerifyData(Encoding.UTF8.GetBytes(viewModel.ForSignData), Convert.FromBase64String(viewModel.Signature),
                    System.Security.Cryptography.HashAlgorithmName.MD5, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                if(verifySign)
                {
                    await SignInManager.SignInAsync(user, true);
                    var token = await GetTokenByUserId(user.Id, TS, principalFactory, options);
                    return new JsonResponce { Success = true, Code = "success", Data = token };
                }
                return new JsonResponce { Success = false, Code = "error" };

            }
            catch(Exception ex)
            {
                return new JsonResponce { Success = false, Message = ex.Message, Code = "error" };
            }
        }

        public async Task<JsonResponce> CheckMethod(SignInViewModel viewModel)
        {
            try
            {
                var user = await Database.Users.FirstOrDefaultAsync(l => l.UserName == viewModel.Username);
                if (user == null)
                    return new JsonResponce { Success = false, Code = "error" };
                var userClaims = await UserManager.GetClaimsAsync(user);
                var token = await _tools.IssueJwtAsync(3600, "http://localhost:9001", userClaims);
                return new JsonResponce { Success = true, Message = token };
            }
            catch(Exception ex)
            {
                return new JsonResponce { Success = false, Message = ex.Message };
            }
        }

        public async Task<JsonResponce> LoginAs(ITokenService TS, IUserClaimsPrincipalFactory<SysUsers> principalFactory,
            IdentityServerOptions options, Guid id)
        {
            var Request = new TokenCreationRequest();
            var User = await Database.SysUsers.FirstOrDefaultAsync(l => l.Id == id);
            var IdentityPricipal = await principalFactory.CreateAsync(User);
            var IdentityUser = new IdentityServerUser(User.Id.ToString());
            IdentityUser.AdditionalClaims = IdentityPricipal.Claims.ToArray();
            IdentityUser.DisplayName = User.UserName;
            IdentityUser.AuthenticationTime = System.DateTime.UtcNow;
            IdentityUser.IdentityProvider = IdentityServerConstants.LocalIdentityProvider;
            Request.Subject = IdentityUser.CreatePrincipal();
            Request.IncludeAllIdentityClaims = true;
            Request.ValidatedRequest = new ValidatedRequest();
            Request.ValidatedRequest.Subject = Request.Subject;
            var client = await Database.SysClients.FirstOrDefaultAsync(l => l.ClientId == "Crypto");
            client.MapDataFromEntity();
            Request.ValidatedRequest.SetClient(client.Client);
            var identityResource = await Database.SysIdentityResources.ToListAsync();
            identityResource.ForEach(re => re.MapDataFromEntity());
            var apiResource = await Database.SysApiResources.ToListAsync();
            apiResource.ForEach(re => re.MapDataFromEntity());
            var scopes = await Database.SysApiScopes.ToListAsync();
            scopes.ForEach(re => re.MapDataFromEntity());
            Request.ValidatedResources = new ResourceValidationResult(new Resources(identityResource.Select(l => l.IdentityResource),
                apiResource.Select(l => l.ApiResource), scopes.Select(l => l.ApiScope)));
            Request.ValidatedRequest.Options = options;
            Request.ValidatedRequest.ClientClaims = IdentityUser.AdditionalClaims;
            var Token = await TS.CreateAccessTokenAsync(Request);
            Token.Issuer = "https://localhost:9001";
            var TokenValue = await TS.CreateSecurityTokenAsync(Token);
            return new JsonResponce { Success = true, Code = "Success", Data = TokenValue };
        }

        private async Task<string> GetTokenByUserId(Guid id, ITokenService TS, IUserClaimsPrincipalFactory<SysUsers> principalFactory,
            IdentityServerOptions options)
        {
            var Request = new TokenCreationRequest();
            var User = await Database.SysUsers.FirstOrDefaultAsync(l => l.Id == id);
            var IdentityPricipal = await principalFactory.CreateAsync(User);
            var IdentityUser = new IdentityServerUser(User.Id.ToString());
            IdentityUser.AdditionalClaims = IdentityPricipal.Claims.ToArray();
            IdentityUser.DisplayName = User.UserName;
            IdentityUser.AuthenticationTime = DateTime.UtcNow;
            IdentityUser.IdentityProvider = IdentityServerConstants.LocalIdentityProvider;
            Request.Subject = IdentityUser.CreatePrincipal();
            Request.IncludeAllIdentityClaims = true;
            Request.ValidatedRequest = new ValidatedRequest();
            Request.ValidatedRequest.Subject = Request.Subject;
            var client = await Database.SysClients.FirstOrDefaultAsync(l => l.ClientId == "Crypto");
            client.MapDataFromEntity();
            Request.ValidatedRequest.SetClient(client.Client);
            var identityResource = await Database.SysIdentityResources.ToListAsync();
            identityResource.ForEach(re => re.MapDataFromEntity());
            var apiResource = await Database.SysApiResources.ToListAsync();
            apiResource.ForEach(re => re.MapDataFromEntity());
            var scopes = await Database.SysApiScopes.ToListAsync();
            scopes.ForEach(re => re.MapDataFromEntity());
            Request.ValidatedResources = new ResourceValidationResult(new Resources(identityResource.Select(l => l.IdentityResource),
                apiResource.Select(l => l.ApiResource), scopes.Select(l => l.ApiScope)));
            Request.ValidatedRequest.Options = options;
            Request.ValidatedRequest.ClientClaims = IdentityUser.AdditionalClaims;
            var Token = await TS.CreateAccessTokenAsync(Request);
            Token.Issuer = "https://localhost:9001";
            var TokenValue = await TS.CreateSecurityTokenAsync(Token);
            return TokenValue;
        }
    }
}
