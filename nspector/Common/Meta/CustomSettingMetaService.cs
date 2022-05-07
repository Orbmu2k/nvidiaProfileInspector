#region

using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector.Common.Meta;

class CustomSettingMetaService:ISettingMetaService
{
    readonly nspector.Common.CustomSettings.CustomSettingNames customSettings;

    public CustomSettingMetaService(nspector.Common.CustomSettings.CustomSettingNames customSettings,
        SettingMetaSource sourceOverride=SettingMetaSource.CustomSettings)
    {
        this.customSettings=customSettings;
        this.Source        =sourceOverride;
    }

    public nspector.Native.NVAPI2.NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)=>null;

    public string GetSettingName(uint settingId)
    {
        var setting=Enumerable.FirstOrDefault(this.customSettings.Settings,x=>x.SettingId.Equals(settingId));

        if(setting!=null)
        {
            return setting.UserfriendlyName;
        }

        return null;
    }

    public uint? GetDwordDefaultValue(uint settingId)
    {
        var setting=Enumerable.FirstOrDefault(this.customSettings.Settings,x=>x.SettingId.Equals(settingId));

        if(setting!=null)
        {
            return setting.DefaultValue;
        }

        return null;
    }

    public string GetStringDefaultValue(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<string>> GetStringValues(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<uint>> GetDwordValues(uint settingId)
    {
        var setting=Enumerable.FirstOrDefault(this.customSettings.Settings,x=>x.SettingId.Equals(settingId));

        if(setting!=null)
        {
            var i=0;
            return Enumerable.ToList(Enumerable.Select(setting.SettingValues,x=>new SettingValue<uint>(this.Source)
            {
                ValuePos=i++,Value=x.SettingValue,ValueName=this.Source==SettingMetaSource.CustomSettings
                    ?x.UserfriendlyName
                    :DrsUtil.GetDwordString(x.SettingValue)+" "+x.UserfriendlyName,
            }));
        }

        return null;
    }

    public System.Collections.Generic.List<uint> GetSettingIds()
    {
        return Enumerable.ToList(Enumerable.Select(this.customSettings.Settings,x=>x.SettingId));
    }


    public string GetGroupName(uint settingId)
    {
        var setting=Enumerable.FirstOrDefault(this.customSettings.Settings,x=>x.SettingId.Equals(settingId));

        if(setting!=null&&!string.IsNullOrWhiteSpace(setting.GroupName))
        {
            return setting.GroupName;
        }

        return null;
    }

    public byte[] GetBinaryDefaultValue(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<byte[]>> GetBinaryValues(uint settingId)=>null;

    public SettingMetaSource Source
    {
        get;
    }
}