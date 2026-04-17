using nvidiaProfileInspector.Common.Meta;
using System;
using System.Globalization;
using System.Linq;

namespace nvidiaProfileInspector.Common
{
    public static class DrsUtil
    {
        public static string GetDwordString(uint dword)
        {
            return "0x" + dword.ToString("X8");
        }

        public static uint ParseDwordByInputSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                int blankPos = input.IndexOf(' ');
                int parseLen = blankPos > 2 ? blankPos - 2 : input.Length - 2;

                if (uint.TryParse(input.Substring(2, parseLen), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint result))
                    return result;
            }
            else
            {
                if (uint.TryParse(input, out uint result))
                    return result;
            }

            return 0;
        }

        public static string ParseToRawValue(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                int blankPos = input.IndexOf(' ');
                return (blankPos == -1) ? input : input.Substring(0, blankPos);
            }

            if (uint.TryParse(input, out uint numericValue))
            {
                return "0x" + numericValue.ToString("X8");
            }

            return input;
        }

        public static uint ParseDwordSettingValue(SettingMeta meta, string text)
        {
            return TryParseDwordSettingValue(meta, text, out var result) ? result : 0;
        }

        public static bool TryParseDwordSettingValue(SettingMeta meta, string text, out uint result)
        {
            result = 0;
            var normalizedText = text?.Trim();

            if (meta?.DwordValues != null && normalizedText != null)
            {
                foreach (var v in meta.DwordValues)
                {
                    if (normalizedText.Equals(v.ValueName, StringComparison.Ordinal))
                    {
                        result = v.Value;
                        return true;
                    }
                }
            }

            return TryParseDwordByInputSafe(normalizedText, out result);
        }

        private static bool TryParseDwordByInputSafe(string input, out uint result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                int blankPos = input.IndexOf(' ');
                int parseLen = blankPos > 2 ? blankPos - 2 : input.Length - 2;

                return uint.TryParse(input.Substring(2, parseLen), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out result);
            }

            return uint.TryParse(input, out result);
        }

        public static string GetDwordSettingValueName(SettingMeta meta, uint dwordValue)
        {
            if (meta?.DwordValues != null)
            {
                foreach (var v in meta.DwordValues)
                {
                    if (v.Value == dwordValue)
                        return v.ValueName;
                }
            }

            return GetDwordString(dwordValue);
        }

        public static string ParseStringSettingValue(SettingMeta meta, string text)
        {
            if (meta?.StringValues != null && text != null)
            {
                foreach (var v in meta.StringValues)
                {
                    if (text.Equals(v.ValueName, StringComparison.Ordinal))
                        return v.Value;
                }
            }

            return text;
        }

        public static string GetStringSettingValueName(SettingMeta meta, string stringValue)
        {
            if (meta?.StringValues != null && stringValue != null)
            {
                foreach (var v in meta.StringValues)
                {
                    if (stringValue.Equals(v.Value, StringComparison.Ordinal))
                        return v.ValueName;
                }
            }

            return stringValue;
        }

        public static string GetBinaryString(byte[] binaryValue)
        {
            if (binaryValue == null || binaryValue.Length == 0)
                return string.Empty;

            if (binaryValue.Length == 8)
                return "0x" + BitConverter.ToUInt64(binaryValue, 0).ToString("X16");

            return BitConverter.ToString(binaryValue);
        }


        public static string GetBinarySettingValueName(SettingMeta meta, byte[] binaryValue)
        {
            if (meta?.BinaryValues != null && binaryValue != null)
            {
                foreach (var v in meta.BinaryValues)
                {
                    if (Enumerable.SequenceEqual(v.Value, binaryValue))
                        return v.ValueName;
                }
            }

            return GetBinaryString(binaryValue);
        }

        public static byte[] ParseBinarySettingValue(SettingMeta meta, string text)
        {
            if (meta?.BinaryValues != null && text != null)
            {
                foreach (var v in meta.BinaryValues)
                {
                    if (text.Equals(v.ValueName, StringComparison.Ordinal))
                        return v.Value;
                }
            }

            return ParseBinaryByInputSafe(text);
        }

        public static byte[] ParseBinaryByInputSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                int blankPos = input.IndexOf(' ');
                int parseLen = blankPos > 2 ? blankPos - 2 : input.Length - 2;

                if (ulong.TryParse(input.Substring(2, parseLen), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ulong qword))
                    return BitConverter.GetBytes(qword);
            }

            if (input.IndexOf('-') != -1)
            {
                try
                {
                    string[] parts = input.Split('-');
                    byte[] bytes = new byte[parts.Length];
                    for (int i = 0; i < parts.Length; i++)
                    {
                        bytes[i] = Convert.ToByte(parts[i], 16);
                    }
                    return bytes;
                }
                catch { return null; }
            }

            return null;
        }
    }
}
