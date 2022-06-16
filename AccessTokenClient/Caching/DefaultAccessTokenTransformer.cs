namespace AccessTokenClient.Caching;

/// <summary>
/// The default transformer which provides no conversion or reversion of the access token.
/// </summary>
public class DefaultAccessTokenTransformer : IAccessTokenTransformer
{
    /// <inheritdoc />
    public string Convert(string accessToken)
    {
        return accessToken;
    }

    /// <inheritdoc />
    public string Revert(string value)
    {
        return value;
    }
}