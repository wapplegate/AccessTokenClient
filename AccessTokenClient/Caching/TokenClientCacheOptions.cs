namespace AccessTokenClient.Caching;

/// <summary>
/// The options used to configure caching for the token client.
/// </summary>
public sealed class TokenClientCacheOptions
{
    /// <summary>
    /// Gets or sets the token expiration buffer in minutes. This buffer reduces the time a
    /// token is cached for, to ensure a token is not cached for longer than its lifetime.
    /// </summary>
    public int ExpirationBuffer { get; set; } = 5;

    /// <summary>
    /// Gets or sets the cache key prefix.
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "AccessTokenClient";
}
