using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using nspector.Common;
using nspector.Common.Helper;
using nspector.Native.WINAPI;

namespace nspector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Remove Zone.Identifier from Alternate Data Stream
                SafeNativeMethods.DeleteFile(Application.ExecutablePath + ":Zone.Identifier");
            }
            catch { }
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

                if (new FileInfo(args[argFileIndex]).Extension.ToLowerInvariant() == ".nip")
                {
                    try
                    {
                        var import = DrsServiceLocator.ImportService;
                        var importReport = import.ImportProfiles(args[argFileIndex]);
                        GC.Collect();
                        Process current = Process.GetCurrentProcess();
                        foreach (
                            Process process in
                                Process.GetProcessesByName(current.ProcessName.Replace(".vshost", "")))
                        {
                            if (process.Id != current.Id && process.MainWindowTitle.Contains("Settings"))
                            {
                                MessageHelper mh = new MessageHelper();
                                mh.sendWindowsStringMessage((int)process.MainWindowHandle, 0, "ProfilesImported");
                            }
                        }

                        if (string.IsNullOrEmpty(importReport) && !ArgExists(args, "-silentImport") && !ArgExists(args, "-silent"))
                        {
                            frmDrvSettings.ShowImportDoneMessage(importReport);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Import Error: " + ex.Message, Application.ProductName + " Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            else if (ArgExists(args, "-createCSN"))
            {
                File.WriteAllText("CustomSettingNames.xml", Properties.Resources.CustomSettingNames);
            }
            else
            {

                bool createdNew = true;
                using (Mutex mutex = new Mutex(true, Application.ProductName, out createdNew))
                {
                    if (createdNew)
                    {
                        Application.Run(new frmDrvSettings(ArgExists(args, "-showOnlyCSN"), ArgExists(args, "-disableScan")));
                    }
                    else
                    {
                        Process current = Process.GetCurrentProcess();
                        foreach (
                            Process process in
                                Process.GetProcessesByName(current.ProcessName.Replace(".vshost", "")))
                        {
                            if (process.Id != current.Id && process.MainWindowTitle.Contains("Settings"))
                            {
                                MessageHelper mh = new MessageHelper();
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

        static bool ArgExists(string[] args, string arg)
        {
            foreach (string a in args)
            {
                if (a.ToUpper() == arg.ToUpper())
                    return true;
            }
            return false;
        }

        static int ArgFileIndex(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                    return i;
            }

            return -1;
        }
    }
}
