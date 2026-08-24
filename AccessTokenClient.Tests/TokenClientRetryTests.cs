using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Net;
using Xunit;

namespace AccessTokenClient.Tests;

public class TokenClientRetryTests
{
    [Theory]
    [InlineData(HttpStatusCode.NotFound, 1)]
    [InlineData(HttpStatusCode.TooManyRequests, 3)]
    [InlineData(HttpStatusCode.RequestTimeout, 3)]
    [InlineData(HttpStatusCode.InternalServerError, 3)]
    public async Task EnsureTokenRequestRetriedOnFailure(HttpStatusCode httpStatusCode, int expectedCalls)
    {
        var services = new ServiceCollection();

        var mockHandler = new MockHttpMessageHandler(string.Empty, httpStatusCode);

        // Configure an http client with the default retry policy handler and a mock http message handler:
        services
            .AddHttpClient("TestingClient")
            .AddPolicyHandler((_, _) => AccessTokenClientPolicy.GetDefaultRetryPolicy())
            .AddHttpMessageHandler(() => mockHandler);

        var provider = services.BuildServiceProvider();

        // Retrieve the configured client from the service provider:
        var httpClient = provider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("TestingClient");

        var logger = provider.GetRequiredService<ILogger<TokenClient>>();
        var tokenClient = new TokenClient(logger, httpClient);

        try
        {
            await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = ["scope:read"]
            });
        }
        catch
        {
            // Swallowed purposefully.
        }

        mockHandler.NumberOfCalls.ShouldBe(expectedCalls);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, 1)]
    [InlineData(HttpStatusCode.TooManyRequests, 3)]
    [InlineData(HttpStatusCode.RequestTimeout, 3)]
    [InlineData(HttpStatusCode.InternalServerError, 3)]
    public async Task EnsureTokenRequestRetriedOnFailureWhenLoggerSpecified(HttpStatusCode httpStatusCode, int expectedCalls)
    {
        var services = new ServiceCollection();

        var mockHandler = new MockHttpMessageHandler(string.Empty, httpStatusCode);

        services.AddLogging();

        // Configure an http client with the default retry policy handler and a mock http message handler:
        services
            .AddHttpClient("TestingClient")
            .AddPolicyHandler((p, _) =>
            {
                var policyLogger = p.GetRequiredService<ILogger<TokenClient>>();
                return AccessTokenClientPolicy.GetDefaultRetryPolicy(policyLogger);
            })
            .AddHttpMessageHandler(() => mockHandler);

        var provider = services.BuildServiceProvider();

        // Retrieve the configured client from the service provider:
        var httpClient = provider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("TestingClient");

        var logger = provider.GetRequiredService<ILogger<TokenClient>>();
        var tokenClient = new TokenClient(logger, httpClient);

        try
        {
            await tokenClient.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = ["scope:read"]
            });
        }
        catch
        {
            // Swallowed purposefully.
        }

        mockHandler.NumberOfCalls.ShouldBe(expectedCalls);
    }
}
