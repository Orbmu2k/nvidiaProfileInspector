using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using nspector.Common.Meta;
using nspector.Common.CustomSettings;
using nspector.Native.NVAPI2;
using nspector.Native.NvApi.DriverSettings;

namespace nspector.Common
{
    internal class DrsSettingsMetaService
    {
        
        private ISettingMetaService ConstantMeta;
        private ISettingMetaService CustomMeta;
        public ISettingMetaService DriverMeta;
        private ISettingMetaService ScannedMeta;
        private ISettingMetaService ReferenceMeta;
        private ISettingMetaService NvD3dUmxMeta;

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

                NvD3dUmxMeta = new NvD3dUmxSettingMetaService();
                MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 6, Service = NvD3dUmxMeta });
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
            foreach (var service in MetaServices.OrderBy(x=>x.Service.Source))
            {
                var settingName = service.Service.GetSettingName(settingId);
                if (settingName != null)
                    return settingName;
            }
            return null;
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

        private List<SettingValue<T>> MergeSettingValues<T>(List<SettingValue<T>> a, List<SettingValue<T>> b)
        {
            if (b == null)
                return a;

            // force scanned settings to add instead of merge
            if (b.Count > 0 && b.First().ValueSource == SettingMetaSource.ScannedSettings)
            {
                a.AddRange(b);
            }
            else
            {
                var newValues = b.Where(xb => !a.Select(xa => xa.Value).Contains(xb.Value)).ToList();
                a.AddRange(newValues);

                foreach (var settingValue in a)
                {
                    var bVal = b.FirstOrDefault(x => x.Value.Equals(settingValue.Value) && x.ValueSource != SettingMetaSource.ScannedSettings);
                    if (bVal != null && bVal.ValueName != null)
                    {
                        settingValue.ValueName = bVal.ValueName;
                        settingValue.ValueSource = bVal.ValueSource;
                        settingValue.ValuePos = bVal.ValuePos;
                    }
                }
            }

            return a.OrderBy(x=>x.Value).ToList();
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
                result = (from v in result.Where(x=> 1==1
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
                    return new [] { 
                        SettingMetaSource.CustomSettings 
                    };
                case SettingViewMode.IncludeScannedSetttings:
                    return new [] { 
                        SettingMetaSource.ConstantSettings,
                        SettingMetaSource.ScannedSettings,  
                        SettingMetaSource.CustomSettings,  
                        SettingMetaSource.DriverSettings,
                        SettingMetaSource.NvD3dUmxSettings,
                        SettingMetaSource.ReferenceSettings,
                    };
                default:
                    return new [] { 
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
                        SettingMetaSource.NvD3dUmxSettings,
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

                DefaultDwordValue = 
                    settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE 
                    ? GetDwordDefaultValue(settingId) : 0,

                DefaultStringValue = 
                    settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ? GetStringDefaultValue(settingId) : null,

                DwordValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE
                    ? GetDwordValues(settingId) : null,

                StringValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ? GetStringValues(settingId) : null,
            };

            return result;
        }

        private SettingMeta PostProcessMeta(uint settingId, SettingMeta settingMeta, SettingViewMode viewMode)
        {
            var newMeta = new SettingMeta()
            {
                DefaultDwordValue = settingMeta.DefaultDwordValue,
                DefaultStringValue = settingMeta.DefaultStringValue,
                SettingName = settingMeta.SettingName,
                SettingType = settingMeta.SettingType,
                GroupName = settingMeta.GroupName,
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
    }
}
