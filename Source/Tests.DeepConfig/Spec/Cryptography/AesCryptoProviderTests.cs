namespace DeepConfig.Spec.Cryptography
{
    using System.Security.Cryptography;
    using System.Text;
    using DeepConfig.Cryptography;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AesCryptoProviderTests
    {
        [TestMethod]
        public void Encrypting_and_decrypting_a_string_with_aes_should_work()
        {
            /* Arrange */
            string source = "SomeValue";
            var crypto = new AesCryptoProvider();

            /* Act */
            string encryptedValue = crypto.Encrypt(source);

            bool isEncrypted = crypto.IsEncrypted(encryptedValue);

            string decryptedVale = crypto.Decrypt(encryptedValue);

            /* Assert */
            encryptedValue.Should().NotBe(source);
            decryptedVale.Should().Be(source);
            isEncrypted.Should().BeTrue();
        }

        [TestMethod]
        public void Encrypting_and_decrypting_null_or_empty_string_should_work()
        {
            /* Arrange */
            var crypto = new AesCryptoProvider();

            /* Act */
            string nullStringEncrypt = crypto.Encrypt((string)null);
            string emptyStringEncrypt = crypto.Encrypt(string.Empty);
            byte[] nullByteEncrypt = crypto.Encrypt((byte[])null);

            bool isEncryptedNullString = crypto.IsEncrypted((string)null);
            bool isEncryptedEmptyString = crypto.IsEncrypted(string.Empty);
            bool isEncryptedNullByte = crypto.IsEncrypted((byte[])null);

            string nullStringDecrypt = crypto.Decrypt((string)null);
            string emptyStringDecrypt = crypto.Decrypt(string.Empty);
            byte[] nullByteDecrypt = crypto.Decrypt((byte[])null);

            /* Assert */
            nullStringEncrypt.Should().BeNull();
            emptyStringEncrypt.Should().BeEmpty();
            nullByteEncrypt.Should().BeNull();

            isEncryptedNullString.Should().BeFalse();
            isEncryptedEmptyString.Should().BeFalse();
            isEncryptedNullByte.Should().BeFalse();

            nullStringDecrypt.Should().BeNull();
            emptyStringDecrypt.Should().BeEmpty();
            nullByteDecrypt.Should().BeNull();
        }

        [TestMethod]
        public void Encrypting_with_one_provider_and_decrypting_with_another_should_not_work()
        {
            /* Arrange */
            string source = "SomeValue";
            var crypto1 = new AesCryptoProvider();
            var crypto2 = new TestCryptoProvider();

            /* Act */
            string encryptedValue = crypto1.Encrypt(source);

            bool isEncrypted = crypto2.IsEncrypted(encryptedValue);

            string decryptedVale = crypto2.Decrypt(encryptedValue);

            /* Assert */
            decryptedVale.Should().NotBe(source);
            isEncrypted.Should().BeFalse();
        }

        [TestMethod]
        public void Checking_for_encryption_should_work()
        {
            /* Arrange */
            string source = "SomePlainValue";
            var crypto = new AesCryptoProvider();

            /* Act */
            bool isEncrypted = crypto.IsEncrypted(source);

            /* Assert */
            isEncrypted.Should().BeFalse();
        }

        [TestMethod]
        public void Decrypting_data_that_could_not_possibly_be_encrypted_should_return_null()
        {
            /* Arrange */
            byte[] sourceTooSmall = new byte[4];
            var crypto = new AesCryptoProvider();

            /* Act */
            bool isEncrypted = crypto.IsEncrypted(sourceTooSmall);

            var decryptedVale = crypto.Decrypt(sourceTooSmall);

            /* Assert */
            decryptedVale.Should().BeNull();
            isEncrypted.Should().BeFalse();
        }

        [TestMethod]
        public void Setting_and_getting_encoding_on_crypto_should_work()
        {
            /* Arrange */
            var crypto1 = new AesCryptoProvider();
            var crypto2 = new AesCryptoProvider();

            /* Act */
            crypto1.Encoding = Encoding.Unicode;
            crypto2.Encoding = null;

            /* Assert */
            crypto1.Encoding.Should().Be(Encoding.Unicode);
            crypto2.Encoding.Should().Be(Encoding.UTF8);
        }

        [TestMethod]
        public void Encrypting_and_decrypting_a_string_with_password_should_work()
        {
            /* Arrange */
            string source = "SomeValue";
            var crypto = new TestCryptoProviderWithPass
            {
                Password = "testingisawesome",
                Iterations = 10,
                SaltBitSize = 128
            };

            /* Act */
            string encryptedValue = crypto.Encrypt(source);

            bool isEncrypted = crypto.IsEncrypted(encryptedValue);

            string decryptedVale = crypto.Decrypt(encryptedValue);

            /* Assert */
            encryptedValue.Should().NotBe(source);
            decryptedVale.Should().Be(source);
            isEncrypted.Should().BeTrue();

            crypto.Password.Should().Be("testingisawesome");
            crypto.Iterations.Should().Be(10);
            crypto.SaltBitSize.Should().Be(128);
        }

        [TestMethod]
        public void Decrypting_data_that_could_not_possibly_be_encrypted_by_pass_should_return_null()
        {
            /* Arrange */
            byte[] sourceTooSmall = new byte[4];
            var crypto = new TestCryptoProviderWithPass("testingisawesome");

            /* Act */
            bool isEncrypted = crypto.IsEncrypted(sourceTooSmall);

            var decryptedVale = crypto.Decrypt(sourceTooSmall);

            /* Assert */
            decryptedVale.Should().BeNull();
            isEncrypted.Should().BeFalse();
        }

        private class TestCryptoProvider : SymmetricalWithKey
        {
            private static readonly byte[] AesKey = new byte[] { 0x16, 0xCA, 0x9B, 0x26, 0xCA, 0xF1, 0xB3, 0xFF, 0xAA, 0x0F, 0xE0, 0x90, 0x22, 0x42, 0xC2, 0x1D, 0x5A, 0x2C, 0x76, 0x23, 0xEB, 0xF8, 0x75, 0x1E, 0x4A, 0x12, 0xA8, 0x8E, 0x65, 0xA1, 0x4A, 0x82 };
            private static readonly byte[] HashKey = new byte[] { 0xD8, 0x45, 0x2B, 0xDF, 0x47, 0x31, 0xD6, 0xBB, 0x38, 0x18, 0x31, 0xA9, 0x57, 0xCA, 0x00, 0x7D, 0xF3, 0x8D, 0xBD, 0x06, 0x09, 0xFD, 0xD4, 0x1E, 0x55, 0x7C, 0xDB, 0xB7, 0x48, 0x24, 0x9A, 0x09 };

            protected override byte[] GetCryptoKey()
            {
                return AesKey;
            }

            protected override byte[] GetAuthKey()
            {
                return HashKey;
            }

            protected sealed override SymmetricAlgorithm CreateServiceProvider()
            {
                return new AesManaged();
            }
        }

        private class TestCryptoProviderWithPass : SymmetricalWithPassword
        {
            public TestCryptoProviderWithPass()
                : base()
            {
            }

            public TestCryptoProviderWithPass(string pass)
                : base(pass)
            {
            }

            protected override SymmetricAlgorithm CreateServiceProvider()
            {
                return new AesManaged();
            }
        }
    }
}
