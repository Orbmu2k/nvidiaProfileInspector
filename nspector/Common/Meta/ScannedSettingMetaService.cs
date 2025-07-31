using nspector.Native.NVAPI2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nspector.Common.Meta
{
    internal class ScannedSettingMetaService : ISettingMetaService
    {
        private readonly List<CachedSettings> CachedSettings;

        public ScannedSettingMetaService(List<CachedSettings> cachedSettings)
        {
            CachedSettings = cachedSettings;
        }

        public SettingMetaSource Source
        {
            get { return SettingMetaSource.ScannedSettings; }
        }

        public NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
        {
            var cached = CachedSettings.FirstOrDefault(x => x.SettingId.Equals(settingId));
            if (cached != null)
                return cached.SettingType;

            return null;
        }

        public string GetSettingName(uint settingId)
        {
            var cached = CachedSettings.FirstOrDefault(x => x.SettingId.Equals(settingId));
            if (cached != null)
                return string.Format("0x{0:X8} ({1} Profiles)", settingId, cached.ProfileCount);

            return null;
        }

        public string GetGroupName(uint settingId)
        {
            return null;
        }

        public string GetAlternateNames(uint settingId)
        {
            return null;
        }

        public uint? GetDwordDefaultValue(uint settingId)
        {
            return null;
        }

        public string GetStringDefaultValue(uint settingId)
        {
            return null;
        }

        public List<SettingValue<string>> GetStringValues(uint settingId)
        {
            var cached = CachedSettings.FirstOrDefault(x => x.SettingId.Equals(settingId));
            if (cached != null)
                return cached.SettingValues.Select(s => new SettingValue<string>(Source)
                {
                    Value = s.ValueStr,
                    ValueName = string.Format("'{0}' ({1})", s.ValueStr.Trim(), s.ProfileNames),

                }).ToList();

            return null;
        }

        public List<SettingValue<uint>> GetDwordValues(uint settingId)
        {
            var cached = CachedSettings.FirstOrDefault(x => x.SettingId.Equals(settingId));
            if (cached != null)
                return cached.SettingValues.Select(s => new SettingValue<uint>(Source)
                {
                    Value = s.Value,
                    ValueName = string.Format("0x{0:X8} ({1})", s.Value, s.ProfileNames),

                }).ToList();

            return null;
        }

        public List<uint> GetSettingIds()
        {
            return CachedSettings.Select(c => c.SettingId).ToList();
        }

        public byte[] GetBinaryDefaultValue(uint settingId)
        {
            return null;
        }

        public List<SettingValue<byte[]>> GetBinaryValues(uint settingId)
        {
            var cached = CachedSettings.FirstOrDefault(x => x.SettingId.Equals(settingId));
            if (cached != null)
                return cached.SettingValues.Select(s => new SettingValue<byte[]>(Source)
                {
                    Value = s.ValueBin,
                    ValueName = string.Format("{0} ({1})", DrsUtil.GetBinaryString(s.ValueBin), s.ProfileNames),

                }).ToList();

            return null;
        }
    }
}
