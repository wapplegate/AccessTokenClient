using AccessTokenClient.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace AccessTokenClient.Tests;

public class MemoryTokenResponseCacheTests
{
    [Fact]
    public void EnsureExceptionThrownWhenInjectedMemoryCacheIsNull()
    {
        var creationFunction = () => new MemoryTokenResponseCache(null!);
        creationFunction.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task EnsureSuccessResponseReturnedWhenCachedResponseExists()
    {
        const string Key = "testing-key";
        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCache.Set(Key, new TokenResponse
        {
            AccessToken = "1234567890",
            ExpiresIn   = 3000
        });
        var cache = new MemoryTokenResponseCache(memoryCache);
        var tokenResponse = await cache.Get(Key);
        tokenResponse!.ShouldNotBeNull();
    }

    [Fact]
    public async Task EnsureSetReturnsTrueWhenCacheSetSuccessfully()
    {
        const string Key = "testing-key";
        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryTokenResponseCache(memoryCache);
        var result = await cache.Set(Key, new TokenResponse
        {
            AccessToken = "1234567890",
            ExpiresIn   = 3000
        }, TimeSpan.FromMinutes(3000));
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task EnsureNullReturnedWhenItemWithMatchingKeyDoesNotExist()
    {
        const string Key = "testing-key";
        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryTokenResponseCache(memoryCache);
        var result = await cache.Get(Key);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task EnsureTokenResponseIsCachedWhenMemoryCacheHasSizeLimit()
    {
        const string Key = "testing-key";
        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10 });
        var cache = new MemoryTokenResponseCache(memoryCache);

        var result = await cache.Set(Key, new TokenResponse
        {
            AccessToken = "1234567890",
            ExpiresIn   = 3000
        }, TimeSpan.FromMinutes(30));

        result.ShouldBeTrue();

        var tokenResponse = await cache.Get(Key);

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldBe("1234567890");
    }

    [Fact]
    public async Task EnsureTokenResponseIsReturnedByCachingDecoratorWhenMemoryCacheHasSizeLimit()
    {
        var tokenClient = new Mock<ITokenClient>();

        tokenClient
            .Setup(m => m.RequestAccessToken(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new TokenResponse { AccessToken = "1234567890", ExpiresIn = 3600 });

        var decorator = new TokenClientCachingDecorator(
            NullLogger<TokenClientCachingDecorator>.Instance,
            tokenClient.Object,
            new TokenClientCacheOptions(),
            new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions { SizeLimit = 10 })),
            new TokenRequestKeyGenerator(),
            new DefaultAccessTokenTransformer());

        var request = new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        };

        var first  = await decorator.RequestAccessToken(request);
        var second = await decorator.RequestAccessToken(request);

        first.AccessToken.ShouldBe("1234567890");
        second.AccessToken.ShouldBe("1234567890");

        tokenClient.Verify(m => m.RequestAccessToken(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
