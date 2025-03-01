using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace AccessTokenClient.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void EnsureServiceProviderReturnsTokenClient()
    {
        var services = new ServiceCollection();

        services.AddMemoryCache();

        services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>();

        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<ITokenClient>();

        client.ShouldNotBeNull();
        client.ShouldBeOfType<TokenClientCachingDecorator>();
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

        var client = provider.GetRequiredService<ITokenClient>();

        client.ShouldNotBeNull();
        client.ShouldBeOfType<TokenClient>();
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
                var logger = p.GetRequiredService<ILogger<ITokenClient>>();
                return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
            });
        });

        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<ITokenClient>();

        client.ShouldNotBeNull();
        client.ShouldBeOfType<TokenClient>();
    }

    [Fact]
    public void EnsureServiceProviderReturnsTokenClientWhenCachingDisabled()
    {
        var services = new ServiceCollection();

        services.AddMemoryCache();

        services.AddAccessTokenClient();

        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<ITokenClient>();

        client.ShouldNotBeNull();
        client.ShouldBeOfType<TokenClient>();
    }

    [Fact]
    public void EnsureExceptionThrownWhenInvalidCachingOptionsSpecified()
    {
        var services = new ServiceCollection();

        Action invalidPrefixAction = () => services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>(options =>
        {
            options.CacheKeyPrefix = string.Empty;
        });

        invalidPrefixAction.ShouldThrow<ArgumentException>();

        Action invalidBufferAction = () => services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>(options =>
        {
            options.ExpirationBuffer = 0;
        });

        invalidBufferAction.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void EnsureExceptionThrownWhenServiceCollectionNullUsingAddAccessTokenClientExtensionMethod()
    {
        IServiceCollection? services = null;

        Action action = () => services!.AddAccessTokenClient();

        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void EnsureExceptionThrownWhenServiceCollectionNullUsingAddAccessTokenClientCacheMethod()
    {
        IServiceCollection? services = null;

        Action action = () => services!.AddAccessTokenClientCache<MemoryTokenResponseCache>();

        action.ShouldThrow<ArgumentNullException>();
    }
}
