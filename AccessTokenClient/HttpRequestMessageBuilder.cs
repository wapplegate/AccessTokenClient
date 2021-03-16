using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AccessTokenClient
{
    /// <summary>
    /// The default <see cref="HttpRequestMessage"/> builder.
    /// </summary>
    public class HttpRequestMessageBuilder : IHttpRequestMessageBuilder
    {
        /// <inheritdoc />
        public HttpRequestMessage GenerateHttpRequestMessage(ITokenRequest request)
        {
            var uri = new Uri(request.TokenEndpoint);

            var scopes = request.Scopes != null ? string.Join(" ", request.Scopes) : string.Empty;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",     request.ClientIdentifier),
                new KeyValuePair<string, string>("client_secret", request.ClientSecret),
                new KeyValuePair<string, string>("grant_type",    "client_credentials"),
                new KeyValuePair<string, string>("scope",         scopes)
            });

            return new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
        }
    }
}