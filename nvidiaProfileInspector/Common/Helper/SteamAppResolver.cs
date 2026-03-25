using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace nvidiaProfileInspector.Common.Helper
{
    public class SteamAppResolver
    {

        public const string SteamExeName = "steam.exe";
        public const string SteamUrlPattern = "steam://rungameid/";
        public const string SteamArgumentPattern = "-applaunch";

        private byte[] _appinfoBytes;
        private readonly string _steamPath;

        public SteamAppResolver()
        {
            _steamPath = GetSteamInstallPath();
            var appInfoLocation = string.IsNullOrEmpty(_steamPath) ? string.Empty : Path.Combine(_steamPath, @"appcache\appinfo.vdf");
            if (!File.Exists(appInfoLocation))
            {
                _appinfoBytes = null;
                return;
            }

            try
            {
                _appinfoBytes = File.ReadAllBytes(appInfoLocation);
            }
            catch (IOException)
            {
                _appinfoBytes = null;
            }
            catch (UnauthorizedAccessException)
            {
                _appinfoBytes = null;
            }
        }

        private string GetSteamInstallPath()
        {
            foreach (var registryValue in new[]
            {
                Tuple.Create(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath"),
                Tuple.Create(Registry.CurrentUser, @"Software\Valve\Steam", "SteamExe"),
                Tuple.Create(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath"),
                Tuple.Create(Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath"),
            })
            {
                var path = TryGetRegistrySteamPath(registryValue.Item1, registryValue.Item2, registryValue.Item3);
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    return path;
                }
            }

            foreach (var candidate in new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"),
            })
            {
                if (!string.IsNullOrEmpty(candidate) && Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }

        public string ResolveExeFromSteamUrl(string url)
        {
            if (!string.IsNullOrEmpty(url) &&
                url.StartsWith(SteamUrlPattern, StringComparison.OrdinalIgnoreCase))
            {
                var appIdStr = ExtractLeadingDigits(url.Substring(SteamUrlPattern.Length));
                int appid = 0;
                if (int.TryParse(appIdStr, out appid))
                {
                    return FindCommonExecutableForApp(appid);
                }
            }
            return "";
        }

        public string ResolveExeFromSteamArguments(string arguments)
        {
            if (!string.IsNullOrEmpty(arguments) &&
                arguments.IndexOf(SteamArgumentPattern, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var rxRungame = new Regex(Regex.Escape(SteamArgumentPattern) + @"\s+(?<appid>\d+)",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                foreach (Match m in rxRungame.Matches(arguments))
                {
                    var appIdStr = m.Result("${appid}");
                    int appid = 0;
                    if (int.TryParse(appIdStr, out appid))
                    {
                        return FindCommonExecutableForApp(appid);
                    }
                }

            }
            return "";
        }

        private string FindCommonExecutableForApp(int appid)
        {
            var apps = FindAllExecutablesForApp(appid);
            if (apps.Count > 0)
            {
                return new FileInfo(apps[0]).Name;
            }
            return "";
        }

        private List<string> FindAllExecutablesForApp(int appid)
        {
            var executables = FindExecutablesFromInstalledGame(appid);
            if (executables.Count > 0)
            {
                return executables;
            }

            return FindExecutablesFromAppInfo(appid);
        }

        private List<string> FindExecutablesFromInstalledGame(int appid)
        {
            var installDir = TryGetInstalledGameDirectory(appid);
            if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir))
            {
                return new List<string>();
            }

            try
            {
                var normalizedInstallDirName = NormalizeName(new DirectoryInfo(installDir).Name);
                return Directory.EnumerateFiles(installDir, "*.exe", SearchOption.AllDirectories)
                    .Where(IsCandidateGameExecutable)
                    .OrderBy(path => ScoreExecutable(path, installDir, normalizedInstallDirName))
                    .ThenBy(path => path.Length)
                    .ToList();
            }
            catch (IOException)
            {
                return new List<string>();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        private List<string> FindExecutablesFromAppInfo(int appid)
        {
            if (_appinfoBytes == null)
                return new List<string>();

            var bid = BitConverter.GetBytes(appid);
            int offset = 0;

            var appidPattern = new byte[] { 0x02, 0x61, 0x70, 0x70, 0x69, 0x64, 0x00, bid[0], bid[1], bid[2], bid[3] };
            var anyAppIdPattern = new byte[] { 0x02, 0x61, 0x70, 0x70, 0x69, 0x64, 0x00, 0xFF, 0xFF, 0xFF, 0xFF };
            var launchPattern = new byte[] { 0x00, 0x6C, 0x61, 0x75, 0x6E, 0x63, 0x68, 0x00 };

            var appidOffset = FindOffset(_appinfoBytes, appidPattern, offset);
            if (appidOffset == -1)
                return new List<string>();
            else
                offset = appidOffset + appidPattern.Length;

            var nextAppOffset = FindOffset(_appinfoBytes, anyAppIdPattern, offset, 0xFF);
            var searchEndOffset = nextAppOffset == -1 ? _appinfoBytes.Length : nextAppOffset;

            var launchOffset = FindOffset(_appinfoBytes, launchPattern, offset, null, searchEndOffset);
            if (launchOffset == -1)
                return new List<string>();
            else
                offset = launchOffset;

            var executables = new List<string>();
            TryFindExecutables(_appinfoBytes, ref offset, searchEndOffset, executables);
            return executables;
        }

        private string TryGetInstalledGameDirectory(int appid)
        {
            foreach (var libraryPath in GetSteamLibraryPaths())
            {
                var manifestPath = Path.Combine(libraryPath, "steamapps", $"appmanifest_{appid}.acf");
                if (!File.Exists(manifestPath))
                {
                    continue;
                }

                string manifestContent;
                try
                {
                    manifestContent = File.ReadAllText(manifestPath);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                var installDir = ExtractVdfValue(manifestContent, "installdir");
                if (string.IsNullOrEmpty(installDir))
                {
                    continue;
                }

                var candidate = Path.Combine(libraryPath, "steamapps", "common", installDir);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }

        private IEnumerable<string> GetSteamLibraryPaths()
        {
            if (string.IsNullOrEmpty(_steamPath))
            {
                return Enumerable.Empty<string>();
            }

            var libraries = new List<string> { _steamPath };
            var libraryFoldersPath = Path.Combine(_steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersPath))
            {
                return libraries;
            }

            try
            {
                var content = File.ReadAllText(libraryFoldersPath);
                foreach (Match match in Regex.Matches(content, "\"path\"\\s*\"(?<path>[^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    var path = match.Groups["path"].Value.Replace(@"\\", @"\");
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path) && !libraries.Contains(path, StringComparer.OrdinalIgnoreCase))
                    {
                        libraries.Add(path);
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return libraries;
        }

        private static string TryGetRegistrySteamPath(RegistryKey root, string subKey, string valueName)
        {
            var key = root.OpenSubKey(subKey, false);
            if (key == null)
            {
                return string.Empty;
            }

            var value = key.GetValue(valueName, null) as string;
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetDirectoryName(value) ?? string.Empty;
            }

            return value;
        }

        private static string ExtractVdfValue(string content, string key)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var match = Regex.Match(content, "\"" + Regex.Escape(key) + "\"\\s*\"(?<value>[^\"]*)\"",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return match.Success ? match.Groups["value"].Value : string.Empty;
        }

        private static bool IsCandidateGameExecutable(string path)
        {
            var fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var normalized = NormalizeName(fileName);
            var excludedFragments = new[]
            {
                "unins", "uninstall", "setup", "redist", "vc", "crash", "report", "launcherhelper",
                "eac", "easyanticheat", "battleye", "cefprocess", "unitycrashhandler", "notificationhelper"
            };

            return !excludedFragments.Any(normalized.Contains);
        }

        private static int ScoreExecutable(string path, string installDir, string normalizedInstallDirName)
        {
            var score = 0;
            var fileName = Path.GetFileNameWithoutExtension(path) ?? string.Empty;
            var normalizedFileName = NormalizeName(fileName);
            var relativePath = path.Substring(installDir.Length).TrimStart(Path.DirectorySeparatorChar);
            var depth = relativePath.Count(ch => ch == Path.DirectorySeparatorChar);

            score += depth * 10;

            if (normalizedFileName == normalizedInstallDirName)
            {
                score -= 50;
            }
            else if (!string.IsNullOrEmpty(normalizedInstallDirName) && normalizedFileName.Contains(normalizedInstallDirName))
            {
                score -= 20;
            }

            if (normalizedFileName.Contains("launcher"))
            {
                score += 20;
            }

            return score;
        }

        private bool TryFindExecutables(byte[] bytes, ref int offset, int endOffset, List<string> executables)
        {
            while (offset < endOffset)
            {
                byte valueType;
                if (!TryReadByte(bytes, ref offset, endOffset, out valueType))
                {
                    return false;
                }

                if (valueType == 0x08)
                {
                    return true;
                }

                string valueName;
                if (!TryReadCString(bytes, ref offset, endOffset, out valueName))
                {
                    return false;
                }

                switch (valueType)
                {
                    case 0:
                        {
                            if (!TryFindExecutables(bytes, ref offset, endOffset, executables))
                            {
                                return false;
                            }

                            break;
                        }
                    case 1:
                        {
                            string valueString;
                            if (!TryReadCString(bytes, ref offset, endOffset, out valueString))
                            {
                                return false;
                            }

                            if (valueName == "executable" && valueString.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                executables.Add(valueString);
                            }

                            break;
                        }
                    case 2:
                        {
                            if (offset > endOffset - 4)
                            {
                                return false;
                            }

                            offset += 4;
                            break;
                        }

                    case 7:
                        {
                            if (offset > endOffset - 8)
                            {
                                return false;
                            }

                            offset += 8;
                            break;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }

            return false;
        }

        private static int FindOffset(byte[] bytes, byte[] pattern, int offset = 0, byte? wildcard = null, int? endOffset = null)
        {
            var maxOffset = endOffset.GetValueOrDefault(bytes.Length);
            for (int i = offset; i < maxOffset; i++)
            {
                if (pattern[0] == bytes[i] && maxOffset - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch == true; j++)
                    {
                        if (wildcard.HasValue && pattern[j] == wildcard.Value)
                        {
                            continue;
                        }

                        if (bytes[i + j] != pattern[j])
                        {
                            ismatch = false;
                            break;
                        }
                    }
                    if (ismatch)
                        return i;

                }
            }
            return -1;
        }

        private static bool TryReadByte(byte[] bytes, ref int offset, int endOffset, out byte value)
        {
            value = 0;
            if (offset >= endOffset)
            {
                return false;
            }

            offset += 1;
            value = bytes[offset - 1];
            return true;
        }

        private static bool TryReadCString(byte[] bytes, ref int offset, int endOffset, out string value)
        {
            value = string.Empty;
            var tmpOffset = offset;
            while (tmpOffset < endOffset && bytes[tmpOffset] != 0)
            {
                tmpOffset++;
            }

            if (tmpOffset >= endOffset)
            {
                return false;
            }

            var start = offset;
            var length = tmpOffset - offset;
            offset += length + 1;

            value = Encoding.UTF8.GetString(bytes, start, length);
            return true;

        }

        private static string ExtractLeadingDigits(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var length = 0;
            while (length < value.Length && char.IsDigit(value[length]))
            {
                length++;
            }

            return length == 0 ? string.Empty : value.Substring(0, length);
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }

    }
}
