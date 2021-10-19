using AccessTokenClient.Caching;
using AccessTokenClient.Serialization;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientCachingDecoratorTests
    {
        [Fact]
        public async Task EnsureTokenResponseIsReturnedWhenCacheKeyIsFound()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new TokenResponse
            {
                AccessToken = "1234567890"
            });

            var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

            var mockTransformer = new Mock<IAccessTokenTransformer>();

            // Set-up the access token client, token client, and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            ITokenClient cachingDecorator = new TokenClientCachingDecorator(
                decoratorLogger,
                tokenClient,
                new TokenClientCacheOptions(),
                cacheMock.Object,
                keyGeneratorMock.Object,
                mockTransformer.Object
            );

            var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            tokenResponse.ShouldNotBeNull();

            messageHandler.NumberOfCalls.Should().Be(0);
        }

        [Fact]
        public async Task EnsureTokenResponseIsReturnedWhenCacheKeyIsNotFound()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null);

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

            var mockTransformer = new Mock<IAccessTokenTransformer>();

            // Set-up the token client and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            ITokenClient cachingDecorator = new TokenClientCachingDecorator(
                decoratorLogger,
                tokenClient,
                new TokenClientCacheOptions(),
                cacheMock.Object,
                keyGeneratorMock.Object,
                mockTransformer.Object
            );

            var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNull();
            tokenResponse.AccessToken.Should().Be("1234567890");

            messageHandler.NumberOfCalls.Should().Be(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnsureTokenResponseReturnedWhenCacheSetOperationIsSuccessfulOrFails(bool setResult)
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();

            cacheMock
                .Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TokenResponse)null);

            cacheMock
                .Setup(m => m.Set(It.IsAny<string>(),It.IsAny<TokenResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(setResult);

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

            var mockTransformer = new Mock<IAccessTokenTransformer>();

            // Set-up the token client and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            ITokenClient cachingDecorator = new TokenClientCachingDecorator(
                decoratorLogger,
                tokenClient,
                new TokenClientCacheOptions(),
                cacheMock.Object,
                keyGeneratorMock.Object,
                mockTransformer.Object
            );

            var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNull();
            tokenResponse.AccessToken.Should().Be("1234567890");

            messageHandler.NumberOfCalls.Should().Be(1);
        }

        [Fact]
        public async Task EnsureTokenResponseIsReturnedWhenScopesAreNotSpecified()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null);

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

            var mockTransformer = new Mock<IAccessTokenTransformer>();

            // Set-up the token client and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            var cachingDecorator = new TokenClientCachingDecorator(
                decoratorLogger,
                tokenClient,
                new TokenClientCacheOptions(),
                cacheMock.Object,
                keyGeneratorMock.Object,
                mockTransformer.Object
            );

            var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
            });

            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNull();
            tokenResponse.AccessToken.Should().Be("1234567890");

            messageHandler.NumberOfCalls.Should().Be(1);
        }

        [Fact]
        public void EnsureExceptionThrownWhenCancellationRequested()
        {
            Func<Task<TokenResponse>> creationAction = async () =>
            {
                // Cancel the token before executing the decorator so it throws immediately:
                var source = new CancellationTokenSource();
                source.Cancel();

                var decorator = new TokenClientCachingDecorator(
                    new NullLogger<TokenClientCachingDecorator>(),
                    new TokenClient(new NullLogger<TokenClient>(), new HttpClient(), new ResponseDeserializer()),
                    new TokenClientCacheOptions(),
                    new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                    new TokenRequestKeyGenerator(),
                    new DefaultAccessTokenTransformer());

                var response = await decorator.RequestAccessToken(new TokenRequest
                {
                    TokenEndpoint    = "http://www.token-endpoint.com",
                    ClientIdentifier = "client-identifier",
                    ClientSecret     = "client-secret",
                    Scopes           = new[] { "scope:read" }
                }, cancellationToken: source.Token);

                return response;
            };

            creationAction.Should().ThrowAsync<OperationCanceledException>();
        }

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