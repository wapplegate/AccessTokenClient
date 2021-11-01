using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void EnsureServiceProviderReturnsTokenClient()
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
        public void EnsureServiceProviderReturnsTokenClientWhenRetryPolicySpecified()
        {
            var services = new ServiceCollection();

            services.AddAccessTokenClient(builder =>
            {
                builder.AddPolicyHandler(_ => AccessTokenClientPolicy.GetDefaultRetryPolicy());
            });

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<ITokenClient>();

            client.ShouldNotBeNull();
            client.Should().BeOfType<TokenClient>();
        }

        [Fact]
        public void EnsureServiceProviderReturnsTokenClientWhenRetryPolicySpecifiedWithLogger()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddAccessTokenClient(builder =>
            {
                builder.AddPolicyHandler((p, _) =>
                {
                    var logger = p.GetService<ILogger<ITokenClient>>();
                    return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
                });
            });

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<ITokenClient>();

            client.ShouldNotBeNull();
            client.Should().BeOfType<TokenClient>();
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

        [Fact]
        public void EnsureExceptionThrownWhenInvalidCachingOptionsSpecified()
        {
            var services = new ServiceCollection();

            Action invalidPrefixAction = () => services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>(options =>
            {
                options.CacheKeyPrefix = string.Empty;
            });

            invalidPrefixAction.Should().Throw<ArgumentException>();

            Action invalidBufferAction = () => services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>(options =>
            {
                options.ExpirationBuffer = 0;
            });

            invalidBufferAction.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenServiceCollectionNullUsingAddAccessTokenClientExtensionMethod()
        {
            IServiceCollection services = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => services.AddAccessTokenClient();

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenServiceCollectionNullUsingAddAccessTokenClientCacheMethod()
        {
            IServiceCollection services = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => services.AddAccessTokenClientCache<MemoryTokenResponseCache>();

            action.Should().Throw<ArgumentNullException>();
        }
    }
}