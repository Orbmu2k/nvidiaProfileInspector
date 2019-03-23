using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace nspector.Common.Helper
{
    internal static class XMLHelper<T> where T : new()
    {
        static XmlSerializer xmlSerializer;

        static XMLHelper()
        {
            xmlSerializer = new XmlSerializer(typeof(T));
        }

        internal static string SerializeToXmlString(T xmlObject, Encoding encoding, bool removeNamespace)
        {
            var memoryStream = new MemoryStream();
            var xmlWriter = new XmlTextWriter(memoryStream, encoding) { Formatting = Formatting.Indented };

            if (removeNamespace)
            {
                var xs = new XmlSerializerNamespaces();
                xs.Add("", "");
                xmlSerializer.Serialize(xmlWriter, xmlObject, xs);
            }
            else
                xmlSerializer.Serialize(xmlWriter, xmlObject);

            return encoding.GetString(memoryStream.ToArray());
        }

        internal static void SerializeToXmlFile(T xmlObject, string filename, Encoding encoding, bool removeNamespace)
        {
            File.WriteAllText(filename, SerializeToXmlString(xmlObject, encoding, removeNamespace));
        }

        internal static T DeserializeFromXmlString(string xml)
        {
            var reader = new StringReader(xml);
            var xmlObject = (T)xmlSerializer.Deserialize(reader);
            return xmlObject;
        }

        internal static T DeserializeFromXMLFile(string filename)
        {
            return DeserializeFromXmlString(File.ReadAllText(filename));
        }

    }

}
