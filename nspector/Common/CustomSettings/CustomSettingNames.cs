using System;
using System.Collections.Generic;
using System.Text;
using nspector.Common.Helper;

namespace nspector.Common.CustomSettings
{
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
