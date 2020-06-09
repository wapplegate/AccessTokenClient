using AccessTokenClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestingThreePointOneApplication
{
    public class TestingClient : ITestingClient
    {
        private readonly HttpClient client;

        public TestingClient(HttpClient client)
        {
            this.client = client;
        }

        public async Task<string> Get()
        {
            var response = await client.GetAsync(new Uri("https://google.com"));

            return await response.Content.ReadAsStringAsync();
        }
    }

    public interface ITestingClient
    {
        Task<string> Get();
    }

    public class TestingClientOptions : ITokenEndpointOptions
    {
        public string TokenEndpoint { get; set; }

        public string ClientIdentifier { get; set; }

        public string ClientSecret { get; set; }

        public string[] Scopes { get; set; }
    }
}