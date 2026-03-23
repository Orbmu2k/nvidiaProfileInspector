using nvidiaProfileInspector.Native.NVAPI2;
using System.Collections.Generic;
using System.Linq;

namespace nvidiaProfileInspector.Common
{

    public class CachedSettings
    {
        public CachedSettings() { }

        public CachedSettings(uint settingId, NVDRS_SETTING_TYPE settingType)
        {
            SettingId = settingId;
            SettingType = settingType;
        }

        public uint SettingId;

        public List<CachedSettingValue> SettingValues = new List<CachedSettingValue>();

        public uint ProfileCount = 0;

        public NVDRS_SETTING_TYPE SettingType = NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;

        public void AddDwordValue(uint valueDword, string Profile)
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

        public void AddStringValue(string valueStr, string Profile)
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

        public void AddBinaryValue(byte[] valueBin, string Profile)
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