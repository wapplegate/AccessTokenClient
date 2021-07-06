using AccessTokenClient;
using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Benchmarks
{
    public class Program
    {
        public class AccessTokenClientBenchmarks
        {
            private readonly ITokenClient client;

            public AccessTokenClientBenchmarks()
            {
                var services = new ServiceCollection();

                services.AddMemoryCache().AddAccessTokenClient().AddAccessTokenClientCaching<MemoryTokenResponseCache>();

                var provider = services.BuildServiceProvider();

                client = provider.GetService<ITokenClient>();
            }

            [Benchmark(Baseline = true)]
            public async Task<TokenResponse> GetAccessToken()
            {
                return await client.RequestAccessToken(new TokenRequest
                {
                    TokenEndpoint    = "https://localhost:44303/connect/token",
                    ClientIdentifier = "testing_client_identifier",
                    ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                    Scopes = new[]
                    {
                        "employee:read", "employee:create", "employee:edit", "employee:delete"
                    }
                });
            }
        }

        public static void Main()
        {
            var summary = BenchmarkRunner.Run<AccessTokenClientBenchmarks>();
        }
    }
}