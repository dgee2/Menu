using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MenuApi.Integration.Tests.Factory;

internal class ApiAuthentication
{
    private IConfiguration Configuration { get; }


    public ApiAuthentication()
    {
        // Building the configuration by reading environment variables and optionally a local file.
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)  // Local defaults, if any.
            .AddEnvironmentVariables()                        // Overwrites with env-specific secrets.
            .AddUserSecrets<ApiTestFixture>()                     // User secrets for local development.
            .Build();
    }


    public class Auth0AuthenticationRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string audience { get; set; }
        public string grant_type { get; set; }
    }



    public class Auth0AuthenticationResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }


    public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue()
    {
        var client = new HttpClient();

        var body = new Auth0AuthenticationRequest()
        {
            client_id = "wXe2tHmpInp2Jgki0UpSZRWY2tZ8vDGJ",
            client_secret = "eD2obmWOCr17D-FbFWYQAzliQpRzDnpHe3tLQqdrEeBafmg6M1g6d748_mGGEFJF",
            audience = "http://localhost:65273",
            grant_type = "client_credentials"
        };

        var authResponse = await client.PostAsJsonAsync("https://dev-oz81ytsjd1h4r1lz.uk.auth0.com/oauth/token", body);

        authResponse.EnsureSuccessStatusCode();
        var responseBody = await authResponse.Content.ReadFromJsonAsync<Auth0AuthenticationResponse>();

        return new AuthenticationHeaderValue(responseBody.token_type, responseBody.access_token);
    }
}
