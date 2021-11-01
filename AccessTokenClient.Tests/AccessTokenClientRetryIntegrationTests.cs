using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class AccessTokenClientRetryIntegrationTests
    {
        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        public async Task Test(HttpStatusCode statusCode)
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
                        var logger = provider.GetService<ILogger<ITokenClient>>();
                        return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
                    });

                    // Set-up a delegating handler to mock the response for the test:
                    builder.AddHttpMessageHandler(() => mockHandler);
                })
                .AddAccessTokenClientCache<MemoryTokenResponseCache>();

            var client = services.BuildServiceProvider().GetService<ITokenClient>();

            client.ShouldNotBeNull();


            Func<Task> func = async () => await client.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint = "https://service/token",
                ClientIdentifier = "client-identifier",
                ClientSecret = "client-secret",
                Scopes = new[]
                {
                    "scope:read", "scope:create", "scope:edit", "scope:delete"
                }
            });

            await func.Should().ThrowAsync<Exception>();

            mockHandler.NumberOfCalls.Should().Be(3);
        }
    }
}