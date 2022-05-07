namespace nspector;

static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [System.STAThreadAttribute]
    static void Main(string[] args)
    {
        try
        {
            // Remove Zone.Identifier from Alternate Data Stream
            nspector.Native.WINAPI.SafeNativeMethods.DeleteFile(System.Windows.Forms.Application.ExecutablePath
                +":Zone.Identifier");
        }
        catch {}
    #if RELEASE
            try
            {
    #endif
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        nspector.Common.Helper.DropDownMenuScrollWheelHandler.Enable(true);

        var argFileIndex=Program.ArgFileIndex(args);
        if(argFileIndex!=-1)
        {
            if(new System.IO.FileInfo(args[argFileIndex]).Extension.ToLower()==".nip")
            {
                try
                {
                    var import      =nspector.Common.DrsServiceLocator.ImportService;
                    var importReport=import.ImportProfiles(args[argFileIndex]);
                    System.GC.Collect();
                    var current=System.Diagnostics.Process.GetCurrentProcess();
                    foreach(
                        var process in
                        System.Diagnostics.Process.GetProcessesByName(current.ProcessName.Replace(".vshost","")))
                    {
                        if(process.Id!=current.Id&&process.MainWindowTitle.Contains("Settings"))
                        {
                            var mh=new nspector.Native.WINAPI.MessageHelper();
                            mh.sendWindowsStringMessage((int)process.MainWindowHandle,0,"ProfilesImported");
                        }
                    }

                    if(string.IsNullOrEmpty(importReport)&&!Program.ArgExists(args,"-silentImport")&&
                        !Program.ArgExists(args,                                   "-silent"))
                    {
                        frmDrvSettings.ShowImportDoneMessage(importReport);
                    }
                }
                catch(System.Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Import Error: "+ex.Message,
                        System.Windows.Forms.Application.ProductName     +" Error",
                        System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }

        else if(Program.ArgExists(args,"-createCSN"))
        {
            System.IO.File.WriteAllText("CustomSettingNames.xml",nspector.Properties.Resources.CustomSettingNames);
        }
        else
        {
            var createdNew=true;
            using(var mutex
                =new System.Threading.Mutex(true,System.Windows.Forms.Application.ProductName,out createdNew))
            {
                if(createdNew)
                {
                    System.Windows.Forms.Application.Run(
                        new frmDrvSettings(Program.ArgExists(args,"-showOnlyCSN"),
                            Program.ArgExists(args,               "-disableScan")));
                }
                else
                {
                    var current=System.Diagnostics.Process.GetCurrentProcess();
                    foreach(
                        var process in
                        System.Diagnostics.Process.GetProcessesByName(current.ProcessName.Replace(".vshost","")))
                    {
                        if(process.Id!=current.Id&&process.MainWindowTitle.Contains("Settings"))
                        {
                            var mh=new nspector.Native.WINAPI.MessageHelper();
                            mh.bringAppToFront((int)process.MainWindowHandle);
                        }
                    }
                }
            }
        }
    #if RELEASE
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace ,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
    #endif
    }

    static bool ArgExists(string[] args,string arg)
    {
        foreach(var a in args)
        {
            if(a.ToUpper()==arg.ToUpper())
            {
                return true;
            }
        }

        return false;
    }

    static int ArgFileIndex(string[] args)
    {
        for(var i=0;i<args.Length;i++)
        {
            if(System.IO.File.Exists(args[i]))
            {
                return i;
            }
        }

        return-1;
    }
}