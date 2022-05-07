namespace nspector.Common.Helper;

static class XMLHelper<T> where T:new()
{
    static readonly System.Xml.Serialization.XmlSerializer xmlSerializer;

    static XMLHelper()=>XMLHelper<T>.xmlSerializer=new System.Xml.Serialization.XmlSerializer(typeof(T));

    internal static string SerializeToXmlString(T xmlObject,System.Text.Encoding encoding,bool removeNamespace)
    {
        var memoryStream=new System.IO.MemoryStream();
        var xmlWriter=new System.Xml.XmlTextWriter(memoryStream,encoding)
        {
            Formatting=System.Xml.Formatting.Indented,
        };

        if(removeNamespace)
        {
            var xs=new System.Xml.Serialization.XmlSerializerNamespaces();
            xs.Add("","");
            XMLHelper<T>.xmlSerializer.Serialize(xmlWriter,xmlObject,xs);
        }
        else
        {
            XMLHelper<T>.xmlSerializer.Serialize(xmlWriter,xmlObject);
        }

        return encoding.GetString(memoryStream.ToArray());
    }

    internal static void SerializeToXmlFile(T xmlObject,string filename,System.Text.Encoding encoding,
        bool                                  removeNamespace)
    {
        System.IO.File.WriteAllText(filename,XMLHelper<T>.SerializeToXmlString(xmlObject,encoding,removeNamespace));
    }

    internal static T DeserializeFromXmlString(string xml)
    {
        var reader   =new System.IO.StringReader(xml);
        var xmlObject=(T)XMLHelper<T>.xmlSerializer.Deserialize(reader);
        return xmlObject;
    }

    internal static T DeserializeFromXMLFile(string filename)
        =>XMLHelper<T>.DeserializeFromXmlString(System.IO.File.ReadAllText(filename));
}