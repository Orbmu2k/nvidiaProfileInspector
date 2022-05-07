namespace nspector.Common.Meta;

interface ISettingMetaService
{
    SettingMetaSource Source
    {
        get;
    }

    nspector.Native.NVAPI2.NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId);

    string GetSettingName(uint settingId);

    string GetGroupName(uint settingId);

    uint? GetDwordDefaultValue(uint settingId);

    string GetStringDefaultValue(uint settingId);

    byte[] GetBinaryDefaultValue(uint settingId);

    System.Collections.Generic.List<SettingValue<string>> GetStringValues(uint settingId);

    System.Collections.Generic.List<SettingValue<uint>> GetDwordValues(uint settingId);

    System.Collections.Generic.List<SettingValue<byte[]>> GetBinaryValues(uint settingId);

    System.Collections.Generic.List<uint> GetSettingIds();
}