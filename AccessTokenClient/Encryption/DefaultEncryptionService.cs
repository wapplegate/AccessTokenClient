namespace AccessTokenClient.Encryption
{
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