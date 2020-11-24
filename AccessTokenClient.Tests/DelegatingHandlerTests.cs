using FluentAssertions;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class DelegatingHandlerTests
    {
        [Fact]
        public void EnsureExceptionThrownWhenOptionsAreNull()
        {
            ITokenEndpointOptions options = new Options();
            Action action = () => new AccessTokenDelegatingHandler(options, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureExceptionThrownWhenTokenClientIsNull()
        {
            var mockClient = new Mock<ITokenClient>();
            Action action = () => new AccessTokenDelegatingHandler(null, mockClient.Object);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task EnsureAccessTokenAddedToRequest()
        {
            ITokenEndpointOptions options = new Options();
            var mockClient = new Mock<ITokenClient>();

            mockClient
                .Setup(m => m.RequestAccessToken(It.IsAny<TokenRequest>(), It.IsAny<Func<TokenRequest, Task<TokenResponse>>>()))
                .ReturnsAsync(new TokenResponse
                {
                    AccessToken = "1234567890",
                    ExpiresIn   = 3000,
                    TokenType   = "type"
                });

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            var handler = new AccessTokenDelegatingHandler(options, mockClient.Object)
            {
                InnerHandler = new TestHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(httpRequestMessage, new CancellationToken());
        }
    }

    public class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK), cancellationToken);
        }
    }

    public class Options : ITokenEndpointOptions
    {
        public string TokenEndpoint { get; set; }

        public string ClientIdentifier { get; set; }

        public string ClientSecret { get; set; }

        public string[] Scopes { get; set; }
    }
}