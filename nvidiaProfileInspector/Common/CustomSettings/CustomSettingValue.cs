using System;
using System.Xml.Serialization;

namespace nvidiaProfileInspector.Common.CustomSettings
{
    [Serializable]
    public class CustomSettingValue
    {
        public uint SettingValue
        {
            get { return Convert.ToUInt32(HexValue.Trim(), 16); }
        }

        public ulong QwordSettingValue
        {
            get { return Convert.ToUInt64(HexValue.Trim(), 16); }
        }

        public string UserfriendlyName { get; set; }

        public string HexValue { get; set; }

        [XmlIgnore]
        public string SearchTerms { get; set; }

    }
}
