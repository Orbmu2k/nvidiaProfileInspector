using System.Runtime.InteropServices;

namespace nvidiaProfileInspector.Native.WINAPI
{
    static class SafeNativeMethods
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string name);
    }
}
