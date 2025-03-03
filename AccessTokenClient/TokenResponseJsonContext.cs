using System.Text.Json.Serialization;

namespace AccessTokenClient;

[JsonSerializable(typeof(HttpTokenResponse))]
internal partial class TokenResponseJsonContext : JsonSerializerContext
{
}
