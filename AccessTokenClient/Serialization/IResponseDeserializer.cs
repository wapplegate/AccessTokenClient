namespace AccessTokenClient.Serialization
{
    public interface IResponseDeserializer
    {
        TokenResponse Deserialize(string content);
    }
}