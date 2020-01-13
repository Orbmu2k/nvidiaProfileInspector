using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace nspector.Common.Helper
{
    public class SteamAppResolver
    {

        public const string SteamExeName = "steam.exe";
        public const string SteamUrlPattern = "steam://rungameid/";
        public const string SteamArgumentPattern = "-applaunch";

        private byte[] _appinfoBytes;

        public SteamAppResolver()
        {
            var appInfoLocation = GetSteamAppInfoLocation();
            if (File.Exists(appInfoLocation))
                _appinfoBytes = File.ReadAllBytes(appInfoLocation);
            else
                _appinfoBytes = null;
        }

        private string GetSteamAppInfoLocation()
        {
            var reg = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam", false);

            if (reg != null)
            {
                string steamPath = (string)reg.GetValue("SteamPath", null);
                if (steamPath != null)
                    return Path.Combine(steamPath, @"appcache\appinfo.vdf");
            }

            return "";
        }

        public string ResolveExeFromSteamUrl(string url)
        {
            if (url.StartsWith(SteamUrlPattern))
            {
                var appIdStr = url.Substring(SteamUrlPattern.Length);
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
            if (arguments.Contains(SteamArgumentPattern))
            {
                var rxRungame = new Regex(SteamArgumentPattern + @"\s+(?<appid>\d+)");
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
            if (_appinfoBytes == null)
                return new List<string>();

            var bid = BitConverter.GetBytes(appid);
            int offset = 0;

            var appidPattern = new byte[] { 0x08, bid[0], bid[1], bid[2], bid[3] };
            var launchPattern = new byte[] { 0x00, 0x6C, 0x61, 0x75, 0x6E, 0x63, 0x68, 0x00 };

            var appidOffset = FindOffset(_appinfoBytes, appidPattern, offset);
            if (appidOffset == -1)
                return new List<string>();
            else
                offset = appidOffset + appidPattern.Length;

            var launchOffset = FindOffset(_appinfoBytes, launchPattern, offset);
            if (launchOffset == -1)
                return new List<string>();
            else
                offset = launchOffset;

            var executables = new List<string>();
            FindExecutables(_appinfoBytes, ref offset, ref executables);
            return executables;
        }


        private void FindExecutables(byte[] bytes, ref int offset, ref List<string> executables)
        {
            while (true)
            {
                var valueType = ReadByte(bytes, ref offset);
                if (valueType == 0x08)
                {
                    break;
                }

                var valueName = ReadCString(bytes, ref offset);
                var valueString = "";
                switch (valueType)
                {
                    case 0:
                        {
                            FindExecutables(bytes, ref offset, ref executables);
                            break;
                        }
                    case 1:
                        {
                            valueString = ReadCString(bytes, ref offset);

                            if (valueName == "executable" && valueString.EndsWith(".exe"))
                            {
                                executables.Add(valueString);
                            }

                            break;
                        }
                    case 2:
                        {
                            offset += 4;
                            break;
                        }

                    case 7:
                        {
                            offset += 8;
                            break;
                        }
                    default: break;
                }
            }
        }

        private static int FindOffset(byte[] bytes, byte[] pattern, int offset = 0, byte? wildcard = null)
        {
            for (int i = offset; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch == true; j++)
                    {
                        if (bytes[i + j] != pattern[j] && ((wildcard.HasValue && wildcard != pattern[j]) || !wildcard.HasValue))
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

        private static byte ReadByte(byte[] bytes, ref int offset)
        {
            offset += 1;
            return bytes[offset - 1];
        }

        private static string ReadCString(byte[] bytes, ref int offset)
        {
            var tmpOffset = offset;
            while (bytes[tmpOffset] != 0)
            {
                tmpOffset++;
            }

            var start = offset;
            var length = tmpOffset - offset;
            offset += length + 1;

            return Encoding.UTF8.GetString(bytes, start, length);

        }

    }
}
