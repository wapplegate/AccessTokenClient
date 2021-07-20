namespace AccessTokenClient
{
    /// <summary>
    /// Represents token request options that are necessary
    /// to make a client credentials request.
    /// </summary>
    public interface ITokenRequestOptions
    {
        /// <summary>
        /// Gets or sets the token endpoint.
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
        /// Gets or sets the scopes.
        /// </summary>
        string[] Scopes { get; set; }
    }
}