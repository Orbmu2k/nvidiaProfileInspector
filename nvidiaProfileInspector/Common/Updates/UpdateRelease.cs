using System;

namespace nvidiaProfileInspector.Common.Updates
{
    public sealed class UpdateRelease
    {
        public UpdateRelease(Version version, bool isPreRelease, string pageUrl, UpdateAsset asset)
        {
            Version = version;
            IsPreRelease = isPreRelease;
            PageUrl = pageUrl;
            Asset = asset;
        }

        public Version Version { get; }
        public bool IsPreRelease { get; }
        public string PageUrl { get; }
        public UpdateAsset Asset { get; }
        public bool IsInstallable => Asset != null && !string.IsNullOrWhiteSpace(Asset.DownloadUrl);
        public UpdateChannel Channel => IsPreRelease ? UpdateChannel.Prerelease : UpdateChannel.Release;
        public string DisplayText => $"{Version}{(IsPreRelease ? " Pre-release" : " Release")}";
    }
}
