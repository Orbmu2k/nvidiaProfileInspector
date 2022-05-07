namespace nspector.Common.Helper;

public class UserSettings
{
    public int WindowTop
    {
        get;
        set;
    }

    public int WindowLeft
    {
        get;
        set;
    }

    public int WindowWidth
    {
        get;
        set;
    }

    public int WindowHeight
    {
        get;
        set;
    }

    public System.Windows.Forms.FormWindowState WindowState
    {
        get;
        set;
    }

    public bool ShowCustomizedSettingNamesOnly
    {
        get;
        set;
    }=false;

    public bool ShowScannedUnknownSettings
    {
        get;
        set;
    }=false;

    static string GetSettingsFilename()
    {
        var fiPortalbleSettings=new System.IO.FileInfo("settings.xml");
        if(fiPortalbleSettings.Exists)
        {
            return fiPortalbleSettings.FullName;
        }

        var path=System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            System.Windows.Forms.Application.ProductName);
        if(!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        return System.IO.Path.Combine(path,"settings.xml");
        ;
    }

    public void SaveSettings()
    {
        XMLHelper<UserSettings>.SerializeToXmlFile(this,UserSettings.GetSettingsFilename(),System.Text.Encoding.Unicode,
            true);
    }

    public static UserSettings LoadSettings()
    {
        var filename=UserSettings.GetSettingsFilename();
        if(!System.IO.File.Exists(filename))
        {
            return new UserSettings();
        }

        return XMLHelper<UserSettings>.DeserializeFromXMLFile(UserSettings.GetSettingsFilename());
    }
}