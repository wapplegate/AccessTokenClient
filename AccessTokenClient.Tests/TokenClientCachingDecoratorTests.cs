using AccessTokenClient.Caching;
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
        public void EnsureExceptionThrownWhenLoggerIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                null,
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                new TokenClientCacheOptions(),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                new DefaultAccessTokenTransformer());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenTokenClientIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new NullLogger<TokenClientCachingDecorator>(),
                null,
                new TokenClientCacheOptions(),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                new DefaultAccessTokenTransformer());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenTokenClientCacheOptionsIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new NullLogger<TokenClientCachingDecorator>(),
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                null,
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                new DefaultAccessTokenTransformer());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenTokenResponseCacheIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new NullLogger<TokenClientCachingDecorator>(),
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                new TokenClientCacheOptions(),
                null,
                new TokenRequestKeyGenerator(),
                new DefaultAccessTokenTransformer());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenKeyGeneratorIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new NullLogger<TokenClientCachingDecorator>(),
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                new TokenClientCacheOptions(),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                null,
                new DefaultAccessTokenTransformer());

            creationAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenTransformerIsNull()
        {
            Func<TokenClientCachingDecorator> creationAction = () => new TokenClientCachingDecorator(
                new NullLogger<TokenClientCachingDecorator>(),
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                new TokenClientCacheOptions(),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                null);

            creationAction.Should().Throw<ArgumentNullException>();
        }
    }
}