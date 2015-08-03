namespace DeepConfig.Cryptography
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Base class for Symmetrical Cryptography using a stored Keys
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class SymmetricalWithKey : SymmetricalBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricalWithKey"/> class.
        /// </summary>
        protected SymmetricalWithKey()
        {
        }

        /// <summary>
        /// The key for the Symmetrical algorithm (see <see cref="System.Security.Cryptography.SymmetricAlgorithm.Key"/> for more info).
        /// </summary>
        /// <returns>The crypto key.</returns>
        protected abstract byte[] GetCryptoKey();

        /// <summary>
        /// The key for the hash algorithm.
        /// </summary>
        /// <returns>The authentication key.</returns>
        protected abstract byte[] GetAuthKey();

        /// <summary>
        /// Decrypts an array of bytes
        /// </summary>
        /// <param name="crypto">The crypto provider to use</param>
        /// <param name="source">The data to decrypt</param>
        /// <returns>The decrypted data</returns>
        protected override byte[] Decrypt(SymmetricAlgorithm crypto, byte[] source)
        {
            return CryptoHelper.Decrypt(crypto, source, this.GetCryptoKey(), this.GetAuthKey());
        }

        /// <summary>
        /// Encrypts an array of bytes
        /// </summary>
        /// <param name="crypto">The crypto provider to use</param>
        /// <param name="source">The data to encrypt</param>
        /// <returns>The encrypted data</returns>
        protected override byte[] Encrypt(SymmetricAlgorithm crypto, byte[] source)
        {
            return CryptoHelper.Encrypt(crypto, source, this.GetCryptoKey(), this.GetAuthKey(), null);
        }

        /// <summary>
        /// Gets whether a given byte array is encrypted
        /// </summary>
        /// <param name="source">The data to check</param>
        /// <returns>
        /// 	<see langword="true"/> if the byte array is encrypted otherwise <see langword="false"/>
        /// </returns>
        protected override bool IsEncryptedInternal(byte[] source)
        {
            return CryptoHelper.IsEncrypted(source, this.GetAuthKey());
        }
    }
}
