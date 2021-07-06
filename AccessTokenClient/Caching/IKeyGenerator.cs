namespace AccessTokenClient.Caching
{
    /// <summary>
    /// Represents a key generator that generates a cache key from a <see cref="TokenRequest"/>.
    /// </summary>
    public interface IKeyGenerator
    {
        /// <summary>
        /// Generates a cache key from the given token request.
        /// </summary>
        /// <param name="request">The token request to generate a key for.</param>
        /// <returns>The token request key.</returns>
        string GenerateTokenRequestKey(TokenRequest request);
    }
}