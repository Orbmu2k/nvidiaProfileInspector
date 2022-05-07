#region

using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector.Common.Meta;

class ConstantSettingMetaService:ISettingMetaService
{
    readonly string[] ignoreSettingNames=
    {
        "TOTAL_DWORD_SETTING_NUM","TOTAL_WSTRING_SETTING_NUM","TOTAL_SETTING_NUM","INVALID_SETTING_ID",
    };

    readonly System.Collections.Generic.Dictionary<nspector.Native.NvApi.DriverSettings.ESetting,System.Type>
        settingEnumTypeCache;

    System.Collections.Generic.HashSet<uint> settingIds;

    public ConstantSettingMetaService()
    {
        this.settingEnumTypeCache=this.CreateSettingEnumTypeCache();
        this.GetSettingIds();
    }

    public nspector.Native.NVAPI2.NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)=>null;

    public string GetSettingName(uint settingId)
    {
        if(this.settingIds.Contains(settingId))
        {
            return((nspector.Native.NvApi.DriverSettings.ESetting)settingId).ToString();
        }

        return null;
    }

    public uint? GetDwordDefaultValue(uint settingId)
    {
        if(this.settingEnumTypeCache.ContainsKey((nspector.Native.NvApi.DriverSettings.ESetting)settingId))
        {
            var enumType=this.settingEnumTypeCache[(nspector.Native.NvApi.DriverSettings.ESetting)settingId];

            var defaultName=Enumerable.FirstOrDefault(System.Enum.GetNames(enumType),x=>x.EndsWith("_DEFAULT"));
            if(defaultName!=null)
            {
                return(uint)System.Enum.Parse(enumType,defaultName);
            }
        }

        return null;
    }

    public string GetStringDefaultValue(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<string>> GetStringValues(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<uint>> GetDwordValues(uint settingId)
    {
        if(this.settingEnumTypeCache.ContainsKey((nspector.Native.NvApi.DriverSettings.ESetting)settingId))
        {
            var enumType=this.settingEnumTypeCache[(nspector.Native.NvApi.DriverSettings.ESetting)settingId];

            var validNames=Enumerable.ToList(Enumerable.Where(System.Enum.GetNames(enumType),x=>1==1
                    &&!x.EndsWith("_DEFAULT")
                    &&!x.EndsWith("_NUM_VALUES")
                //&& !x.EndsWith("_NUM")
                //&& !x.EndsWith("_MASK")
                //&& (!x.EndsWith("_MIN") || x.Equals("PREFERRED_PSTATE_PREFER_MIN"))
                //&& (!x.EndsWith("_MAX") || x.Equals("PREFERRED_PSTATE_PREFER_MAX"))
            ));

            return Enumerable.ToList(Enumerable.Select(validNames,x=>new SettingValue<uint>(this.Source)
            {
                Value    =this.ParseEnumValue(enumType,x),
                ValueName=DrsUtil.GetDwordString(this.ParseEnumValue(enumType,x))+" "+x,
            }));
        }

        return null;
    }

    public System.Collections.Generic.List<uint> GetSettingIds()
    {
        if(this.settingIds==null)
        {
            this.settingIds=new System.Collections.Generic.HashSet<uint>(
                Enumerable.ToList(Enumerable.Distinct(Enumerable.Cast<uint>(Enumerable.Where(
                    Enumerable.Cast<nspector.Native.NvApi.DriverSettings.ESetting>(
                        System.Enum.GetValues(typeof(nspector.Native.NvApi.DriverSettings.ESetting))),
                    x=>!Enumerable.Contains(this.ignoreSettingNames,x.ToString()))))));
        }

        return Enumerable.ToList(this.settingIds);
    }


    public string GetGroupName(uint settingId)=>null;

    public byte[] GetBinaryDefaultValue(uint settingId)=>null;

    public System.Collections.Generic.List<SettingValue<byte[]>> GetBinaryValues(uint settingId)=>null;

    public SettingMetaSource Source
    {
        get
        {
            return SettingMetaSource.ConstantSettings;
        }
    }

    System.Collections.Generic.Dictionary<nspector.Native.NvApi.DriverSettings.ESetting,System.Type>
        CreateSettingEnumTypeCache()
    {
        var result
            =new System.Collections.Generic.Dictionary<nspector.Native.NvApi.DriverSettings.ESetting,System.Type>();

        var drsEnumTypes=Enumerable.ToList(Enumerable.Where(
            System.Reflection.Assembly.GetExecutingAssembly().GetTypes(),t
                =>t.Namespace=="nspector.Native.NvApi.DriverSettings"
                &&t.IsEnum&&t.Name.StartsWith("EValues_")));

        var settingIdNames
            =Enumerable.ToList(
                Enumerable.Distinct(System.Enum.GetNames(typeof(nspector.Native.NvApi.DriverSettings.ESetting))));

        foreach(var settingIdName in settingIdNames)
        {
            if(Enumerable.Contains(this.ignoreSettingNames,settingIdName))
            {
                continue;
            }

            var enumType=Enumerable.FirstOrDefault(drsEnumTypes,x=>settingIdName
                .Substring(0,settingIdName.Length-3)
                .Equals(x.Name.Substring(8))
            );

            if(enumType!=null)
            {
                var settingIdVal
                    =(nspector.Native.NvApi.DriverSettings.ESetting)System.Enum.Parse(
                        typeof(nspector.Native.NvApi.DriverSettings.ESetting),settingIdName);
                result.Add(settingIdVal,enumType);
            }
        }

        return result;
    }

    public System.Type GetSettingEnumType(uint settingId)
    {
        if(this.settingEnumTypeCache.ContainsKey((nspector.Native.NvApi.DriverSettings.ESetting)settingId))
        {
            return this.settingEnumTypeCache[(nspector.Native.NvApi.DriverSettings.ESetting)settingId];
        }

        return null;
    }

    uint ParseEnumValue(System.Type enumType,string enumText)
    {
        try
        {
            return(uint)System.Enum.Parse(enumType,enumText);
        }
        catch(System.InvalidCastException)
        {
            var intValue=(int)System.Enum.Parse(enumType,enumText);
            var bytes   =System.BitConverter.GetBytes(intValue);
            return System.BitConverter.ToUInt32(bytes,0);
        }
    }
}