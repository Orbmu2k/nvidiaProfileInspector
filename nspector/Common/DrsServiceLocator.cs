namespace nspector.Common;

class DrsServiceLocator
{
    static readonly        nspector.Common.CustomSettings.CustomSettingNames CustomSettings;
    public static readonly nspector.Common.CustomSettings.CustomSettingNames ReferenceSettings;
    public static readonly DrsSettingsMetaService                            MetaService;
    public static readonly DrsSettingsService                                SettingService;
    public static readonly DrsImportService                                  ImportService;
    public static readonly DrsScannerService                                 ScannerService;
    public static readonly DrsDecrypterService                               DecrypterService;

    static DrsServiceLocator()
    {
        DrsServiceLocator.CustomSettings   =DrsServiceLocator.LoadCustomSettings();
        DrsServiceLocator.ReferenceSettings=DrsServiceLocator.LoadReferenceSettings();

        DrsServiceLocator.MetaService
            =new DrsSettingsMetaService(DrsServiceLocator.CustomSettings,DrsServiceLocator.ReferenceSettings);
        DrsServiceLocator.DecrypterService=new DrsDecrypterService(DrsServiceLocator.MetaService);
        DrsServiceLocator.ScannerService
            =new DrsScannerService(DrsServiceLocator.MetaService,DrsServiceLocator.DecrypterService);
        DrsServiceLocator.SettingService
            =new DrsSettingsService(DrsServiceLocator.MetaService,DrsServiceLocator.DecrypterService);
        DrsServiceLocator.ImportService=new DrsImportService(DrsServiceLocator.MetaService,
            DrsServiceLocator.SettingService,DrsServiceLocator.ScannerService,DrsServiceLocator.DecrypterService);
    }

    static nspector.Common.CustomSettings.CustomSettingNames LoadCustomSettings()
    {
        var csnDefaultPath=System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+
            "\\CustomSettingNames.xml";

        if(System.IO.File.Exists(csnDefaultPath))
        {
            return nspector.Common.CustomSettings.CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);
        }

        return nspector.Common.CustomSettings.CustomSettingNames.FactoryLoadFromString(nspector.Properties.Resources
            .CustomSettingNames);
    }

    static nspector.Common.CustomSettings.CustomSettingNames LoadReferenceSettings()
    {
        var csnDefaultPath=System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            +"\\Reference.xml";

        if(System.IO.File.Exists(csnDefaultPath))
        {
            return nspector.Common.CustomSettings.CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);
        }

        return null;
    }
}