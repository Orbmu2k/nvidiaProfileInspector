using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace nspector.Common.CustomSettings;

[Serializable]
public class CustomSetting
{
    public string UserfriendlyName { get; set; }

    [XmlElement(ElementName = "HexSettingID")]
    public string HexSettingId { get; set; }

    public string Description { get; set; }
    public string GroupName { get; set; }
    public string OverrideDefault { get; set; }
    public float MinRequiredDriverVersion { get; set; }

    public List<CustomSettingValue> SettingValues { get; set; }

    internal uint SettingId => Convert.ToUInt32(HexSettingId.Trim(), 16);

    internal uint? DefaultValue =>
        string.IsNullOrEmpty(OverrideDefault) ? null : Convert.ToUInt32(OverrideDefault.Trim(), 16);
}