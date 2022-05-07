namespace nspector.Common.CustomSettings;

[System.SerializableAttribute]
public class CustomSetting
{
    public string UserfriendlyName
    {
        get;
        set;
    }

    [System.Xml.Serialization.XmlElementAttribute(ElementName="HexSettingID")]
    public string HexSettingId
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public string GroupName
    {
        get;
        set;
    }

    public string OverrideDefault
    {
        get;
        set;
    }

    public float MinRequiredDriverVersion
    {
        get;
        set;
    }

    public System.Collections.Generic.List<CustomSettingValue> SettingValues
    {
        get;
        set;
    }

    internal uint SettingId
    {
        get
        {
            return System.Convert.ToUInt32(this.HexSettingId.Trim(),16);
        }
    }

    internal uint? DefaultValue
    {
        get
        {
            return string.IsNullOrEmpty(this.OverrideDefault)?null
                :System.Convert.ToUInt32(this.OverrideDefault.Trim(),16);
        }
    }
}