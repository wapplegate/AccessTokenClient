﻿using AccessTokenClient.Extensions;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace AccessTokenClient.Tests
{
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

            var client = provider.GetService<TestClient>();

            client.ShouldNotBeNull();
        }

        public class TestClient
        {
            private readonly HttpClient client;

            public TestClient(HttpClient client)
            {
                this.client = client;
            }
        }

        public class TestClientTokenOptions : ITokenEndpointOptions
        {
            public string TokenEndpoint { get; set; }

            public string ClientIdentifier { get; set; }

            public string ClientSecret { get; set; }

            public string[] Scopes { get; set; }
        }
    }
}