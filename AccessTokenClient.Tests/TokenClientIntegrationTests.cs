using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientIntegrationTests
    {
        [Fact(Skip = "test")]
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
                ClientIdentifier = "8sO5k8ECd5LlHEB2QZ2Dwji1zeeP1X7L",
                ClientSecret     = "85578vmVYEXM_5LgkA2eyxsiEmRXa8gGlHJ9SQYfFBnefOnnKNfiEsDBeElCJks9",
                Scopes           = new[] { "features:read" }
            });

            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNull();
        }
    }
}