namespace nspector.Native.WINAPI;

[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,
    CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
struct THUMBBUTTON
{
    readonly int           dwMask;
    readonly uint          iId;
    readonly uint          iBitmap;
    readonly System.IntPtr hIcon;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=260)]
    readonly string szTip;

    readonly int dwFlags;
}

[System.FlagsAttribute]
enum THBF
{
    THBF_ENABLED     =0x0000,THBF_DISABLED=0x0001,THBF_DISMISSONCLICK=0x0002,
    THBF_NOBACKGROUND=0x0004,THBF_HIDDEN  =0x0008,
}

[System.FlagsAttribute]
enum THB
{
    THB_BITMAP=0x0001,THB_ICON    =0x0002,THB_TOOLTIP=0x0004,
    THB_FLAGS =0x0008,THBN_CLICKED=0x1800,
}

enum TBPFLAG
{
    TBPF_NOPROGRESS=0,TBPF_INDETERMINATE=0x1,TBPF_NORMAL=0x2,
    TBPF_ERROR     =0x4,TBPF_PAUSED     =0x8,
}

enum TBATFLAG
{
    TBATF_USEMDITHUMBNAIL=0x1,TBATF_USEMDILIVEPREVIEW=0x2,
}

[System.Runtime.InteropServices.ComImportAttribute,
 System.Runtime.InteropServices.GuidAttribute("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF"),
 System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType
     .InterfaceIsIUnknown),]
interface ITaskbarList3
{
    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void HrInit();

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void AddTab([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void DeleteTab([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void ActivateTab([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetActiveAlt([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void MarkFullscreenWindow([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute,
         System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool),] 
        bool fFullscreen);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetProgressValue([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute]
        ulong ullCompleted,
        [System.Runtime.InteropServices.InAttribute]
        ulong ullTotal);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetProgressState([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute]
        TBPFLAG tbpFlags);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void RegisterTab([System.Runtime.InteropServices.InAttribute]System.IntPtr hwndTab,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr hwndMDI);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void UnregisterTab([System.Runtime.InteropServices.InAttribute]System.IntPtr hwndTab);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetTabOrder([System.Runtime.InteropServices.InAttribute]System.IntPtr hwndTab,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr hwndInsertBefore);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetTabActive([System.Runtime.InteropServices.InAttribute]System.IntPtr hwndTab,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr hwndMDI,
        [System.Runtime.InteropServices.InAttribute]
        TBATFLAG tbatFlags);

    //preliminary
    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void ThumbBarAddButtons([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute]
        uint cButtons,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr pButton);

    ///* [size_is][in] */ __RPC__in_ecount_full(cButtons) LPTHUMBBUTTON pButton);

    //preliminary
    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void ThumbBarUpdateButtons([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute]
        uint cButtons,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr pButton);

    ///* [size_is][in] */ __RPC__in_ecount_full(cButtons) LPTHUMBBUTTON pButton);
    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void ThumbBarSetImageList([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr himl);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetOverlayIcon([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr hIcon,
        [System.Runtime.InteropServices.InAttribute,
         System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr),] 
        string pszDescription);

    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetThumbnailTooltip([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute,
         System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr),] 
        string pszTip);

    //preliminary
    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall,
        MethodCodeType=System.Runtime.CompilerServices.MethodCodeType.Runtime)]
    void SetThumbnailClip([System.Runtime.InteropServices.InAttribute]System.IntPtr hwnd,
        [System.Runtime.InteropServices.InAttribute]
        System.IntPtr prcClip);
}

[System.Runtime.InteropServices.ComImportAttribute,
 System.Runtime.InteropServices.GuidAttribute("56FDF344-FD6D-11d0-958A-006097C9A090"),]
class TaskbarList {}