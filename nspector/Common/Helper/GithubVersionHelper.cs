using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nspector.Common.Helper
{
    public static class GithubVersionHelper
    {
        // Check latest release info (ignores pre-release versions)
        private const string _repoUrl = "https://api.github.com/repos/Orbmu2k/nvidiaProfileInspector/releases/latest";

        public static async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "nvidiaProfileInspector/" + currentVersion.ToString());

                var response = await httpClient.GetAsync(_repoUrl);
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();

                var tagName = ExtractJsonString(content, "tag_name");

                if (string.IsNullOrEmpty(tagName))
                    return false;

                var versionString = tagName.TrimStart('v').Trim();

                if (Version.TryParse(versionString, out Version latestVersion))
                {
                    return latestVersion > currentVersion;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static string ExtractJsonString(string json, string fieldName)
        {
            var pattern = $"\"{fieldName}\"\\s*:\\s*\"([^\"\\\\]*(\\\\.[^\"\\\\]*)*)\"";
            var match = Regex.Match(json, pattern);

            if (match.Success)
            {
                var value = match.Groups[1].Value;
                value = value.Replace("\\\"", "\"");
                value = value.Replace("\\\\", "\\");
                value = value.Replace("\\n", "\n");
                value = value.Replace("\\r", "\r");
                value = value.Replace("\\t", "\t");
                return value;
            }

            return null;
        }
    }
}