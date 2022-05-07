namespace nspector.Common.Import;

[System.SerializableAttribute]
public class ProfileSetting
{
    [System.Xml.Serialization.XmlElementAttribute(ElementName="SettingID")]
    public uint SettingId;

    public string SettingNameInfo="";

    public string SettingValue="0";

    public SettingValueType ValueType=SettingValueType.Dword;
}