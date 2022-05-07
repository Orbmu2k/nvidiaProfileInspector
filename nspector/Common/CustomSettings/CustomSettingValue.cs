namespace nspector.Common.CustomSettings;

[System.SerializableAttribute]
public class CustomSettingValue
{
    internal uint SettingValue
    {
        get
        {
            return System.Convert.ToUInt32(this.HexValue.Trim(),16);
        }
    }

    public string UserfriendlyName
    {
        get;
        set;
    }

    public string HexValue
    {
        get;
        set;
    }
}