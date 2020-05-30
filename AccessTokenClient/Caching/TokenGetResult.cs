namespace AccessTokenClient.Caching
{
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