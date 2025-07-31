using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace nspector.Common.CustomSettings
{
    [Serializable]
    public class CustomSetting
    {

        public string UserfriendlyName { get; set; }
        [XmlElement(ElementName = "HexSettingID")]
        public string HexSettingId { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public string AlternateNames { get; set; }
        public string OverrideDefault { get; set; }
        public float MinRequiredDriverVersion { get; set; }
        public float MaxRequiredDriverVersion { get; set; }
        public bool Hidden { get; set; }
        public bool HasConstraints { get; set; }
        public string DataType { get; set; }

        public List<CustomSettingValue> SettingValues { get; set; }

        internal uint SettingId
        {
            get { return Convert.ToUInt32(HexSettingId.Trim(), 16); }
        }

        internal uint? DefaultValue
        {
            get { return string.IsNullOrEmpty(OverrideDefault) ? null : (uint?)Convert.ToUInt32(OverrideDefault.Trim(), 16); }
        }

    }
}