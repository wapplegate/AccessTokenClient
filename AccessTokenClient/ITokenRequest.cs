namespace AccessTokenClient
{
    /// <summary>
    /// An abstraction of the token request.
    /// </summary>
    public interface ITokenRequest
    {
        /// <summary>
        /// Gets or sets the token endpoint URL.
        /// </summary>
        string TokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        string ClientIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the requested scopes.
        /// </summary>
        string[] Scopes { get; set; }
    }
}