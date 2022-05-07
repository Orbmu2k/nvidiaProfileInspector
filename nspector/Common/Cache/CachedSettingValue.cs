namespace nspector.Common;

class CachedSettingValue
{
    internal System.Text.StringBuilder ProfileNames;
    internal uint                      Value;
    internal byte[]                    ValueBin=new byte[0];
    internal uint                      ValueProfileCount;

    internal string ValueStr="";

    internal CachedSettingValue() {}

    internal CachedSettingValue(uint Value,string ProfileNames)
    {
        this.Value            =Value;
        this.ProfileNames     =new System.Text.StringBuilder(ProfileNames);
        this.ValueProfileCount=1;
    }

    internal CachedSettingValue(string ValueStr,string ProfileNames)
    {
        this.ValueStr         =ValueStr;
        this.ProfileNames     =new System.Text.StringBuilder(ProfileNames);
        this.ValueProfileCount=1;
    }

    internal CachedSettingValue(byte[] ValueBin,string ProfileNames)
    {
        this.ValueBin         =ValueBin;
        this.ProfileNames     =new System.Text.StringBuilder(ProfileNames);
        this.ValueProfileCount=1;
    }
}