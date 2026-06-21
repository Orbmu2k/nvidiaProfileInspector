using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;


namespace nvidiaProfileInspector.Common.Helper
{
    public class UserSettings
    {
        public int WindowTop { get; set; }

        public int WindowLeft { get; set; }

        public int WindowWidth { get; set; }

        public int WindowHeight { get; set; }

        public WindowState WindowState { get; set; }

        public int SettingsFilterMode { get; set; } = -1;

        public List<string> HiddenSettingGroups { get; set; } = new List<string>();

        public bool DisableUpdateCheck { get; set; } = false;

        public bool DisableSplashScreen { get; set; } = false;

        public string UpdateChannel { get; set; } = "Release";

        public List<uint> FavoriteSettingIds { get; set; } = new List<uint>();

        public string Theme { get; set; } = "DarkTheme.xaml";

        public string DisplayDensity { get; set; } = "Modern";

        public string Win11BackdropMode { get; set; } = "Default";

        // Source filter selection (replaces the legacy SettingsFilterMode combo).
        // Setting sources decide which sources contribute setting rows.
        public bool SettingSourceCommon { get; set; } = true;

        public bool SettingSourceDriver { get; set; } = false;

        public bool SettingSourceConstants { get; set; } = false;

        public bool SettingSourceReference { get; set; } = false;

        public bool SettingSourceScan { get; set; } = false;

        // Value sources decide which sources contribute predefined values to a dropdown.
        public bool ValueSourceCommon { get; set; } = true;

        public bool ValueSourceDriver { get; set; } = false;

        public bool ValueSourceConstants { get; set; } = true;

        public bool ValueSourceReference { get; set; } = true;

        public bool ValueSourceScan { get; set; } = true;

        // Post-filter: only show settings with a user override or an unsaved edit.
        public bool ModifiedOnly { get; set; } = false;

        // When true, settings that are active in the current profile (predefined, global,
        // or user value) appear regardless of whether their setting source is enabled.
        public bool ShowActiveFromDisabledSources { get; set; } = true;

        // Value dropdown behavior: merge same-value entries across sources into one, and
        // let the predefined scan (app list) values merge into the common values too.
        public bool MergeDistinctValues { get; set; } = true;

        public bool AddPredefinedAppListToCommon { get; set; } = false;

        // Prefix the raw value to common (CSN) value names. Off by default.
        public bool AddRawValueToCommon { get; set; } = false;

        // Allow a setting's name and description to come from sources that are not currently
        // enabled as setting sources.
        public bool AllowMetaFromInactiveSources { get; set; } = true;

        private static string GetSettingsFilename()
        {
            var fiPortalbleSettings = new FileInfo("settings.xml");
            if (fiPortalbleSettings.Exists) return fiPortalbleSettings.FullName;

            string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(
                    Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute))).Product;

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), productName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return Path.Combine(path, "settings.xml"); ;
        }

        public void SaveSettings()
        {
            XMLHelper<UserSettings>.SerializeToXmlFile(this, GetSettingsFilename(), Encoding.Unicode, true);
        }

        public static UserSettings LoadSettings()
        {
            var filename = GetSettingsFilename();
            if (!File.Exists(filename)) return new UserSettings();

            try
            {
                return XMLHelper<UserSettings>.DeserializeFromXMLFile(GetSettingsFilename());
            }
            catch
            {
                return new UserSettings();
            }
        }
    }
}
