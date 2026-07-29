using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace nvidiaProfileInspector.Common.Helper
{
    public static class FileAssociationHelper
    {
        private const string Extension = ".nip";
        private const string ProgId = "nvidiaProfileInspector.nip";
        private const string FileTypeDescription = "NVIDIA PROFILE INSPECTOR Profile";
        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static void RegisterNipAssociation()
        {
            var executablePath = GetExecutablePath();

            var openCommand = $"\"{executablePath}\" \"%1\"";
            var iconPath = $"\"{executablePath}\",0";

            // Only rewrite the registry (and notify the shell) when the existing registration
            // is missing or points at a different executable path. Rewriting on every launch
            // is wasteful and makes Explorer flush its icon/association cache needlessly.
            var changed = false;
            changed |= TryEnsureRegisteredForRoot(Registry.CurrentUser, @"Software\Classes", executablePath, openCommand, iconPath);
            changed |= TryEnsureRegisteredForRoot(Registry.LocalMachine, @"Software\Classes", executablePath, openCommand, iconPath);

            if (changed)
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        private static bool TryEnsureRegisteredForRoot(RegistryKey root, string classesPath, string executablePath, string openCommand, string iconPath)
        {
            try
            {
                if (IsRegistrationCurrent(root, classesPath, openCommand, iconPath))
                    return false;

                RegisterForRoot(root, classesPath, executablePath, openCommand, iconPath);
                return true;
            }
            catch
            {
                // Writing under HKLM requires elevation; ignore so a failure for one root
                // (or one already handled by another instance) does not abort the others.
                return false;
            }
        }

        private static bool IsRegistrationCurrent(RegistryKey root, string classesPath, string openCommand, string iconPath)
        {
            // Read-only probe so a current HKLM registration never needs write access.
            using (var classesRoot = root.OpenSubKey(classesPath))
            {
                if (classesRoot == null)
                    return false;

                using (var extensionKey = classesRoot.OpenSubKey(Extension))
                {
                    if (extensionKey == null ||
                        !string.Equals(extensionKey.GetValue(string.Empty) as string, ProgId, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                using (var openCommandKey = classesRoot.OpenSubKey($@"{ProgId}\shell\open\command"))
                {
                    if (openCommandKey == null ||
                        !string.Equals(openCommandKey.GetValue(string.Empty) as string, openCommand, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                using (var defaultIconKey = classesRoot.OpenSubKey($@"{ProgId}\DefaultIcon"))
                {
                    if (defaultIconKey == null ||
                        !string.Equals(defaultIconKey.GetValue(string.Empty) as string, iconPath, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            return true;
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
                    applicationsKey.SetValue("FriendlyAppName", "NVIDIA PROFILE INSPECTOR", RegistryValueKind.String);

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
