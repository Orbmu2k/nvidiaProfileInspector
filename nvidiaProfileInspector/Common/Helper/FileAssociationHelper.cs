using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace nvidiaProfileInspector.Common.Helper
{
    public static class FileAssociationHelper
    {
        private const string Extension = ".nip";
        private const string ProgId = "nvidiaProfileInspector.nip";
        private const string FileTypeDescription = "NVIDIA Profile Inspector Profile";
        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static void RegisterNipAssociation()
        {
            var executablePath = GetExecutablePath();

            var openCommand = $"\"{executablePath}\" \"%1\"";
            var iconPath = $"\"{executablePath}\",0";

            RegisterForRoot(Registry.CurrentUser, @"Software\Classes", executablePath, openCommand, iconPath);
            RegisterForRoot(Registry.LocalMachine, @"Software\Classes", executablePath, openCommand, iconPath);

            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        private static void RegisterForRoot(RegistryKey root, string classesPath, string executablePath, string openCommand, string iconPath)
        {
            using (var classesRoot = root.CreateSubKey(classesPath))
            {
                using (var extensionKey = classesRoot.CreateSubKey(Extension))
                {
                    extensionKey.SetValue(string.Empty, ProgId, RegistryValueKind.String);
                    extensionKey.SetValue("PerceivedType", "document", RegistryValueKind.String);

                    using (var openWithProgIdsKey = extensionKey.CreateSubKey("OpenWithProgids"))
                    {
                        openWithProgIdsKey.SetValue(ProgId, string.Empty, RegistryValueKind.String);
                    }
                }

                using (var progIdKey = classesRoot.CreateSubKey(ProgId))
                {
                    progIdKey.SetValue(string.Empty, FileTypeDescription, RegistryValueKind.String);

                    using (var defaultIconKey = progIdKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIconKey.SetValue(string.Empty, iconPath, RegistryValueKind.String);
                    }

                    using (var openCommandKey = progIdKey.CreateSubKey(@"shell\open\command"))
                    {
                        openCommandKey.SetValue(string.Empty, openCommand, RegistryValueKind.String);
                    }
                }

                using (var applicationsKey = classesRoot.CreateSubKey($@"Applications\{Path.GetFileName(executablePath)}"))
                {
                    applicationsKey.SetValue("FriendlyAppName", "NVIDIA Profile Inspector", RegistryValueKind.String);

                    using (var supportedTypesKey = applicationsKey.CreateSubKey("SupportedTypes"))
                    {
                        supportedTypesKey.SetValue(Extension, string.Empty, RegistryValueKind.String);
                    }

                    using (var defaultIconKey = applicationsKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIconKey.SetValue(string.Empty, iconPath, RegistryValueKind.String);
                    }

                    using (var openCommandKey = applicationsKey.CreateSubKey(@"shell\open\command"))
                    {
                        openCommandKey.SetValue(string.Empty, openCommand, RegistryValueKind.String);
                    }
                }
            }
        }

        private static string GetExecutablePath()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null && File.Exists(entryAssembly.Location))
                return Path.GetFullPath(entryAssembly.Location);

            var currentProcessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrWhiteSpace(currentProcessPath) && File.Exists(currentProcessPath))
                return Path.GetFullPath(currentProcessPath);

            return Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
        }
    }
}
