using System.Text;

namespace nvidiaProfileInspector.Common
{
    public class CachedSettingValue
    {

        public CachedSettingValue() { }

        public CachedSettingValue(uint Value, string ProfileNames)
        {
            this.Value = Value;
            this.ProfileNames = new StringBuilder(ProfileNames);
            this.ValueProfileCount = 1;
        }

        public CachedSettingValue(string ValueStr, string ProfileNames)
        {
            this.ValueStr = ValueStr;
            this.ProfileNames = new StringBuilder(ProfileNames);
            this.ValueProfileCount = 1;
        }

        public CachedSettingValue(byte[] ValueBin, string ProfileNames)
        {
            this.ValueBin = ValueBin;
            this.ProfileNames = new StringBuilder(ProfileNames);
            this.ValueProfileCount = 1;
        }

        public string ValueStr = "";
        public uint Value = 0;
        public byte[] ValueBin = new byte[0];
        public StringBuilder ProfileNames;
        public uint ValueProfileCount;
    }
}