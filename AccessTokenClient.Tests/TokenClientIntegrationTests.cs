using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientIntegrationTests
    {
        [Fact]
        public async Task EnsureClientCredentialsRequestSuccessful()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache().AddAccessTokenClient(x => x.EnableCaching = false);

            var provider = services.BuildServiceProvider();

            var tokenClient = provider.GetService<ITokenClient>();

            tokenClient.ShouldNotBeNull();

            var tokenResponse = await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "https://dev-85v9a1ld.auth0.com/oauth/token",
                ClientIdentifier = "",
                ClientSecret     = "",
                Scopes           = new[] { "features:read" }
            });

            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNull();
        }
    }
}