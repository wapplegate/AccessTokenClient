using AccessTokenClient.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
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
        public async Task EnsureKeyExistsReturnsTrueWhenMemoryCacheKeyExistsReturnsTrue()
        {
            const string Key = "testing-key";
            var mockMemoryCache = new Mock<IMemoryCache>();
            object value;
            mockMemoryCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out value)).Returns(true);
            var cache = new MemoryTokenResponseCache(mockMemoryCache.Object);
            var result = await cache.KeyExists(Key);
            result.Should().BeTrue();
        }
    }
}