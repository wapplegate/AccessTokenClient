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
        public void EnsureExceptionThrownIfServiceCollectionIsNull()
        {
            IServiceCollection services = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => services.AddAccessTokenClient();

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureServiceProviderReturnsTokenClient()
        {
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddAccessTokenClient();

            var provider = services.BuildServiceProvider();

            var client = provider.GetService<ITokenClient>();

            client.ShouldNotBeNull();
            client.Should().BeOfType<TokenClientValidationDecorator>();
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
            client.Should().BeOfType<TokenClientValidationDecorator>();
        }
    }
}