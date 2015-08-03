namespace DeepConfig.TestTypes
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using DeepConfig.Cryptography;
    using DeepConfig.Handlers;

    public class HandlerCustom : IExtendedConfigurationSectionHandler
    {
        private List<ConfigError> _errors;

        public IEnumerable<ConfigError> Errors
        {
            get { return _errors; }
        }

        public object ReadSection(XElement section, ICryptoProvider cryptoProvider, bool decryptAll)
        {
            _errors = new List<ConfigError>();

            var res = new ConfigWithCustomHandler
            {
                ErrorsNullOnRead = GetVal(section, "ErrorsNullOnRead"),
                ErrorsNullOnWrite = GetVal(section, "ErrorsNullOnWrite"),
                HasErrorOnRead = GetVal(section, "HasErrorOnRead"),
                HasErrorOnWrite = GetVal(section, "HasErrorOnWrite"),
                ReadByHandler = true,
                ThrowOnRead = GetVal(section, "ThrowOnRead"),
                ThrowOnWrite = GetVal(section, "ThrowOnWrite"),
                WrittenByHandler = GetVal(section, "WrittenByHandler")
            };

            if (res.ThrowOnRead)
            {
                throw new Exception("Custom handler write error");
            }

            if (res.ErrorsNullOnRead)
            {
                _errors = null;
            }

            if (res.HasErrorOnRead)
            {
                _errors.Add(new ConfigError(ConfigErrorCode.InvalidConfigType, string.Empty));
            }

            return res;
        }

        public XElement WriteSection(string sectionXmlName, object sectionSource, ICryptoProvider cryptoProvider, bool encryptAll)
        {
            _errors = new List<ConfigError>();

            var o = sectionSource as ConfigWithCustomHandler;

            if (o.ThrowOnWrite)
            {
                throw new Exception("Custom handler write error");
            }

            if (o.ErrorsNullOnWrite)
            {
                _errors = null;
            }

            if (o.HasErrorOnWrite)
            {
                _errors.Add(new ConfigError(ConfigErrorCode.InvalidConfigType, string.Empty));
            }

            return new XElement(
                sectionXmlName,
                new XElement("ErrorsNullOnRead", o.ErrorsNullOnRead.ToString()),
                new XElement("ErrorsNullOnWrite", o.ErrorsNullOnWrite.ToString()),
                new XElement("HasErrorOnRead", o.HasErrorOnRead.ToString()),
                new XElement("HasErrorOnWrite", o.HasErrorOnWrite.ToString()),
                new XElement("ReadByHandler", o.ReadByHandler.ToString()),
                new XElement("ThrowOnRead", o.ThrowOnRead.ToString()),
                new XElement("ThrowOnWrite", o.ThrowOnWrite.ToString()),
                new XElement("WrittenByHandler", true.ToString()));
        }

        private bool GetVal(XElement e, string name)
        {
            var c = e.Element(name);

            return c != null ? bool.Parse(c.Value) : false;
        }
    }
}
