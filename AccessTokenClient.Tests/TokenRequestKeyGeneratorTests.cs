using AccessTokenClient.Caching;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AccessTokenClient.Tests;

public class TokenRequestKeyGeneratorTests
{
    [Fact]
    public void EnsureKeyGeneratedSuccessfully()
    {
        var generator = new TokenRequestKeyGenerator();

        var request = new TokenRequest
        {
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = new[] { "scope_1", "scope_2", "scope_3" },
            TokenEndpoint    = "https://www.token-endpoint.com"
        };

        var key = generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        key.ShouldNotBeNull();
        key.Should().Contain("AccessTokenClient::");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EnsureExceptionThrownWhenTokenEndpointIsInvalid(string tokenEndpoint)
    {
        var generator = new TokenRequestKeyGenerator();

        var request = new TokenRequest
        {
            TokenEndpoint    = tokenEndpoint,
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = new[] { "scope_1", "scope_2", "scope_3" }
        };

        Action action = () => generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EnsureExceptionThrownWhenClientIdentifierIsInvalid(string clientIdentifier)
    {
        var generator = new TokenRequestKeyGenerator();

        var request = new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = clientIdentifier,
            ClientSecret     = "client-secret",
            Scopes           = new[] { "scope_1", "scope_2", "scope_3" },
        };

        Action action = () => generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EnsureExceptionThrownWhenClientSecretIsInvalid(string clientSecret)
    {
        var generator = new TokenRequestKeyGenerator();

        var request = new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = clientSecret,
            Scopes           = new[] { "scope_1", "scope_2", "scope_3" }
        };

        Action action = () => generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureRequestGeneratesDifferentKeysWhenCaseIsDifferent()
    {
        var generator = new TokenRequestKeyGenerator();

        var tokenRequestOne = new TokenRequest
        {
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = new[] { "scope_1", "scope_2", "scope_3" },
            TokenEndpoint    = "https://www.token-endpoint.com"
        };

        var keyOne = generator.GenerateTokenRequestKey(tokenRequestOne, "AccessTokenClient");

        var tokenRequestTwo = new TokenRequest
        {
            ClientIdentifier = "CLIENT-IDENTIFIER",
            ClientSecret     = "CLIENT-SECRET",
            Scopes           = new[] { "SCOPE_1", "SCOPE_2", "SCOPE_3" },
            TokenEndpoint    = "https://www.token-endpoint.com"
        };

        var keyTwo = generator.GenerateTokenRequestKey(tokenRequestTwo, "AccessTokenClient");

        keyOne.ShouldNotBeNull();
        keyTwo.ShouldNotBeNull();

        keyOne.Should().NotBeEquivalentTo(keyTwo);
    }
}