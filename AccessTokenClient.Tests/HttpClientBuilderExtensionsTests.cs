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

        services.AddSingleton(new TestClientTokenOptions());

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
        public string TokenEndpoint { get; set; }

        public string ClientIdentifier { get; set; }

        public string ClientSecret { get; set; }

        public string[] Scopes { get; set; } = [];
    }
}