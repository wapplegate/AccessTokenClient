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

        private readonly ITokenClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenDelegatingHandler"/> class.
        /// </summary>
        /// <param name="options">The token endpoint options.</param>
        /// <param name="client">The access token client.</param>
        public AccessTokenDelegatingHandler(ITokenEndpointOptions options, ITokenClient client)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.client  = client  ?? throw new ArgumentNullException(nameof(client));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await client.RequestAccessToken(new TokenRequest
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
}