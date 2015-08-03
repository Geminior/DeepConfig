namespace DeepConfig.Utilities
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Core;

    /// <summary>
    /// Various Xml related utility functions
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Util", Justification = "Perfectly fine abbreviation")]
    public static class XmlUtil
    {
        /// <summary>
        /// Loads xml config
        /// </summary>
        /// <param name="xml">The xml to load</param>
        /// <param name="ignoreNamespaces">If <see langword="true"/> any namespaces in the xml will be ignored, i.e. element names will be accessible without namespace prefixes.</param>
        /// <returns>An XDocument containing the xml.</returns>
        public static XDocument LoadXmlString(string xml, bool ignoreNamespaces)
        {
            Ensure.ArgumentNotNull(xml, "xml");

            if (string.IsNullOrWhiteSpace(xml))
            {
                return CreateEmptyConfig();
            }

            try
            {
                using (var strReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(strReader))
                {
                    return LoadXml(xmlReader, ignoreNamespaces);
                }
            }
            catch (XmlException)
            {
                if (!ignoreNamespaces)
                {
                    throw;
                }

                using (var strReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(strReader))
                {
                    var doc = LoadXml(xmlReader, false);

                    return doc.StripNameSpaces(false);
                }
            }
        }

        /// <summary>
        /// Loads an xml config file
        /// </summary>
        /// <param name="xmlFile">Name of the xml file to load</param>
        /// <param name="ignoreNamespaces">If <see langword="true"/> any namespaces in the xml file will be ignored, i.e. element names will be accessible without namespace prefixes.</param>
        /// <returns>An XDocument containing the xml of the xml file.</returns>
        internal static XDocument LoadXmlFile(string xmlFile, bool ignoreNamespaces)
        {
            try
            {
                using (var xmlReader = new XmlTextReader(xmlFile))
                {
                    return LoadXml(xmlReader, ignoreNamespaces);
                }
            }
            catch (XmlException)
            {
                using (var contentsChecker = new StreamReader(xmlFile))
                {
                    if (string.IsNullOrWhiteSpace(contentsChecker.ReadToEnd()))
                    {
                        return CreateEmptyConfig();
                    }
                }

                if (!ignoreNamespaces)
                {
                    throw;
                }

                using (var xmlReader = new XmlTextReader(xmlFile))
                {
                    var doc = LoadXml(xmlReader, false);

                    return doc.StripNameSpaces(false);
                }
            }
        }

        /// <summary>
        /// Writes an xml document to a string with formatting.
        /// </summary>
        /// <param name="xmlData">The document to write.</param>
        /// <returns>The formatted xml</returns>
        internal static string ToFormattedString(this XDocument xmlData)
        {
            var enc = xmlData.GetEncoding(Encoding.UTF8);

            using (var mem = new MemoryStream())
            {
                //This may seem needlessly complex, but we want to strip the resulting string of any byte order marks, and using a StreamReader accomplishes this
                //Also there is no need to dispose of the reader since its inner stream is already closed
                var reader = new StreamReader(mem, enc);

                using (var writer = new XmlTextWriter(mem, enc))
                {
                    writer.Formatting = Formatting.Indented;

                    xmlData.Save(writer);
                    writer.Flush();

                    mem.Position = 0;
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Gets the encoding specified for the given xml document
        /// </summary>
        /// <param name="doc">The document</param>
        /// <param name="defaultEncoding">The default encoding to return if no encoding info is found in the document</param>
        /// <returns>The <see cref="System.Text.Encoding"/> of the document or <see langword="null"/> if unable to resolve.</returns>
        internal static Encoding GetEncoding(this XDocument doc, Encoding defaultEncoding)
        {
            var dec = doc.Declaration;
            if (dec != null)
            {
                string encodingName = dec.Encoding;
                if (!string.IsNullOrEmpty(encodingName))
                {
                    try
                    {
                        return Encoding.GetEncoding(encodingName);
                    }
                    catch
                    {
                        /* NOOP */
                    }
                }
            }

            return defaultEncoding;
        }

        /// <summary>
        /// Creates a new empty config doc
        /// </summary>
        /// <returns>An empty config document</returns>
        internal static XDocument CreateEmptyConfig()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(ConfigElement.RootNode));
        }

        /// <summary>
        /// Returns the fist element under the Root that matches the element name, and optionally creates it if it is missing.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="elementName">The name of the element to look for.</param>
        /// <param name="createIfMissing">Whether to create the element if it is not found.</param>
        /// <returns>The element if found (or createIfMissing is <see langword="true"/>), otherwise null.</returns>
        internal static XElement Element(this XDocument doc, XName elementName, bool createIfMissing)
        {
            var element = doc.Root.Element(elementName);

            if (element == null && createIfMissing)
            {
                element = new XElement(elementName);
                doc.Root.Add(element);
            }

            return element;
        }

        /// <summary>
        /// Returns the  first child element that matches the name and satisfies the predicate.
        /// </summary>
        /// <param name="parent">The parent node to inspect.</param>
        /// <param name="elementName">The name of the element to look for.</param>
        /// <param name="predicate">The predicate the element must satisfy.</param>
        /// <returns>The element if found, otherwise null.</returns>
        internal static XElement Element(this XElement parent, XName elementName, Func<XElement, bool> predicate)
        {
            return parent.Elements(elementName).FirstOrDefault(predicate);
        }

        /// <summary>
        /// Removes all child elements that match the name.
        /// </summary>
        /// <param name="parent">The parent node to inspect</param>
        /// <param name="elementName">The name of the element(s) to remove</param>
        internal static void RemoveChildren(this XElement parent, XName elementName)
        {
            var children = parent.Elements(elementName).ToList();

            foreach (var child in children)
            {
                child.Remove();
            }
        }

        /// <summary>
        /// Removes the  first child element that matches the name and satisfies the predicate.
        /// </summary>
        /// <param name="parent">The parent node to inspect</param>
        /// <param name="elementName">The name of the element to remove</param>
        /// <param name="predicate">The predicate the element must satisfy</param>
        internal static void RemoveChildren(this XElement parent, XName elementName, Func<XElement, bool> predicate)
        {
            var children = parent.Elements(elementName).Where(predicate).ToList();

            foreach (var child in children)
            {
                child.Remove();
            }
        }

        /// <summary>
        /// Checks if an attribute with a specific value exists. Safe to call even if the attribute is null.
        /// </summary>
        /// <param name="attrib">The attribute to check</param>
        /// <param name="value">The required value</param>
        /// <returns><see langword="true"/> if the attribute exists and has the specified value, otherwise <see langword="false"/></returns>
        internal static bool ExistsWithValue(this XAttribute attrib, string value)
        {
            if (attrib == null)
            {
                return false;
            }

            return attrib.Value == value;
        }

        /// <summary>
        /// Checks if an attribute with a specific value exists on an element. Safe to call even if the element is null.
        /// </summary>
        /// <param name="element">The parent element to check</param>
        /// <param name="attributeName">The name of the attribute to check for</param>
        /// <param name="value">The value of the attribute</param>
        /// <returns><see langword="true"/> if the attribute exists and has the specified value, otherwise <see langword="false"/></returns>
        internal static bool HasAttribute(this XElement element, XName attributeName, string value)
        {
            if (element == null)
            {
                return false;
            }

            return ExistsWithValue(element.Attribute(attributeName), value);
        }

        /// <summary>
        /// Gets the value of an attribute
        /// </summary>
        /// <param name="element">The element to check for the attribute</param>
        /// <param name="attributeName">The attribute name</param>
        /// <param name="defaultValue">A default value to return of the attribute was not found</param>
        /// <returns>The attribute's value or null if the attribute was not found</returns>
        internal static string GetAttributeValue(this XElement element, XName attributeName, string defaultValue = null)
        {
            if (element == null)
            {
                return defaultValue;
            }

            var attrib = element.Attribute(attributeName);

            return (attrib != null) ? attrib.Value : defaultValue;
        }

        /// <summary>
        /// Removes all namespace info from the document.
        /// </summary>
        /// <param name="doc">The document from which to remove namespaces (or create a clone without namespaces)</param>
        /// <param name="cloneFirst">Whether to clone the document before stripping namespaces, i.e. whether to alter the original or not</param>
        /// <returns>The document or its clone, without namespaces</returns>
        internal static XDocument StripNameSpaces(this XDocument doc, bool cloneFirst)
        {
            if (cloneFirst)
            {
                doc = new XDocument(doc);
            }

            var root = doc.Root;

            foreach (var e in root.DescendantsAndSelf())
            {
                e.Name = e.Name.LocalName;

                e.ReplaceAttributes((from a in e.Attributes().Where(at => !at.IsNamespaceDeclaration) select new XAttribute(a.Name.LocalName, a.Value)));
            }

            return doc;
        }

        private static XDocument LoadXml(XmlTextReader xmlTxtReader, bool ignoreNamespaces)
        {
            xmlTxtReader.Namespaces = !ignoreNamespaces;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.None;
            settings.IgnoreWhitespace = true;

            using (var xmlReader = XmlReader.Create(xmlTxtReader, settings))
            {
                return XDocument.Load(xmlReader);
            }
        }
    }
}
