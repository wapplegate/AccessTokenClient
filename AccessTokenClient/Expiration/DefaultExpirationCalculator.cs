using System;

namespace AccessTokenClient.Expiration
{
    /// <summary>
    /// An expiration calculator the calculates the minutes until the access token expires.
    /// </summary>
    public class DefaultExpirationCalculator : IExpirationCalculator
    {
        /// <summary>
        /// Calculates the expiration for the token response.
        /// </summary>
        /// <param name="response">The token response.</param>
        /// <returns>A <see cref="TimeSpan"/> indicating when the token expires.</returns>
        public TimeSpan CalculateExpiration(TokenResponse response)
        {
            // Reduces the time by 5 minutes to ensure token is valid when used:
            return TimeSpan.FromMinutes(response.ExpiresIn / 60 - 5);
        }
    }
}