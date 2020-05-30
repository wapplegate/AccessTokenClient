using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient
{
    public class AccessTokenDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenEndpointOptions options;

        private readonly IAccessTokenClient client;

        public AccessTokenDelegatingHandler(ITokenEndpointOptions options, IAccessTokenClient client)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.client  = client  ?? throw new ArgumentNullException(nameof(client));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await client.GetAccessToken(new TokenRequest
            {
                TokenEndpoint    = options.TokenEndpoint,
                ClientIdentifier = options.ClientIdentifier,
                ClientSecret     = options.ClientSecret,
                Scopes           = options.Scopes
            });

            if (result != null && !string.IsNullOrWhiteSpace(result.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }

    public interface ITokenEndpointOptions
    {
        string TokenEndpoint { get; set; }

        string ClientIdentifier { get; set; }

        string ClientSecret { get; set; }

        string[] Scopes { get; set; }
    }

    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddClientAccessTokenHandler<T>(this IHttpClientBuilder httpClientBuilder) where T : ITokenEndpointOptions
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var options = provider.GetRequiredService<T>();
                
                var tokenClient = provider.GetRequiredService<IAccessTokenClient>();

                return new AccessTokenDelegatingHandler(options, tokenClient);
            });
        }
    }
}