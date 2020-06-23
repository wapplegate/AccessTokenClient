using System.Text.Json;

namespace AccessTokenClient.Serialization
{
    public class ResponseDeserializer : IResponseDeserializer
    {
        /// <inheritdoc />
        public TokenResponse Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TokenResponse>(content);
        }
    }
}