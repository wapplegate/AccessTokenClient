using AccessTokenClient.Extensions;
using AccessTokenClient.Serialization;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
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
                .AddPolicyHandler((_, _) => AccessTokenClientPolicy.GetDefaultRetryPolicy())
                .AddHttpMessageHandler(() => mockHandler);

            var provider = services.BuildServiceProvider();

            // Retrieve the configured client from the service provider:
            var httpClient = provider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient("TestingClient");

            var logger = provider.GetRequiredService<ILogger<TokenClient>>();
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            try
            {
                await tokenClient.RequestAccessToken(new TokenRequest
                {
                    TokenEndpoint = "http://www.token-endpoint.com",
                    ClientIdentifier = "client-identifier",
                    ClientSecret = "client-secret",
                    Scopes = new[] { "scope:read" }
                });
            }
            catch
            {
                // Swallowed purposefully.
            }

            // Currently, the retry policy is configured to retry twice, thus the total number of calls will be three:
            mockHandler.NumberOfCalls.Should().Be(3);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task EnsureTokenRequestRetriedOnFailureWhenLoggerSpecified(HttpStatusCode httpStatusCode)
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
            var tokenClient = new TokenClient(logger, httpClient, new ResponseDeserializer());

            try
            {
                await tokenClient.RequestAccessToken(new TokenRequest
                {
                    TokenEndpoint = "http://www.token-endpoint.com",
                    ClientIdentifier = "client-identifier",
                    ClientSecret = "client-secret",
                    Scopes = new[] { "scope:read" }
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