using System.Collections.Generic;
using System.Text;
using nspector.Native.NVAPI2;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;

namespace nspector.Common.Meta
{

    internal class DriverSettingMetaService : ISettingMetaService
    {

        private readonly Dictionary<uint, SettingMeta> _settingMetaCache = new Dictionary<uint, SettingMeta>();
        private readonly List<uint> _settingIds;

        public DriverSettingMetaService()
        {
            _settingIds = InitSettingIds();
        }

        private List<uint> InitSettingIds()
        {
            var settingIds = new List<uint>();

            var nvRes = nvw.DRS_EnumAvailableSettingIds(out settingIds, 512);
            if (nvRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_EnumAvailableSettingIds", nvRes);

            return settingIds;
        }

        private SettingMeta GetDriverSettingMetaInternal(uint settingId)
        {
            // temporary fix for 571.96 overflow bug by emoose
            if ((settingId & 0xFFFFF000) == 0x10c7d000)
                return null;


            var values = new NVDRS_SETTING_VALUES();
            values.version = nvw.NVDRS_SETTING_VALUES_VER;
            uint valueCount = 255;

            var nvRes = nvw.DRS_EnumAvailableSettingValues(settingId, ref valueCount, ref values);

            if (nvRes == NvAPI_Status.NVAPI_SETTING_NOT_FOUND)
                return null;

            if (nvRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_EnumAvailableSettingValues", nvRes);


            var sbSettingName = new StringBuilder((int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX);
            nvRes = nvw.DRS_GetSettingNameFromId(settingId, sbSettingName);
            if (nvRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_GetSettingNameFromId", nvRes);

            var settingName = sbSettingName.ToString();
            if (string.IsNullOrWhiteSpace(settingName))
                settingName = DrsUtil.GetDwordString(settingId);

            var result = new SettingMeta
            {
                SettingType = values.settingType,
                SettingName = settingName,
            };


            if (values.settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
            {
                result.DefaultDwordValue = values.defaultValue.dwordValue;
                result.DwordValues = new List<SettingValue<uint>>();
                for (int i = 0; i < values.numSettingValues; i++)
                {
                    result.DwordValues.Add(
                        new SettingValue<uint>(Source)
                        {
                            Value = values.settingValues[i].dwordValue,
                            ValueName = DrsUtil.GetDwordString(values.settingValues[i].dwordValue),
                        });
                }
            }

            if (values.settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
            {
                result.DefaultStringValue = values.defaultValue.stringValue;
                result.StringValues = new List<SettingValue<string>>();
                for (int i = 0; i < values.numSettingValues; i++)
                {
                    var strValue = values.settingValues[i].stringValue;
                    if (strValue != null)
                    {
                        result.StringValues.Add(
                            new SettingValue<string>(Source)
                            {
                                Value = strValue,
                                ValueName = strValue,
                            });
                    }
                }
            }

            if (values.settingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
            {
                result.DefaultBinaryValue = values.defaultValue.binaryValue;
                result.BinaryValues = new List<SettingValue<byte[]>>();
                for (int i = 0; i < values.numSettingValues; i++)
                {
                    var binValue = values.settingValues[i].binaryValue;
                    if (binValue != null)
                    {
                        result.BinaryValues.Add(
                            new SettingValue<byte[]>(Source)
                            {
                                Value = binValue,
                                ValueName = DrsUtil.GetBinaryString(binValue),
                            });
                    }
                }
            }
            return result;

        }

        private SettingMeta GetSettingsMeta(uint settingId)
        {
            if (_settingMetaCache.ContainsKey(settingId))
                return _settingMetaCache[settingId];
            else
            {
                var settingMeta = GetDriverSettingMetaInternal(settingId);
                if (settingMeta != null)
                {
                    _settingMetaCache.Add(settingId, settingMeta);
                    return settingMeta;
                }

                return null;
            }
        }

        public string GetSettingName(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.SettingName;

            return null;
        }

        public uint? GetDwordDefaultValue(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.DefaultDwordValue;

            return null;
        }

        public string GetStringDefaultValue(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.DefaultStringValue;

            return null;
        }

        public List<SettingValue<string>> GetStringValues(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.StringValues;

            return null;
        }

        public List<SettingValue<uint>> GetDwordValues(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.DwordValues;

            return null;
        }

        public List<uint> GetSettingIds()
        {
            return _settingIds;
        }

        public NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.SettingType;

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

        public byte[] GetBinaryDefaultValue(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.DefaultBinaryValue;

            return null;
        }

        public List<SettingValue<byte[]>> GetBinaryValues(uint settingId)
        {
            var settingMeta = GetSettingsMeta(settingId);
            if (settingMeta != null)
                return settingMeta.BinaryValues;

            return null;
        }

        public SettingMetaSource Source
        {
            get { return SettingMetaSource.DriverSettings; }
        }
    }
}
