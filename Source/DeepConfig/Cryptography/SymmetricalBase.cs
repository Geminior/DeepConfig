namespace DeepConfig.Cryptography
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Base class for Symmetrical Cryptography
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class SymmetricalBase : ICryptoProvider
    {
        /// <summary>
        /// The encoding used for encoding and decoding strings.
        /// </summary>
        private Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricalBase"/> class.
        /// </summary>
        protected SymmetricalBase()
        {
        }

        /// <summary>
        /// Gets or sets the encoding used for encoding and decoding strings.
        /// <para>
        /// A string Encrypted using one encoding must be Decrypted using the same encoding.
        /// </para>
        /// <para>
        /// Default is <see cref="System.Text.Encoding.UTF8"/>
        /// </para>
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                return _encoding;
            }

            set
            {
                if (value == null)
                {
                    value = Encoding.UTF8;
                }

                _encoding = value;
            }
        }

        /// <overloads>Decrypts a value.</overloads>
        /// <summary>
        /// Decrypts the string
        /// </summary>
        /// <param name="source">The source to be decrypted</param>
        /// <returns>The decrypted string</returns>
        /// <exception cref="ArgumentException">If <c>source</c> is not valid for the chosen <see cref="Encoding"/></exception>
        /// <exception cref="FormatException">If <c>source</c> is not in Base64</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">If <c>source</c> was not previously encrypted with the same Key/IV</exception>
        public string Decrypt(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var data = Convert.FromBase64String(source);

            var bytes = Decrypt(data);

            if (bytes == null)
            {
                return null;
            }

            return _encoding.GetString(bytes);
        }

        /// <summary>
        /// Decrypts the byte array
        /// </summary>
        /// <param name="source">The source to be decrypted</param>
        /// <returns>The decrypted byte array</returns>
        /// <exception cref="ArgumentException">If <c>source</c> is not valid for the chosen <see cref="Encoding"/></exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">If <c>source</c> was not previously encrypted with the same Key/IV</exception>
        public byte[] Decrypt(byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return source;
            }

            using (var crypter = CreateServiceProvider())
            {
                return Decrypt(crypter, source);
            }
        }

        /// <overloads>Encypts a value.</overloads>
        /// <summary>
        /// Encrypts the string
        /// </summary>
        /// <param name="source">The source to be encrypted</param>
        /// <returns>An encrypted string in Base64 format</returns>
        /// <exception cref="ArgumentException">If <c>source</c> is not valid for the chosen <see cref="Encoding"/></exception>
        public string Encrypt(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            byte[] data = _encoding.GetBytes(source);

            return Convert.ToBase64String(Encrypt(data));
        }

        /// <summary>
        /// Encrypts the byte array
        /// </summary>
        /// <param name="source">The source to be encrypted</param>
        /// <returns>An encrypted byte array</returns>
        public byte[] Encrypt(byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return source;
            }

            using (var crypter = CreateServiceProvider())
            {
                return Encrypt(crypter, source);
            }
        }

        /// <summary>
        /// Gets whether the string is encrypted (by this encrypter)
        /// </summary>
        /// <param name="source">The source to check</param>
        /// <returns><see langword="true" /> if the string is encrypted otherwise <see langword="false" /></returns>
        public bool IsEncrypted(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            try
            {
                var data = Convert.FromBase64String(source);

                return IsEncrypted(data);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether the byte array is encrypted (by this encrypter)
        /// </summary>
        /// <param name="source">The source to check</param>
        /// <returns><see langword="true" /> if the byte array is encrypted otherwise <see langword="false" /></returns>
        public bool IsEncrypted(byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return false;
            }

            return IsEncryptedInternal(source);
        }

        /// <summary>
        /// Factory method for creating the specific symmetric crypto service provider to use
        /// </summary>
        /// <returns>An instance of a <see cref="SymmetricAlgorithm"/> derived class</returns>
        protected abstract SymmetricAlgorithm CreateServiceProvider();

        /// <summary>
        /// Override to implement the actual decryption logic
        /// </summary>
        /// <param name="crypto">The crypto provider to use</param>
        /// <param name="source">The data to decrypt</param>
        /// <returns>The decrypted data</returns>
        protected abstract byte[] Decrypt(SymmetricAlgorithm crypto, byte[] source);

        /// <summary>
        /// Override to implement the actual encryption logic
        /// </summary>
        /// <param name="crypto">The crypto provider to use</param>
        /// <param name="source">The data to encrypt</param>
        /// <returns>The encrypted data</returns>
        protected abstract byte[] Encrypt(SymmetricAlgorithm crypto, byte[] source);

        /// <summary>
        /// Override to implement the encryption check logic
        /// </summary>
        /// <param name="source">The data to check</param>
        /// <returns><see langword="true" /> if the byte array is encrypted otherwise <see langword="false" /></returns>
        protected abstract bool IsEncryptedInternal(byte[] source);
    }
}
