namespace AccessTokenClient
{
    public interface ITokenEndpointOptions
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