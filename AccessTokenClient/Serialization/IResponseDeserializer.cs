namespace AccessTokenClient.Serialization
{
    /// <summary>
    /// Represents a deserializer that converts a <see cref="string"/> to a <see cref="TokenResponse"/>.
    /// </summary>
    public interface IResponseDeserializer
    {
        /// <summary>
        /// Deserializes the specified string to a <see cref="TokenResponse"/>.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <returns>The token response.</returns>
        TokenResponse Deserialize(string content);
    }
}