using AccessTokenClient.Caching;
using AccessTokenClient.Encryption;
using AccessTokenClient.Expiration;
using AccessTokenClient.Keys;
using AccessTokenClient.Serialization;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;
using System.Net.Http;
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
            cacheMock.Setup(m => m.KeyExists(It.IsAny<string>())).ReturnsAsync(true);
            cacheMock.Setup(m => m.Get(It.IsAny<string>())).ReturnsAsync(new TokenGetResult<TokenResponse>
            {
                Successful = true,
                Value = new TokenResponse
                {
                    AccessToken = "1234567890"
                }
            });

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>())).Returns("KEY-123");

            // Set-up the key calculator mock:
            var calculatorMock = new Mock<IExpirationCalculator>();
            calculatorMock.Setup(m => m.CalculateExpiration(It.IsAny<TokenResponse>())).Returns(10);
            var mockCipherService = new Mock<IEncryptionService>();

            // Set-up the access token client, token client, and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            ITokenClient cachingDecorator = new TokenClientCachingDecorator(
                tokenClient, 
                cacheMock.Object, 
                keyGeneratorMock.Object, 
                calculatorMock.Object, 
                mockCipherService.Object
            );

            ITokenClient validationDecorator = new TokenClientValidationDecorator(cachingDecorator);

            var tokenResponse = await validationDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] {"esp"}
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

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.KeyExists(It.IsAny<string>())).ReturnsAsync(false);

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>())).Returns("KEY-123");

            // Set-up the key calculator mock:
            var calculatorMock = new Mock<IExpirationCalculator>();
            calculatorMock.Setup(m => m.CalculateExpiration(It.IsAny<TokenResponse>())).Returns(10);
            var mockCipherService = new Mock<IEncryptionService>();

            // Set-up the token client and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            ITokenClient cachingDecorator = new TokenClientCachingDecorator(
                tokenClient,
                cacheMock.Object, 
                keyGeneratorMock.Object,
                calculatorMock.Object,
                mockCipherService.Object
            );

            ITokenClient validationDecorator = new TokenClientValidationDecorator(cachingDecorator);

            var tokenResponse = await validationDecorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.test.com",
                ClientIdentifier = "123",
                ClientSecret     = "456",
                Scopes           = new[] {"testing"}
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

            // Set-up the token response cache mock:
            var cacheMock = new Mock<ITokenResponseCache>();
            cacheMock.Setup(m => m.KeyExists(It.IsAny<string>())).ReturnsAsync(false);

            // Set-up the key generator mock:
            var keyGeneratorMock = new Mock<IKeyGenerator>();
            keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>())).Returns("KEY-123");

            // Set-up the key calculator mock:
            var calculatorMock = new Mock<IExpirationCalculator>();
            calculatorMock.Setup(m => m.CalculateExpiration(It.IsAny<TokenResponse>())).Returns(10);
            var mockCipherService = new Mock<IEncryptionService>();

            // Set-up the token client and the caching decorator:
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            var cachingDecorator = new TokenClientCachingDecorator(
                tokenClient,
                cacheMock.Object,
                keyGeneratorMock.Object,
                calculatorMock.Object,
                mockCipherService.Object
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
    }
}