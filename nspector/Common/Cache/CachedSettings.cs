using nspector.Native.NVAPI2;
using System.Collections.Generic;
using System.Linq;

namespace nspector.Common
{
    internal class CachedSettings
    {
        internal CachedSettings() { }

        internal CachedSettings(uint settingId, NVDRS_SETTING_TYPE settingType)
        {
            SettingId = settingId;
            SettingType = settingType;
        }

        internal uint SettingId;

        internal List<CachedSettingValue> SettingValues = new List<CachedSettingValue>();

        internal uint ProfileCount = 0;

        internal NVDRS_SETTING_TYPE SettingType = NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;

        internal void AddDwordValue(uint valueDword, string Profile)
        {
            var setting = SettingValues.FirstOrDefault(s => s.Value == valueDword);
            if (setting == null)
            {
                SettingValues.Add(new CachedSettingValue(valueDword, Profile));
            }
            else
            {
                setting.ProfileNames.Append(", " + Profile);
                setting.ValueProfileCount++;
            }
            ProfileCount++;
        }

        internal void AddStringValue(string valueStr, string Profile)
        {

            var setting = SettingValues.FirstOrDefault(s => s.ValueStr == valueStr);
            if (setting == null)
            {
                SettingValues.Add(new CachedSettingValue(valueStr, Profile));
            }
            else
            {
                setting.ProfileNames.Append(", " + Profile);
                setting.ValueProfileCount++;
            }
            ProfileCount++;
        }

        internal void AddBinaryValue(byte[] valueBin, string Profile)
        {

            var setting = SettingValues.FirstOrDefault(s => s.ValueBin.SequenceEqual(valueBin));
            if (setting == null)
            {
                SettingValues.Add(new CachedSettingValue(valueBin, Profile));
            }
            else
            {
                setting.ProfileNames.Append(", " + Profile);
                setting.ValueProfileCount++;
            }
            ProfileCount++;
        }
    }
}