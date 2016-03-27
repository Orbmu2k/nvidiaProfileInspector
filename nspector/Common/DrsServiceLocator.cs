using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using nspector.Common.CustomSettings;

using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;

namespace nspector.Common
{
    internal class DrsServiceLocator
    {
        private static readonly CustomSettingNames CustomSettings;
        public static readonly CustomSettingNames ReferenceSettings;
        public static readonly DrsSettingsMetaService MetaService;
        public static readonly DrsSettingsService SettingService;
        public static readonly DrsImportService ImportService;
        public static readonly DrsScannerService ScannerService;

        private static IntPtr _Session;

        static DrsServiceLocator()
        {
            CustomSettings = LoadCustomSettings();
            ReferenceSettings = LoadReferenceSettings();

            ReCreateSession();

            MetaService = new DrsSettingsMetaService(CustomSettings, ReferenceSettings);
            SettingService = new DrsSettingsService(MetaService, _Session);
            ImportService = new DrsImportService(MetaService, SettingService, _Session);
            ScannerService = new DrsScannerService(MetaService, _Session);
        }

        public static void ReCreateSession()
        {
            if (_Session != null && _Session != IntPtr.Zero)
            {
                nvw.DRS_SaveSettings(_Session);
                nvw.DRS_DestroySession(_Session);
            }

            _Session = DrsSettingsServiceBase.CreateAndLoadSession();
        }

        private static CustomSettingNames LoadCustomSettings()
        {
            string csnDefaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CustomSettingNames.xml";

            if (File.Exists(csnDefaultPath))
                return CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);
            else
                return CustomSettingNames.FactoryLoadFromString(Properties.Resources.CustomSettingNames);
        }

        private static CustomSettingNames LoadReferenceSettings()
        {
            string csnDefaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Reference.xml";

            if (File.Exists(csnDefaultPath))
                return CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);

            return null;
        }
        
    }
}
