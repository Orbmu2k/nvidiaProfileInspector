using nvidiaProfileInspector.Native.NVAPI2;
using System.Collections.Generic;

namespace nvidiaProfileInspector.Common.Meta
{
    public interface ISettingMetaService
    {
        SettingMetaSource Source { get; }

        NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId);

        string GetSettingName(uint settingId);

        string GetGroupName(uint settingId);

        string GetAlternateNames(uint settingId);

        uint? GetDwordDefaultValue(uint settingId);

        string GetStringDefaultValue(uint settingId);

        byte[] GetBinaryDefaultValue(uint settingId);

        List<SettingValue<string>> GetStringValues(uint settingId);

        List<SettingValue<uint>> GetDwordValues(uint settingId);

        List<SettingValue<byte[]>> GetBinaryValues(uint settingId);

        List<uint> GetSettingIds();
    }
}
