using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace nspector.Native.WINAPI
{
    internal class MessageHelper
    {
        [DllImport("User32.dll")]
        private static extern int RegisterWindowMessage(string lpString);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        internal static extern Int32 FindWindow(String lpClassName, String lpWindowName);

        //For use with WM_COPYDATA and COPYDATASTRUCT
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        //For use with WM_COPYDATA and COPYDATASTRUCT
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        internal static extern int PostMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        internal static extern int PostMessage(int hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        internal static extern bool SetForegroundWindow(int hWnd);

        [DllImport("User32.dll")]
        static extern bool SetWindowPlacement(int hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(int hWnd, ref WINDOWPLACEMENT lpwndpl);

        #region Message Constants

        internal const int WM_NULL = 0x00;
        internal const int WM_CREATE = 0x01;
        internal const int WM_DESTROY = 0x02;
        internal const int WM_MOVE = 0x03;
        internal const int WM_SIZE = 0x05;
        internal const int WM_ACTIVATE = 0x06;
        internal const int WM_SETFOCUS = 0x07;
        internal const int WM_KILLFOCUS = 0x08;
        internal const int WM_ENABLE = 0x0A;
        internal const int WM_SETREDRAW = 0x0B;
        internal const int WM_SETTEXT = 0x0C;
        internal const int WM_GETTEXT = 0x0D;
        internal const int WM_GETTEXTLENGTH = 0x0E;
        internal const int WM_PAINT = 0x0F;
        internal const int WM_CLOSE = 0x10;
        internal const int WM_QUERYENDSESSION = 0x11;
        internal const int WM_QUIT = 0x12;
        internal const int WM_QUERYOPEN = 0x13;
        internal const int WM_ERASEBKGND = 0x14;
        internal const int WM_SYSCOLORCHANGE = 0x15;
        internal const int WM_ENDSESSION = 0x16;
        internal const int WM_SYSTEMERROR = 0x17;
        internal const int WM_SHOWWINDOW = 0x18;
        internal const int WM_CTLCOLOR = 0x19;
        internal const int WM_WININICHANGE = 0x1A;
        internal const int WM_SETTINGCHANGE = 0x1A;
        internal const int WM_DEVMODECHANGE = 0x1B;
        internal const int WM_ACTIVATEAPP = 0x1C;
        internal const int WM_FONTCHANGE = 0x1D;
        internal const int WM_TIMECHANGE = 0x1E;
        internal const int WM_CANCELMODE = 0x1F;
        internal const int WM_SETCURSOR = 0x20;
        internal const int WM_MOUSEACTIVATE = 0x21;
        internal const int WM_CHILDACTIVATE = 0x22;
        internal const int WM_QUEUESYNC = 0x23;
        internal const int WM_GETMINMAXINFO = 0x24;
        internal const int WM_PAINTICON = 0x26;
        internal const int WM_ICONERASEBKGND = 0x27;
        internal const int WM_NEXTDLGCTL = 0x28;
        internal const int WM_SPOOLERSTATUS = 0x2A;
        internal const int WM_DRAWITEM = 0x2B;
        internal const int WM_MEASUREITEM = 0x2C;
        internal const int WM_DELETEITEM = 0x2D;
        internal const int WM_VKEYTOITEM = 0x2E;
        internal const int WM_CHARTOITEM = 0x2F;

        internal const int WM_SETFONT = 0x30;
        internal const int WM_GETFONT = 0x31;
        internal const int WM_SETHOTKEY = 0x32;
        internal const int WM_GETHOTKEY = 0x33;
        internal const int WM_QUERYDRAGICON = 0x37;
        internal const int WM_COMPAREITEM = 0x39;
        internal const int WM_COMPACTING = 0x41;
        internal const int WM_WINDOWPOSCHANGING = 0x46;
        internal const int WM_WINDOWPOSCHANGED = 0x47;
        internal const int WM_POWER = 0x48;
        internal const int WM_COPYDATA = 0x4A;
        internal const int WM_CANCELJOURNAL = 0x4B;
        internal const int WM_NOTIFY = 0x4E;
        internal const int WM_INPUTLANGCHANGEREQUEST = 0x50;
        internal const int WM_INPUTLANGCHANGE = 0x51;
        internal const int WM_TCARD = 0x52;
        internal const int WM_HELP = 0x53;
        internal const int WM_USERCHANGED = 0x54;
        internal const int WM_NOTIFYFORMAT = 0x55;
        internal const int WM_CONTEXTMENU = 0x7B;
        internal const int WM_STYLECHANGING = 0x7C;
        internal const int WM_STYLECHANGED = 0x7D;
        internal const int WM_DISPLAYCHANGE = 0x7E;
        internal const int WM_GETICON = 0x7F;
        internal const int WM_SETICON = 0x80;

        internal const int WM_NCCREATE = 0x81;
        internal const int WM_NCDESTROY = 0x82;
        internal const int WM_NCCALCSIZE = 0x83;
        internal const int WM_NCHITTEST = 0x84;
        internal const int WM_NCPAINT = 0x85;
        internal const int WM_NCACTIVATE = 0x86;
        internal const int WM_GETDLGCODE = 0x87;
        internal const int WM_NCMOUSEMOVE = 0xA0;
        internal const int WM_NCLBUTTONDOWN = 0xA1;
        internal const int WM_NCLBUTTONUP = 0xA2;
        internal const int WM_NCLBUTTONDBLCLK = 0xA3;
        internal const int WM_NCRBUTTONDOWN = 0xA4;
        internal const int WM_NCRBUTTONUP = 0xA5;
        internal const int WM_NCRBUTTONDBLCLK = 0xA6;
        internal const int WM_NCMBUTTONDOWN = 0xA7;
        internal const int WM_NCMBUTTONUP = 0xA8;
        internal const int WM_NCMBUTTONDBLCLK = 0xA9;

        internal const int WM_KEYFIRST = 0x100;
        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_KEYUP = 0x101;
        internal const int WM_CHAR = 0x102;
        internal const int WM_DEADCHAR = 0x103;
        internal const int WM_SYSKEYDOWN = 0x104;
        internal const int WM_SYSKEYUP = 0x105;
        internal const int WM_SYSCHAR = 0x106;
        internal const int WM_SYSDEADCHAR = 0x107;
        internal const int WM_KEYLAST = 0x108;

        internal const int WM_IME_STARTCOMPOSITION = 0x10D;
        internal const int WM_IME_ENDCOMPOSITION = 0x10E;
        internal const int WM_IME_COMPOSITION = 0x10F;
        internal const int WM_IME_KEYLAST = 0x10F;

        internal const int WM_INITDIALOG = 0x110;
        internal const int WM_COMMAND = 0x111;
        internal const int WM_SYSCOMMAND = 0x112;
        internal const int WM_TIMER = 0x113;
        internal const int WM_HSCROLL = 0x114;
        internal const int WM_VSCROLL = 0x115;
        internal const int WM_INITMENU = 0x116;
        internal const int WM_INITMENUPOPUP = 0x117;
        internal const int WM_MENUSELECT = 0x11F;
        internal const int WM_MENUCHAR = 0x120;
        internal const int WM_ENTERIDLE = 0x121;

        internal const int WM_CTLCOLORMSGBOX = 0x132;
        internal const int WM_CTLCOLOREDIT = 0x133;
        internal const int WM_CTLCOLORLISTBOX = 0x134;
        internal const int WM_CTLCOLORBTN = 0x135;
        internal const int WM_CTLCOLORDLG = 0x136;
        internal const int WM_CTLCOLORSCROLLBAR = 0x137;
        internal const int WM_CTLCOLORSTATIC = 0x138;

        internal const int WM_MOUSEFIRST = 0x200;
        internal const int WM_MOUSEMOVE = 0x200;
        internal const int WM_LBUTTONDOWN = 0x201;
        internal const int WM_LBUTTONUP = 0x202;
        internal const int WM_LBUTTONDBLCLK = 0x203;
        internal const int WM_RBUTTONDOWN = 0x204;
        internal const int WM_RBUTTONUP = 0x205;
        internal const int WM_RBUTTONDBLCLK = 0x206;
        internal const int WM_MBUTTONDOWN = 0x207;
        internal const int WM_MBUTTONUP = 0x208;
        internal const int WM_MBUTTONDBLCLK = 0x209;
        internal const int WM_MOUSELAST = 0x20A;
        internal const int WM_MOUSEWHEEL = 0x20A;

        internal const int WM_PARENTNOTIFY = 0x210;
        internal const int WM_ENTERMENULOOP = 0x211;
        internal const int WM_EXITMENULOOP = 0x212;
        internal const int WM_NEXTMENU = 0x213;
        internal const int WM_SIZING = 0x214;
        internal const int WM_CAPTURECHANGED = 0x215;
        internal const int WM_MOVING = 0x216;
        internal const int WM_POWERBROADCAST = 0x218;
        internal const int WM_DEVICECHANGE = 0x219;

        internal const int WM_MDICREATE = 0x220;
        internal const int WM_MDIDESTROY = 0x221;
        internal const int WM_MDIACTIVATE = 0x222;
        internal const int WM_MDIRESTORE = 0x223;
        internal const int WM_MDINEXT = 0x224;
        internal const int WM_MDIMAXIMIZE = 0x225;
        internal const int WM_MDITILE = 0x226;
        internal const int WM_MDICASCADE = 0x227;
        internal const int WM_MDIICONARRANGE = 0x228;
        internal const int WM_MDIGETACTIVE = 0x229;
        internal const int WM_MDISETMENU = 0x230;
        internal const int WM_ENTERSIZEMOVE = 0x231;
        internal const int WM_EXITSIZEMOVE = 0x232;
        internal const int WM_DROPFILES = 0x233;
        internal const int WM_MDIREFRESHMENU = 0x234;

        internal const int WM_IME_SETCONTEXT = 0x281;
        internal const int WM_IME_NOTIFY = 0x282;
        internal const int WM_IME_CONTROL = 0x283;
        internal const int WM_IME_COMPOSITIONFULL = 0x284;
        internal const int WM_IME_SELECT = 0x285;
        internal const int WM_IME_CHAR = 0x286;
        internal const int WM_IME_KEYDOWN = 0x290;
        internal const int WM_IME_KEYUP = 0x291;

        internal const int WM_MOUSEHOVER = 0x2A1;
        internal const int WM_NCMOUSELEAVE = 0x2A2;
        internal const int WM_MOUSELEAVE = 0x2A3;

        internal const int WM_CUT = 0x300;
        internal const int WM_COPY = 0x301;
        internal const int WM_PASTE = 0x302;
        internal const int WM_CLEAR = 0x303;
        internal const int WM_UNDO = 0x304;

        internal const int WM_RENDERFORMAT = 0x305;
        internal const int WM_RENDERALLFORMATS = 0x306;
        internal const int WM_DESTROYCLIPBOARD = 0x307;
        internal const int WM_DRAWCLIPBOARD = 0x308;
        internal const int WM_PAINTCLIPBOARD = 0x309;
        internal const int WM_VSCROLLCLIPBOARD = 0x30A;
        internal const int WM_SIZECLIPBOARD = 0x30B;
        internal const int WM_ASKCBFORMATNAME = 0x30C;
        internal const int WM_CHANGECBCHAIN = 0x30D;
        internal const int WM_HSCROLLCLIPBOARD = 0x30E;
        internal const int WM_QUERYNEWPALETTE = 0x30F;
        internal const int WM_PALETTEISCHANGING = 0x310;
        internal const int WM_PALETTECHANGED = 0x311;

        internal const int WM_HOTKEY = 0x312;
        internal const int WM_PRINT = 0x317;
        internal const int WM_PRINTCLIENT = 0x318;

        internal const int WM_HANDHELDFIRST = 0x358;
        internal const int WM_HANDHELDLAST = 0x35F;
        internal const int WM_PENWINFIRST = 0x380;
        internal const int WM_PENWINLAST = 0x38F;
        internal const int WM_COALESCE_FIRST = 0x390;
        internal const int WM_COALESCE_LAST = 0x39F;
        internal const int WM_DDE_FIRST = 0x3E0;
        internal const int WM_DDE_INITIATE = 0x3E0;
        internal const int WM_DDE_TERMINATE = 0x3E1;
        internal const int WM_DDE_ADVISE = 0x3E2;
        internal const int WM_DDE_UNADVISE = 0x3E3;
        internal const int WM_DDE_ACK = 0x3E4;
        internal const int WM_DDE_DATA = 0x3E5;
        internal const int WM_DDE_REQUEST = 0x3E6;
        internal const int WM_DDE_POKE = 0x3E7;
        internal const int WM_DDE_EXECUTE = 0x3E8;
        internal const int WM_DDE_LAST = 0x3E8;

        internal const int WM_USER = 0x400;
        internal const int WM_APP = 0x8000;


        internal const int SW_HIDE = 0;
        internal const int SW_SHOWNORMAL = 1;
        internal const int SW_NORMAL = 1;
        internal const int SW_SHOWMINIMIZED = 2;
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_MAXIMIZE = 3;
        internal const int SW_SHOWNOACTIVATE = 4;
        internal const int SW_SHOW = 5;
        internal const int SW_MINIMIZE = 6;
        internal const int SW_SHOWMINNOACTIVE = 7;
        internal const int SW_SHOWNA = 8;
        internal const int SW_RESTORE = 9;

        #endregion

        //Used for WM_COPYDATA for string messages
        internal struct COPYDATASTRUCT
        {
            internal IntPtr dwData;
            internal int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string lpData;
        }


        internal struct WINDOWPLACEMENT
        {
            internal int length;
            internal int flags;
            internal int showCmd;
            internal System.Drawing.Point ptMinPosition;
            internal System.Drawing.Point ptMaxPosition;
            internal System.Drawing.Rectangle rcNormalPosition;
        }

        internal bool bringAppToFront(int hWnd)
        {
            WINDOWPLACEMENT param = new WINDOWPLACEMENT();
            if (GetWindowPlacement(hWnd, ref param))
            {
                if (param.showCmd != SW_NORMAL)
                {
                    param.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    param.showCmd = SW_NORMAL;
                    SetWindowPlacement(hWnd, ref param);
                }
            }
            return SetForegroundWindow(hWnd);
        }

        internal int sendWindowsStringMessage(int hWnd, int wParam, string msg)
        {
            int result = 0;

            if (hWnd > 0)
            {
                byte[] sarr = System.Text.Encoding.Default.GetBytes(msg);
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)100;
                cds.lpData = msg;
                cds.cbData = len + 1;
                result = SendMessage(hWnd, WM_COPYDATA, wParam, ref cds);
            }

            return result;
        }

        internal int sendWindowsMessage(int hWnd, int Msg, int wParam, int lParam)
        {
            int result = 0;

            if (hWnd > 0)
            {
                result = SendMessage(hWnd, Msg, wParam, lParam);
            }

            return result;
        }

        internal int getWindowId(string className, string windowName)
        {
            return FindWindow(className, windowName);
        }

        internal int checkIfProcessRunning(string process)
        {
            Process[] processes = Process.GetProcessesByName(process);
            return processes.Length;
        }

        internal void closeProcesses(string process, bool force)
        {
            Process[] processes = Process.GetProcessesByName(process);

            for (int i = 0; i < processes.Length; i++)
            {
                processes[i].CloseMainWindow();

                if (force)
                    processes[i].Kill();
                else
                    processes[i].WaitForExit();
            }
        }

        internal void startProcess(string processName)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = processName;
            process.Start();
        }
    }
}