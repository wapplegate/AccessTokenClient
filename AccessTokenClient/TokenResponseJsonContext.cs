using System.Text.Json.Serialization;

namespace AccessTokenClient;

[JsonSerializable(typeof(TokenResponseHttp))]
public partial class TokenResponseJsonContext : JsonSerializerContext
{
}