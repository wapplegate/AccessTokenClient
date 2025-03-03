using AccessTokenClient.Caching;
using Microsoft.Extensions.Caching.Memory;
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
}
