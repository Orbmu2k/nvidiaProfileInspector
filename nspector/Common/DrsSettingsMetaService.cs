using System;
using System.Collections.Generic;
using System.Linq;
using nspector.Common.Meta;
using nspector.Common.CustomSettings;
using nspector.Native.NVAPI2;

namespace nspector.Common
{
    internal class DrsSettingsMetaService
    {

        private ISettingMetaService ConstantMeta;
        private ISettingMetaService CustomMeta;
        public ISettingMetaService DriverMeta;
        private ISettingMetaService ScannedMeta;
        private ISettingMetaService ReferenceMeta;

        private readonly CustomSettingNames _customSettings;
        private readonly CustomSettingNames _referenceSettings;

        private List<MetaServiceItem> MetaServices = new List<MetaServiceItem>();

        private Dictionary<uint, SettingMeta> settingMetaCache = new Dictionary<uint, SettingMeta>();

        public DrsSettingsMetaService(CustomSettingNames customSettings, CustomSettingNames referenceSettings = null)
        {
            _customSettings = customSettings;
            _referenceSettings = referenceSettings;

            ResetMetaCache(true);
        }

        public void ResetMetaCache(bool initOnly = false)
        {
            settingMetaCache = new Dictionary<uint, SettingMeta>();
            MetaServices = new List<MetaServiceItem>();

            CustomMeta = new CustomSettingMetaService(_customSettings);
            MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 1, Service = CustomMeta });

            if (!initOnly)
            {
                DriverMeta = new DriverSettingMetaService();
                MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 5, Service = DriverMeta });

                ConstantMeta = new ConstantSettingMetaService();
                MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 2, Service = ConstantMeta });

                if (DrsServiceLocator.ScannerService != null)
                {
                    ScannedMeta = new ScannedSettingMetaService(DrsServiceLocator.ScannerService.CachedSettings);
                    MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 3, Service = ScannedMeta });
                }

                if (_referenceSettings != null)
                {
                    ReferenceMeta = new CustomSettingMetaService(_referenceSettings, SettingMetaSource.ReferenceSettings);
                    MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 4, Service = ReferenceMeta });
                }
            }

        }

        private NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
        {
            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var settingValueType = service.Service.GetSettingValueType(settingId);
                if (settingValueType != null)
                    return settingValueType.Value;
            }

            return NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;
        }

        private string GetSettingName(uint settingId)
        {
            string hexCandidate = null;

            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var settingName = service.Service.GetSettingName(settingId);

                if (!string.IsNullOrEmpty(settingName))
                {
                    if (settingName.StartsWith("0x"))
                    {
                        hexCandidate = settingName;
                        continue;
                    }
                    return settingName;
                }
            }
            return hexCandidate;
        }

        private string GetGroupName(uint settingId)
        {
            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var groupName = service.Service.GetGroupName(settingId);
                if (groupName != null)
                    return groupName;
            }
            return null;
        }

        private string GetAlternateNames(uint settingId)
        {
            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var altNames = service.Service.GetAlternateNames(settingId);
                if (altNames != null)
                    return altNames;
            }
            return null;
        }

        private uint GetDwordDefaultValue(uint settingId)
        {
            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var settingDefault = service.Service.GetDwordDefaultValue(settingId);
                if (settingDefault != null)
                    return settingDefault.Value;
            }

            return 0;
        }

        private string GetStringDefaultValue(uint settingId)
        {
            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var settingDefault = service.Service.GetStringDefaultValue(settingId);
                if (settingDefault != null)
                    return settingDefault;
            }

            return null;
        }

        private byte[] GetBinaryDefaultValue(uint settingId)
        {
            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var settingDefault = service.Service.GetBinaryDefaultValue(settingId);
                if (settingDefault != null)
                    return settingDefault;
            }

            return null;
        }

        private List<SettingValue<T>> MergeSettingValues<T>(List<SettingValue<T>> a, List<SettingValue<T>> b)
        {
            if (b == null)
                return a;

            // force scanned settings to add instead of merge
            var isScannedValuesList = (b.Count > 0 && b.First().ValueSource == SettingMetaSource.ScannedSettings);
            if (isScannedValuesList)
            {
                a.AddRange(b);
            }
            else
            {
                var currentNonScannedValues = a.Where(xa => xa.ValueSource != SettingMetaSource.ScannedSettings).Select(xa => xa.Value).ToList();

                var newNonScannedValues = b.Where(xb => !currentNonScannedValues.Contains(xb.Value)).ToList();
                a.AddRange(newNonScannedValues);

                foreach (var settingValue in a)
                {
                    var bVal = b.FirstOrDefault(x => x.Value.Equals(settingValue.Value) && settingValue.ValueSource != SettingMetaSource.ScannedSettings);
                    if (bVal != null && bVal.ValueName != null)
                    {
                        settingValue.ValueName = bVal.ValueName;
                        settingValue.ValueSource = bVal.ValueSource;
                        settingValue.ValuePos = bVal.ValuePos;
                    }
                }
            }

            var atmp = a.FirstOrDefault();
            if (atmp != null && atmp is IComparable)
                return a.OrderBy(x => x.Value).ToList();
            else
                return a.ToList();
        }

        private List<SettingValue<byte[]>> GetBinaryValues(uint settingId)
        {
            var result = new List<SettingValue<byte[]>>();

            foreach (var service in MetaServices.OrderByDescending(x => x.ValueNamePrio))
            {
                result = MergeSettingValues(result, service.Service.GetBinaryValues(settingId));
            }

            return result;
        }

        private List<SettingValue<string>> GetStringValues(uint settingId)
        {
            var result = new List<SettingValue<string>>();

            foreach (var service in MetaServices.OrderByDescending(x => x.ValueNamePrio))
            {
                result = MergeSettingValues(result, service.Service.GetStringValues(settingId));
            }

            return result;
        }

        private List<SettingValue<uint>> GetDwordValues(uint settingId)
        {
            var result = new List<SettingValue<uint>>();

            foreach (var service in MetaServices.OrderByDescending(x => x.ValueNamePrio))
            {
                result = MergeSettingValues(result, service.Service.GetDwordValues(settingId));
            }

            if (result != null)
            {
                result = (from v in result.Where(x => 1 == 1
                              && !x.ValueName.EndsWith("_NUM")
                              && !x.ValueName.EndsWith("_MASK")
                              && (!x.ValueName.EndsWith("_MIN") || x.ValueName.Equals("PREFERRED_PSTATE_PREFER_MIN"))
                              && (!x.ValueName.EndsWith("_MAX") || x.ValueName.Equals("PREFERRED_PSTATE_PREFER_MAX"))

                              )
                          group v by v.ValueName into g
                          select g.First(t => t.ValueName == g.Key))
                            .OrderBy(v => v.ValueSource)
                            .ThenBy(v => v.ValuePos)
                            .ThenBy(v => v.ValueName).ToList();

            }

            return result;
        }

        public List<uint> GetSettingIds(SettingViewMode viewMode)
        {
            var settingIds = new List<uint>();
            var allowedSourcesForViewMode = GetAllowedSettingIdMetaSourcesForViewMode(viewMode);

            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                if (allowedSourcesForViewMode.Contains(service.Service.Source))
                {
                    settingIds.AddRange(service.Service.GetSettingIds());
                }
            }
            return settingIds.Distinct().ToList();
        }

        private SettingMetaSource[] GetAllowedSettingIdMetaSourcesForViewMode(SettingViewMode viewMode)
        {
            switch (viewMode)
            {
                case SettingViewMode.CustomSettingsOnly:
                    return new[] {
                        SettingMetaSource.CustomSettings
                    };
                case SettingViewMode.IncludeScannedSetttings:
                    return new[] {
                        SettingMetaSource.ConstantSettings,
                        SettingMetaSource.ScannedSettings,
                        SettingMetaSource.CustomSettings,
                        SettingMetaSource.DriverSettings,
                        SettingMetaSource.ReferenceSettings,
                    };
                default:
                    return new[] {
                        SettingMetaSource.CustomSettings,
                        SettingMetaSource.DriverSettings,
                    };
            }
        }

        private SettingMetaSource[] GetAllowedSettingValueMetaSourcesForViewMode(SettingViewMode viewMode)
        {
            switch (viewMode)
            {
                case SettingViewMode.CustomSettingsOnly:
                    return new[] {
                        SettingMetaSource.CustomSettings,
                        SettingMetaSource.ScannedSettings,
                    };
                default:
                    return new[] {
                        SettingMetaSource.ConstantSettings,
                        SettingMetaSource.ScannedSettings,
                        SettingMetaSource.CustomSettings,
                        SettingMetaSource.DriverSettings,
                        SettingMetaSource.ReferenceSettings,

                    };
            }
        }

        private SettingMeta CreateSettingMeta(uint settingId)
        {
            var settingType = GetSettingValueType(settingId);
            var settingName = GetSettingName(settingId);
            var groupName = GetGroupName(settingId);

            if (groupName == null)
                groupName = GetLegacyGroupName(settingId, settingName);

            var result = new SettingMeta()
            {
                SettingType = settingType,
                SettingName = settingName,
                GroupName = groupName,
                AlternateNames = GetAlternateNames(settingId),

                IsApiExposed = GetIsApiExposed(settingId),
                IsSettingHidden = GetIsSettingHidden(settingId),
                Description = GetDescription(settingId),

                DefaultDwordValue =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE
                    ? GetDwordDefaultValue(settingId) : 0,

                DefaultStringValue =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ? GetStringDefaultValue(settingId) : null,

                DefaultBinaryValue =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE
                    ? GetBinaryDefaultValue(settingId) : null,

                DwordValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE
                    ? GetDwordValues(settingId) : null,

                StringValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ? GetStringValues(settingId) : null,

                BinaryValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE
                    ? GetBinaryValues(settingId) : null,
            };

            return result;
        }

        private SettingMeta PostProcessMeta(uint settingId, SettingMeta settingMeta, SettingViewMode viewMode)
        {
            var newMeta = new SettingMeta()
            {
                DefaultDwordValue = settingMeta.DefaultDwordValue,
                DefaultStringValue = settingMeta.DefaultStringValue,
                DefaultBinaryValue = settingMeta.DefaultBinaryValue,
                SettingName = settingMeta.SettingName,
                SettingType = settingMeta.SettingType,
                GroupName = settingMeta.GroupName,
                AlternateNames = settingMeta.AlternateNames,
                IsApiExposed = settingMeta.IsApiExposed,
                IsSettingHidden = settingMeta.IsSettingHidden,
                Description = settingMeta.Description,
            };

            if (string.IsNullOrEmpty(newMeta.SettingName))
            {
                newMeta.SettingName = string.Format("0x{0:X8}", settingId);
            }

            var allowedSourcesForViewMode = GetAllowedSettingValueMetaSourcesForViewMode(viewMode);
            if (settingMeta.DwordValues != null)
            {
                newMeta.DwordValues = settingMeta.DwordValues
                    .Where(x => allowedSourcesForViewMode.Contains(x.ValueSource)).ToList();
            }

            if (settingMeta.StringValues != null)
            {
                newMeta.StringValues = settingMeta.StringValues
                    .Where(x => allowedSourcesForViewMode.Contains(x.ValueSource)).ToList();
            }

            if (settingMeta.BinaryValues != null)
            {
                newMeta.BinaryValues = settingMeta.BinaryValues
                    .Where(x => allowedSourcesForViewMode.Contains(x.ValueSource)).ToList();
            }

            return newMeta;
        }

        public SettingMeta GetSettingMeta(uint settingId, SettingViewMode viewMode = SettingViewMode.Normal)
        {
            if (settingMetaCache.ContainsKey(settingId))
            {
                return PostProcessMeta(settingId, settingMetaCache[settingId], viewMode);
            }
            else
            {
                var settingMeta = CreateSettingMeta(settingId);
                settingMetaCache.Add(settingId, settingMeta);
                return PostProcessMeta(settingId, settingMeta, viewMode);
            }
        }

        private string GetLegacyGroupName(uint settingId, string settingName)
        {
            if (settingName == null)
                return null;

            if (settingName.ToUpper().Contains("SLI"))
                return "6 - SLI";
            else if (settingName.ToUpper().Contains("STEREO"))
                return "7 - Stereo";
            else if (settingName.StartsWith("0x"))
                return "Unknown";
            else
                return "Other";

        }
        private bool GetIsApiExposed(uint settingId)
        {
            var driverMeta = MetaServices.FirstOrDefault(m => m.Service.Source == SettingMetaSource.DriverSettings);
            return (driverMeta != null && driverMeta.Service.GetSettingIds().Contains(settingId));
        }

        private bool GetIsSettingHidden(uint settingId)
        {
            var csnMeta = MetaServices.FirstOrDefault(m => m.Service.Source == SettingMetaSource.CustomSettings);
            var refMeta = MetaServices.FirstOrDefault(m => m.Service.Source == SettingMetaSource.ReferenceSettings);

            return (csnMeta != null && ((CustomSettingMetaService)csnMeta.Service).IsSettingHidden(settingId)) || 
                refMeta != null && ((CustomSettingMetaService)refMeta.Service).IsSettingHidden(settingId);
        }

        private string GetDescription(uint settingId)
        {
            var csn = MetaServices.FirstOrDefault(m => m.Service.Source == SettingMetaSource.CustomSettings);
            var csnDescription = csn != null ? ((CustomSettingMetaService)csn.Service).GetDescription(settingId) ?? "" : "";
            
            var refs = MetaServices.FirstOrDefault(m => m.Service.Source == SettingMetaSource.ReferenceSettings);
            var refsDescription = refs != null ? ((CustomSettingMetaService)refs.Service).GetDescription(settingId) ?? "" : "";

            return !string.IsNullOrEmpty(csnDescription) ? csnDescription : refsDescription;
        }
    }
}
