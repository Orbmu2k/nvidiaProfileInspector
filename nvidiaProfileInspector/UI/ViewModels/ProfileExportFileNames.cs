namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;
    using System;
    using System.Globalization;

    public static class ProfileExportFileNames
    {
        public static string ForCurrentProfile(string profileName)
        {
            return CreateTimestampedName(profileName ?? "global_profile", "nip");
        }

        public static string ForSelectedProfiles()
        {
            return CreateTimestampedName("profiles", "nip");
        }

        public static string ForAllCustomizedProfiles()
        {
            return CreateTimestampedName("all_profiles", "nip");
        }

        public static string ForNvidiaTextProfiles()
        {
            return CreateTimestampedName("nvidia_profiles", "txt");
        }

        private static string CreateTimestampedName(string prefix, string extension)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}_{2:yyyyMMdd_HHmmss}.{3}",
                prefix,
                DrsSettingsServiceBase.DriverVersion.ToString("0.00", CultureInfo.InvariantCulture),
                DateTime.Now,
                extension);
        }
    }
}
