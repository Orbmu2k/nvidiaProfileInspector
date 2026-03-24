using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace nvidiaProfileInspector.Native.WINAPI
{
    public static class DragAcceptNativeHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr ChangeWindowMessageFilter(int message, int dwFlag);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr ChangeWindowMessageFilterEx(IntPtr handle, int message, int action, IntPtr pChangeFilterStruct);

        [DllImport("shell32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder lpszFile, int cch);

        [DllImport("shell32.dll", ExactSpelling = true)]
        public static extern void DragFinish(IntPtr hDrop);

        [DllImport("ole32.dll", PreserveSig = true)]
        public static extern int RevokeDragDrop(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public const int MSGFLT_ADD = 1;
        public const int MSGFLT_ALLOW = 1;
        public const int WM_DROPFILES = 0x233;
        public const int WM_COPYDATA = 0x004A;
        public const int WM_COPYGLOBALDATA = 0x0049;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_ACCEPTFILES = 0x00000010;

        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, nIndex)
                : GetWindowLongPtr32(hWnd, nIndex);
        }

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }

        public static string[] GetDroppedFiles(IntPtr hDrop)
        {
            var dropped = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
            if (dropped <= 0)
                return Array.Empty<string>();

            var files = new List<string>();
            for (uint i = 0; i < dropped; i++)
            {
                var size = DragQueryFile(hDrop, i, null, 0);
                if (size <= 0)
                    continue;

                var builder = new StringBuilder((int)size + 1);
                DragQueryFile(hDrop, i, builder, builder.Capacity);
                files.Add(builder.ToString());
            }

            return files.ToArray();
        }
    }
}
