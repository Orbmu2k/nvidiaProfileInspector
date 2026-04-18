using System;

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
                ? UpdateChannel.Prerelease
                : UpdateChannel.Release;
        }

        public static string ToSettingsValue(UpdateChannel channel)
        {
            return channel == UpdateChannel.Prerelease ? "Prerelease" : "Release";
        }

        public static string ToDisplayName(UpdateChannel channel)
        {
            return channel == UpdateChannel.Prerelease ? "Pre-release" : "Release";
        }
    }
}
