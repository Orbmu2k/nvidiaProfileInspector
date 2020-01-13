using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace nspector.Native.WINAPI
{

    internal class ShellLink : IDisposable
    {

        [ComImport()]
        [Guid("0000010C-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersist
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);
        }

        [ComImport()]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load(
               [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
               uint dwMode);

            void Save(
               [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
               [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            void SaveCompleted(
               [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            void GetCurFile(
               [MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        [ComImport()]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkA
        {
            void GetPath(
               [Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
               int cchMaxPath,
               ref _WIN32_FIND_DATAA pfd,
               uint fFlags);

            void GetIDList(out IntPtr ppidl);

            void SetIDList(IntPtr pidl);

            void GetDescription(
               [Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
               int cchMaxName);

            void SetDescription(
               [MarshalAs(UnmanagedType.LPStr)] string pszName);

            void GetWorkingDirectory(
               [Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir,
               int cchMaxPath);

            void SetWorkingDirectory(
               [MarshalAs(UnmanagedType.LPStr)] string pszDir);

            void GetArguments(
               [Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs,
               int cchMaxPath);

            void SetArguments(
               [MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            void GetHotkey(out short pwHotkey);
            void SetHotkey(short pwHotkey);

            void GetShowCmd(out uint piShowCmd);
            void SetShowCmd(uint piShowCmd);

            void GetIconLocation(
               [Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath,
               int cchIconPath,
               out int piIcon);

            void SetIconLocation(
               [MarshalAs(UnmanagedType.LPStr)] string pszIconPath,
               int iIcon);

            void SetRelativePath(
               [MarshalAs(UnmanagedType.LPStr)] string pszPathRel,
               uint dwReserved);

            void Resolve(
               IntPtr hWnd,
               uint fFlags);

            void SetPath(
               [MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }


        [ComImport()]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            void GetPath(
               [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
               int cchMaxPath,
               ref _WIN32_FIND_DATAW pfd,
               uint fFlags);

            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription(
               [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
               int cchMaxName);

            void SetDescription(
               [MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetWorkingDirectory(
               [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
               int cchMaxPath);

            void SetWorkingDirectory(
               [MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            void GetArguments(
               [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
               int cchMaxPath);

            void SetArguments(
               [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            void GetHotkey(out short pwHotkey);
            void SetHotkey(short pwHotkey);

            void GetShowCmd(out uint piShowCmd);
            void SetShowCmd(uint piShowCmd);

            void GetIconLocation(
               [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
               int cchIconPath,
               out int piIcon);

            void SetIconLocation(
               [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
               int iIcon);

            void SetRelativePath(
               [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
               uint dwReserved);

            void Resolve(
               IntPtr hWnd,
               uint fFlags);

            void SetPath(
               [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        [ComImport()]
        private class CShellLink { }

        private enum EShellLinkGP : uint
        {
            SLGP_SHORTPATH = 1,
            SLGP_UNCPRIORITY = 2
        }

        [Flags]
        private enum EShowWindowFlags : uint
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }


        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Unicode)]
        private struct _WIN32_FIND_DATAW
        {
            internal uint dwFileAttributes;
            internal _FILETIME ftCreationTime;
            internal _FILETIME ftLastAccessTime;
            internal _FILETIME ftLastWriteTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            internal string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Ansi)]
        private struct _WIN32_FIND_DATAA
        {
            internal uint dwFileAttributes;
            internal _FILETIME ftCreationTime;
            internal _FILETIME ftLastAccessTime;
            internal _FILETIME ftLastWriteTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            internal string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0)]
        private struct _FILETIME
        {
            internal uint dwLowDateTime;
            internal uint dwHighDateTime;
        }

        private class NativeMethods
        {
            [DllImport("Shell32", CharSet = CharSet.Auto)]
            internal extern static int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)]
            string lpszFile,
               int nIconIndex,
               IntPtr[] phIconLarge,
               IntPtr[] phIconSmall,
               int nIcons);

            [DllImport("user32")]
            internal static extern int DestroyIcon(IntPtr hIcon);
        }

        [Flags]
        internal enum EShellLinkResolveFlags : uint
        {
            SLR_ANY_MATCH = 0x2,
            SLR_INVOKE_MSI = 0x80,
            SLR_NOLINKINFO = 0x40,
            SLR_NO_UI = 0x1,
            SLR_NO_UI_WITH_MSG_PUMP = 0x101,
            SLR_NOUPDATE = 0x8,
            SLR_NOSEARCH = 0x10,
            SLR_NOTRACK = 0x20,
            SLR_UPDATE = 0x4
        }

        internal enum LinkDisplayMode : uint
        {
            edmNormal = EShowWindowFlags.SW_NORMAL,
            edmMinimized = EShowWindowFlags.SW_SHOWMINNOACTIVE,
            edmMaximized = EShowWindowFlags.SW_MAXIMIZE
        }

        private IShellLinkW linkW;
        private IShellLinkA linkA;
        private string shortcutFile = "";

        internal ShellLink()
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                linkW = (IShellLinkW)new CShellLink();
            }
            else
            {
                linkA = (IShellLinkA)new CShellLink();
            }
        }

        internal ShellLink(string linkFile)
            : this()
        {
            Open(linkFile);
        }

        ~ShellLink()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (linkW != null)
            {
                Marshal.ReleaseComObject(linkW);
                linkW = null;
            }
            if (linkA != null)
            {
                Marshal.ReleaseComObject(linkA);
                linkA = null;
            }
        }

        internal string ShortCutFile
        {
            get
            {
                return this.shortcutFile;
            }
            set
            {
                this.shortcutFile = value;
            }
        }

        internal string IconPath
        {
            get
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                return iconPath.ToString();
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                if (linkA == null)
                {
                    linkW.SetIconLocation(value, iconIndex);
                }
                else
                {
                    linkA.SetIconLocation(value, iconIndex);
                }
            }
        }

        internal int IconIndex
        {
            get
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                return iconIndex;
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                iconIndex);
                }
                if (linkA == null)
                {
                    linkW.SetIconLocation(iconPath.ToString(), value);
                }
                else
                {
                    linkA.SetIconLocation(iconPath.ToString(), value);
                }
            }
        }

        internal string Target
        {
            get
            {
                StringBuilder target = new StringBuilder(260, 260);
                if (linkA == null)
                {
                    _WIN32_FIND_DATAW fd = new _WIN32_FIND_DATAW();
                    linkW.GetPath(target, target.Capacity, ref fd,
                     (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                else
                {
                    _WIN32_FIND_DATAA fd = new _WIN32_FIND_DATAA();
                    linkA.GetPath(target, target.Capacity, ref fd,
                     (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                return target.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetPath(value);
                }
                else
                {
                    linkA.SetPath(value);
                }
            }
        }

        internal string WorkingDirectory
        {
            get
            {
                StringBuilder path = new StringBuilder(260, 260);
                if (linkA == null)
                {
                    linkW.GetWorkingDirectory(path, path.Capacity);
                }
                else
                {
                    linkA.GetWorkingDirectory(path, path.Capacity);
                }
                return path.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetWorkingDirectory(value);
                }
                else
                {
                    linkA.SetWorkingDirectory(value);
                }
            }
        }

        internal string Description
        {
            get
            {
                StringBuilder description = new StringBuilder(1024, 1024);
                if (linkA == null)
                {
                    linkW.GetDescription(description, description.Capacity);
                }
                else
                {
                    linkA.GetDescription(description, description.Capacity);
                }
                return description.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetDescription(value);
                }
                else
                {
                    linkA.SetDescription(value);
                }
            }
        }

        internal string Arguments
        {
            get
            {
                StringBuilder arguments = new StringBuilder(260, 260);
                if (linkA == null)
                {
                    linkW.GetArguments(arguments, arguments.Capacity);
                }
                else
                {
                    linkA.GetArguments(arguments, arguments.Capacity);
                }
                return arguments.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetArguments(value);
                }
                else
                {
                    linkA.SetArguments(value);
                }
            }
        }

        internal LinkDisplayMode DisplayMode
        {
            get
            {
                uint cmd = 0;
                if (linkA == null)
                {
                    linkW.GetShowCmd(out cmd);
                }
                else
                {
                    linkA.GetShowCmd(out cmd);
                }
                return (LinkDisplayMode)cmd;
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetShowCmd((uint)value);
                }
                else
                {
                    linkA.SetShowCmd((uint)value);
                }
            }
        }

        internal Keys HotKey
        {
            get
            {
                short key = 0;
                if (linkA == null)
                {
                    linkW.GetHotkey(out key);
                }
                else
                {
                    linkA.GetHotkey(out key);
                }
                return (Keys)key;
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetHotkey((short)value);
                }
                else
                {
                    linkA.SetHotkey((short)value);
                }
            }
        }

        internal void Save()
        {
            Save(shortcutFile);
        }

        internal void Save(string linkFile
           )
        {
            if (linkA == null)
            {
                ((IPersistFile)linkW).Save(linkFile, true);
                shortcutFile = linkFile;
            }
            else
            {
                ((IPersistFile)linkA).Save(linkFile, true);
                shortcutFile = linkFile;
            }
        }

        internal void Open(string linkFile)
        {
            Open(linkFile,
               IntPtr.Zero,
               (EShellLinkResolveFlags.SLR_ANY_MATCH |
                EShellLinkResolveFlags.SLR_NO_UI), 1);
        }

        internal void Open(
           string linkFile,
           IntPtr hWnd,
           EShellLinkResolveFlags resolveFlags
           )
        {
            Open(linkFile,
               hWnd,
               resolveFlags,
               1);
        }

        internal void Open(
           string linkFile,
           IntPtr hWnd,
           EShellLinkResolveFlags resolveFlags,
           ushort timeOut
           )
        {
            uint flags;

            if ((resolveFlags & EShellLinkResolveFlags.SLR_NO_UI)
               == EShellLinkResolveFlags.SLR_NO_UI)
            {
                flags = (uint)((int)resolveFlags | (timeOut << 16));
            }
            else
            {
                flags = (uint)resolveFlags;
            }

            if (linkA == null)
            {
                ((IPersistFile)linkW).Load(linkFile, 0); //STGM_DIRECT)
                linkW.Resolve(hWnd, flags);
                this.shortcutFile = linkFile;
            }
            else
            {
                ((IPersistFile)linkA).Load(linkFile, 0); //STGM_DIRECT)
                linkA.Resolve(hWnd, flags);
                this.shortcutFile = linkFile;
            }
        }
    }

}

