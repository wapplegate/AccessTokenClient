using AccessTokenClient;
using AccessTokenClient.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Benchmarks
{
    public class Parsing
    {
        private readonly IAccessTokenClient client;

        public Parsing()
        {
            var services = new ServiceCollection();
            services.AddTokenClient();
            var provider = services.BuildServiceProvider();

            services.AddHttpClient();

            client = provider.GetService<IAccessTokenClient>();
        }

        [Benchmark]
        public async Task GetToken()
        {
            var tokenResponse = await client.GetAccessToken(new TokenRequest
            {
                ClientIdentifier = "client",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes           = new[] {"api1"},
                TokenEndpoint    = "https://localhost:5001/connect/token"
            });
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            var summary = BenchmarkRunner.Run<Parsing>();
        }
    }
}