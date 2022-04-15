#region

using System.Runtime.InteropServices;

#endregion

namespace nspector.Native.WINAPI;

internal static class SafeNativeMethods
{
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteFile(string name);
}