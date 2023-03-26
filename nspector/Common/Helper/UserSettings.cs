using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace nspector.Common.Helper
{
    public class UserSettings
    {
        public int WindowTop { get; set; }

        public int WindowLeft { get; set; }

        public int WindowWidth { get; set; }

        public int WindowHeight { get; set; }

        public FormWindowState WindowState { get; set; }

        public bool ShowCustomizedSettingNamesOnly { get; set; } = false;

        public bool ShowScannedUnknownSettings { get; set; } = false;

        private static string GetSettingsFilename()
        {
            var fiPortalbleSettings = new FileInfo("settings.xml");
            if (fiPortalbleSettings.Exists) return fiPortalbleSettings.FullName;

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName);
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
