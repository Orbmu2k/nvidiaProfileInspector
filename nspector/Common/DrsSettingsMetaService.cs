#region

using System.Linq;
using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector.Common;

class DrsSettingsMetaService
{
    readonly nspector.Common.CustomSettings.CustomSettingNames _customSettings;
    readonly nspector.Common.CustomSettings.CustomSettingNames _referenceSettings;

    nspector.Common.Meta.ISettingMetaService        ConstantMeta;
    nspector.Common.Meta.ISettingMetaService        CustomMeta;
    public nspector.Common.Meta.ISettingMetaService DriverMeta;

    System.Collections.Generic.List<nspector.Common.Meta.MetaServiceItem> MetaServices
        =new System.Collections.Generic.List<nspector.Common.Meta.MetaServiceItem>();

    nspector.Common.Meta.ISettingMetaService ReferenceMeta;
    nspector.Common.Meta.ISettingMetaService ScannedMeta;

    System.Collections.Generic.Dictionary<uint,nspector.Common.Meta.SettingMeta> settingMetaCache
        =new System.Collections.Generic.Dictionary<uint,nspector.Common.Meta.SettingMeta>();

    public DrsSettingsMetaService(nspector.Common.CustomSettings.CustomSettingNames customSettings,
        nspector.Common.CustomSettings.CustomSettingNames                           referenceSettings=null)
    {
        this._customSettings   =customSettings;
        this._referenceSettings=referenceSettings;

        this.ResetMetaCache(true);
    }

    public void ResetMetaCache(bool initOnly=false)
    {
        this.settingMetaCache=new System.Collections.Generic.Dictionary<uint,nspector.Common.Meta.SettingMeta>();
        this.MetaServices    =new System.Collections.Generic.List<nspector.Common.Meta.MetaServiceItem>();

        this.CustomMeta=new nspector.Common.Meta.CustomSettingMetaService(this._customSettings);
        this.MetaServices.Add(new nspector.Common.Meta.MetaServiceItem
        {
            ValueNamePrio=1,Service=this.CustomMeta,
        });

        if(!initOnly)
        {
            this.DriverMeta=new nspector.Common.Meta.DriverSettingMetaService();
            this.MetaServices.Add(new nspector.Common.Meta.MetaServiceItem
            {
                ValueNamePrio=5,Service=this.DriverMeta,
            });

            this.ConstantMeta=new nspector.Common.Meta.ConstantSettingMetaService();
            this.MetaServices.Add(new nspector.Common.Meta.MetaServiceItem
            {
                ValueNamePrio=2,Service=this.ConstantMeta,
            });

            if(DrsServiceLocator.ScannerService!=null)
            {
                this.ScannedMeta
                    =new nspector.Common.Meta.ScannedSettingMetaService(DrsServiceLocator.ScannerService
                        .CachedSettings);
                this.MetaServices.Add(new nspector.Common.Meta.MetaServiceItem
                {
                    ValueNamePrio=3,Service=this.ScannedMeta,
                });
            }

            if(this._referenceSettings!=null)
            {
                this.ReferenceMeta=new nspector.Common.Meta.CustomSettingMetaService(this._referenceSettings,
                    nspector.Common.Meta.SettingMetaSource.ReferenceSettings);
                this.MetaServices.Add(new nspector.Common.Meta.MetaServiceItem
                {
                    ValueNamePrio=4,Service=this.ReferenceMeta,
                });
            }
        }
    }

    nspector.Native.NVAPI2.NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
    {
        foreach(var service in Enumerable.OrderBy(this.MetaServices,x=>x.Service.Source))
        {
            var settingValueType=service.Service.GetSettingValueType(settingId);
            if(settingValueType!=null)
            {
                return settingValueType.Value;
            }
        }

        return nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;
    }

    string GetSettingName(uint settingId)
    {
        string hexCandidate=null;

        foreach(var service in Enumerable.OrderBy(this.MetaServices,x=>x.Service.Source))
        {
            var settingName=service.Service.GetSettingName(settingId);

            if(!string.IsNullOrEmpty(settingName))
            {
                if(settingName.StartsWith("0x"))
                {
                    hexCandidate=settingName;
                    continue;
                }

                return settingName;
            }
        }

        return hexCandidate;
    }

    string GetGroupName(uint settingId)
    {
        foreach(var service in Enumerable.OrderBy(this.MetaServices,x=>x.Service.Source))
        {
            var groupName=service.Service.GetGroupName(settingId);
            if(groupName!=null)
            {
                return groupName;
            }
        }

        return null;
    }

    uint GetDwordDefaultValue(uint settingId)
    {
        foreach(var service in Enumerable.OrderBy(this.MetaServices,x=>x.Service.Source))
        {
            var settingDefault=service.Service.GetDwordDefaultValue(settingId);
            if(settingDefault!=null)
            {
                return settingDefault.Value;
            }
        }

        return 0;
    }

    string GetStringDefaultValue(uint settingId)
    {
        foreach(var service in Enumerable.OrderBy(this.MetaServices,x=>x.Service.Source))
        {
            var settingDefault=service.Service.GetStringDefaultValue(settingId);
            if(settingDefault!=null)
            {
                return settingDefault;
            }
        }

        return null;
    }

    byte[] GetBinaryDefaultValue(uint settingId)
    {
        foreach(var service in Enumerable.OrderBy(this.MetaServices,x=>x.Service.Source))
        {
            var settingDefault=service.Service.GetBinaryDefaultValue(settingId);
            if(settingDefault!=null)
            {
                return settingDefault;
            }
        }

        return null;
    }

    System.Collections.Generic.List<nspector.Common.Meta.SettingValue<T>> MergeSettingValues<T>(
        System.Collections.Generic.List<nspector.Common.Meta.SettingValue<T>> a,
        System.Collections.Generic.List<nspector.Common.Meta.SettingValue<T>> b)
    {
        if(b==null)
        {
            return a;
        }

        // force scanned settings to add instead of merge
        var isScannedValuesList
            =b.Count>0&&Enumerable.First(b).ValueSource==nspector.Common.Meta.SettingMetaSource.ScannedSettings;
        if(isScannedValuesList)
        {
            a.AddRange(b);
        }
        else
        {
            var currentNonScannedValues=Enumerable.ToList(Enumerable.Select(
                Enumerable.Where(a,xa=>xa.ValueSource!=nspector.Common.Meta.SettingMetaSource.ScannedSettings),
                xa=>xa.Value));

            var newNonScannedValues
                =Enumerable.ToList(Enumerable.Where(b,xb=>!currentNonScannedValues.Contains(xb.Value)));
            a.AddRange(newNonScannedValues);

            foreach(var settingValue in a)
            {
                var bVal=Enumerable.FirstOrDefault(b,x=>
                    x.Value.Equals(settingValue.Value)&&
                    settingValue.ValueSource!=nspector.Common.Meta.SettingMetaSource.ScannedSettings);
                if(bVal!=null&&bVal.ValueName!=null)
                {
                    settingValue.ValueName  =bVal.ValueName;
                    settingValue.ValueSource=bVal.ValueSource;
                    settingValue.ValuePos   =bVal.ValuePos;
                }
            }
        }

        var atmp=Enumerable.FirstOrDefault(a);
        if(atmp!=null&&atmp is System.IComparable)
        {
            return Enumerable.ToList(Enumerable.OrderBy(a,x=>x.Value));
        }

        return Enumerable.ToList(a);
    }

    System.Collections.Generic.List<nspector.Common.Meta.SettingValue<byte[]>> GetBinaryValues(uint settingId)
    {
        var result=new System.Collections.Generic.List<nspector.Common.Meta.SettingValue<byte[]>>();

        foreach(var service in Enumerable.OrderByDescending(this.MetaServices,x=>x.ValueNamePrio))
        {
            result=this.MergeSettingValues(result,service.Service.GetBinaryValues(settingId));
        }

        return result;
    }

    System.Collections.Generic.List<nspector.Common.Meta.SettingValue<string>> GetStringValues(uint settingId)
    {
        var result=new System.Collections.Generic.List<nspector.Common.Meta.SettingValue<string>>();

        foreach(var service in Enumerable.OrderByDescending(this.MetaServices,x=>x.ValueNamePrio))
        {
            result=this.MergeSettingValues(result,service.Service.GetStringValues(settingId));
        }

        return result;
    }

    System.Collections.Generic.List<nspector.Common.Meta.SettingValue<uint>> GetDwordValues(uint settingId)
    {
        var result=new System.Collections.Generic.List<nspector.Common.Meta.SettingValue<uint>>();

        foreach(var service in Enumerable.OrderByDescending(this.MetaServices,x=>x.ValueNamePrio))
        {
            result=this.MergeSettingValues(result,service.Service.GetDwordValues(settingId));
        }

        if(result!=null)
        {
            result=(from v in Enumerable.Where(result,x=>1==1
                        &&!x.ValueName.EndsWith("_NUM")
                        &&!x.ValueName.EndsWith("_MASK")
                        &&(!x.ValueName.EndsWith("_MIN")||
                            x.ValueName.Equals("PREFERRED_PSTATE_PREFER_MIN"))
                        &&(!x.ValueName.EndsWith("_MAX")||
                            x.ValueName.Equals("PREFERRED_PSTATE_PREFER_MAX"))
                    )
                    group v by v.ValueName
                    into g
                    select g.First(t=>t.ValueName==g.Key))
                .OrderBy(v=>v.ValueSource)
                .ThenBy(v=>v.ValuePos)
                .ThenBy(v=>v.ValueName).ToList();
        }

        return result;
    }

    public System.Collections.Generic.List<uint> GetSettingIds(SettingViewMode viewMode)
    {
        var settingIds               =new System.Collections.Generic.List<uint>();
        var allowedSourcesForViewMode=this.GetAllowedSettingIdMetaSourcesForViewMode(viewMode);

        foreach(var service in this.MetaServices.OrderBy(x=>x.Service.Source))
        {
            if(allowedSourcesForViewMode.Contains(service.Service.Source))
            {
                settingIds.AddRange(service.Service.GetSettingIds());
            }
        }

        return settingIds.Distinct().ToList();
    }

    nspector.Common.Meta.SettingMetaSource[] GetAllowedSettingIdMetaSourcesForViewMode(SettingViewMode viewMode)
    {
        switch(viewMode)
        {
            case SettingViewMode.CustomSettingsOnly:
                return new[]
                {
                    nspector.Common.Meta.SettingMetaSource.CustomSettings,
                };
            case SettingViewMode.IncludeScannedSetttings:
                return new[]
                {
                    nspector.Common.Meta.SettingMetaSource.ConstantSettings,
                    nspector.Common.Meta.SettingMetaSource.ScannedSettings,
                    nspector.Common.Meta.SettingMetaSource.CustomSettings,
                    nspector.Common.Meta.SettingMetaSource.DriverSettings,
                    nspector.Common.Meta.SettingMetaSource.ReferenceSettings,
                };
            default:
                return new[]
                {
                    nspector.Common.Meta.SettingMetaSource.CustomSettings,
                    nspector.Common.Meta.SettingMetaSource.DriverSettings,
                };
        }
    }

    nspector.Common.Meta.SettingMetaSource[] GetAllowedSettingValueMetaSourcesForViewMode(SettingViewMode viewMode)
    {
        switch(viewMode)
        {
            case SettingViewMode.CustomSettingsOnly:
                return new[]
                {
                    nspector.Common.Meta.SettingMetaSource.CustomSettings,
                    nspector.Common.Meta.SettingMetaSource.ScannedSettings,
                };
            default:
                return new[]
                {
                    nspector.Common.Meta.SettingMetaSource.ConstantSettings,
                    nspector.Common.Meta.SettingMetaSource.ScannedSettings,
                    nspector.Common.Meta.SettingMetaSource.CustomSettings,
                    nspector.Common.Meta.SettingMetaSource.DriverSettings,
                    nspector.Common.Meta.SettingMetaSource.ReferenceSettings,
                };
        }
    }

    nspector.Common.Meta.SettingMeta CreateSettingMeta(uint settingId)
    {
        var settingType=this.GetSettingValueType(settingId);
        var settingName=this.GetSettingName(settingId);
        var groupName  =this.GetGroupName(settingId);

        if(groupName==null)
        {
            groupName=this.GetLegacyGroupName(settingId,settingName);
        }


        var result=new nspector.Common.Meta.SettingMeta
        {
            SettingType =settingType,SettingName=settingName,GroupName=groupName,
            IsApiExposed=this.GetIsApiExposed(settingId),DefaultDwordValue=
                settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE
                    ?this.GetDwordDefaultValue(settingId)
                    :0,
            DefaultStringValue=
                settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ?this.GetStringDefaultValue(settingId)
                    :null,
            DefaultBinaryValue=
                settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE
                    ?this.GetBinaryDefaultValue(settingId)
                    :null,
            DwordValues=
                settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE
                    ?this.GetDwordValues(settingId)
                    :null,
            StringValues=
                settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE
                    ?this.GetStringValues(settingId)
                    :null,
            BinaryValues=
                settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE
                    ?this.GetBinaryValues(settingId)
                    :null,
        };

        return result;
    }

    nspector.Common.Meta.SettingMeta PostProcessMeta(uint settingId,nspector.Common.Meta.SettingMeta settingMeta,
        SettingViewMode                                   viewMode)
    {
        var newMeta=new nspector.Common.Meta.SettingMeta
        {
            DefaultDwordValue=settingMeta.DefaultDwordValue,DefaultStringValue=settingMeta.DefaultStringValue,
            DefaultBinaryValue=settingMeta.DefaultBinaryValue,SettingName=settingMeta.SettingName,
            SettingType=settingMeta.SettingType,GroupName=settingMeta.GroupName,IsApiExposed=settingMeta.IsApiExposed,
        };

        if(string.IsNullOrEmpty(newMeta.SettingName))
        {
            newMeta.SettingName=string.Format("0x{0:X8}",settingId);
        }

        var allowedSourcesForViewMode=this.GetAllowedSettingValueMetaSourcesForViewMode(viewMode);
        if(settingMeta.DwordValues!=null)
        {
            newMeta.DwordValues=settingMeta.DwordValues
                .Where(x=>allowedSourcesForViewMode.Contains(x.ValueSource)).ToList();
        }

        if(settingMeta.StringValues!=null)
        {
            newMeta.StringValues=settingMeta.StringValues
                .Where(x=>allowedSourcesForViewMode.Contains(x.ValueSource)).ToList();
        }

        if(settingMeta.BinaryValues!=null)
        {
            newMeta.BinaryValues=settingMeta.BinaryValues
                .Where(x=>allowedSourcesForViewMode.Contains(x.ValueSource)).ToList();
        }

        return newMeta;
    }

    public nspector.Common.Meta.SettingMeta GetSettingMeta(uint settingId,
        SettingViewMode                                         viewMode=SettingViewMode.Normal)
    {
        if(this.settingMetaCache.ContainsKey(settingId))
        {
            return this.PostProcessMeta(settingId,this.settingMetaCache[settingId],viewMode);
        }

        var settingMeta=this.CreateSettingMeta(settingId);
        this.settingMetaCache.Add(settingId,settingMeta);
        return this.PostProcessMeta(settingId,settingMeta,viewMode);
    }

    string GetLegacyGroupName(uint settingId,string settingName)
    {
        if(settingName==null)
        {
            return null;
        }

        if(settingName.ToUpper().Contains("SLI"))
        {
            return"6 - SLI";
        }

        if(settingName.ToUpper().Contains("STEREO"))
        {
            return"7 - Stereo";
        }

        if(settingName.StartsWith("0x"))
        {
            return"Unknown";
        }

        return"Other";
    }

    bool GetIsApiExposed(uint settingId)
    {
        var driverMeta
            =this.MetaServices.FirstOrDefault(m
                =>m.Service.Source==nspector.Common.Meta.SettingMetaSource.DriverSettings);
        return driverMeta!=null&&driverMeta.Service.GetSettingIds().Contains(settingId);
    }
}