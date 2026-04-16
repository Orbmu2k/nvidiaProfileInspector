using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nvidiaProfileInspector.Common.Helper
{
    public static class GithubVersionHelper
    {
        private const string _releasesUrl = "https://api.github.com/repos/Orbmu2k/nvidiaProfileInspector/releases";

        public static async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "nvidiaProfileInspector/" + currentVersion.ToString());

                var response = await httpClient.GetAsync(_releasesUrl);
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();

                var releases = ParseReleases(content);

                // Check if the current version matches a pre-release tag
                bool currentIsPreRelease = false;
                bool foundRelease = false;
                foreach (var release in releases)
                {
                    if (release.Version != null && release.Version.Equals(currentVersion))
                    {
                        currentIsPreRelease = release.IsPreRelease;
                        foundRelease = true;
                        break;
                    }
                }

                if (!foundRelease)
                {
                    // Failed to find this version as a release, treat it as a pre-release version.
                    currentIsPreRelease = true;
                }

                // Find the latest version; include pre-releases only if current is a pre-release
                Version latestVersion = null;
                foreach (var release in releases)
                {
                    if (release.Version == null)
                        continue;
                    if (!currentIsPreRelease && release.IsPreRelease)
                        continue;
                    if (latestVersion == null || release.Version > latestVersion)
                        latestVersion = release.Version;
                }

                if (latestVersion != null)
                    return latestVersion > currentVersion;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static List<(Version Version, bool IsPreRelease)> ParseReleases(string json)
        {
            var releases = new List<(Version Version, bool IsPreRelease)>();

            var tagMatches = Regex.Matches(json, "\"tag_name\"\\s*:\\s*\"([^\"\\\\]*(\\\\.[^\"\\\\]*)*)\"");
            var prereleaseMatches = Regex.Matches(json, "\"prerelease\"\\s*:\\s*(true|false)");

            var count = Math.Min(tagMatches.Count, prereleaseMatches.Count);
            for (int i = 0; i < count; i++)
            {
                var tagName = tagMatches[i].Groups[1].Value.TrimStart('v').Trim();
                var isPreRelease = prereleaseMatches[i].Groups[1].Value == "true";

                if (Version.TryParse(tagName, out Version version))
                {
                    // Normalize 3-part versions (e.g. 3.0.0) to 4-part (3.0.0.0)
                    // so they compare correctly with the assembly version
                    if (version.Revision == -1)
                        version = new Version(version.Major, version.Minor, version.Build, 0);

                    releases.Add((version, isPreRelease));
                }
            }

            return releases;
        }
    }
}