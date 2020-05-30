namespace AccessTokenClient.Keys
{
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