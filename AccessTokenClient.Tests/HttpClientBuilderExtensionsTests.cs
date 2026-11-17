using AccessTokenClient.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace AccessTokenClient.Tests;

public class HttpClientBuilderExtensionsTests
{
    [Fact]
    public void EnsureAccessTokenDelegatingHandlerRegisteredSuccessfully()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddMemoryCache();

        services.AddAccessTokenClient();

        services.AddSingleton(new TestClientTokenOptions
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret"
        });

        services.AddHttpClient<TestClient>().AddClientAccessTokenHandler<TestClientTokenOptions>();

        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<TestClient>();

        client.ShouldNotBeNull();
    }

    private class TestClient
    {
        private readonly HttpClient client;

        public TestClient(HttpClient client)
        {
            this.client = client;
        }
    }

    private class TestClientTokenOptions : ITokenRequestOptions
    {
        public required string TokenEndpoint { get; set; }

        public required string ClientIdentifier { get; set; }

        public required string ClientSecret { get; set; }

        public string[] Scopes { get; set; } = [];
    }
}
