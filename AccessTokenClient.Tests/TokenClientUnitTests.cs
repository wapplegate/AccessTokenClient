using AccessTokenClient.Serialization;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientUnitTests
    {
        [Fact]
        public async Task EnsureExceptionThrownWhenAccessTokenIsEmpty()
        {
            const string Response = @"{""access_token"":"""",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<HttpRequestException>();
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
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenServerErrorReturned()
        {
            const string Response = "";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.InternalServerError);
            var httpClient = new HttpClient(messageHandler);

            var mockDeserializer = new Mock<IResponseDeserializer>();
            mockDeserializer.Setup(m => m.Deserialize(It.IsAny<string>())).Returns((TokenResponse)null);

            var tokenClient = new TokenClient(logger, httpClient, mockDeserializer.Object);

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenDeserializerReturnsNullTokenResponse()
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
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenDeserializerReturnsEmptyAccessToken()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var mockDeserializer = new Mock<IResponseDeserializer>();
            mockDeserializer.Setup(m => m.Deserialize(It.IsAny<string>())).Returns(new TokenResponse
            {
                AccessToken = "",
                ExpiresIn   = 3000
            });

            var tokenClient = new TokenClient(logger, httpClient, mockDeserializer.Object);

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<HttpRequestException>();
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
                var _ = new TokenClient(null, httpClient, new ResponseDeserializer());
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
                var _ = new TokenClient(logger, null, new ResponseDeserializer());
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
                var _ = new TokenClient(logger, httpClient, null);
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task EnsureExceptionThrownWhenCancellationRequested()
        {
            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            // Cancel the token before executing the token client so it throws immediately:
            var source = new CancellationTokenSource();
            source.Cancel();
            
            var client = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () =>
            {
                var tokenResponse = await client.RequestAccessToken(new TokenRequest
                {
                    TokenEndpoint    = "http://www.token-endpoint.com",
                    ClientIdentifier = "client-identifier",
                    ClientSecret     = "client-secret",
                    Scopes           = new[] { "scope:read" }
                }, cancellationToken: source.Token);

                return tokenResponse;
            };

            await function.Should().ThrowAsync<OperationCanceledException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task EnsureExceptionThrownWhenTokenEndpointNotSet(string tokenEndpoint)
        {
            const string Response = @"{""access_token"":"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = tokenEndpoint,
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task EnsureExceptionThrownWhenClientIdentifierNotSet(string clientIdentifier)
        {
            const string Response = @"{""access_token"":"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = clientIdentifier,
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task EnsureExceptionThrownWhenClientSecretNotSet(string clientSecret)
        {
            const string Response = @"{""access_token"":"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = clientSecret,
                Scopes           = new[] { "scope:read" }
            });

            await function.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task EnsureTokenResponseFromOptionalFunctionIsReturned()
        {
            const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

            var logger = new NullLogger<TokenClient>();
            var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler);

            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            var tokenResponse = await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = new[] { "scope:read" }
            }, execute: request => Task.FromResult(new TokenResponse 
            {
                AccessToken = "access-token",
                ExpiresIn   = 8000
            }));

            // Ensure the access token and expiration match what is returned from the execute func:
            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.Should().Be("access-token");
            tokenResponse.ExpiresIn.Should().Be(8000);
            messageHandler.NumberOfCalls.Should().Be(0);
        }
    }
}