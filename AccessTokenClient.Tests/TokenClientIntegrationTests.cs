using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientIntegrationTests
    {
        [Fact]
        public void EnsureServiceProviderReturnsTokenClient()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddTokenClient();

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<IAccessTokenClient>();

            client.ShouldNotBeNull();
        }
    }
}