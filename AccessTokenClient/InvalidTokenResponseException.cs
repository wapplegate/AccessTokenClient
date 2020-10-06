using System;

namespace AccessTokenClient
{
    /// <summary>
    /// Exception that is thrown when an invalid token response is returned.
    /// </summary>
    public class InvalidTokenResponseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTokenResponseException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InvalidTokenResponseException(string message) : base(message)
        {
        }
    }
}
