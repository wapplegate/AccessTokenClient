using System;
using System.Runtime.Serialization;

namespace AccessTokenClient
{
    /// <summary>
    /// Exception that is thrown when an unsuccessful token response is returned.
    /// </summary>
    [Serializable]
    public class UnsuccessfulTokenResponseException : Exception
    {
        public UnsuccessfulTokenResponseException()
        {
        }

        public UnsuccessfulTokenResponseException(string message) : base(message)
        {
        }

        public UnsuccessfulTokenResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsuccessfulTokenResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}