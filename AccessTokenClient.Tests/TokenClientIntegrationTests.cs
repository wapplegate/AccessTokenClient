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

            services.AddAccessTokenClient();

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<ITokenClient>();

            client.ShouldNotBeNull();
        }

        [Fact]
        public void EnsureServiceProviderReturnsTokenClientWhenCachingDisabled()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddAccessTokenClient(x => x.EnableCaching = false);

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<ITokenClient>();

            client.ShouldNotBeNull();
        }
    }
}