using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AccessTokenClient.Extensions;

/// <summary>
/// This static class contains policies that can be applied
/// easily to the access token client to increase resiliency.
/// </summary>
public static class AccessTokenClientPolicy
{
    /// <summary>
    /// Returns the default retry policy for the access token client.
    /// </summary>
    /// <param name="logger">An optional logger instance.</param>
    /// <returns>
    /// A default <see cref="IAsyncPolicy{TResult}"/> that can be applied to the
    /// <see cref="HttpClient"/> instance that is injected into the access token client.
    /// This policy retries the token request in the event of transient http errors
    /// (5XX and 408) as well as when a 429 is encountered. The request will be retried
    /// twice using exponential backoff with jitter, or the server's Retry-After value.
    /// </returns>
    public static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy(ILogger<ITokenClient>? logger = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                2,
                (retryAttempt, outcome, _) => GetRetryDelay(retryAttempt, outcome.Result),
                (_, timespan, retryAttempt, _) =>
                {
                    logger?.LogWarning("Delaying for {delay}ms, then making retry attempt #{retry}.", timespan.TotalMilliseconds, retryAttempt);
                    return Task.CompletedTask;
                });
    }

    private static TimeSpan GetRetryDelay(int retryAttempt, HttpResponseMessage? response)
    {
        var retryAfter = response?.Headers.RetryAfter;

        if (retryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return Min(delta, TimeSpan.FromSeconds(30));
        }

        if (retryAfter?.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;

            if (delay > TimeSpan.Zero)
            {
                return Min(delay, TimeSpan.FromSeconds(30));
            }
        }

        var exponentialDelay = 200 * Math.Pow(2, retryAttempt - 1);
        var jitter = Random.Shared.Next(0, 101);

        return TimeSpan.FromMilliseconds(exponentialDelay + jitter);
    }

    private static TimeSpan Min(TimeSpan first, TimeSpan second)
    {
        return first <= second ? first : second;
    }
}
