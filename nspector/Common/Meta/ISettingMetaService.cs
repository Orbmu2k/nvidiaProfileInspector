#region

using System.Collections.Generic;
using nspector.Native.NVAPI;

#endregion

namespace nspector.Common.Meta;

internal interface ISettingMetaService
{
    SettingMetaSource Source { get; }

    NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId);

    string GetSettingName(uint settingId);

    string GetGroupName(uint settingId);

    uint? GetDwordDefaultValue(uint settingId);

    string GetStringDefaultValue(uint settingId);

    byte[] GetBinaryDefaultValue(uint settingId);

    List<SettingValue<string>> GetStringValues(uint settingId);

    List<SettingValue<uint>> GetDwordValues(uint settingId);

    List<SettingValue<byte[]>> GetBinaryValues(uint settingId);

    List<uint> GetSettingIds();
}