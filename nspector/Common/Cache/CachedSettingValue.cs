using System;
using System.Text;

namespace nspector.Common;

internal class CachedSettingValue
{
    internal StringBuilder ProfileNames;
    internal uint Value;
    internal byte[] ValueBin = new byte[0];
    internal uint ValueProfileCount;

    internal string ValueStr = "";

    internal CachedSettingValue() { }

    internal CachedSettingValue(uint Value, string ProfileNames)
    {
        this.Value = Value;
        this.ProfileNames = new StringBuilder(ProfileNames);
        ValueProfileCount = 1;
    }

    internal CachedSettingValue(string ValueStr, string ProfileNames)
    {
        this.ValueStr = ValueStr;
        this.ProfileNames = new StringBuilder(ProfileNames);
        ValueProfileCount = 1;
    }

    internal CachedSettingValue(byte[] ValueBin, string ProfileNames)
    {
        this.ValueBin = ValueBin;
        this.ProfileNames = new StringBuilder(ProfileNames);
        ValueProfileCount = 1;
    }
}
