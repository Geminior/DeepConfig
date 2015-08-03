namespace DeepConfig.TestTypes
{
    using System.Text;
    using DeepConfig.Cryptography;

    public class CryptoCustom : ICryptoProvider
    {
        public Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }

            set
            {
            }
        }

        public string Decrypt(string source)
        {
            return source;
        }

        public byte[] Decrypt(byte[] source)
        {
            return source;
        }

        public string Encrypt(string source)
        {
            return source;
        }

        public byte[] Encrypt(byte[] source)
        {
            return source;
        }

        public bool IsEncrypted(string source)
        {
            return true;
        }

        public bool IsEncrypted(byte[] source)
        {
            return true;
        }
    }
}
