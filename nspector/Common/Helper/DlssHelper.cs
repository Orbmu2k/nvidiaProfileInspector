using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace nspector.Common.Helper
{
    public class IniParser
    {
        public Dictionary<string, Dictionary<string, string>> Data { get; } = new();

        public void Load(string filePath)
        {
            using var reader = new StreamReader(filePath);
            string? line;
            string? currentSection = null;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // Skip empty lines and comments
                if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                // Section
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2).Trim();
                    if (!Data.ContainsKey(currentSection))
                        Data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                // Key=Value
                else if (currentSection != null && line.Contains('='))
                {
                    int idx = line.IndexOf('=');
                    var key = line.Substring(0, idx).Trim();
                    var value = line.Substring(idx + 1).Trim();
                    Data[currentSection][key] = value;
                }
            }
        }

        public string? GetValue(string section, string key)
        {
            if (Data.TryGetValue(section, out var sectionDict) &&
                sectionDict.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        public List<string> GetSections()
        {
            return Data.Keys.ToList();
        }
    }

    public static class DlssHelper
    {
        private static Dictionary<string, Version> _ngxVersions = FetchVersions();

        // Fetches latest versions installed in C:\ProgramData\NVIDIA\NGX\models\ folder
        private static Dictionary<string, Version> FetchVersions()
        {
            Dictionary<string, Version> versions = new Dictionary<string, Version>();

            try
            {
                string ngxDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"NVIDIA\NGX\models\");
                string ngxConfigPath = Path.Combine(ngxDataPath, "nvngx_config.txt");
                if (!File.Exists(ngxConfigPath))
                    return versions;

                var ini = new IniParser();
                ini.Load(ngxConfigPath);

                foreach (string section in ini.GetSections())
                {
                    string versionStr = ini.GetValue(section, "app_E658700");
                    if (string.IsNullOrEmpty(versionStr))
                        continue;

                    Version ver = new Version(versionStr.Trim());

                    versions[section] = ver;
                }
            }
            catch
            {
                versions.Clear();
            }

            return versions;
        }

        public static string GetSnippetLatestVersion(string snippet)
        {
            if (!_ngxVersions.ContainsKey(snippet))
                return "unknown";
            return "v" + _ngxVersions[snippet].ToString();
        }

        public static string ReplaceDlssVersions(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Contains("${DlssVersion}"))
                str = str.Replace("${DlssVersion}", DlssHelper.GetSnippetLatestVersion("dlss").ToString());

            if (str.Contains("${DlssgVersion}"))
                str = str.Replace("${DlssgVersion}", DlssHelper.GetSnippetLatestVersion("dlssg").ToString());

            if (str.Contains("${DlssdVersion}"))
                str = str.Replace("${DlssdVersion}", DlssHelper.GetSnippetLatestVersion("dlssd").ToString());

            return str;
        }
    }
}
