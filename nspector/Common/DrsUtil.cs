#region

using System.Linq;
using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector.Common;

public static class DrsUtil
{
    public static string StringValueRaw="Text";

    public static string GetDwordString(uint dword)=>string.Format("0x{0:X8}",dword);

    public static uint ParseDwordByInputSafe(string input)
    {
        uint result=0;
        if(input.ToLower().StartsWith("0x"))
        {
            try
            {
                var blankPos=input.IndexOf(' ');
                var parseLen=blankPos>2?blankPos-2:input.Length-2;
                result=uint.Parse(input.Substring(2,parseLen),System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch {}
        }
        else
        {
            try
            {
                result=uint.Parse(input);
            }
            catch {}
        }

        return result;
    }

    internal static uint ParseDwordSettingValue(nspector.Common.Meta.SettingMeta meta,string text)
    {
        var valueByName=Enumerable.FirstOrDefault(meta.DwordValues,x=>x.ValueName!=null&&x.ValueName.Equals(text));
        if(valueByName!=null)
        {
            return valueByName.Value;
        }

        return DrsUtil.ParseDwordByInputSafe(text);
    }

    internal static string GetDwordSettingValueName(nspector.Common.Meta.SettingMeta meta,uint dwordValue)
    {
        var settingValue=Enumerable.FirstOrDefault(meta.DwordValues,x=>x.Value.Equals(dwordValue));

        return settingValue==null?DrsUtil.GetDwordString(dwordValue):settingValue.ValueName;
    }

    internal static string ParseStringSettingValue(nspector.Common.Meta.SettingMeta meta,string text)
    {
        var valueByName=Enumerable.FirstOrDefault(meta.StringValues,x=>x.ValueName!=null&&x.ValueName.Equals(text));
        if(valueByName!=null)
        {
            return valueByName.Value;
        }

        return text;
    }

    internal static string GetStringSettingValueName(nspector.Common.Meta.SettingMeta meta,string stringValue)
    {
        var settingValue=Enumerable.FirstOrDefault(meta.StringValues,x=>x.Value.Equals(stringValue));

        return settingValue==null?stringValue:settingValue.ValueName;
    }

    public static string GetBinaryString(byte[] binaryValue)
    {
        if(binaryValue==null)
        {
            return"";
        }

        if(binaryValue.Length==8)
        {
            return string.Format("0x{0:X16}",System.BitConverter.ToUInt64(binaryValue,0));
        }

        return System.BitConverter.ToString(binaryValue);
    }

    internal static string GetBinarySettingValueName(nspector.Common.Meta.SettingMeta meta,byte[] binaryValue)
    {
        var settingValue=meta.BinaryValues?
            .FirstOrDefault(x=>x.Value.Equals(binaryValue));

        return settingValue==null?DrsUtil.GetBinaryString(binaryValue):settingValue.ValueName;
    }

    internal static byte[] ParseBinarySettingValue(nspector.Common.Meta.SettingMeta meta,string text)
    {
        var valueByName=Enumerable.FirstOrDefault(meta.BinaryValues,x=>x.ValueName!=null&&x.ValueName.Equals(text));
        if(valueByName!=null)
        {
            return valueByName.Value;
        }

        return DrsUtil.ParseBinaryByInputSafe(text);
    }

    public static byte[] ParseBinaryByInputSafe(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        if(input.StartsWith("0x"))
        {
            var blankPos=input.IndexOf(' ');
            var parseLen=blankPos>2?blankPos-2:input.Length-2;
            var qword   =ulong.Parse(input.Substring(2,parseLen),System.Globalization.NumberStyles.AllowHexSpecifier);
            return System.BitConverter.GetBytes(qword);
        }

        if(input.Contains("-"))
        {
            return System.Array.ConvertAll(input.Split('-'),s=>System.Convert.ToByte(s,16));
        }

        return null;
    }
}