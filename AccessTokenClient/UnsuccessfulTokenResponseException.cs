using System;

namespace AccessTokenClient
{
    /// <summary>
    /// Exception that is thrown when an unsuccessful token response is returned.
    /// </summary>
    public class UnsuccessfulTokenResponseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsuccessfulTokenResponseException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public UnsuccessfulTokenResponseException(string message) : base(message)
        {
        }
    }
}