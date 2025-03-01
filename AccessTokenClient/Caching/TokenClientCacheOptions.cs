namespace AccessTokenClient.Caching;

public sealed class TokenClientCacheOptions
{
    /// <summary>
    /// Gets or sets the token expiration buffer.
    /// </summary>
    public int ExpirationBuffer { get; set; } = 5;

    /// <summary>
    /// Gets or sets the cache key prefix.
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "AccessTokenClient";
}
