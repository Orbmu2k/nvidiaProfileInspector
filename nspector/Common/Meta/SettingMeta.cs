using nspector.Native.NVAPI2;
using System.Collections.Generic;

namespace nspector.Common.Meta
{
    internal class SettingMeta
    {
        public NVDRS_SETTING_TYPE? SettingType { get; set; }

        public string GroupName { get; set; }

        public string AlternateNames { get; set; }

        public string SettingName { get; set; }

        public string DefaultStringValue { get; set; }

        public uint DefaultDwordValue { get; set; }

        public byte[] DefaultBinaryValue { get; set; }

        public bool IsApiExposed { get; set; }

        public bool IsSettingHidden { get; set; }

        public string Description { get; set; }

        public List<SettingValue<string>> StringValues { get; set; }

        public List<SettingValue<uint>> DwordValues { get; set; }

        public List<SettingValue<byte[]>> BinaryValues { get; set; }
    }
}
