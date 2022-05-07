#region

using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector.Common;

class CachedSettings
{
    internal uint ProfileCount;

    internal uint SettingId;

    internal nspector.Native.NVAPI2.NVDRS_SETTING_TYPE SettingType
        =nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;

    internal System.Collections.Generic.List<CachedSettingValue> SettingValues
        =new System.Collections.Generic.List<CachedSettingValue>();

    internal CachedSettings() {}

    internal CachedSettings(uint settingId,nspector.Native.NVAPI2.NVDRS_SETTING_TYPE settingType)
    {
        this.SettingId  =settingId;
        this.SettingType=settingType;
    }

    internal void AddDwordValue(uint valueDword,string Profile)
    {
        var setting=Enumerable.FirstOrDefault(this.SettingValues,s=>s.Value==valueDword);
        if(setting==null)
        {
            this.SettingValues.Add(new CachedSettingValue(valueDword,Profile));
        }
        else
        {
            setting.ProfileNames.Append(", "+Profile);
            setting.ValueProfileCount++;
        }

        this.ProfileCount++;
    }

    internal void AddStringValue(string valueStr,string Profile)
    {
        var setting=Enumerable.FirstOrDefault(this.SettingValues,s=>s.ValueStr==valueStr);
        if(setting==null)
        {
            this.SettingValues.Add(new CachedSettingValue(valueStr,Profile));
        }
        else
        {
            setting.ProfileNames.Append(", "+Profile);
            setting.ValueProfileCount++;
        }

        this.ProfileCount++;
    }

    internal void AddBinaryValue(byte[] valueBin,string Profile)
    {
        var setting=Enumerable.FirstOrDefault(this.SettingValues,s=>Enumerable.SequenceEqual(s.ValueBin,valueBin));
        if(setting==null)
        {
            this.SettingValues.Add(new CachedSettingValue(valueBin,Profile));
        }
        else
        {
            setting.ProfileNames.Append(", "+Profile);
            setting.ValueProfileCount++;
        }

        this.ProfileCount++;
    }
}