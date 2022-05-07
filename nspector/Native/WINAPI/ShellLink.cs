namespace nspector.Native.WINAPI;

class ShellLink:System.IDisposable
{
    IShellLinkA linkA;

    IShellLinkW linkW;

    internal ShellLink()
    {
        if(System.Environment.OSVersion.Platform==System.PlatformID.Win32NT)
        {
            this.linkW=(IShellLinkW)new CShellLink();
        }
        else
        {
            this.linkA=(IShellLinkA)new CShellLink();
        }
    }

    internal ShellLink(string linkFile)
        :this()
    {
        this.Open(linkFile);
    }

    internal string ShortCutFile
    {
        get;
        set;
    }="";

    internal string IconPath
    {
        get
        {
            var iconPath =new System.Text.StringBuilder(260,260);
            var iconIndex=0;
            if(this.linkA==null)
            {
                this.linkW.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }
            else
            {
                this.linkA.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }

            return iconPath.ToString();
        }
        set
        {
            var iconPath =new System.Text.StringBuilder(260,260);
            var iconIndex=0;
            if(this.linkA==null)
            {
                this.linkW.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }
            else
            {
                this.linkA.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }

            if(this.linkA==null)
            {
                this.linkW.SetIconLocation(value,iconIndex);
            }
            else
            {
                this.linkA.SetIconLocation(value,iconIndex);
            }
        }
    }

    internal int IconIndex
    {
        get
        {
            var iconPath =new System.Text.StringBuilder(260,260);
            var iconIndex=0;
            if(this.linkA==null)
            {
                this.linkW.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }
            else
            {
                this.linkA.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }

            return iconIndex;
        }
        set
        {
            var iconPath =new System.Text.StringBuilder(260,260);
            var iconIndex=0;
            if(this.linkA==null)
            {
                this.linkW.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }
            else
            {
                this.linkA.GetIconLocation(iconPath,iconPath.Capacity,out
                    iconIndex);
            }

            if(this.linkA==null)
            {
                this.linkW.SetIconLocation(iconPath.ToString(),value);
            }
            else
            {
                this.linkA.SetIconLocation(iconPath.ToString(),value);
            }
        }
    }

    internal string Target
    {
        get
        {
            var target=new System.Text.StringBuilder(260,260);
            if(this.linkA==null)
            {
                var fd=new _WIN32_FIND_DATAW();
                this.linkW.GetPath(target,target.Capacity,ref fd,
                    (uint)EShellLinkGP.SLGP_UNCPRIORITY);
            }
            else
            {
                var fd=new _WIN32_FIND_DATAA();
                this.linkA.GetPath(target,target.Capacity,ref fd,
                    (uint)EShellLinkGP.SLGP_UNCPRIORITY);
            }

            return target.ToString();
        }
        set
        {
            if(this.linkA==null)
            {
                this.linkW.SetPath(value);
            }
            else
            {
                this.linkA.SetPath(value);
            }
        }
    }

    internal string WorkingDirectory
    {
        get
        {
            var path=new System.Text.StringBuilder(260,260);
            if(this.linkA==null)
            {
                this.linkW.GetWorkingDirectory(path,path.Capacity);
            }
            else
            {
                this.linkA.GetWorkingDirectory(path,path.Capacity);
            }

            return path.ToString();
        }
        set
        {
            if(this.linkA==null)
            {
                this.linkW.SetWorkingDirectory(value);
            }
            else
            {
                this.linkA.SetWorkingDirectory(value);
            }
        }
    }

    internal string Description
    {
        get
        {
            var description=new System.Text.StringBuilder(1024,1024);
            if(this.linkA==null)
            {
                this.linkW.GetDescription(description,description.Capacity);
            }
            else
            {
                this.linkA.GetDescription(description,description.Capacity);
            }

            return description.ToString();
        }
        set
        {
            if(this.linkA==null)
            {
                this.linkW.SetDescription(value);
            }
            else
            {
                this.linkA.SetDescription(value);
            }
        }
    }

    internal string Arguments
    {
        get
        {
            var arguments=new System.Text.StringBuilder(260,260);
            if(this.linkA==null)
            {
                this.linkW.GetArguments(arguments,arguments.Capacity);
            }
            else
            {
                this.linkA.GetArguments(arguments,arguments.Capacity);
            }

            return arguments.ToString();
        }
        set
        {
            if(this.linkA==null)
            {
                this.linkW.SetArguments(value);
            }
            else
            {
                this.linkA.SetArguments(value);
            }
        }
    }

    internal LinkDisplayMode DisplayMode
    {
        get
        {
            uint cmd=0;
            if(this.linkA==null)
            {
                this.linkW.GetShowCmd(out cmd);
            }
            else
            {
                this.linkA.GetShowCmd(out cmd);
            }

            return(LinkDisplayMode)cmd;
        }
        set
        {
            if(this.linkA==null)
            {
                this.linkW.SetShowCmd((uint)value);
            }
            else
            {
                this.linkA.SetShowCmd((uint)value);
            }
        }
    }

    internal System.Windows.Forms.Keys HotKey
    {
        get
        {
            short key=0;
            if(this.linkA==null)
            {
                this.linkW.GetHotkey(out key);
            }
            else
            {
                this.linkA.GetHotkey(out key);
            }

            return(System.Windows.Forms.Keys)key;
        }
        set
        {
            if(this.linkA==null)
            {
                this.linkW.SetHotkey((short)value);
            }
            else
            {
                this.linkA.SetHotkey((short)value);
            }
        }
    }

    public void Dispose()
    {
        if(this.linkW!=null)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(this.linkW);
            this.linkW=null;
        }

        if(this.linkA!=null)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(this.linkA);
            this.linkA=null;
        }
    }

    ~ShellLink()
    {
        this.Dispose();
    }

    internal void Save()
    {
        this.Save(this.ShortCutFile);
    }

    internal void Save(string linkFile
    )
    {
        if(this.linkA==null)
        {
            ((IPersistFile)this.linkW).Save(linkFile,true);
            this.ShortCutFile=linkFile;
        }
        else
        {
            ((IPersistFile)this.linkA).Save(linkFile,true);
            this.ShortCutFile=linkFile;
        }
    }

    internal void Open(string linkFile)
    {
        this.Open(linkFile,
            System.IntPtr.Zero,
            EShellLinkResolveFlags.SLR_ANY_MATCH|
            EShellLinkResolveFlags.SLR_NO_UI,1);
    }

    internal void Open(
        string                 linkFile,
        System.IntPtr          hWnd,
        EShellLinkResolveFlags resolveFlags
    )
    {
        this.Open(linkFile,
            hWnd,
            resolveFlags,
            1);
    }

    internal void Open(
        string                 linkFile,
        System.IntPtr          hWnd,
        EShellLinkResolveFlags resolveFlags,
        ushort                 timeOut
    )
    {
        uint flags;

        if((resolveFlags&EShellLinkResolveFlags.SLR_NO_UI)
            ==EShellLinkResolveFlags.SLR_NO_UI)
        {
            flags=(uint)((int)resolveFlags|timeOut<<16);
        }
        else
        {
            flags=(uint)resolveFlags;
        }

        if(this.linkA==null)
        {
            ((IPersistFile)this.linkW).Load(linkFile,0);//STGM_DIRECT)
            this.linkW.Resolve(hWnd,flags);
            this.ShortCutFile=linkFile;
        }
        else
        {
            ((IPersistFile)this.linkA).Load(linkFile,0);//STGM_DIRECT)
            this.linkA.Resolve(hWnd,flags);
            this.ShortCutFile=linkFile;
        }
    }

    [System.Runtime.InteropServices.ComImportAttribute,
     System.Runtime.InteropServices.GuidAttribute("0000010C-0000-0000-C000-000000000046"),
     System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType
         .InterfaceIsIUnknown),]
    interface IPersist
    {
        [System.Runtime.InteropServices.PreserveSigAttribute]
        void GetClassID(out System.Guid pClassID);
    }

    [System.Runtime.InteropServices.ComImportAttribute,
     System.Runtime.InteropServices.GuidAttribute("0000010B-0000-0000-C000-000000000046"),
     System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType
         .InterfaceIsIUnknown),]
    interface IPersistFile
    {
        [System.Runtime.InteropServices.PreserveSigAttribute]
        void GetClassID(out System.Guid pClassID);

        void IsDirty();

        void Load(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszFileName,
            uint dwMode);

        void Save(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszFileName,
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
            bool fRemember);

        void SaveCompleted(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszFileName);

        void GetCurFile(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            out string ppszFileName);
    }

    [System.Runtime.InteropServices.ComImportAttribute,
     System.Runtime.InteropServices.GuidAttribute("000214EE-0000-0000-C000-000000000046"),
     System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType
         .InterfaceIsIUnknown),]
    interface IShellLinkA
    {
        void GetPath(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr),] 
            System.Text.StringBuilder pszFile,
            int                   cchMaxPath,
            ref _WIN32_FIND_DATAA pfd,
            uint                  fFlags);

        void GetIDList(out System.IntPtr ppidl);

        void SetIDList(System.IntPtr pidl);

        void GetDescription(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr),] 
            System.Text.StringBuilder pszFile,
            int cchMaxName);

        void SetDescription(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            string pszName);

        void GetWorkingDirectory(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr),] 
            System.Text.StringBuilder pszDir,
            int cchMaxPath);

        void SetWorkingDirectory(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            string pszDir);

        void GetArguments(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr),] 
            System.Text.StringBuilder pszArgs,
            int cchMaxPath);

        void SetArguments(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            string pszArgs);

        void GetHotkey(out short pwHotkey);
        void SetHotkey(short     pwHotkey);

        void GetShowCmd(out uint piShowCmd);
        void SetShowCmd(uint     piShowCmd);

        void GetIconLocation(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr),] 
            System.Text.StringBuilder pszIconPath,
            int     cchIconPath,
            out int piIcon);

        void SetIconLocation(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            string pszIconPath,
            int iIcon);

        void SetRelativePath(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            string pszPathRel,
            uint dwReserved);

        void Resolve(
            System.IntPtr hWnd,
            uint          fFlags);

        void SetPath(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            string pszFile);
    }


    [System.Runtime.InteropServices.ComImportAttribute,
     System.Runtime.InteropServices.GuidAttribute("000214F9-0000-0000-C000-000000000046"),
     System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType
         .InterfaceIsIUnknown),]
    interface IShellLinkW
    {
        void GetPath(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr),] 
            System.Text.StringBuilder pszFile,
            int                   cchMaxPath,
            ref _WIN32_FIND_DATAW pfd,
            uint                  fFlags);

        void GetIDList(out System.IntPtr ppidl);
        void SetIDList(System.IntPtr     pidl);

        void GetDescription(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr),] 
            System.Text.StringBuilder pszFile,
            int cchMaxName);

        void SetDescription(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszName);

        void GetWorkingDirectory(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr),] 
            System.Text.StringBuilder pszDir,
            int cchMaxPath);

        void SetWorkingDirectory(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszDir);

        void GetArguments(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr),] 
            System.Text.StringBuilder pszArgs,
            int cchMaxPath);

        void SetArguments(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszArgs);

        void GetHotkey(out short pwHotkey);
        void SetHotkey(short     pwHotkey);

        void GetShowCmd(out uint piShowCmd);
        void SetShowCmd(uint     piShowCmd);

        void GetIconLocation(
            [System.Runtime.InteropServices.OutAttribute,
             System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr),] 
            System.Text.StringBuilder pszIconPath,
            int     cchIconPath,
            out int piIcon);

        void SetIconLocation(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszIconPath,
            int iIcon);

        void SetRelativePath(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszPathRel,
            uint dwReserved);

        void Resolve(
            System.IntPtr hWnd,
            uint          fFlags);

        void SetPath(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string pszFile);
    }

    [System.Runtime.InteropServices.GuidAttribute("00021401-0000-0000-C000-000000000046"),
     System.Runtime.InteropServices.ClassInterfaceAttribute(System.Runtime.InteropServices.ClassInterfaceType.None),
     System.Runtime.InteropServices.ComImportAttribute,]
    class CShellLink {}

    enum EShellLinkGP:uint
    {
        SLGP_SHORTPATH=1,SLGP_UNCPRIORITY=2,
    }

    [System.FlagsAttribute]
    enum EShowWindowFlags:uint
    {
        SW_HIDE           =0,SW_SHOWNORMAL   =1,SW_NORMAL  =1,
        SW_SHOWMINIMIZED  =2,SW_SHOWMAXIMIZED=3,SW_MAXIMIZE=3,
        SW_SHOWNOACTIVATE =4,SW_SHOW         =5,SW_MINIMIZE=6,
        SW_SHOWMINNOACTIVE=7,SW_SHOWNA       =8,SW_RESTORE =9,
        SW_SHOWDEFAULT    =10,SW_MAX         =10,
    }


    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=4,
        Size=0,                                           CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
    struct _WIN32_FIND_DATAW
    {
        internal readonly uint      dwFileAttributes;
        internal readonly _FILETIME ftCreationTime;
        internal readonly _FILETIME ftLastAccessTime;
        internal readonly _FILETIME ftLastWriteTime;
        internal readonly uint      nFileSizeHigh;
        internal readonly uint      nFileSizeLow;
        internal readonly uint      dwReserved0;
        internal readonly uint      dwReserved1;

        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
            SizeConst=260)]// MAX_PATH
        internal readonly string cFileName;

        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
            SizeConst=14)]
        internal readonly string cAlternateFileName;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=4,
        Size=0,                                           CharSet=System.Runtime.InteropServices.CharSet.Ansi)]
    struct _WIN32_FIND_DATAA
    {
        internal readonly uint      dwFileAttributes;
        internal readonly _FILETIME ftCreationTime;
        internal readonly _FILETIME ftLastAccessTime;
        internal readonly _FILETIME ftLastWriteTime;
        internal readonly uint      nFileSizeHigh;
        internal readonly uint      nFileSizeLow;
        internal readonly uint      dwReserved0;
        internal readonly uint      dwReserved1;

        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
            SizeConst=260)]// MAX_PATH
        internal readonly string cFileName;

        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
            SizeConst=14)]
        internal readonly string cAlternateFileName;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=4,
        Size=0)]
    struct _FILETIME
    {
        internal readonly uint dwLowDateTime;
        internal readonly uint dwHighDateTime;
    }

    class NativeMethods
    {
        [System.Runtime.InteropServices.DllImportAttribute("Shell32",
            CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        internal static extern int ExtractIconEx(
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            string lpszFile,
            int             nIconIndex,
            System.IntPtr[] phIconLarge,
            System.IntPtr[] phIconSmall,
            int             nIcons);

        [System.Runtime.InteropServices.DllImportAttribute("user32")]
        internal static extern int DestroyIcon(System.IntPtr hIcon);
    }

    [System.FlagsAttribute]
    internal enum EShellLinkResolveFlags:uint
    {
        SLR_ANY_MATCH=0x2,SLR_INVOKE_MSI         =0x80,SLR_NOLINKINFO=0x40,
        SLR_NO_UI    =0x1,SLR_NO_UI_WITH_MSG_PUMP=0x101,SLR_NOUPDATE =0x8,
        SLR_NOSEARCH =0x10,SLR_NOTRACK           =0x20,SLR_UPDATE    =0x4,
    }

    internal enum LinkDisplayMode:uint
    {
        edmNormal   =EShowWindowFlags.SW_NORMAL,edmMinimized=EShowWindowFlags.SW_SHOWMINNOACTIVE,
        edmMaximized=EShowWindowFlags.SW_MAXIMIZE,
    }
}