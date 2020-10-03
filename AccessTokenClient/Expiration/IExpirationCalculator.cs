namespace AccessTokenClient.Expiration
{
    /// <summary>
    /// Represents a calculator that calculates the minutes until the access token expires.
    /// </summary>
    public interface IExpirationCalculator
    {
        /// <summary>
        /// Calculates the expiration for the token response.
        /// </summary>
        /// <param name="response">The token response.</param>
        /// <returns>The expiration of the token in minutes.</returns>
        int CalculateExpiration(TokenResponse response);
    }
}