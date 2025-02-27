using System.Net;
using AccessTokenClient;
using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Benchmarks;

public class Program
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60, baseline: true)]
    public class AccessTokenClientBenchmarks
    {
        private readonly ITokenClient client;

        public AccessTokenClientBenchmarks()
        {
            const string Response = "{\"access_token\":\"1234567890\",\"token_type\":\"Bearer\",\"expires_in\":7199}";

            var mockHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);

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

            client = services.BuildServiceProvider().GetRequiredService<ITokenClient>();
        }

        [Benchmark]
        public async Task<TokenResponse> GetAccessToken()
        {
            return await client.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "https://localhost:44303/connect/token",
                ClientIdentifier = "testing_client_identifier",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes =
                [
                    "movie:read", "movie:create", "movie:edit", "movie:delete"
                ]
            });
        }
    }

    public static void Main()
    {
        var summary = BenchmarkRunner.Run<AccessTokenClientBenchmarks>();
    }
}