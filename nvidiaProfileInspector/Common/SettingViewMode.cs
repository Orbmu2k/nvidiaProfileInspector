namespace nvidiaProfileInspector.Common
{
    public enum SettingViewMode
    {
        /// <summary>Curated common settings plus the live driver settings.</summary>
        CommonAndDriver,

        /// <summary>Everything: constant, scanned, custom, driver and reference settings.</summary>
        AllSettings,

        /// <summary>Only the curated common (custom) settings.</summary>
        CommonOnly,
    }
}
