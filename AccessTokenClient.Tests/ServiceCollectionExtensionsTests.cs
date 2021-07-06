using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void EnsureExceptionThrownWhenServiceCollectionIsNull()
        {
            IServiceCollection services = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action1 = () => services.AddAccessTokenClient();

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action2 = () => services.AddAccessTokenClientCaching<MemoryTokenResponseCache>();

            action1.Should().Throw<ArgumentNullException>();
            action2.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureServiceProviderReturnsTokenClientWhenCachingEnabled()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddAccessTokenClient().AddAccessTokenClientCaching<MemoryTokenResponseCache>();

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