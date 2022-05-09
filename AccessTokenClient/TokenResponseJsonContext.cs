using System.Text.Json.Serialization;

namespace AccessTokenClient;

[JsonSerializable(typeof(TokenResponse))]
public partial class TokenResponseJsonContext : JsonSerializerContext
{
}