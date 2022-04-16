﻿using System.IO;
using System.Reflection;
using nspector.Common.CustomSettings;

namespace nspector.Common;

internal class DrsServiceLocator
{
    private static readonly CustomSettingNames CustomSettings;
    public static readonly CustomSettingNames ReferenceSettings;
    public static readonly DrsSettingsMetaService MetaService;
    public static readonly DrsSettingsService SettingService;
    public static readonly DrsImportService ImportService;
    public static readonly DrsScannerService ScannerService;
    public static readonly DrsDecrypterService DecrypterService;

    static DrsServiceLocator()
    {
        CustomSettings = LoadCustomSettings();
        ReferenceSettings = LoadReferenceSettings();

        MetaService = new DrsSettingsMetaService(CustomSettings, ReferenceSettings);
        DecrypterService = new DrsDecrypterService(MetaService);
        ScannerService = new DrsScannerService(MetaService, DecrypterService);
        SettingService = new DrsSettingsService(MetaService, DecrypterService);
        ImportService = new DrsImportService(MetaService, SettingService, ScannerService, DecrypterService);
    }

    private static CustomSettingNames LoadCustomSettings()
    {
        var csnDefaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                             "\\CustomSettingNames.xml";

        if (File.Exists(csnDefaultPath))
            return CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);
        return CustomSettingNames.FactoryLoadFromString(Properties.Resources.CustomSettingNames);
    }

    private static CustomSettingNames LoadReferenceSettings()
    {
        var csnDefaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Reference.xml";

        if (File.Exists(csnDefaultPath))
            return CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);

        return null;
    }
}