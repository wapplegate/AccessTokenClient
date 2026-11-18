using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Net;
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

    [Fact]
    public void EnsureExceptionThrownWhenCacheAddedBeforeTokenClient()
    {
        var services = new ServiceCollection();

        Action action = () => services.AddAccessTokenClientCache<MemoryTokenResponseCache>();

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task EnsureTokenClientIsOnlyDecoratedOnceWhenCacheAddedMultipleTimes()
    {
        const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);

        var services = new ServiceCollection();

        services.AddMemoryCache();

        services
            .AddAccessTokenClient(builder => builder.ConfigurePrimaryHttpMessageHandler(() => messageHandler))
            .AddAccessTokenClientCache<MemoryTokenResponseCache>()
            .AddAccessTokenClientCache<MemoryTokenResponseCache>();

        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<ITokenClient>();

        client.ShouldBeOfType<TokenClientCachingDecorator>();

        var request = new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        };

        var first  = await client.RequestAccessToken(request).WaitAsync(TimeSpan.FromSeconds(10));
        var second = await client.RequestAccessToken(request).WaitAsync(TimeSpan.FromSeconds(10));

        first.AccessToken.ShouldBe("1234567890");
        second.AccessToken.ShouldBe("1234567890");

        messageHandler.NumberOfCalls.ShouldBe(1);
    }

    [Fact]
    public void EnsureTokenClientLifetimeIsPreservedWhenCacheAdded()
    {
        var services = new ServiceCollection();

        services.AddMemoryCache();

        services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>();

        var descriptor = services.Single(d => d.ServiceType == typeof(ITokenClient));

        descriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var provider = services.BuildServiceProvider();

        var first  = provider.GetRequiredService<ITokenClient>();
        var second = provider.GetRequiredService<ITokenClient>();

        first.ShouldBeOfType<TokenClientCachingDecorator>();
        second.ShouldBeOfType<TokenClientCachingDecorator>();
        first.ShouldNotBeSameAs(second);
    }

    [Fact]
    public async Task EnsureTokenClientRegisteredByTypeIsDecorated()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMemoryCache();
        services.AddSingleton<CallCounter>();
        services.AddSingleton<ITokenClient, StubTokenClient>();

        services.AddAccessTokenClientCache<MemoryTokenResponseCache>();

        await EnsureTokenClientIsDecoratedAndCaches(services);
    }

    [Fact]
    public async Task EnsureTokenClientRegisteredByInstanceIsDecorated()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMemoryCache();

        var counter = new CallCounter();
        services.AddSingleton(counter);
        services.AddSingleton<ITokenClient>(new StubTokenClient(counter));

        services.AddAccessTokenClientCache<MemoryTokenResponseCache>();

        await EnsureTokenClientIsDecoratedAndCaches(services);
    }

    private static async Task EnsureTokenClientIsDecoratedAndCaches(IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<ITokenClient>();

        client.ShouldBeOfType<TokenClientCachingDecorator>();

        var request = new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        };

        var first  = await client.RequestAccessToken(request);
        var second = await client.RequestAccessToken(request);

        first.AccessToken.ShouldBe("1234567890");
        second.AccessToken.ShouldBe("1234567890");

        provider.GetRequiredService<CallCounter>().NumberOfCalls.ShouldBe(1);
    }

    private sealed class CallCounter
    {
        public int NumberOfCalls { get; set; }
    }

    private sealed class StubTokenClient(CallCounter counter) : ITokenClient
    {
        public Task<TokenResponse> RequestAccessToken(TokenRequest request, CancellationToken cancellationToken = default)
        {
            counter.NumberOfCalls++;

            return Task.FromResult(new TokenResponse { AccessToken = "1234567890", ExpiresIn = 3600 });
        }
    }
}
