namespace AccessTokenClient.Caching
{
    /// <summary>
    /// A wrapper class that indicates whether the <see cref="TokenResponse"/> was retrieved successfully from cache.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class TokenGetResult<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the get operation was successful.
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// Gets or sets the cached value.
        /// </summary>
        public T Value { get; set; }
    }
}