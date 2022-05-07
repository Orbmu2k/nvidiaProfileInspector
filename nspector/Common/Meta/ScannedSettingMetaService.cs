#region

using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector.Common.Meta;

class ScannedSettingMetaService:ISettingMetaService
{
    readonly System.Collections.Generic.List<CachedSettings> CachedSettings;

    public ScannedSettingMetaService(System.Collections.Generic.List<CachedSettings> cachedSettings)
        =>this.CachedSettings=cachedSettings;

    public SettingMetaSource Source
    {
        get
        {
            return SettingMetaSource.ScannedSettings;
        }
    }

    public nspector.Native.NVAPI2.NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
    {
        var cached=Enumerable.FirstOrDefault(this.CachedSettings,x=>x.SettingId.Equals(settingId));
        if(cached!=null)
        {
            return cached.SettingType;
        }

        return null;
    }

    public string GetSettingName(uint settingId)
    {
        var cached=Enumerable.FirstOrDefault(this.CachedSettings,x=>x.SettingId.Equals(settingId));
        if(cached!=null)
        {
            return string.Format("0x{0:X8} ({1} Profiles)",settingId,cached.ProfileCount);
        }

        return null;
    }

    public string GetGroupName(uint settingId)=>null;

    public uint? GetDwordDefaultValue(uint settingId)=>null;

    public string GetStringDefaultValue(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<string>> GetStringValues(uint settingId)
    {
        var cached=Enumerable.FirstOrDefault(this.CachedSettings,x=>x.SettingId.Equals(settingId));
        if(cached!=null)
        {
            return Enumerable.ToList(Enumerable.Select(cached.SettingValues,s=>new SettingValue<string>(this.Source)
            {
                Value=s.ValueStr,ValueName=string.Format("'{0}' ({1})",s.ValueStr.Trim(),s.ProfileNames),
            }));
        }

        return null;
    }

    public System.Collections.Generic.List<SettingValue<uint>> GetDwordValues(uint settingId)
    {
        var cached=Enumerable.FirstOrDefault(this.CachedSettings,x=>x.SettingId.Equals(settingId));
        if(cached!=null)
        {
            return Enumerable.ToList(Enumerable.Select(cached.SettingValues,s=>new SettingValue<uint>(this.Source)
            {
                Value=s.Value,ValueName=string.Format("0x{0:X8} ({1})",s.Value,s.ProfileNames),
            }));
        }

        return null;
    }

    public System.Collections.Generic.List<uint> GetSettingIds()
    {
        return Enumerable.ToList(Enumerable.Select(this.CachedSettings,c=>c.SettingId));
    }

    public byte[] GetBinaryDefaultValue(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<byte[]>> GetBinaryValues(uint settingId)
    {
        var cached=Enumerable.FirstOrDefault(this.CachedSettings,x=>x.SettingId.Equals(settingId));
        if(cached!=null)
        {
            return Enumerable.ToList(Enumerable.Select(cached.SettingValues,s=>new SettingValue<byte[]>(this.Source)
            {
                Value    =s.ValueBin,
                ValueName=string.Format("{0} ({1})",DrsUtil.GetBinaryString(s.ValueBin),s.ProfileNames),
            }));
        }

        return null;
    }
}