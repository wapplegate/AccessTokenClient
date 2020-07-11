using AccessTokenClient.Encryption;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class DefaultEncryptionServiceTests
    {
        [Fact]
        public void EnsureEncryptedValueIsTheSameAsOriginal()
        {
            const string Value = "test";

            var encryptionService = new DefaultEncryptionService();
            var encryptedValue = encryptionService.Encrypt(Value);

            encryptedValue.ShouldNotBeNull();
            encryptedValue.Should().Be(Value);
        }

        [Fact]
        public void EnsureDecryptedValueIsTheSameAsOriginal()
        {
            const string Value = "test";

            var encryptionService = new DefaultEncryptionService();
            var decryptedValue = encryptionService.Decrypt(Value);

            decryptedValue.ShouldNotBeNull();
            decryptedValue.Should().Be(Value);
        }
    }
}