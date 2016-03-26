using System;
using System.Runtime.InteropServices;

namespace nspector.Native.WINAPI
{
    internal static class DragAcceptNativeHelper
    {
        /// <summary>
        /// Modifies the User Interface Privilege Isolation (UIPI) message filter for whole process. (Vista only)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dwFlag"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr ChangeWindowMessageFilter(int message, int dwFlag);

        /// <summary>
        /// Modifies the User Interface Privilege Isolation (UIPI) message filter for a specified window. (Win7 or higher)
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="message"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr ChangeWindowMessageFilterEx(IntPtr handle, int message, int action, IntPtr pChangeFilterStruct);

        //ChangeWindowMessageFilter
        internal const int MSGFLT_ADD = 1;
        internal const int MSGFLT_REMOVE = 2;

        //ChangeWindowMessageFilterEx
        internal const int MSGFLT_ALLOW = 1;
        internal const int MSGFLT_DISALLOW = 2;
        internal const int MSGFLT_RESET = 3;


        [DllImport("shell32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

        internal const int WM_DROPFILES = 0x233;
        internal const int WM_COPYDATA = 0x004A;
        internal const int WM_COPYGLOBALDATA = 0x0049;

    }
}
