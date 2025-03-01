using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using System.Net;
using Xunit;

namespace AccessTokenClient.Tests;

public class TokenClientUnitTests
{
    [Fact]
    public async Task EnsureExceptionThrownWhenAccessTokenIsEmpty()
    {
        const string Response = @"{""access_token"":"""",""token_type"":""Bearer"",""expires_in"":7199}";

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        var tokenClient = new TokenClient(logger, httpClient);

        Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        await function.ShouldThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task EnsureExceptionThrownWhenTokenResponseIsEmpty()
    {
        const string Response = "";

        var logger         = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        var httpClient     = new HttpClient(messageHandler);

        var tokenClient = new TokenClient(logger, httpClient);

        Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        await function.ShouldThrowAsync<Exception>();
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task EnsureExceptionThrownWhenNonSuccessStatusCodeReturned(HttpStatusCode statusCode)
    {
        const string Response = "";

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(Response, statusCode);
        var httpClient = new HttpClient(messageHandler);

        var tokenClient = new TokenClient(logger, httpClient);

        Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        await function.ShouldThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task EnsureExceptionThrownWhenCancellationRequested()
    {
        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        // Cancel the token before executing the token client so it throws immediately:
        var source = new CancellationTokenSource();
        await source.CancelAsync();

        var client = new TokenClient(logger, httpClient);

        Func<Task<TokenResponse>> function = async () =>
        {
            var tokenResponse = await client.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "https://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = ["scope:read"]
            }, cancellationToken: source.Token);

            return tokenResponse;
        };

        await function.ShouldThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EnsureExceptionThrownWhenTokenEndpointNotSet(string? tokenEndpoint)
    {
        var response = string.Empty;

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        var tokenClient = new TokenClient(logger, httpClient);

        Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = tokenEndpoint,
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        await function.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EnsureExceptionThrownWhenClientIdentifierNotSet(string? clientIdentifier)
    {
        var response = string.Empty;

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        var tokenClient = new TokenClient(logger, httpClient);

        Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = clientIdentifier,
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        await function.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task EnsureExceptionThrownWhenClientSecretNotSet(string? clientSecret)
    {
        var response = string.Empty;

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        var tokenClient = new TokenClient(logger, httpClient);

        Func<Task<TokenResponse>> function = async () => await tokenClient.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = clientSecret,
            Scopes           = ["scope:read"]
        });

        await function.ShouldThrowAsync<ArgumentException>();
    }
}
