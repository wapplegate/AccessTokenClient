namespace AccessTokenClient.Encryption
{
    /// <summary>
    /// The default encryption service which provides no encryption or decryption.
    /// </summary>
    public class DefaultEncryptionService : IEncryptionService
    {
        /// <inheritdoc />
        public string Encrypt(string value)
        {
            return value;
        }

        /// <inheritdoc />
        public string Decrypt(string value)
        {
            return value;
        }
    }
}