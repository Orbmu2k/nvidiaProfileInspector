namespace nspector.Common.Helper;

public class ShortcutResolver
{
    public static string GetUrlFromInternetShortcut(string filePath)
    {
        var lines=System.IO.File.ReadAllLines(filePath);
        foreach(var line in lines)
        {
            if(line.StartsWith("URL="))
            {
                var splitLine=line.Split('=');
                if(splitLine.Length>0)
                {
                    return splitLine[1];
                }
            }
        }

        return"";
    }

    public static string ResolveExecuteable(string filename,out string profileName)
    {
        var fileInfo=new System.IO.FileInfo(filename);
        profileName=fileInfo.Name.Substring(0,fileInfo.Name.Length-fileInfo.Extension.Length);

        try
        {
            switch(fileInfo.Extension.ToLower())
            {
                case".lnk":
                    return ShortcutResolver.ResolveFromShellLinkFile(fileInfo.FullName);
                case".url":
                    return ShortcutResolver.ResolveFromUrlFile(fileInfo.FullName);
                case".exe":
                    return fileInfo.Name;
                default:
                    return"";
            }
        }
        catch
        {
            return"";
        }
    }

    static string ResolveFromShellLinkFile(string filename)
    {
        var shellLink=new nspector.Native.WINAPI.ShellLink(filename);
        if(shellLink.Arguments.StartsWith(SteamAppResolver.SteamUrlPattern))
        {
            var resolver=new SteamAppResolver();
            return resolver.ResolveExeFromSteamUrl(shellLink.Arguments);
        }

        var targetInfo=new System.IO.FileInfo(shellLink.Target);
        if(targetInfo.Name.ToLower()==SteamAppResolver.SteamExeName)
        {
            if(shellLink.Arguments.Contains(SteamAppResolver.SteamArgumentPattern))
            {
                var resolver=new SteamAppResolver();
                return resolver.ResolveExeFromSteamArguments(shellLink.Arguments);
            }
        }

        if(targetInfo.Extension.ToLower().Equals(".exe"))
        {
            return targetInfo.Name;
        }

        return"";
    }

    static string ResolveFromUrlFile(string filename)
    {
        var url=ShortcutResolver.GetUrlFromInternetShortcut(filename);
        if(url.StartsWith(SteamAppResolver.SteamUrlPattern))
        {
            var resolver=new SteamAppResolver();
            return resolver.ResolveExeFromSteamUrl(url);
        }

        return"";
    }
}