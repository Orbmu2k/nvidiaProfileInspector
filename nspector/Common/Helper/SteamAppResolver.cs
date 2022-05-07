namespace nspector.Common.Helper;

public class SteamAppResolver
{
    public const string SteamExeName        ="steam.exe";
    public const string SteamUrlPattern     ="steam://rungameid/";
    public const string SteamArgumentPattern="-applaunch";

    readonly byte[] _appinfoBytes;

    public SteamAppResolver()
    {
        var appInfoLocation=this.GetSteamAppInfoLocation();
        if(System.IO.File.Exists(appInfoLocation))
        {
            this._appinfoBytes=System.IO.File.ReadAllBytes(appInfoLocation);
        }
        else
        {
            this._appinfoBytes=null;
        }
    }

    string GetSteamAppInfoLocation()
    {
        var reg=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam",false);

        if(reg!=null)
        {
            var steamPath=(string)reg.GetValue("SteamPath",null);
            if(steamPath!=null)
            {
                return System.IO.Path.Combine(steamPath,@"appcache\appinfo.vdf");
            }
        }

        return"";
    }

    public string ResolveExeFromSteamUrl(string url)
    {
        if(url.StartsWith(SteamAppResolver.SteamUrlPattern))
        {
            var appIdStr=url.Substring(SteamAppResolver.SteamUrlPattern.Length);
            var appid   =0;
            if(int.TryParse(appIdStr,out appid))
            {
                return this.FindCommonExecutableForApp(appid);
            }
        }

        return"";
    }

    public string ResolveExeFromSteamArguments(string arguments)
    {
        if(arguments.Contains(SteamAppResolver.SteamArgumentPattern))
        {
            var rxRungame
                =new System.Text.RegularExpressions.Regex(SteamAppResolver.SteamArgumentPattern+@"\s+(?<appid>\d+)");
            foreach(System.Text.RegularExpressions.Match m in rxRungame.Matches(arguments))
            {
                var appIdStr=m.Result("${appid}");
                var appid   =0;
                if(int.TryParse(appIdStr,out appid))
                {
                    return this.FindCommonExecutableForApp(appid);
                }
            }
        }

        return"";
    }

    string FindCommonExecutableForApp(int appid)
    {
        var apps=this.FindAllExecutablesForApp(appid);
        if(apps.Count>0)
        {
            return new System.IO.FileInfo(apps[0]).Name;
        }

        return"";
    }

    System.Collections.Generic.List<string> FindAllExecutablesForApp(int appid)
    {
        if(this._appinfoBytes==null)
        {
            return new System.Collections.Generic.List<string>();
        }

        var bid   =System.BitConverter.GetBytes(appid);
        var offset=0;

        var appidPattern=new byte[]
        {
            0x08,bid[0],bid[1],bid[2],bid[3],
        };
        var launchPattern=new byte[]
        {
            0x00,0x6C,0x61,0x75,0x6E,0x63,0x68,0x00,
        };

        var appidOffset=SteamAppResolver.FindOffset(this._appinfoBytes,appidPattern,offset);
        if(appidOffset==-1)
        {
            return new System.Collections.Generic.List<string>();
        }

        offset=appidOffset+appidPattern.Length;

        var launchOffset=SteamAppResolver.FindOffset(this._appinfoBytes,launchPattern,offset);
        if(launchOffset==-1)
        {
            return new System.Collections.Generic.List<string>();
        }

        offset=launchOffset;

        var executables=new System.Collections.Generic.List<string>();
        this.FindExecutables(this._appinfoBytes,ref offset,ref executables);
        return executables;
    }


    void FindExecutables(byte[] bytes,ref int offset,ref System.Collections.Generic.List<string> executables)
    {
        while(true)
        {
            var valueType=SteamAppResolver.ReadByte(bytes,ref offset);
            if(valueType==0x08)
            {
                break;
            }

            var valueName  =SteamAppResolver.ReadCString(bytes,ref offset);
            var valueString="";
            switch(valueType)
            {
                case 0:
                {
                    this.FindExecutables(bytes,ref offset,ref executables);
                    break;
                }
                case 1:
                {
                    valueString=SteamAppResolver.ReadCString(bytes,ref offset);

                    if(valueName=="executable"&&valueString.EndsWith(".exe"))
                    {
                        executables.Add(valueString);
                    }

                    break;
                }
                case 2:
                {
                    offset+=4;
                    break;
                }

                case 7:
                {
                    offset+=8;
                    break;
                }
            }
        }
    }

    static int FindOffset(byte[] bytes,byte[] pattern,int offset=0,byte? wildcard=null)
    {
        for(var i=offset;i<bytes.Length;i++)
        {
            if(pattern[0]==bytes[i]&&bytes.Length-i>=pattern.Length)
            {
                var ismatch=true;
                for(var j=1;j<pattern.Length&&ismatch;j++)
                {
                    if(bytes[i+j]!=pattern[j]&&
                        (wildcard.HasValue&&wildcard!=pattern[j]||!wildcard.HasValue))
                    {
                        ismatch=false;
                        break;
                    }
                }

                if(ismatch)
                {
                    return i;
                }
            }
        }

        return-1;
    }

    static byte ReadByte(byte[] bytes,ref int offset)
    {
        offset+=1;
        return bytes[offset-1];
    }

    static string ReadCString(byte[] bytes,ref int offset)
    {
        var tmpOffset=offset;
        while(bytes[tmpOffset]!=0)
        {
            tmpOffset++;
        }

        var start =offset;
        var length=tmpOffset-offset;
        offset+=length+1;

        return System.Text.Encoding.UTF8.GetString(bytes,start,length);
    }
}