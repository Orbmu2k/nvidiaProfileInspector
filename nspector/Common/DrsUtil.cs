using nspector.Common.Meta;
using System;
using System.Globalization;
using System.Linq;

namespace nspector.Common
{
    public static class DrsUtil
    {
        public static string StringValueRaw = "Text";

        public static string GetDwordString(uint dword)
        {
            return string.Format("0x{0:X8}", dword);
        }

        public static uint ParseDwordByInputSafe(string input)
        {
            uint result = 0;
            if (input.ToLowerInvariant().StartsWith("0x"))
            {
                try
                {
                    int blankPos = input.IndexOf(' ');
                    int parseLen = blankPos > 2 ? blankPos - 2 : input.Length - 2;
                    result = uint.Parse(input.Substring(2, parseLen), NumberStyles.AllowHexSpecifier);
                }
                catch { }
            }
            else
                try { result = uint.Parse(input); }
                catch { }

            return result;
        }

        internal static uint ParseDwordSettingValue(SettingMeta meta, string text)
        {
            var valueByName = meta.DwordValues.FirstOrDefault(x => x.ValueName != null && x.ValueName.Equals(text));
            if (valueByName != null)
                return valueByName.Value;

            return ParseDwordByInputSafe(text);
        }

        internal static string GetDwordSettingValueName(SettingMeta meta, uint dwordValue)
        {
            var settingValue = meta.DwordValues
                       .FirstOrDefault(x => x.Value.Equals(dwordValue));

            return settingValue == null ? GetDwordString(dwordValue) : settingValue.ValueName;
        }

        internal static string ParseStringSettingValue(SettingMeta meta, string text)
        {
            var valueByName = meta.StringValues?.FirstOrDefault(x => x.ValueName != null && x.ValueName.Equals(text));
            if (valueByName != null)
                return valueByName.Value;

            return text;
        }

        internal static string GetStringSettingValueName(SettingMeta meta, string stringValue)
        {
            var settingValue = meta.StringValues
                       .FirstOrDefault(x => x.Value.Equals(stringValue));

            return settingValue == null ? stringValue : settingValue.ValueName;
        }

        public static string GetBinaryString(byte[] binaryValue)
        {
            if (binaryValue == null)
                return "";

            if (binaryValue.Length == 8)
                return string.Format("0x{0:X16}", BitConverter.ToUInt64(binaryValue, 0));

            return BitConverter.ToString(binaryValue);
        }

        internal static string GetBinarySettingValueName(SettingMeta meta, byte[] binaryValue)
        {
            var settingValue = meta.BinaryValues?
                       .FirstOrDefault(x => x.Value.Equals(binaryValue));

            return settingValue == null ? GetBinaryString(binaryValue) : settingValue.ValueName;
        }

        internal static byte[] ParseBinarySettingValue(SettingMeta meta, string text)
        {
            var valueByName = meta.BinaryValues.FirstOrDefault(x => x.ValueName != null && x.ValueName.Equals(text));
            if (valueByName != null)
                return valueByName.Value;

            return ParseBinaryByInputSafe(text);
        }

        public static byte[] ParseBinaryByInputSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (input.StartsWith("0x"))
            {
                int blankPos = input.IndexOf(' ');
                int parseLen = blankPos > 2 ? blankPos - 2 : input.Length - 2;
                var qword = ulong.Parse(input.Substring(2, parseLen), NumberStyles.AllowHexSpecifier);
                return BitConverter.GetBytes(qword);
            }

            if (input.Contains("-"))
                return Array.ConvertAll<string, byte>(input.Split('-'), s => Convert.ToByte(s, 16));

            return null;
        }

    }
}
