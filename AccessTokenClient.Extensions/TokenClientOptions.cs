namespace AccessTokenClient.Extensions
{
    /// <summary>
    /// Options that can be used to configure the access token client. 
    /// </summary>
    public class TokenClientOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to enable caching of the access token response.
        /// </summary>
        public bool EnableCaching { get; set; } = true;
    }
}