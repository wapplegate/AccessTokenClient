using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AccessTokenClient.Extensions
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Executes the token request.
        /// </summary>
        /// <param name="client">The http client.</param>
        /// <param name="request">The token request.</param>
        /// <returns>The token response.</returns>
        public static async Task<string> ExecuteClientCredentialsTokenRequest(this HttpClient client, TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.TokenEndpoint))
            {
                throw new ArgumentNullException(nameof(request.TokenEndpoint));
            }

            var uri = new Uri(request.TokenEndpoint);

            var scopes = request.Scopes != null ? string.Join(" ", request.Scopes) : string.Empty;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",     request.ClientIdentifier),
                new KeyValuePair<string, string>("client_secret", request.ClientSecret),
                new KeyValuePair<string, string>("grant_type",    "client_credentials"),
                new KeyValuePair<string, string>("scope",         scopes)
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };

            var responseMessage = await client.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("The request to the token endpoint was unsuccessful.");
            }

            return await responseMessage.Content.ReadAsStringAsync();
        }
    }
}