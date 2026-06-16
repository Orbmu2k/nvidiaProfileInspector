using nvidiaProfileInspector.Common.CustomSettings;
using nvidiaProfileInspector.Common.Meta;
using nvidiaProfileInspector.Native.NVAPI2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nvidiaProfileInspector.Common
{
    public class DrsSettingsMetaService
    {

        private ISettingMetaService ConstantMeta;
        public ISettingMetaService CustomMeta;
        public ISettingMetaService DriverMeta;
        private ISettingMetaService ScannedMeta;
        public ISettingMetaService ReferenceMeta;

        private readonly CustomSettingNames _customSettings;
        private readonly CustomSettingNames _referenceSettings;

        // True when a Reference.xml was loaded; lets the UI disable the reference sources.
        public bool HasReferenceSource => _referenceSettings != null;

        private List<MetaServiceItem> MetaServices = new List<MetaServiceItem>();

        private Dictionary<uint, SettingMeta> settingMetaCache = new Dictionary<uint, SettingMeta>();
        private Dictionary<uint, SettingMeta> postProcessedSettingMetaCache = new Dictionary<uint, SettingMeta>();

        // Source visibility filters driven by the UI. EnabledSettingSources decides which
        // sources contribute setting rows; EnabledValueSources decides which sources
        // contribute predefined values to each setting's dropdown. Defaults to everything
        // until the view model applies the persisted user selection via SetSourceFilters.
        public HashSet<SettingMetaSource> EnabledSettingSources = new HashSet<SettingMetaSource>
        {
            SettingMetaSource.CustomSettings,
            SettingMetaSource.DriverSettings,
            SettingMetaSource.ConstantSettings,
            SettingMetaSource.ReferenceSettings,
        };

        public HashSet<SettingMetaSource> EnabledValueSources = new HashSet<SettingMetaSource>
        {
            SettingMetaSource.CustomSettings,
            SettingMetaSource.DriverSettings,
            SettingMetaSource.ConstantSettings,
            SettingMetaSource.ReferenceSettings,
            SettingMetaSource.ScannedSettings,
        };

        // Whether same-value entries from different sources are merged into a single dropdown
        // entry (highest-priority name wins). When off, every source keeps its own entry.
        public bool MergeDistinctValues = true;

        // Whether the predefined scan (app list) values take part in the merge. When on, they
        // merge away into the matching common value; when off, they stay as their own distinct
        // app-list rows. Only relevant while MergeDistinctValues is on.
        public bool AddPredefinedAppListToCommon = true;

        // Whether a setting's name and description may still be pulled from sources that are
        // not currently enabled as setting sources.
        public bool AllowMetaFromInactiveSources = true;

        public void SetSourceFilters(
            IEnumerable<SettingMetaSource> settingSources,
            IEnumerable<SettingMetaSource> valueSources,
            bool mergeDistinctValues,
            bool addPredefinedAppListToCommon,
            bool allowMetaFromInactiveSources)
        {
            EnabledSettingSources = new HashSet<SettingMetaSource>(settingSources);
            EnabledValueSources = new HashSet<SettingMetaSource>(valueSources);
            MergeDistinctValues = mergeDistinctValues;
            AddPredefinedAppListToCommon = addPredefinedAppListToCommon;
            AllowMetaFromInactiveSources = allowMetaFromInactiveSources;

            // Value lists are cached per setting, so they must be rebuilt under the new filter.
            settingMetaCache = new Dictionary<uint, SettingMeta>();
            postProcessedSettingMetaCache = new Dictionary<uint, SettingMeta>();
        }

        public DrsSettingsMetaService(CustomSettingNames customSettings, CustomSettingNames referenceSettings = null)
        {
            _customSettings = customSettings;
            _referenceSettings = referenceSettings;

            ResetMetaCache(true);
        }

        public void ResetMetaCache(bool initOnly = false)
        {
            settingMetaCache = new Dictionary<uint, SettingMeta>();
            postProcessedSettingMetaCache = new Dictionary<uint, SettingMeta>();
            MetaServices = new List<MetaServiceItem>();

            CustomMeta = new CustomSettingMetaService(_customSettings);
            MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 1, TypePrio = 3, Service = CustomMeta });

            if (!initOnly)
            {
                DriverMeta = new DriverSettingMetaService();
                MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 5, TypePrio = 1, Service = DriverMeta });

                ConstantMeta = new ConstantSettingMetaService();
                MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 2, TypePrio = 5, Service = ConstantMeta });


                var scannerService = App.Bootstrapper.Resolve<DrsScannerService>();
                if (scannerService != null)
                {
                    ScannedMeta = new ScannedSettingMetaService(scannerService.CachedSettings);
                    MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 3, TypePrio = 2, Service = ScannedMeta });
                }

                if (_referenceSettings != null)
                {
                    ReferenceMeta = new CustomSettingMetaService(_referenceSettings, SettingMetaSource.ReferenceSettings);
                    MetaServices.Add(new MetaServiceItem() { ValueNamePrio = 4, TypePrio = 4, Service = ReferenceMeta });
                }
            }

        }

        private NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
        {
            // Driver first, then scanned observations, then the XML fallback types; the type
            // can change between driver generations (e.g. rBAR Size Limit BINARY -> QWORD),
            // so static sources must never override what the current driver reports.
            foreach (var service in MetaServices.OrderBy(x => x.TypePrio))
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
                if (!AllowMetaFromInactiveSources && !EnabledSettingSources.Contains(service.Service.Source))
                    continue;

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

        private ulong GetQwordDefaultValue(uint settingId)
        {
            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                var settingDefault = service.Service.GetQwordDefaultValue(settingId);
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

        private uint GetValueNamePrio(SettingMetaSource source)
        {
            foreach (var item in MetaServices)
            {
                if (item.Service.Source == source)
                    return item.ValueNamePrio;
            }
            return uint.MaxValue;
        }

        // The predefined scan names look like "0x12345678 (ProfileA, ProfileB)". This pulls
        // out the trailing "(ProfileA, ProfileB)" app list so it can be appended elsewhere.
        private static string ExtractAppList(string scannedValueName)
        {
            if (string.IsNullOrEmpty(scannedValueName))
                return null;

            int open = scannedValueName.IndexOf('(');
            int close = scannedValueName.LastIndexOf(')');
            if (open >= 0 && close > open)
                return scannedValueName.Substring(open, close - open + 1);

            return null;
        }

        // Applies the user's merge options and the final ordering to an already collected and
        // name-filtered list of values. valueKey returns the numeric value (or a stable key
        // for binary) used to detect "same value" across sources.
        private List<SettingValue<T>> AggregateValues<T>(List<SettingValue<T>> collected, Func<SettingValue<T>, object> valueKey)
        {
            IEnumerable<SettingValue<T>> values;

            if (MergeDistinctValues && AddPredefinedAppListToCommon)
            {
                // Merge everything by value. A non-scanned name wins. The predefined scan app
                // list is appended only to common (CSN) values; if it can't be appended it is
                // kept as its own standalone entry instead of being merged away.
                values = collected.GroupBy(valueKey).SelectMany(g =>
                {
                    var group = g.ToList();
                    var nonScanned = group.Where(x => x.ValueSource != SettingMetaSource.ScannedSettings).ToList();
                    var scanned = group.FirstOrDefault(x => x.ValueSource == SettingMetaSource.ScannedSettings);

                    // value only seen in the scan -> keep the scan entry as-is
                    if (nonScanned.Count == 0)
                        return new[] { group.OrderBy(x => GetValueNamePrio(x.ValueSource)).First() };

                    var winner = nonScanned.OrderBy(x => GetValueNamePrio(x.ValueSource)).First();
                    var appList = scanned == null ? null : ExtractAppList(scanned.ValueName);

                    // append the app list only to a common (CSN) winner (new value so cached
                    // source entries are never mutated)
                    if (!string.IsNullOrEmpty(appList) && winner.ValueSource == SettingMetaSource.CustomSettings)
                    {
                        return new[]
                        {
                            new SettingValue<T>(winner.ValueSource)
                            {
                                Value = winner.Value,
                                ValuePos = winner.ValuePos,
                                ValueName = (winner.ValueName ?? string.Empty) + " - " + appList,
                            }
                        };
                    }

                    // could not append -> keep the winner and, if present, the scan entry as
                    // its own standalone row so it is never lost
                    return scanned != null ? new[] { winner, scanned } : new[] { winner };
                });
            }
            else if (MergeDistinctValues)
            {
                // Merge the common sources by value, but keep the predefined scan (app list)
                // entries as their own distinct rows.
                var scanned = collected.Where(x => x.ValueSource == SettingMetaSource.ScannedSettings);
                var mergedCommon = collected
                    .Where(x => x.ValueSource != SettingMetaSource.ScannedSettings)
                    .GroupBy(valueKey)
                    .Select(g => g.OrderBy(x => GetValueNamePrio(x.ValueSource)).First());
                values = mergedCommon.Concat(scanned);
            }
            else
            {
                // No merge: keep every source's entry, drop only exact (value + name) dupes.
                values = collected
                    .GroupBy(x => new { Key = valueKey(x), x.ValueName })
                    .Select(g => g.OrderBy(x => x.ValueSource).First());
            }

            return values
                .OrderBy(v => v.ValueSource)
                .ThenBy(v => v.ValuePos)
                .ThenBy(v => v.ValueName)
                .ToList();
        }

        private List<SettingValue<byte[]>> GetBinaryValues(uint settingId)
        {
            var collected = new List<SettingValue<byte[]>>();

            foreach (var service in MetaServices.OrderByDescending(x => x.ValueNamePrio))
            {
                if (!EnabledValueSources.Contains(service.Service.Source))
                    continue;
                var values = service.Service.GetBinaryValues(settingId);
                if (values != null)
                    collected.AddRange(values);
            }

            return AggregateValues(collected, x => x.Value == null ? string.Empty : BitConverter.ToString(x.Value));
        }

        private List<SettingValue<string>> GetStringValues(uint settingId)
        {
            var collected = new List<SettingValue<string>>();

            foreach (var service in MetaServices.OrderByDescending(x => x.ValueNamePrio))
            {
                if (!EnabledValueSources.Contains(service.Service.Source))
                    continue;
                var values = service.Service.GetStringValues(settingId);
                if (values != null)
                    collected.AddRange(values);
            }

            return AggregateValues(collected, x => (object)x.Value);
        }

        private List<SettingValue<uint>> GetDwordValues(uint settingId)
        {
            var collected = new List<SettingValue<uint>>();

            foreach (var service in MetaServices.OrderByDescending(x => x.ValueNamePrio))
            {
                if (!EnabledValueSources.Contains(service.Service.Source))
                    continue;
                var values = service.Service.GetDwordValues(settingId);
                if (values != null)
                    collected.AddRange(values);
            }

            collected = collected.Where(x => 1 == 1
                    && !x.ValueName.EndsWith("_NUM")
                    && !x.ValueName.EndsWith("_MASK")
                    && (!x.ValueName.EndsWith("_MIN") || x.ValueName.Equals("PREFERRED_PSTATE_PREFER_MIN"))
                    && (!x.ValueName.EndsWith("_MAX") || x.ValueName.Equals("PREFERRED_PSTATE_PREFER_MAX")))
                .ToList();

            return AggregateValues(collected, x => (object)x.Value);
        }

        private List<SettingValue<ulong>> GetQwordValues(uint settingId)
        {
            var collected = new List<SettingValue<ulong>>();

            foreach (var service in MetaServices.OrderByDescending(x => x.ValueNamePrio))
            {
                if (!EnabledValueSources.Contains(service.Service.Source))
                    continue;
                var values = service.Service.GetQwordValues(settingId);
                if (values != null)
                    collected.AddRange(values);
            }

            collected = collected.Where(x => 1 == 1
                    && !x.ValueName.EndsWith("_NUM")
                    && !x.ValueName.EndsWith("_MASK")
                    && !x.ValueName.EndsWith("_MIN")
                    && !x.ValueName.EndsWith("_MAX"))
                .ToList();

            return AggregateValues(collected, x => (object)x.Value);
        }

        public List<uint> GetSettingIds()
        {
            var settingIds = new List<uint>();

            foreach (var service in MetaServices.OrderBy(x => x.Service.Source))
            {
                if (EnabledSettingSources.Contains(service.Service.Source))
                {
                    settingIds.AddRange(service.Service.GetSettingIds());
                }
            }
            return settingIds.Distinct().ToList();
        }

        private SettingMeta CreateSettingMeta(uint settingId)
        {
            var settingType = GetSettingValueType(settingId);
            var settingName = GetSettingName(settingId);
            var groupName = GetGroupName(settingId);

            if (groupName == null)
                groupName = GetFallbackGroupName(settingId, settingName);

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

                DefaultQwordValue =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_QWORD_TYPE
                    ? GetQwordDefaultValue(settingId) : 0,

                DefaultStringValue =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ? GetStringDefaultValue(settingId) : null,

                DefaultBinaryValue =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE
                    ? GetBinaryDefaultValue(settingId) : null,

                DwordValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE
                    ? GetDwordValues(settingId) : null,

                QwordValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_QWORD_TYPE
                    ? GetQwordValues(settingId) : null,

                StringValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ? GetStringValues(settingId) : null,

                BinaryValues =
                    settingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE
                    ? GetBinaryValues(settingId) : null,
            };

            return result;
        }

        private SettingMeta PostProcessMeta(uint settingId, SettingMeta settingMeta)
        {
            if (postProcessedSettingMetaCache.TryGetValue(settingId, out var cachedMeta))
                return cachedMeta;

            var newMeta = new SettingMeta()
            {
                DefaultDwordValue = settingMeta.DefaultDwordValue,
                DefaultQwordValue = settingMeta.DefaultQwordValue,
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

            // Always expose every available value source. The view mode only decides which
            // settings get listed, never which predefined values are selectable for a setting.
            newMeta.DwordValues = settingMeta.DwordValues?.ToList();
            newMeta.QwordValues = settingMeta.QwordValues?.ToList();
            newMeta.StringValues = settingMeta.StringValues?.ToList();
            newMeta.BinaryValues = settingMeta.BinaryValues?.ToList();

            postProcessedSettingMetaCache.Add(settingId, newMeta);
            return newMeta;
        }

        public SettingMeta GetSettingMeta(uint settingId)
        {
            if (settingMetaCache.ContainsKey(settingId))
            {
                return PostProcessMeta(settingId, settingMetaCache[settingId]);
            }
            else
            {
                var settingMeta = CreateSettingMeta(settingId);
                settingMetaCache.Add(settingId, settingMeta);
                return PostProcessMeta(settingId, settingMeta);
            }
        }

        private string GetFallbackGroupName(uint settingId, string settingName)
        {
            if ((settingId & 0x70000000) == 0x70000000)
                return "09 - Stereo";

            if ((settingId & 0x20000000) == 0x20000000)
                return "10 - OpenGL";

            if (settingName == null)
                return "11 - Unknown Driver Settings";

            var normalizedName = settingName.ToUpperInvariant();

            if (normalizedName.Contains("STEREO"))
                return "09 - Stereo";

            if (normalizedName.Contains("OGL") || normalizedName.Contains("OPENGL"))
                return "10 - OpenGL";

            return "11 - Unknown Driver Settings";
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

        public string GetDescription(uint settingId)
        {
            var csnDescription = "";
            if (AllowMetaFromInactiveSources || EnabledSettingSources.Contains(SettingMetaSource.CustomSettings))
            {
                var csn = MetaServices.FirstOrDefault(m => m.Service.Source == SettingMetaSource.CustomSettings);
                csnDescription = csn != null ? ((CustomSettingMetaService)csn.Service).GetDescription(settingId) ?? "" : "";
            }

            var refsDescription = "";
            if (AllowMetaFromInactiveSources || EnabledSettingSources.Contains(SettingMetaSource.ReferenceSettings))
            {
                var refs = MetaServices.FirstOrDefault(m => m.Service.Source == SettingMetaSource.ReferenceSettings);
                refsDescription = refs != null ? ((CustomSettingMetaService)refs.Service).GetDescription(settingId) ?? "" : "";
            }

            return !string.IsNullOrEmpty(csnDescription) ? csnDescription : refsDescription;
        }
    }
}
