namespace AccessTokenClient.Encryption
{
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The string value to encrypt.</param>
        /// <returns>The encrypted value.</returns>
        string Encrypt(string value);

        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The string value to encrypt.</param>
        /// <returns>The encrypted value.</returns>
        string Decrypt(string value);
    }
}