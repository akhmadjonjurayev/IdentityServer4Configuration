using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace Cloudexis
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            using(var client = new HttpClient())
            {
                var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
                if(disco.IsError)
                {
                    Console.WriteLine(disco.Issuer);
                    return;
                }
                var tokenResponce = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "client",
                    ClientSecret = "secret",
                    Scope = "api1"
                });
                if(tokenResponce.IsError)
                {
                    Console.WriteLine(tokenResponce.Error);
                    return;
                }
                Console.WriteLine(tokenResponce.Json);
                using(var apiClient = new HttpClient())
                {
                    apiClient.SetBearerToken(tokenResponce.AccessToken);
                    var responce = await apiClient.GetAsync("https://localhost:6001/identity");
                    if(!responce.IsSuccessStatusCode)
                    {
                        Console.WriteLine(responce.StatusCode);
                    }
                    else
                    {
                        var content = await responce.Content.ReadAsStringAsync();
                        Console.WriteLine(JArray.Parse(content));
                    }
                }
            }
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
