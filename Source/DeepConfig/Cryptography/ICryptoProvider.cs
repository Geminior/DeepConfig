namespace DeepConfig.Cryptography
{
    using System.Text;

    /// <summary>
    /// Interface for Cryptography providers
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// Gets or sets the encoding used for encoding and decoding strings.
        /// <para>
        /// A string Encrypted using one encoding must be Decrypted using the same encoding.
        /// </para>
        /// </summary>
        Encoding Encoding
        {
            get;
            set;
        }

        /// <overloads>Decrypts a value.</overloads>
        /// <summary>
        /// Decrypts the string
        /// </summary>
        /// <param name="source">The source to be decrypted</param>
        /// <returns>The decrypted string</returns>
        string Decrypt(string source);

        /// <summary>
        /// Decrypts the byte array
        /// </summary>
        /// <param name="source">The source to be decrypted</param>
        /// <returns>The decrypted byte array</returns>
        byte[] Decrypt(byte[] source);

        /// <overloads>Encypts a value.</overloads>
        /// <summary>
        /// Encrypts the string
        /// </summary>
        /// <param name="source">The source to be encrypted</param>
        /// <returns>An encrypted string in Base64 format</returns>
        string Encrypt(string source);

        /// <summary>
        /// Encrypts the byte array
        /// </summary>
        /// <param name="source">The source to be encrypted</param>
        /// <returns>The encrypted byte array</returns>
        byte[] Encrypt(byte[] source);

        /// <summary>
        /// Gets whether the string is encrypted (by this encrypter)
        /// </summary>
        /// <param name="source">The source to check</param>
        /// <returns><see langword="true" /> if the string is encrypted otherwise <see langword="false" /></returns>
        bool IsEncrypted(string source);

        /// <summary>
        /// Gets whether the byte array is encrypted (by this encrypter)
        /// </summary>
        /// <param name="source">The source to check</param>
        /// <returns><see langword="true" /> if the byte array is encrypted otherwise <see langword="false" /></returns>
        bool IsEncrypted(byte[] source);
    }
}
