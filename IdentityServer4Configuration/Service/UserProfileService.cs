using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4Configuration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4Configuration.Service
{
    public class UserProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<SysUsers> _claimsFactory;
        private readonly IConfiguration _configuration;
        private readonly UserManager<SysUsers> _userManager;
        private readonly RoleManager<SysRole> _roleManager;

        public UserProfileService(UserManager<SysUsers> userManager, RoleManager<SysRole> roleManager, IUserClaimsPrincipalFactory<SysUsers> claimsFactory, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _claimsFactory = claimsFactory;
            _configuration = configuration;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();

            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();

            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            claims.Add(new Claim(JwtClaimTypes.GivenName, user.UserName));

            IList<string> roleNames = await _userManager.GetRolesAsync(user);
            if (roleNames != null && roleNames.Count > 0)
            {
                foreach (var roleName in roleNames)
                {
                    //claims.Add(new Claim(ClaimTypes.Role, roleName));
                    claims.Add(new Claim(JwtClaimTypes.Role, roleName));
                }
            }

            if (_userManager.SupportsUserEmail && !string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.Email, user.Email));
            }

            if (_userManager.SupportsUserPhoneNumber && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.AddRange(new[]
                {
                    new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
                    new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                });
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}
