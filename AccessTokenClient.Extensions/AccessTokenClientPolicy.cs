using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;

namespace AccessTokenClient.Extensions
{
    /// <summary>
    /// This static class contains policies that can be applied
    /// easily to the access token client to increase resiliency.
    /// </summary>
    public static class AccessTokenClientPolicy
    {
        /// <summary>
        /// Returns the default retry policy for the access token client.
        /// </summary>
        /// <returns>
        /// A default <see cref="IAsyncPolicy{TResult}"/> that can be applied to the
        /// <see cref="HttpClient"/> instance that is injected into the access token client.
        /// This policy retries the token request in the event of transient http errors
        /// (5XX and 408) as well as when a 404 is encountered. The request will be retried
        /// twice, with a 1 second wait time between retries.
        /// </returns>
        public static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(message => message.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(1));
        }
    }
}