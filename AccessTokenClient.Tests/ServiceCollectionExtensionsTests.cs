using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void EnsureServiceProviderReturnsTokenClientCachingDecoratorWhenCachingEnabled()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>();

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<ITokenClient>();

            client.ShouldNotBeNull();
            client.Should().BeOfType<TokenClientCachingDecorator>();
        }

        [Fact]
        public void EnsureServiceProviderReturnsTokenClientWhenCachingDisabled()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddAccessTokenClient();

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<ITokenClient>();

            client.ShouldNotBeNull();
            client.Should().BeOfType<TokenClient>();
        }
    }
}