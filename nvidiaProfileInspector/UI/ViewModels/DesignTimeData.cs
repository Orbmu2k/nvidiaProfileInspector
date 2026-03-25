namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;
    using System.Collections.ObjectModel;

    public static class DesignTimeData
    {
        public static MainViewModel MainViewModel => new MainViewModel(null, null, null, null);
        public static BitEditorViewModel BitEditorViewModel => new BitEditorViewModel(
            0x10F9DC81,
            0x0000100D,
            "Antialiasing Compatibility Flags",
            null,
            null,
            null);

        public static ObservableCollection<SettingItemViewModel> SampleSettings { get; } = new ObservableCollection<SettingItemViewModel>
        {
            new SettingItemViewModel(new SettingItem
            {
                SettingId = 0x00000001,
                SettingText = "Antialiasing - Mode",
                ValueText = "Enhance the application settings",
                ValueRaw = "0x00000002",
                GroupName = "Antialiasing",
                State = SettingState.NvidiaSetting
            }),
            new SettingItemViewModel(new SettingItem
            {
                SettingId = 0x00000002,
                SettingText = "Antialiasing - Setting",
                ValueText = "4x MSAA",
                ValueRaw = "0x00000004",
                GroupName = "Antialiasing",
                State = SettingState.UserdefinedSetting
            }),
            new SettingItemViewModel(new SettingItem
            {
                SettingId = 0x00000003,
                SettingText = "Texture Filtering - Quality",
                ValueText = "High Performance",
                ValueRaw = "0x00000001",
                GroupName = "Texture Filtering",
                State = SettingState.GlobalSetting
            }),
            new SettingItemViewModel(new SettingItem
            {
                SettingId = 0x00000004,
                SettingText = "Vertical Sync",
                ValueText = "On",
                ValueRaw = "0x00000001",
                GroupName = "Sync",
                State = SettingState.UserdefinedSetting,
                IsSettingHidden = true
            }),
            new SettingItemViewModel(new SettingItem
            {
                SettingId = 0x00000005,
                SettingText = "Power Management Mode",
                ValueText = "Prefer Maximum Performance",
                ValueRaw = "0x00000002",
                GroupName = "Power Management",
                State = SettingState.NvidiaSetting
            })
        };
    }
}
