using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class ValidationDecoratorTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EnsureExceptionThrownWhenTokenEndpointNullOrEmpty(string endpoint)
        {
            var decorator = new TokenClientValidationDecorator(null);

            Func<Task> requestFunction = async () => await decorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = endpoint,
                ClientIdentifier = "testing-identifier",
                ClientSecret     = "testing-secret"
            });

            requestFunction.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EnsureExceptionThrownWhenClientIdentifierNullOrEmpty(string identifier)
        {
            var decorator = new TokenClientValidationDecorator(null);

            Func<Task> requestFunction = async () => await decorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "testing-endpoint",
                ClientIdentifier = identifier,
                ClientSecret     = "testing-secret"
            });

            requestFunction.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EnsureExceptionThrownWhenClientSecretNullOrEmpty(string secret)
        {
            var decorator = new TokenClientValidationDecorator(null);

            Func<Task> requestFunction = async () => await decorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "testing-endpoint",
                ClientIdentifier = "testing-identifier",
                ClientSecret     = secret
            });

            requestFunction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task EnsureExceptionNotThrownRequestComplete()
        {
            const string AccessToken = "123";
            const int ExpiresIn      = 5000;
            const string TokenType   = "token-type";

            var tokenClientMock = new Mock<ITokenClient>();
            tokenClientMock.Setup(m => m.RequestAccessToken(It.IsAny<TokenRequest>(), null)).ReturnsAsync(new TokenResponse
            {
                AccessToken = AccessToken,
                ExpiresIn   = ExpiresIn,
                TokenType   = TokenType
            });
            var decoratedTokenClient = tokenClientMock.Object;

            var decorator = new TokenClientValidationDecorator(decoratedTokenClient);

            var result = await decorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "testing-endpoint",
                ClientIdentifier = "testing-identifier",
                ClientSecret     = "testing-secret"
            });

            result.ShouldNotBeNull();
            result.AccessToken.Should().Be(AccessToken);
            result.ExpiresIn.Should().Be(ExpiresIn);
            result.TokenType.Should().Be(TokenType);
        }
    }
}