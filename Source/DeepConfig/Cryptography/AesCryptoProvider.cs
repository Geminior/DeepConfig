namespace DeepConfig.Cryptography
{
    using System.Security.Cryptography;

    /// <summary>
    /// An <see cref="ICryptoProvider"/> implementation based on the AES algorithm.
    /// </summary>
    public sealed class AesCryptoProvider : SymmetricalWithKey
    {
        /// <summary>
        /// The crypto key for this implementation
        /// </summary>
        private static readonly byte[] AesKey = new byte[] { 0x81, 0x60, 0xB1, 0xE4, 0x91, 0xAE, 0xCB, 0xA5, 0xC4, 0x16, 0x2A, 0xD7, 0x27, 0x88, 0xD5, 0x5E, 0x33, 0xA8, 0x04, 0x26, 0xA2, 0x61, 0x04, 0x67, 0xC5, 0x09, 0xA9, 0x02, 0xFD, 0xA5, 0xA6, 0x6A };

        /// <summary>
        /// The authentication key for this implementation
        /// </summary>
        private static readonly byte[] HashKey = new byte[] { 0x92, 0xEC, 0xDF, 0x3C, 0x79, 0xA4, 0x29, 0x7A, 0xEB, 0xB0, 0x54, 0xD3, 0x60, 0xDC, 0x99, 0x85, 0x72, 0xFE, 0xFC, 0x6C, 0xD0, 0x9E, 0x38, 0x20, 0x5C, 0x4A, 0x1E, 0x87, 0x05, 0xC8, 0xA8, 0x1F };

        /// <summary>
        /// The key for the Symmetrical algorithm (see <see cref="System.Security.Cryptography.SymmetricAlgorithm.Key"/> for more info)
        /// </summary>
        /// <returns>The crypto key.</returns>
        protected override byte[] GetCryptoKey()
        {
            return AesKey;
        }

        /// <summary>
        /// The key for the hash algorithm)
        /// </summary>
        /// <returns>The authentication key.</returns>
        protected override byte[] GetAuthKey()
        {
            return HashKey;
        }

        /// <summary>
        /// Factory method for creating the specific symmetric crypto service provider to use
        /// </summary>
        /// <returns>
        /// An instance of a <see cref="AesManaged"/> class
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Obviously not")]
        protected override SymmetricAlgorithm CreateServiceProvider()
        {
            return new AesManaged
            {
                BlockSize = 128,
                KeySize = 256,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
        }
    }
}
