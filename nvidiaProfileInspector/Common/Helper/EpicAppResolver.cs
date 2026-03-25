using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace nvidiaProfileInspector.Common.Helper
{
    public class EpicAppResolver
    {
        public const string EpicUrlPattern = "com.epicgames.launcher://apps/";

        public string ResolveExeFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url) ||
                !url.StartsWith(EpicUrlPattern, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var targetIds = ParseTargetIds(url);
            if (targetIds.Count == 0)
            {
                return string.Empty;
            }

            foreach (var manifestPath in GetManifestPaths())
            {
                var executable = TryResolveExecutableFromManifest(manifestPath, targetIds);
                if (!string.IsNullOrEmpty(executable))
                {
                    return executable;
                }
            }

            return string.Empty;
        }

        private static List<string> ParseTargetIds(string url)
        {
            var decodedUrl = Uri.UnescapeDataString(url);
            var ids = new List<string>();

            var appSegmentStart = decodedUrl.IndexOf(EpicUrlPattern, StringComparison.OrdinalIgnoreCase);
            if (appSegmentStart < 0)
            {
                return ids;
            }

            var appSegment = decodedUrl.Substring(appSegmentStart + EpicUrlPattern.Length);
            var queryIndex = appSegment.IndexOf('?');
            if (queryIndex >= 0)
            {
                appSegment = appSegment.Substring(0, queryIndex);
            }

            foreach (var value in appSegment.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!ids.Contains(value, StringComparer.OrdinalIgnoreCase))
                {
                    ids.Add(value);
                }
            }

            return ids;
        }

        private static IEnumerable<string> GetManifestPaths()
        {
            foreach (var directory in GetManifestDirectories())
            {
                if (!Directory.Exists(directory))
                {
                    continue;
                }

                IEnumerable<string> paths;
                try
                {
                    paths = Directory.EnumerateFiles(directory, "*.item", SearchOption.TopDirectoryOnly)
                        .Concat(Directory.EnumerateFiles(directory, "*.manifest", SearchOption.TopDirectoryOnly));
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (var path in paths)
                {
                    yield return path;
                }
            }
        }

        private static IEnumerable<string> GetManifestDirectories()
        {
            var directories = new List<string>();
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (!string.IsNullOrEmpty(programData))
            {
                directories.Add(Path.Combine(programData, "Epic", "EpicGamesLauncher", "Data", "Manifests"));
            }

            var launcherPath = TryGetLauncherInstallPath();
            if (!string.IsNullOrEmpty(launcherPath))
            {
                directories.Add(Path.Combine(launcherPath, "Data", "Manifests"));
            }

            return directories.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string TryGetLauncherInstallPath()
        {
            foreach (var registryValue in new[]
            {
                Tuple.Create(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Epic Games\EpicGamesLauncher", "AppDataPath"),
                Tuple.Create(Registry.LocalMachine, @"SOFTWARE\Epic Games\EpicGamesLauncher", "AppDataPath"),
            })
            {
                var key = registryValue.Item1.OpenSubKey(registryValue.Item2, false);
                if (key == null)
                {
                    continue;
                }

                var value = key.GetValue(registryValue.Item3, null) as string;
                if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string TryResolveExecutableFromManifest(string manifestPath, List<string> targetIds)
        {
            string content;
            try
            {
                content = File.ReadAllText(manifestPath);
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }

            if (!ManifestMatches(content, targetIds))
            {
                return string.Empty;
            }

            var launchExecutable = ExtractJsonValue(content, "LaunchExecutable");
            var installLocation = ExtractJsonValue(content, "InstallLocation");
            if (string.IsNullOrEmpty(launchExecutable))
            {
                return string.Empty;
            }

            var normalizedExecutable = launchExecutable.Replace('/', '\\');
            if (!Path.IsPathRooted(normalizedExecutable) && !string.IsNullOrEmpty(installLocation))
            {
                normalizedExecutable = Path.Combine(installLocation, normalizedExecutable);
            }

            return Path.GetFileName(normalizedExecutable);
        }

        private static bool ManifestMatches(string content, List<string> targetIds)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            var manifestIds = new[]
            {
                ExtractJsonValue(content, "AppName"),
                ExtractJsonValue(content, "ArtifactId"),
                ExtractJsonValue(content, "CatalogNamespace"),
                ExtractJsonValue(content, "CatalogItemId"),
                ExtractJsonValue(content, "MainGameAppName"),
                ExtractJsonValue(content, "MandatoryAppFolderName"),
            };

            return targetIds.Any(targetId =>
                manifestIds.Any(manifestId => !string.IsNullOrEmpty(manifestId) &&
                    string.Equals(targetId, manifestId, StringComparison.OrdinalIgnoreCase)));
        }

        private static string ExtractJsonValue(string content, string propertyName)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                content,
                "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success ? UnescapeJsonString(match.Groups["value"].Value) : string.Empty;
        }

        private static string UnescapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != '\\' || i == value.Length - 1)
                {
                    builder.Append(value[i]);
                    continue;
                }

                i++;
                switch (value[i])
                {
                    case '\\':
                    case '/':
                    case '"':
                        builder.Append(value[i]);
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'u':
                        if (i + 4 < value.Length)
                        {
                            var hex = value.Substring(i + 1, 4);
                            int codePoint;
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out codePoint))
                            {
                                builder.Append((char)codePoint);
                                i += 4;
                            }
                        }
                        break;
                    default:
                        builder.Append(value[i]);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
