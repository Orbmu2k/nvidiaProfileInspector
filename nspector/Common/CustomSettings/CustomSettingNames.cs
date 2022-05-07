namespace nspector.Common.CustomSettings;

[System.SerializableAttribute]
public class CustomSettingNames
{
    public System.Collections.Generic.List<CustomSetting> Settings=new System.Collections.Generic.List<CustomSetting>();

    public void StoreToFile(string filename)
    {
        nspector.Common.Helper.XMLHelper<CustomSettingNames>.SerializeToXmlFile(this,filename,
            System.Text.Encoding.Unicode,true);
    }

    public static CustomSettingNames FactoryLoadFromFile(string filename)
        =>nspector.Common.Helper.XMLHelper<CustomSettingNames>.DeserializeFromXMLFile(filename);

    public static CustomSettingNames FactoryLoadFromString(string xml)
        =>nspector.Common.Helper.XMLHelper<CustomSettingNames>.DeserializeFromXmlString(xml);
}