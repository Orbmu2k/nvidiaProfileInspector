namespace nspector.Native.WINAPI;

static class SafeNativeMethods
{
    [System.Runtime.InteropServices.DllImportAttribute(        "kernel32",
        CharSet=System.Runtime.InteropServices.CharSet.Unicode,SetLastError=true)]
    [return:System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static extern bool DeleteFile(string name);
}