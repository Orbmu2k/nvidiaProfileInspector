using System.IO;
using nspector.Native.WINAPI;

namespace nspector.Common.Helper
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
                    string[] splitLine = line.Split('=');
                    if (splitLine.Length > 0)
                    {
                        return splitLine[1];
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
            if (shellLink.Arguments.StartsWith(SteamAppResolver.SteamUrlPattern))
            {
                var resolver = new SteamAppResolver();
                return resolver.ResolveExeFromSteamUrl(shellLink.Arguments);
            }

            var targetInfo = new FileInfo(shellLink.Target);
            if (targetInfo.Name.ToLowerInvariant() == SteamAppResolver.SteamExeName)
            {
                if (shellLink.Arguments.Contains(SteamAppResolver.SteamArgumentPattern))
                {
                    var resolver = new SteamAppResolver();
                    return resolver.ResolveExeFromSteamArguments(shellLink.Arguments);
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
            if (url.StartsWith(SteamAppResolver.SteamUrlPattern))
            {
                var resolver = new SteamAppResolver();
                return resolver.ResolveExeFromSteamUrl(url);
            }
            return "";
        }

    }
}
