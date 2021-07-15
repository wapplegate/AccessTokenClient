using AccessTokenClient.Caching;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class MemoryTokenResponseCacheTests
    {
        [Fact]
        public void EnsureExceptionThrownWhenInjectedMemoryCacheIsNull()
        {
            Func<MemoryTokenResponseCache> creationFunction = () => new MemoryTokenResponseCache(null);
            creationFunction.Should().Throw<ArgumentNullException>();
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
            tokenResponse.ShouldNotBeNull();
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
            result.Should().BeTrue();
        }

        [Fact]
        public async Task EnsureNullReturnedWhenItemWithMatchingKeyDoesNotExist()
        {
            const string Key = "testing-key";
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new MemoryTokenResponseCache(memoryCache);
            var result = await cache.Get(Key);
            result.Should().BeNull();
        }
    }
}