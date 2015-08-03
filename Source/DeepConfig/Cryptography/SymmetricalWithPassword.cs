namespace DeepConfig.Cryptography
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Base class for Symmetrical Cryptography using a password. This form of encryption is harder to break, but also vastly slower than using a <see cref="SymmetricalWithKey"/>.
    /// </summary>
    /// <remarks>
    /// NOTE: Depending on the number of iterations (default is 1000) this can be a very slow method of encryption.
    /// </remarks>
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class SymmetricalWithPassword : SymmetricalBase
    {
        /// <summary>
        /// Helper field to avoid having to instantiate an encrypter to get the KeySize.
        /// </summary>
        private int? _keyBitSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricalWithPassword"/> class.
        /// </summary>
        protected SymmetricalWithPassword()
            : this(null, 64, 1000)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricalWithPassword"/> class.
        /// </summary>
        /// <param name="password">The password to use for encryption/descryption.</param>
        protected SymmetricalWithPassword(string password)
            : this(password, 64, 1000)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricalWithPassword"/> class.
        /// </summary>
        /// <param name="password">The password to use for encryption/descryption.</param>
        /// <param name="saltBitSize">Size of the salt in bits.</param>
        /// <param name="iterations">The number of iterations to make when creating a derived key.</param>
        protected SymmetricalWithPassword(string password, int saltBitSize, int iterations)
            : base()
        {
            this.Password = password;
            this.SaltBitSize = saltBitSize;
            this.Iterations = iterations;
        }

        /// <summary>
        /// Gets or sets the password used for encryption and decryption
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the salt in bits
        /// </summary>
        public int SaltBitSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of iterations to make when creating a derived key. The more the safer.
        /// </summary>
        public int Iterations
        {
            get;
            set;
        }

        /// <summary>
        /// Decrypts an array of bytes
        /// </summary>
        /// <param name="crypto">The crypto provider to use</param>
        /// <param name="source">The data to decrypt</param>
        /// <returns>The decrypted data</returns>
        protected override byte[] Decrypt(SymmetricAlgorithm crypto, byte[] source)
        {
            return CryptoHelper.DecryptWithPassword(crypto, source, this.Password, this.SaltBitSize, this.Iterations);
        }

        /// <summary>
        /// Encrypts an array of bytes
        /// </summary>
        /// <param name="crypto">The crypto provider to use</param>
        /// <param name="source">The data to encrypt</param>
        /// <returns>The encrypted data</returns>
        protected override byte[] Encrypt(SymmetricAlgorithm crypto, byte[] source)
        {
            return CryptoHelper.EncryptWithPassword(crypto, source, this.Password, this.SaltBitSize, this.Iterations);
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
            if (!_keyBitSize.HasValue)
            {
                using (var crypter = CreateServiceProvider())
                {
                    _keyBitSize = crypter.KeySize;
                }
            }

            return CryptoHelper.IsEncrypted(source, this.Password, _keyBitSize.Value, this.SaltBitSize, this.Iterations);
        }
    }
}
