using System;
using System.Xml.Serialization;

namespace nspector.Common.Import;

[Serializable]
public class ProfileSetting
{

    [XmlElement(ElementName = "SettingID")]
    public uint SettingId;
    public string SettingNameInfo = "";

    public string SettingValue = "0";

    public SettingValueType ValueType = SettingValueType.Dword;
}
