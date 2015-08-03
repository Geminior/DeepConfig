namespace DeepConfig.TestTypes
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using DeepConfig.Cryptography;
    using DeepConfig.Handlers;

    public class HandlerInvalidCtor : IExtendedConfigurationSectionHandler
    {
        public HandlerInvalidCtor(string someArg)
        {
        }

        public IEnumerable<ConfigError> Errors
        {
            get { throw new NotImplementedException(); }
        }

        public object ReadSection(XElement section, ICryptoProvider cryptoProvider, bool decryptAll)
        {
            throw new NotImplementedException();
        }

        public XElement WriteSection(string sectionXmlName, object sectionSource, ICryptoProvider cryptoProvider, bool encryptAll)
        {
            throw new NotImplementedException();
        }
    }
}
