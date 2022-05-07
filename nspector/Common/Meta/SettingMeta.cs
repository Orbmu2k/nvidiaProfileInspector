namespace nspector.Common.Meta;

class SettingMeta
{
    public nspector.Native.NVAPI2.NVDRS_SETTING_TYPE? SettingType
    {
        get;
        set;
    }

    public string GroupName
    {
        get;
        set;
    }

    public string SettingName
    {
        get;
        set;
    }

    public string DefaultStringValue
    {
        get;
        set;
    }

    public uint DefaultDwordValue
    {
        get;
        set;
    }

    public byte[] DefaultBinaryValue
    {
        get;
        set;
    }

    public bool IsApiExposed
    {
        get;
        set;
    }

    public System.Collections.Generic.List<SettingValue<string>> StringValues
    {
        get;
        set;
    }

    public System.Collections.Generic.List<SettingValue<uint>> DwordValues
    {
        get;
        set;
    }

    public System.Collections.Generic.List<SettingValue<byte[]>> BinaryValues
    {
        get;
        set;
    }
}