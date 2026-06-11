using System;
using nvidiaProfileInspector.Localization;

namespace nvidiaProfileInspector.Common.Updates
{
    public enum UpdateChannel
    {
        Release,
        Prerelease
    }

    public static class UpdateChannelFormatter
    {
        public static UpdateChannel Parse(string value)
        {
            return string.Equals(value, "Prerelease", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Pre-release", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, UIStrings.UpdateChannelPrerelease, StringComparison.CurrentCultureIgnoreCase)
                ? UpdateChannel.Prerelease
                : UpdateChannel.Release;
        }

        public static string ToSettingsValue(UpdateChannel channel)
        {
            return channel == UpdateChannel.Prerelease ? "Prerelease" : "Release";
        }

        public static string ToDisplayName(UpdateChannel channel)
        {
            return channel == UpdateChannel.Prerelease
                ? UIStrings.UpdateChannelPrerelease
                : UIStrings.UpdateChannelRelease;
        }
    }
}
