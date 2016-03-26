using System;
using System.Text;

namespace nspector.Common
{
    internal class CachedSettingValue : IComparable<CachedSettingValue>
    {

        public int CompareTo(CachedSettingValue other)
        {
            if (IsStringValue)
                return ValueStr.CompareTo(other.ValueStr);
            else
                return Value.CompareTo(other.Value);
        }

        internal CachedSettingValue()  {  }

        internal CachedSettingValue(uint Value, string ProfileNames)
        {
            this.Value = Value;
            this.ProfileNames = new StringBuilder(ProfileNames);
            this.ValueProfileCount = 1;
        }

        internal CachedSettingValue(string ValueStr, string ProfileNames)
        {
            IsStringValue = true;
            this.ValueStr = ValueStr;
            this.ProfileNames = new StringBuilder(ProfileNames);
            this.ValueProfileCount = 1;
        }

        internal readonly bool IsStringValue = false;
        internal string ValueStr = "";
        internal uint Value = 0;
        internal StringBuilder ProfileNames;
        internal uint ValueProfileCount;
    }
}