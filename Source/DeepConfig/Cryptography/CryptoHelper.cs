/*
 * Based on work (Modern Encryption of a String C#, by James Tuley), 
 * identified by James Tuley, free of known copyright restrictions.
 * https://gist.github.com/4336842
 * http://creativecommons.org/publicdomain/mark/1.0/ 
 */
namespace DeepConfig.Cryptography
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Helper class for cryptography
    /// </summary>
    internal static class CryptoHelper
    {
        /// <summary>
        /// Encrypts data using a key and authentication hash.
        /// </summary>
        /// <param name="crypter">The crypto algorithm implementation to use.</param>
        /// <param name="data">The data to be encrypted.</param>
        /// <param name="cryptoKey">The crypto key. Size must match the KeySize of the crypter.</param>
        /// <param name="authKey">The auth key. Size must match the KeySize of the crypter.</param>
        /// <param name="clearTextPayload">The clear text payload.</param>
        /// <returns>The encrypted data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Memory streams will not fail from being disposed multiple times, and this makes for more readable code.")]
        internal static byte[] Encrypt(SymmetricAlgorithm crypter, byte[] data, byte[] cryptoKey, byte[] authKey, byte[] clearTextPayload)
        {
            byte[] cipherText;

            //Get an IV
            crypter.GenerateIV();
            byte[] iv = crypter.IV;

            //Encrypt Data
            using (var encrypter = crypter.CreateEncryptor(cryptoKey, iv))
            using (var cipherStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                }

                cipherText = cipherStream.ToArray();
            }

            //Assemble encrypted message and add authentication
            using (var hmac = new HMACSHA256(authKey))
            using (var encryptedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(encryptedStream))
                {
                    //Add clear text data first
                    if (clearTextPayload != null)
                    {
                        binaryWriter.Write(clearTextPayload);
                    }

                    //The add the IV
                    binaryWriter.Write(iv);

                    //Write Ciphertext
                    binaryWriter.Write(cipherText);

                    //Authenticate all data
                    var tag = hmac.ComputeHash(encryptedStream.ToArray());

                    //Append tag
                    binaryWriter.Write(tag);
                }

                return encryptedStream.ToArray();
            }
        }

        /// <summary>
        /// Decrypts data using a key and authentication hash.
        /// </summary>
        /// <param name="crypter">The crypto algorithm implementation to use.</param>
        /// <param name="encryptedData">The data to be decrypted.</param>
        /// <param name="cryptoKey">The crypto key. Size must match the KeySize of the crypter.</param>
        /// <param name="authKey">The auth key. Size must match the KeySize of the crypter.</param>
        /// <param name="clearTextPayloadLength">The number of bytes of clear text payload.</param>
        /// <returns>The decrypted data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Memory streams will not fail from being disposed multiple times, and this makes for more readable code.")]
        internal static byte[] Decrypt(SymmetricAlgorithm crypter, byte[] encryptedData, byte[] cryptoKey, byte[] authKey, int clearTextPayloadLength = 0)
        {
            using (var hmac = new HMACSHA256(authKey))
            {
                //Ready the tag and iv array
                var dataTag = new byte[hmac.HashSize / 8];
                var iv = new byte[crypter.BlockSize / 8];

                //if message length is too small just return null
                if (encryptedData.Length < dataTag.Length + clearTextPayloadLength + iv.Length)
                {
                    return null;
                }

                //Get the sent tag and the expected tag
                Array.Copy(encryptedData, encryptedData.Length - dataTag.Length, dataTag, 0, dataTag.Length);

                var expectedTag = hmac.ComputeHash(encryptedData, 0, encryptedData.Length - dataTag.Length);

                //Compare Tags with constant time comparison
                var compare = 0;
                for (var i = 0; i < dataTag.Length; i++)
                {
                    compare |= dataTag[i] ^ expectedTag[i];
                }

                //if message doesn't authenticate return null
                if (compare != 0)
                {
                    return null;
                }

                //Get the IV from message
                Array.Copy(encryptedData, clearTextPayloadLength, iv, 0, iv.Length);

                using (var decrypter = crypter.CreateDecryptor(cryptoKey, iv))
                using (var plainTextStream = new MemoryStream())
                {
                    using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                    {
                        //Decrypt Cipher Text from Message
                        decrypterStream.Write(
                            encryptedData,
                            clearTextPayloadLength + iv.Length,
                            encryptedData.Length - clearTextPayloadLength - iv.Length - dataTag.Length);

                        decrypterStream.FlushFinalBlock();
                    }

                    //Return Plain Text
                    return plainTextStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Determines whether the specified data is encrypted.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <param name="authKey">The auth key. Size must match the KeySize of the crypter expected to have been doing the encryption.</param>
        /// <returns>
        /// 	<see langword="true"/> if the specified data is encrypted; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsEncrypted(byte[] data, byte[] authKey)
        {
            using (var hmac = new HMACSHA256(authKey))
            {
                //Ready the tag and iv array
                var dataTag = new byte[hmac.HashSize / 8];

                //if message length is too small its clearly not encrypted
                if (data.Length < dataTag.Length)
                {
                    return false;
                }

                //Get the sent tag and the expected tag
                Array.Copy(data, data.Length - dataTag.Length, dataTag, 0, dataTag.Length);

                var expectedTag = hmac.ComputeHash(data, 0, data.Length - dataTag.Length);

                //Compare Tags with constant time comparison
                var compare = 0;
                for (var i = 0; i < dataTag.Length; i++)
                {
                    compare |= dataTag[i] ^ expectedTag[i];
                }

                return (compare == 0);
            }
        }

        /// <summary>
        /// Encrypts data using a password.
        /// </summary>
        /// <param name="crypter">The crypto algorithm implementation to use.</param>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="password">The password.</param>
        /// <param name="saltBitSize">Size of the salt in bits.</param>
        /// <param name="iterations">The number of iterations to run when creating a derived key.</param>
        /// <returns>The encrypted data</returns>
        internal static byte[] EncryptWithPassword(SymmetricAlgorithm crypter, byte[] data, string password, int saltBitSize, int iterations)
        {
            var payload = new byte[(saltBitSize / 8) * 2];
            int payloadIdx = 0;

            byte[] cryptKey;
            byte[] authKey;

            //Use Random Salt to prevent pre-generated weak password attacks.
            using (var generator = new Rfc2898DeriveBytes(password, saltBitSize / 8, iterations))
            {
                var salt = generator.Salt;

                //Generate Keys
                cryptKey = generator.GetBytes(crypter.KeySize / 8);

                //Create Non Secret Payload
                Array.Copy(salt, 0, payload, payloadIdx, salt.Length);
                payloadIdx += salt.Length;
            }

            //Deriving separate key, might be less efficient than using HKDF, 
            //but now compatible with RNEncryptor which had a very similar wireformat and requires less code than HKDF.
            using (var generator = new Rfc2898DeriveBytes(password, saltBitSize / 8, iterations))
            {
                var salt = generator.Salt;

                //Generate Keys
                authKey = generator.GetBytes(crypter.KeySize / 8);

                //Create Rest of Non Secret Payload
                Array.Copy(salt, 0, payload, payloadIdx, salt.Length);
            }

            return Encrypt(crypter, data, cryptKey, authKey, payload);
        }

        /// <summary>
        /// Decrypts data using a password.
        /// </summary>
        /// <param name="crypter">The crypto algorithm implementation to use.</param>
        /// <param name="encryptedData">The data to decrypt.</param>
        /// <param name="password">The password.</param>
        /// <param name="saltBitSize">Size of the salt in bits.</param>
        /// <param name="iterations">The number of iterations to run when creating a derived key.</param>
        /// <returns>The decrypted data</returns>
        internal static byte[] DecryptWithPassword(SymmetricAlgorithm crypter, byte[] encryptedData, string password, int saltBitSize, int iterations)
        {
            var cryptSalt = new byte[saltBitSize / 8];
            var authSalt = new byte[saltBitSize / 8];
            int saltsLength = cryptSalt.Length + authSalt.Length;

            if (encryptedData.Length < saltsLength)
            {
                return null;
            }

            //Grab Salt from clear text payload
            Array.Copy(encryptedData, 0, cryptSalt, 0, cryptSalt.Length);
            Array.Copy(encryptedData, cryptSalt.Length, authSalt, 0, authSalt.Length);

            byte[] cryptKey;
            byte[] authKey;

            //Generate crypt key
            using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, iterations))
            {
                cryptKey = generator.GetBytes(crypter.KeySize / 8);
            }

            //Generate auth key
            using (var generator = new Rfc2898DeriveBytes(password, authSalt, iterations))
            {
                authKey = generator.GetBytes(crypter.KeySize / 8);
            }

            return Decrypt(crypter, encryptedData, cryptKey, authKey, saltsLength);
        }

        /// <summary>
        /// Determines whether the specified data is encrypted.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <param name="password">The password.</param>
        /// <param name="keyBitSize">Size of the key expected to have been used to encrypt the data</param>
        /// <param name="saltBitSize">Size of the salt in bits.</param>
        /// <param name="iterations">The number of iterations to run when creating a derived key.</param>
        /// <returns>
        /// 	<see langword="true"/> if the specified data is encrypted; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsEncrypted(byte[] data, string password, int keyBitSize, int saltBitSize, int iterations)
        {
            var authSalt = new byte[saltBitSize / 8];

            if (data.Length < authSalt.Length)
            {
                return false;
            }

            //Grab Salt from clear text payload, skipping the crypt key salt, which is the same size as the auth salt
            Array.Copy(data, authSalt.Length, authSalt, 0, authSalt.Length);

            byte[] authKey;

            //Generate auth key
            using (var generator = new Rfc2898DeriveBytes(password, authSalt, iterations))
            {
                authKey = generator.GetBytes(keyBitSize / 8);
            }

            return IsEncrypted(data, authKey);
        }
    }
}
