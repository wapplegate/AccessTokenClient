using AccessTokenClient.Caching;
using AccessTokenClient.Expiration;
using AccessTokenClient.Keys;
using AccessTokenClient.Serialization;
using AccessTokenClient.Tests.Helpers;
using AccessTokenClient.Transformation;
using FluentAssertions;
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
    public class TokenClientUnitTests
    {
        [Fact]
        public async Task EnsureTokenResponseIsReturnedWhenCacheKeyIsFound()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger         = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient     = new HttpClient(messageHandler);

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new TokenResponse
            {
                AccessToken = "1234567890"
            });

            var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>())).Returns("KEY-123");

            // Set-up the key calculator mock:
            var calculatorMock = new Mock<IExpirationCalculator>();
            calculatorMock.Setup(m => m.CalculateExpiration(It.IsAny<TokenResponse>())).Returns(TimeSpan.FromMinutes(10));
            var mockTransformer = new Mock<IAccessTokenTransformer>();

            // Set-up the access token client, token client, and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            ITokenClient cachingDecorator = new TokenClientCachingDecorator(
                decoratorLogger,
                tokenClient, 
                cacheMock.Object, 
                keyGeneratorMock.Object, 
                calculatorMock.Object, 
                mockTransformer.Object
            );

            ITokenClient validationDecorator = new TokenClientValidationDecorator(cachingDecorator);

            var tokenResponse = await validationDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] {"scope:read"}
            });

            tokenResponse.ShouldNotBeNull();

            messageHandler.NumberOfCalls.ShouldNotBeNull();
            messageHandler.NumberOfCalls.Should().Be(0);
        }

        [Fact]
        public async Task EnsureTokenResponseIsReturnedWhenCacheKeyIsNotFound()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger         = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient     = new HttpClient(messageHandler);

            var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null);

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>())).Returns("KEY-123");

            // Set-up the key calculator mock:
            var calculatorMock = new Mock<IExpirationCalculator>();
            calculatorMock.Setup(m => m.CalculateExpiration(It.IsAny<TokenResponse>())).Returns(TimeSpan.FromMinutes(10));
            var mockTransformer = new Mock<IAccessTokenTransformer>();

            // Set-up the token client and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            ITokenClient cachingDecorator = new TokenClientCachingDecorator(
                decoratorLogger,
                tokenClient,
                cacheMock.Object, 
                keyGeneratorMock.Object,
                calculatorMock.Object,
                mockTransformer.Object
            );

            ITokenClient validationDecorator = new TokenClientValidationDecorator(cachingDecorator);

            var tokenResponse = await validationDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] {"scope:read"}
            });

            tokenResponse.ShouldNotBeNull();

            messageHandler.NumberOfCalls.ShouldNotBeNull();
            messageHandler.NumberOfCalls.Should().Be(1);
        }

        [Fact]
        public async Task EnsureTokenResponseIsReturnedWhenScopesAreNotSpecified()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger         = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient     = new HttpClient(messageHandler);

            var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null);

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>())).Returns("KEY-123");

            // Set-up the key calculator mock:
            var calculatorMock = new Mock<IExpirationCalculator>();
            calculatorMock.Setup(m => m.CalculateExpiration(It.IsAny<TokenResponse>())).Returns(TimeSpan.FromMinutes(10));
            var mockTransformer = new Mock<IAccessTokenTransformer>();

            // Set-up the token client and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            var cachingDecorator = new TokenClientCachingDecorator(
                decoratorLogger,
                tokenClient,
                cacheMock.Object,
                keyGeneratorMock.Object,
                calculatorMock.Object,
                mockTransformer.Object
            );

            ITokenClient validationDecorator = new TokenClientValidationDecorator(cachingDecorator);

            var tokenResponse = await validationDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456"
            });

            tokenResponse.ShouldNotBeNull();

            messageHandler.NumberOfCalls.ShouldNotBeNull();
            messageHandler.NumberOfCalls.Should().Be(1);
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenAccessTokenIsEmpty()
        {
            const string Response = @"{""access_token"":"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenTokenResponseIsEmpty()
        {
            const string Response = "";

            var logger         = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient     = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<Exception>();
        }


        [Fact]
        public async Task Ensure()
        {
            const string Response = "";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.NotFound);
            var httpClient = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<UnsuccessfulTokenResponseException>();
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenTokenResponseIsNull()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var mockDeserializer = new Mock<IResponseDeserializer>();
            mockDeserializer.Setup(m => m.Deserialize(It.IsAny<string>())).Returns((TokenResponse)null);

            var tokenClient = new TokenClient(logger, httpClient, mockDeserializer.Object);

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<InvalidTokenResponseException>();
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenTokenResponseAccessTokenIsEmpty()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var mockDeserializer = new Mock<IResponseDeserializer>();
            mockDeserializer.Setup(m => m.Deserialize(It.IsAny<string>())).Returns((TokenResponse)new TokenResponse
            {
                AccessToken = "",
                ExpiresIn   = 3000
            });

            var tokenClient = new TokenClient(logger, httpClient, mockDeserializer.Object);

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<InvalidTokenResponseException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenLoggerIsNull()
        {
            var messageHandler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);
            var mockDeserializer = new Mock<IResponseDeserializer>();
            mockDeserializer.Setup(m => m.Deserialize(It.IsAny<string>())).Returns((TokenResponse)null);

            Action action = () =>
            {
                var tokenClient = new TokenClient(null, httpClient, new ResponseDeserializer());
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenClientIsNull()
        {
            var logger = new NullLogger<TokenClient>();
            var mockDeserializer = new Mock<IResponseDeserializer>();
            mockDeserializer.Setup(m => m.Deserialize(It.IsAny<string>())).Returns((TokenResponse)null);

            Action action = () =>
            {
                var tokenClient = new TokenClient(logger, null, new ResponseDeserializer());
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenDeserializerIsNull()
        {
            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            Action action = () =>
            {
                var tokenClient = new TokenClient(logger, httpClient, null);
            };

            action.Should().Throw<ArgumentNullException>();
        }
    }
}