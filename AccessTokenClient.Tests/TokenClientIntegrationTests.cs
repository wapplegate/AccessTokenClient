using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientIntegrationTests
    {
        [Fact(Skip = "Skipping")]
        public async Task EnsureClientCredentialsRequestSuccessful()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache().AddAccessTokenClient(x => x.EnableCaching = false);

            var provider = services.BuildServiceProvider();

            var tokenClient = provider.GetService<ITokenClient>();

            var tokenResponse = await tokenClient.RequestAccessToken(new TokenRequest
            {
                ClientIdentifier = "",
                ClientSecret     = "",
                Scopes           = new[] { "" },
                TokenEndpoint    = ""
            });

            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNull();
        }
    }
}