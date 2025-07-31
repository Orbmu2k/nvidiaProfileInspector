using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nspector.Native.NvApi.DriverSettings;
using nspector.Native.NVAPI2;

namespace nspector.Common.Meta
{
    internal class ConstantSettingMetaService : ISettingMetaService
    {

        public ConstantSettingMetaService()
        {
            settingEnumTypeCache = CreateSettingEnumTypeCache();
            GetSettingIds();
        }

        private readonly Dictionary<ESetting, Type> settingEnumTypeCache;

        private string[] ignoreSettingNames = new[] { "TOTAL_DWORD_SETTING_NUM", "TOTAL_WSTRING_SETTING_NUM",
                                                      "TOTAL_SETTING_NUM", "INVALID_SETTING_ID" };

        private Dictionary<ESetting, Type> CreateSettingEnumTypeCache()
        {
            var result = new Dictionary<ESetting, Type>();

            var drsEnumTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "nspector.Native.NvApi.DriverSettings"
                    && t.IsEnum && t.Name.StartsWith("EValues_")).ToList();

            var settingIdNames = Enum.GetNames(typeof(ESetting)).Distinct().ToList();

            foreach (var settingIdName in settingIdNames)
            {
                if (ignoreSettingNames.Contains(settingIdName))
                    continue;

                var enumType = drsEnumTypes
                    .FirstOrDefault(x => settingIdName
                        .Substring(0, settingIdName.Length - 3)
                        .Equals(x.Name.Substring(8))
                        );

                if (enumType != null)
                {
                    var settingIdVal = (ESetting)Enum.Parse(typeof(ESetting), settingIdName);
                    result.Add(settingIdVal, enumType);
                }
            }

            return result;
        }

        public Type GetSettingEnumType(uint settingId)
        {
            if (settingEnumTypeCache.ContainsKey((ESetting)settingId))
                return settingEnumTypeCache[(ESetting)settingId];

            return null;
        }

        public NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
        {
            return null;
        }

        public string GetSettingName(uint settingId)
        {
            if (settingIds.Contains(settingId))
                return ((ESetting)settingId).ToString();

            return null;
        }

        public uint? GetDwordDefaultValue(uint settingId)
        {
            if (settingEnumTypeCache.ContainsKey((ESetting)settingId))
            {
                var enumType = settingEnumTypeCache[(ESetting)settingId];

                var defaultName = Enum.GetNames(enumType).FirstOrDefault(x => x.EndsWith("_DEFAULT"));
                if (defaultName != null)
                    return (uint)Enum.Parse(enumType, defaultName);
            }
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

        private uint ParseEnumValue(Type enumType, string enumText)
        {
            try
            {
                return (uint)Enum.Parse(enumType, enumText);
            }
            catch (InvalidCastException)
            {
                var intValue = (int)Enum.Parse(enumType, enumText);
                var bytes = BitConverter.GetBytes(intValue);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        public List<SettingValue<uint>> GetDwordValues(uint settingId)
        {
            if (settingEnumTypeCache.ContainsKey((ESetting)settingId))
            {
                var enumType = settingEnumTypeCache[(ESetting)settingId];

                var validNames = Enum.GetNames(enumType)
                    .Where(x => 1 == 1
                        && !x.EndsWith("_DEFAULT")
                        && !x.EndsWith("_NUM_VALUES")
                    //&& !x.EndsWith("_NUM")
                    //&& !x.EndsWith("_MASK")
                    //&& (!x.EndsWith("_MIN") || x.Equals("PREFERRED_PSTATE_PREFER_MIN"))
                    //&& (!x.EndsWith("_MAX") || x.Equals("PREFERRED_PSTATE_PREFER_MAX"))
                        ).ToList();

                return validNames.Select(x => new SettingValue<uint>(Source)
                {
                    Value = ParseEnumValue(enumType, x),
                    ValueName = DrsUtil.GetDwordString(ParseEnumValue(enumType, x)) + " " + x
                }).ToList();

            }
            return null;
        }

        private HashSet<uint> settingIds;
        public List<uint> GetSettingIds()
        {
            if (settingIds == null)
                settingIds = new HashSet<uint>(
                    Enum.GetValues(typeof(ESetting))
                    .Cast<ESetting>()
                    .Where(x => !ignoreSettingNames.Contains(x.ToString()))
                    .Cast<uint>()
                    .Distinct()
                    .ToList());

            return settingIds.ToList();
        }


        public string GetGroupName(uint settingId)
        {
            return null;
        }

        public string GetAlternateNames(uint settingId)
        {
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

        public SettingMetaSource Source
        {
            get { return SettingMetaSource.ConstantSettings; }
        }
    }
}
