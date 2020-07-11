using AccessTokenClient.Caching;
using AccessTokenClient.Encryption;
using AccessTokenClient.Expiration;
using AccessTokenClient.Keys;
using AccessTokenClient.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net.Http;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientCachingDecoratorTests
    {
        [Fact]
        public void EnsureExceptionThrownWhenTokenClientIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                null,
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                new DefaultExpirationCalculator(),
                new DefaultEncryptionService());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenTokenResponseCacheIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                null,
                new TokenRequestKeyGenerator(),
                new DefaultExpirationCalculator(),
                new DefaultEncryptionService());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenKeyGeneratorIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                null,
                new DefaultExpirationCalculator(),
                new DefaultEncryptionService());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenExpirationCalculatorIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                null,
                new DefaultEncryptionService());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenEncryptionServiceIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                new DefaultExpirationCalculator(),
                null);

            creationAction.Should().Throw<ArgumentNullException>();
        }
    }
}