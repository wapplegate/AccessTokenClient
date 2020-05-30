namespace AccessTokenClient
{
    public class TokenRequest
    {
        /// <summary>
        /// Gets or sets the token endpoint URL.
        /// </summary>
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the requested scopes.
        /// </summary>
        public string[] Scopes { get; set; }
    }
}