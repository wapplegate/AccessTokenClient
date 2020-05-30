using AccessTokenClient.Caching;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class MemoryTokenResponseCacheTests
    {
        [Fact]
        public async Task Test()
        {
            var cache = new MemoryTokenResponseCache();
            var doesKeyExist = await cache.KeyExists("key");

            doesKeyExist.Should().BeFalse();

            var tokenResponse = new TokenResponse
            {
                AccessToken = "ABCDEFGHIJKL"
            };

            var setResult = await cache.Set("key", tokenResponse, TimeSpan.FromMinutes(5));

            setResult.Should().BeTrue();

            await Task.Delay(300000);

            var getResult = await cache.Get("key");

            var s = getResult;
        }
    }
}