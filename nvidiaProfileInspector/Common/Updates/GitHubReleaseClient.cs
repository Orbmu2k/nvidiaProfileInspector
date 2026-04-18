using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace nvidiaProfileInspector.Common.Updates
{
    public sealed class GitHubReleaseClient
    {
        private const string ReleasesUrl = "https://api.github.com/repos/Orbmu2k/nvidiaProfileInspector/releases";
        private readonly string _userAgent;

        public GitHubReleaseClient(Version currentVersion)
        {
            _userAgent = "nvidiaProfileInspector/" + currentVersion;
        }

        public async Task<IReadOnlyList<UpdateRelease>> GetReleasesAsync()
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);

            var response = await httpClient.GetAsync(ReleasesUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var serializer = new DataContractJsonSerializer(typeof(List<GitHubReleaseDto>));
            var releases = serializer.ReadObject(stream) as List<GitHubReleaseDto> ?? new List<GitHubReleaseDto>();

            return releases
                .Where(release => !release.draft)
                .Select(ToUpdateRelease)
                .Where(release => release != null)
                .OrderByDescending(release => release.Version)
                .ToList();
        }

        private static UpdateRelease ToUpdateRelease(GitHubReleaseDto release)
        {
            var tagName = (release.tag_name ?? "").TrimStart('v', 'V').Trim();
            if (!Version.TryParse(tagName, out var version))
                return null;

            var asset = SelectUpdateAsset(release.assets);
            return new UpdateRelease(NormalizeVersion(version), release.prerelease, release.html_url, asset);
        }

        private static Version NormalizeVersion(Version version)
        {
            if (version.Build == -1)
                return new Version(version.Major, version.Minor, 0, 0);

            if (version.Revision == -1)
                return new Version(version.Major, version.Minor, version.Build, 0);

            return version;
        }

        private static UpdateAsset SelectUpdateAsset(List<GitHubReleaseAssetDto> assets)
        {
            if (assets == null || assets.Count == 0)
                return null;

            return assets
                .Select(ToUpdateAsset)
                .Where(asset => asset != null)
                .OrderByDescending(asset => asset.PackageType == UpdatePackageType.Zip)
                .ThenByDescending(asset => (asset.Name ?? "").IndexOf("nvidiaProfileInspector", StringComparison.OrdinalIgnoreCase) >= 0)
                .FirstOrDefault();
        }

        private static UpdateAsset ToUpdateAsset(GitHubReleaseAssetDto asset)
        {
            var extension = Path.GetExtension(asset?.name ?? "");
            if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
                return new UpdateAsset(asset.name, asset.browser_download_url, UpdatePackageType.Zip);

            if (string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
                return new UpdateAsset(asset.name, asset.browser_download_url, UpdatePackageType.Exe);

            return null;
        }

        [DataContract]
        private class GitHubReleaseDto
        {
            [DataMember]
            public string tag_name { get; set; }

            [DataMember]
            public bool prerelease { get; set; }

            [DataMember]
            public bool draft { get; set; }

            [DataMember]
            public string html_url { get; set; }

            [DataMember]
            public List<GitHubReleaseAssetDto> assets { get; set; }
        }

        [DataContract]
        private class GitHubReleaseAssetDto
        {
            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string browser_download_url { get; set; }
        }
    }
}
