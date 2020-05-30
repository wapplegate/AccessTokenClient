using Newtonsoft.Json;

namespace AccessTokenClient.Serialization
{
    public class NewtonsoftResponseDeserializer : IResponseDeserializer
    {
        /// <summary>
        /// Deserializes the specified string to a <see cref="TokenResponse"/>.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <returns>The token response.</returns>
        public TokenResponse Deserialize(string content)
        {
            return JsonConvert.DeserializeObject<TokenResponse>(content);
        }
    }
}