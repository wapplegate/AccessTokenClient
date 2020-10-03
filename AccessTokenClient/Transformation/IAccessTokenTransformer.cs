namespace AccessTokenClient.Transformation
{
    /// <summary>
    /// Represents a transformer that converts and reverts an access token from one state to another.
    /// </summary>
    public interface IAccessTokenTransformer
    {
        /// <summary>
        /// Converts the access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The converted access token.</returns>
        string Convert(string accessToken);

        /// <summary>
        /// Reverts the access token back to its original state.
        /// </summary>
        /// <param name="value">The converted access token.</param>
        /// <returns>The access token.</returns>
        string Revert(string value);
    }
}