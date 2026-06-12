using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace nvidiaProfileInspector.Common.CustomSettings
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

        /// <summary>
        /// Setting value type used only when the driver itself can't report one (not exposed,
        /// no predefined values anywhere). Omitted for DWORD, which is the implicit default.
        /// </summary>
        public string FallbackType { get; set; }

        /// <summary>Legacy alias: older external CustomSettingNames.xml files use &lt;DataType&gt;.</summary>
        [XmlElement(ElementName = "DataType")]
        public string LegacyDataType
        {
            get { return null; }
            set
            {
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(FallbackType))
                    FallbackType = value;
            }
        }

        public List<CustomSettingValue> SettingValues { get; set; }

        public uint SettingId
        {
            get { return Convert.ToUInt32(HexSettingId.Trim(), 16); }
        }

        public uint? DefaultValue
        {
            get { return string.IsNullOrEmpty(OverrideDefault) ? null : (uint?)Convert.ToUInt32(OverrideDefault.Trim(), 16); }
        }

        public ulong? DefaultQwordValue
        {
            get { return string.IsNullOrEmpty(OverrideDefault) ? null : (ulong?)Convert.ToUInt64(OverrideDefault.Trim(), 16); }
        }

    }
}
