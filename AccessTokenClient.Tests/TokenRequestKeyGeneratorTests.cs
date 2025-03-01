using AccessTokenClient.Caching;
using Shouldly;
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
            Scopes           = ["scope_1", "scope_2", "scope_3"],
            TokenEndpoint    = "https://www.token-endpoint.com"
        };

        var key = generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        key.ShouldNotBeNull();
        key.ShouldContain("AccessTokenClient::");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EnsureExceptionThrownWhenTokenEndpointIsInvalid(string? tokenEndpoint)
    {
        var generator = new TokenRequestKeyGenerator();

        var request = new TokenRequest
        {
            TokenEndpoint    = tokenEndpoint,
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope_1", "scope_2", "scope_3"]
        };

        Action action = () => generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EnsureExceptionThrownWhenClientIdentifierIsInvalid(string? clientIdentifier)
    {
        var generator = new TokenRequestKeyGenerator();

        var request = new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = clientIdentifier,
            ClientSecret     = "client-secret",
            Scopes           = ["scope_1", "scope_2", "scope_3"],
        };

        Action action = () => generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EnsureExceptionThrownWhenClientSecretIsInvalid(string? clientSecret)
    {
        var generator = new TokenRequestKeyGenerator();

        var request = new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = clientSecret,
            Scopes           = ["scope_1", "scope_2", "scope_3"]
        };

        Action action = () => generator.GenerateTokenRequestKey(request, "AccessTokenClient");

        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void EnsureRequestGeneratesDifferentKeysWhenCaseIsDifferent()
    {
        var generator = new TokenRequestKeyGenerator();

        var tokenRequestOne = new TokenRequest
        {
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope_1", "scope_2", "scope_3"],
            TokenEndpoint    = "https://www.token-endpoint.com"
        };

        var keyOne = generator.GenerateTokenRequestKey(tokenRequestOne, "AccessTokenClient");

        var tokenRequestTwo = new TokenRequest
        {
            ClientIdentifier = "CLIENT-IDENTIFIER",
            ClientSecret     = "CLIENT-SECRET",
            Scopes           = ["SCOPE_1", "SCOPE_2", "SCOPE_3"],
            TokenEndpoint    = "https://www.token-endpoint.com"
        };

        var keyTwo = generator.GenerateTokenRequestKey(tokenRequestTwo, "AccessTokenClient");

        keyOne.ShouldNotBeNull();
        keyTwo.ShouldNotBeNull();

        keyOne.ShouldNotBeSameAs(keyTwo);
    }
}
