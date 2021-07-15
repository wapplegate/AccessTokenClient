using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            services.AddMemoryCache().AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>();

            var provider = services.BuildServiceProvider();

            var tokenClient = provider.GetService<ITokenClient>();

            if (tokenClient == null)
            {
                throw new Exception("The token client is null.");
            }

            var tokenResponse = await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "",
                ClientIdentifier = "",
                ClientSecret     = "",
                Scopes           = new[] { "" }
            });

            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNull();
        }
    }
}