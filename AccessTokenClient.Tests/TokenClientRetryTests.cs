using AccessTokenClient.Extensions;
using AccessTokenClient.Serialization;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenClientRetryTests
    {
        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task EnsureTokenRequestRetriedOnFailure(HttpStatusCode httpStatusCode)
        {
            var services = new ServiceCollection();

            var mockHandler = new MockHttpMessageHandler(string.Empty, httpStatusCode);

            // Configure an http client with the default retry policy handler and a mock http message handler:
            services
                .AddHttpClient("TestingClient")
                .AddPolicyHandler(AccessTokenClientPolicy.GetDefaultRetryPolicy())
                .AddHttpMessageHandler(() => mockHandler);

            // Retrieve the configured client from the service provider:
            var httpClient = services
                .BuildServiceProvider()
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient("TestingClient");

            var logger = new NullLogger<TokenClient>();
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            try
            {
                await tokenClient.RequestAccessToken(new TokenRequest
                {
                    TokenEndpoint    = "http://www.token-endpoint.com",
                    ClientIdentifier = "client-identifier",
                    ClientSecret     = "client-secret",
                    Scopes           = new[] { "scope:read" }
                });
            }
            catch
            {
                // Swallowed purposefully.
            }

            // Currently, the retry policy is configured to retry twice, thus the total number of calls will be three:
            mockHandler.NumberOfCalls.Should().Be(3);
        }
    }
}