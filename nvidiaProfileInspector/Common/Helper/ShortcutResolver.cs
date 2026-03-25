using nvidiaProfileInspector.Native.WINAPI;
using System;
using System.IO;

namespace nvidiaProfileInspector.Common.Helper
{
    public class ShortcutResolver
    {

        public static string GetUrlFromInternetShortcut(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("URL="))
                {
                    var separatorIndex = line.IndexOf('=');
                    if (separatorIndex >= 0 && separatorIndex < line.Length - 1)
                    {
                        return line.Substring(separatorIndex + 1);
                    }
                }
            }
            return "";
        }

        public static string ResolveExecuteable(string filename, out string profileName)
        {
            var fileInfo = new FileInfo(filename);
            profileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);

            try
            {
                switch (fileInfo.Extension.ToLowerInvariant())
                {
                    case ".lnk": return ResolveFromShellLinkFile(fileInfo.FullName);
                    case ".url": return ResolveFromUrlFile(fileInfo.FullName);
                    case ".exe": return fileInfo.Name;
                    default: return "";
                }
            }
            catch
            {
                return "";
            }
        }

        private static string ResolveFromShellLinkFile(string filename)
        {
            var shellLink = new ShellLink(filename);
            var arguments = shellLink.Arguments ?? string.Empty;
            if (arguments.StartsWith(SteamAppResolver.SteamUrlPattern, StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new SteamAppResolver();
                return resolver.ResolveExeFromSteamUrl(arguments);
            }

            if (arguments.StartsWith(EpicAppResolver.EpicUrlPattern, StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new EpicAppResolver();
                return resolver.ResolveExeFromUrl(arguments);
            }

            var targetInfo = new FileInfo(shellLink.Target);
            if (targetInfo.Name.ToLowerInvariant() == SteamAppResolver.SteamExeName)
            {
                if (arguments.IndexOf(SteamAppResolver.SteamArgumentPattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var resolver = new SteamAppResolver();
                    return resolver.ResolveExeFromSteamArguments(arguments);
                }
            }

            if (targetInfo.Extension.ToLowerInvariant().Equals(".exe"))
            {
                return targetInfo.Name;
            }
            return "";
        }

        private static string ResolveFromUrlFile(string filename)
        {
            var url = GetUrlFromInternetShortcut(filename);
            if (url.StartsWith(SteamAppResolver.SteamUrlPattern, StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new SteamAppResolver();
                return resolver.ResolveExeFromSteamUrl(url);
            }

            if (url.StartsWith(EpicAppResolver.EpicUrlPattern, StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new EpicAppResolver();
                return resolver.ResolveExeFromUrl(url);
            }

            return "";
        }

    }
}
