using AccessTokenClient;
using AccessTokenClient.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Benchmarks
{
    public class AccessTokenClientBenchmark
    {
        private readonly IAccessTokenClient client;

        public AccessTokenClientBenchmark()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddTokenClient();
            var provider = services.BuildServiceProvider();

            services.AddHttpClient();

            client = provider.GetService<IAccessTokenClient>();
        }

        [Benchmark]
        public async Task<TokenResponse> GetToken()
        {
            var tokenResponse = await client.GetAccessToken(new TokenRequest
            {
                ClientIdentifier = "client",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes           = new[] {"api1"},
                TokenEndpoint    = "https://localhost:44342//connect/token"
            });

            return tokenResponse;
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            var summary = BenchmarkRunner.Run<AccessTokenClientBenchmark>();
        }
    }
}