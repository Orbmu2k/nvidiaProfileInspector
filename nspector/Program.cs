#region

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using nspector.Common;
using nspector.Common.Helper;
using nspector.Native.WINAPI;
using nspector.Properties;

#endregion

namespace nspector;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            // Remove Zone.Identifier from Alternate Data Stream
            SafeNativeMethods.DeleteFile(Application.ExecutablePath + ":Zone.Identifier");
        }
        catch
        {
        }
#if RELEASE
            try
            {
#endif
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        DropDownMenuScrollWheelHandler.Enable(true);

        var argFileIndex = ArgFileIndex(args);
        if (argFileIndex != -1)
        {

            if (new FileInfo(args[argFileIndex]).Extension.ToLower() == ".nip")
                try
                {
                    var import = DrsServiceLocator.ImportService;
                    var importReport = import.ImportProfiles(args[argFileIndex]);
                    GC.Collect();
                    var current = Process.GetCurrentProcess();
                    foreach (
                        var process in
                        Process.GetProcessesByName(current.ProcessName.Replace(".vshost", "")))
                        if (process.Id != current.Id && process.MainWindowTitle.Contains("Settings"))
                        {
                            var mh = new MessageHelper();
                            mh.sendWindowsStringMessage((int) process.MainWindowHandle, 0, "ProfilesImported");
                        }

                    if (string.IsNullOrEmpty(importReport) && !ArgExists(args, "-silentImport") && !ArgExists(args, "-silent"))
                        frmDrvSettings.ShowImportDoneMessage(importReport);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Import Error: " + ex.Message, Application.ProductName + " Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        else if (ArgExists(args, "-createCSN"))
        {
            File.WriteAllText("CustomSettingNames.xml", Resources.CustomSettingNames);
        }
        else
        {

            var createdNew = true;
            using (var mutex = new Mutex(true, Application.ProductName, out createdNew))
            {
                if (createdNew)
                {
                    Application.Run(new frmDrvSettings(ArgExists(args, "-showOnlyCSN"), ArgExists(args, "-disableScan")));
                }
                else
                {
                    var current = Process.GetCurrentProcess();
                    foreach (
                        var process in
                        Process.GetProcessesByName(current.ProcessName.Replace(".vshost", "")))
                        if (process.Id != current.Id && process.MainWindowTitle.Contains("Settings"))
                        {
                            var mh = new MessageHelper();
                            mh.bringAppToFront((int) process.MainWindowHandle);
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

    private static bool ArgExists(string[] args, string arg)
    {
        foreach (var a in args)
            if (a.ToUpper() == arg.ToUpper())
                return true;
        return false;
    }

    private static int ArgFileIndex(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
            if (File.Exists(args[i]))
                return i;

        return -1;
    }
}