using System;
using System.Runtime.Serialization;

namespace AccessTokenClient
{
    /// <summary>
    /// Exception that is thrown when an invalid token response is returned.
    /// </summary>
    [Serializable]
    public class InvalidTokenResponseException : Exception
    {
        public InvalidTokenResponseException()
        {
        }

        public InvalidTokenResponseException(string message) : base(message)
        {
        }

        public InvalidTokenResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTokenResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}