using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientIntegrationTests
    {
        [Fact(Skip = "Skipped until integration tests can resolve secrets.")]
        public async Task EnsureServiceProviderReturnsTokenClient()
        {
            var services = new ServiceCollection();

            services.AddTokenClient();

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<IAccessTokenClient>();

            var request = new TokenRequest
            {
                TokenEndpoint    = "",
                ClientIdentifier = "",
                ClientSecret     = "",
                Scopes           = new[] { "" }
            };

            var response = await client.GetAccessToken(request);

            response.ShouldNotBeNull();
            response.AccessToken.ShouldNotBeNull();
            response.TokenType.ShouldNotBeNull();
            response.ExpiresIn.ShouldNotBeNull();
        }
    }
}