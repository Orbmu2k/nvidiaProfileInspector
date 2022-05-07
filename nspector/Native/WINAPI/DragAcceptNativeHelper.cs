namespace nspector.Native.WINAPI;

static class DragAcceptNativeHelper
{
    //ChangeWindowMessageFilter
    internal const int MSGFLT_ADD   =1;
    internal const int MSGFLT_REMOVE=2;

    //ChangeWindowMessageFilterEx
    internal const int MSGFLT_ALLOW   =1;
    internal const int MSGFLT_DISALLOW=2;
    internal const int MSGFLT_RESET   =3;

    internal const int WM_DROPFILES     =0x233;
    internal const int WM_COPYDATA      =0x004A;
    internal const int WM_COPYGLOBALDATA=0x0049;

    /// <summary>
    ///     Modifies the User Interface Privilege Isolation (UIPI) message filter for whole process. (Vista only)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="dwFlag"></param>
    /// <returns></returns>
    [System.Runtime.InteropServices.DllImportAttribute("user32.dll",SetLastError=true)]
    internal static extern System.IntPtr ChangeWindowMessageFilter(int message,int dwFlag);

    /// <summary>
    ///     Modifies the User Interface Privilege Isolation (UIPI) message filter for a specified window. (Win7 or higher)
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="message"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [System.Runtime.InteropServices.DllImportAttribute("user32.dll",SetLastError=true)]
    internal static extern System.IntPtr ChangeWindowMessageFilterEx(System.IntPtr handle,int message,int action,
        System.IntPtr                                                              pChangeFilterStruct);


    [System.Runtime.InteropServices.DllImportAttribute(     "shell32.dll",
        CharSet=System.Runtime.InteropServices.CharSet.Ansi,ExactSpelling=true)]
    public static extern void DragAcceptFiles(System.IntPtr hWnd,bool fAccept);
}