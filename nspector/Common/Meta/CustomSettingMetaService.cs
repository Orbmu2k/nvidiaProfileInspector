using nspector.Common.CustomSettings;
using nspector.Native.NVAPI2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nspector.Common.Meta
{
    internal class CustomSettingMetaService : ISettingMetaService
    {

        private readonly CustomSettingNames customSettings;
        private readonly SettingMetaSource _source;

        public CustomSettingMetaService(CustomSettingNames customSettings, SettingMetaSource sourceOverride = SettingMetaSource.CustomSettings)
        {
            this.customSettings = customSettings;
            _source = sourceOverride;
        }

        public NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
        {
            var setting = customSettings.Settings
                .FirstOrDefault(x => x.SettingId.Equals(settingId));

            return MapType(setting?.DataType);
        }

        private NVDRS_SETTING_TYPE? MapType(string type)
        {
            if (string.IsNullOrEmpty(type)) return null;

            switch(type.ToLowerInvariant())
            {
                case "dword":
                    return NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;
                case "string":
                    return NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE;
                case "binary":
                    return NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE;
                default: throw new ArgumentOutOfRangeException(type);
            }
        }

        public string GetSettingName(uint settingId)
        {
            var setting = customSettings.Settings
                .FirstOrDefault(x => x.SettingId.Equals(settingId));

            if (setting != null)
                return setting.UserfriendlyName;

            return null;
        }

        public uint? GetDwordDefaultValue(uint settingId)
        {
            var setting = customSettings.Settings
                .FirstOrDefault(x => x.SettingId.Equals(settingId));

            if (setting != null)
                return setting.DefaultValue;

            return null;
        }

        public string GetStringDefaultValue(uint settingId)
        {
            return null;
        }

        public List<SettingValue<string>> GetStringValues(uint settingId)
        {
            return null;
        }

        public List<SettingValue<uint>> GetDwordValues(uint settingId)
        {
            var setting = customSettings.Settings
               .FirstOrDefault(x => x.SettingId.Equals(settingId));

            if (setting != null)
            {
                var i = 0;
                return setting.SettingValues.Select(x => new SettingValue<uint>(Source)
                {
                    ValuePos = i++,
                    Value = x.SettingValue,
                    ValueName = _source == SettingMetaSource.CustomSettings ? x.UserfriendlyName : DrsUtil.GetDwordString(x.SettingValue) + " " + x.UserfriendlyName,
                }).ToList();
            }

            return null;
        }

        public List<uint> GetSettingIds()
        {
            return customSettings.Settings
                 .Select(x => x.SettingId).ToList();
        }


        public string GetGroupName(uint settingId)
        {
            var setting = customSettings.Settings
               .FirstOrDefault(x => x.SettingId.Equals(settingId));

            if (setting != null && !string.IsNullOrWhiteSpace(setting.GroupName))
                return setting.GroupName;

            return null;
        }

        public byte[] GetBinaryDefaultValue(uint settingId)
        {
            return null;
        }

        public List<SettingValue<byte[]>> GetBinaryValues(uint settingId)
        {
            return null;
        }

        public bool IsSettingHidden(uint settingId)
        {
            var setting = customSettings.Settings
               .FirstOrDefault(x => x.SettingId.Equals(settingId));

            return setting?.Hidden ?? false;
        }

        public string GetDescription(uint settingId)
        {
            var setting = customSettings.Settings
               .FirstOrDefault(x => x.SettingId.Equals(settingId));

            return setting?.Description;
        }

        public SettingMetaSource Source
        {
            get { return _source; }
        }
    }
}
