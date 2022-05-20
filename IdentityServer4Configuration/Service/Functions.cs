using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace IdentityServer4Configuration.Service
{
    public class Functions
    {
        public static string GetPersonId(HttpContext httpContext)
        {
            var _bearer_token = httpContext.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(_bearer_token);
            var personId = jwt.Claims.First(l => l.Type == "personId").Value;
            return personId;
        }
    }
}
