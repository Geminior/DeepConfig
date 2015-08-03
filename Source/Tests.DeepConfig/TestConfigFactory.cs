namespace DeepConfig
{
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using DeepConfig.Core;

    public static class TestConfigFactory
    {
        public static XDocument CreateConfig(params XName[] sections)
        {
            var xmlDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(ConfigElement.RootNode));

            var q = from s in sections
                    select new XElement(s);

            xmlDoc.Root.Add(q);

            return xmlDoc;
        }

        public static XDocument CreateConfigEx(params object[] content)
        {
            var xmlDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(ConfigElement.RootNode));

            xmlDoc.Root.Add(content);

            return xmlDoc;
        }

        public static void CreateXmlFile(string filePath, string xml, bool readOnly)
        {
            if (readOnly && File.Exists(filePath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }

            using (var w = new StreamWriter(filePath, false))
            {
                w.Write(xml);
            }

            if (readOnly)
            {
                File.SetAttributes(filePath, FileAttributes.ReadOnly);
            }
        }
    }
}
