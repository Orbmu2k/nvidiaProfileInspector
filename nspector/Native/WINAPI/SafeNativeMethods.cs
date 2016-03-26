using System.Runtime.InteropServices;

namespace nspector.Native.WINAPI
{
    static class SafeNativeMethods
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(string name);
    }
}
