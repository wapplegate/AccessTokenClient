using System.Text.Json;

namespace AccessTokenClient.Serialization
{
    /// <summary>
    /// A response deserializer that converts a json <see cref="string"/> to a <see cref="TokenResponse"/>.
    /// </summary>
    public class ResponseDeserializer : IResponseDeserializer
    {
        /// <inheritdoc />
        public TokenResponse Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TokenResponse>(content);
        }
    }
}