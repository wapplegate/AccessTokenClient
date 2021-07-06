namespace AccessTokenClient.Caching
{
    /// <summary>
    /// Options that can be used to configure the access token client. 
    /// </summary>
    public class TokenClientCacheOptions
    {
        public int ExpirationBuffer { get; set; } = 5;
    }
}