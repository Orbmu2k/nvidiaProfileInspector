using nvidiaProfileInspector.Common.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace nvidiaProfileInspector.Common.CustomSettings
{
    public interface ICustomSettingNames
    {

    }

    [Serializable]
    public class CustomSettingNames
    {
        public List<CustomSetting> Settings = new List<CustomSetting>();

        public void StoreToFile(string filename)
        {
            XMLHelper<CustomSettingNames>.SerializeToXmlFile(this, filename, Encoding.Unicode, true);
        }

        public static CustomSettingNames FactoryLoadFromFile(string filename)
        {
            return XMLHelper<CustomSettingNames>.DeserializeFromXMLFile(filename);
        }

        public static CustomSettingNames FactoryLoadFromString(string xml)
        {
            return XMLHelper<CustomSettingNames>.DeserializeFromXmlString(xml);
        }
    }
}
