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

        public bool ShowCustomizedSettingNamesOnly { get; set; } = true;

        public bool ShowScannedUnknownSettings { get; set; } = false;

        public List<string> HiddenSettingGroups { get; set; } = new List<string>();

        public bool DisableUpdateCheck { get; set; } = false;

        public List<uint> FavoriteSettingIds { get; set; } = new List<uint>();

public string Theme { get; set; } = "DarkTheme.xaml";

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
