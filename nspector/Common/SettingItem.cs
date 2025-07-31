namespace nspector.Common
{

    internal enum SettingState
    {
        NotAssiged,
        GlobalSetting,
        UserdefinedSetting,
        NvidiaSetting,
    }

    internal class SettingItem
    {
        public uint SettingId { get; set; }

        public string SettingText { get; set; }

        public string ValueText { get; set; }

        public string ValueRaw { get; set; }

        public string GroupName { get; set; }

        public string AlternateNames { get; set; }

        public SettingState State { get; set; }

        public bool IsStringValue { get; set; }

        public bool IsApiExposed { get; set; }

        public bool IsSettingHidden { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; 0x{1:X8}; {2}; {3}; {4};", State, SettingId, SettingText, ValueText, ValueRaw);
        }

    }
}
