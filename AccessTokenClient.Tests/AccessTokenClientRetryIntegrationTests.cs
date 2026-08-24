using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Shouldly;
using Xunit;

namespace AccessTokenClient.Tests;

public class AccessTokenClientRetryIntegrationTests
{
    [Theory]
    [InlineData(HttpStatusCode.NotFound, 1)]
    [InlineData(HttpStatusCode.TooManyRequests, 3)]
    [InlineData(HttpStatusCode.RequestTimeout, 3)]
    [InlineData(HttpStatusCode.InternalServerError, 3)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 3)]
    [InlineData(HttpStatusCode.GatewayTimeout, 3)]
    public async Task Test(HttpStatusCode statusCode, int expectedCalls)
    {
        var mockHandler = new MockHttpMessageHandler(string.Empty, statusCode);

        var services = new ServiceCollection()
            .AddLogging()
            .AddMemoryCache()
            .AddAccessTokenClient(builder =>
            {
                // Add the default retry policy:
                builder.AddPolicyHandler((provider, _) =>
                {
                    var logger = provider.GetRequiredService<ILogger<ITokenClient>>();
                    return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
                });

                // Set up a delegating handler to mock the response for the test:
                builder.AddHttpMessageHandler(() => mockHandler);
            })
            .AddAccessTokenClientCache<MemoryTokenResponseCache>();

        var client = services.BuildServiceProvider().GetRequiredService<ITokenClient>();

        client.ShouldNotBeNull();

        Func<Task> func = async () => await client.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "https://service/token",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           =
            [
                "scope:read", "scope:create", "scope:edit", "scope:delete"
            ]
        });

        await func.ShouldThrowAsync<Exception>();

        mockHandler.NumberOfCalls.ShouldBe(expectedCalls);
    }
}
