using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace nspector.Native.WINAPI
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct THUMBBUTTON
    {
        Int32 dwMask;
        uint iId;
        uint iBitmap;
        IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        string szTip;
        Int32 dwFlags;
    }

    [Flags]
    internal enum THBF
    {
        THBF_ENABLED = 0x0000,
        THBF_DISABLED = 0x0001,
        THBF_DISMISSONCLICK = 0x0002,
        THBF_NOBACKGROUND = 0x0004,
        THBF_HIDDEN = 0x0008
    }

    [Flags]
    internal enum THB
    {
        THB_BITMAP = 0x0001,
        THB_ICON = 0x0002,
        THB_TOOLTIP = 0x0004,
        THB_FLAGS = 0x0008,
        THBN_CLICKED = 0x1800
    }

    internal enum TBPFLAG
    {
        TBPF_NOPROGRESS = 0,
        TBPF_INDETERMINATE = 0x1,
        TBPF_NORMAL = 0x2,
        TBPF_ERROR = 0x4,
        TBPF_PAUSED = 0x8
    }

    internal enum TBATFLAG
    {
        TBATF_USEMDITHUMBNAIL = 0x1,
        TBATF_USEMDILIVEPREVIEW = 0x2
    }

    [ComImport,
    Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITaskbarList3
    {

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void HrInit();

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void AddTab([In] IntPtr hwnd);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void DeleteTab([In] IntPtr hwnd);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void ActivateTab([In] IntPtr hwnd);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetActiveAlt([In] IntPtr hwnd);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void MarkFullscreenWindow([In] IntPtr hwnd,
                                 [In, MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetProgressValue([In] IntPtr hwnd,
                             [In] ulong ullCompleted,
                             [In] ulong ullTotal);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetProgressState([In] IntPtr hwnd,
                             [In] TBPFLAG tbpFlags);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void RegisterTab([In] IntPtr hwndTab,
                        [In] IntPtr hwndMDI);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void UnregisterTab([In] IntPtr hwndTab);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetTabOrder([In] IntPtr hwndTab,
                        [In] IntPtr hwndInsertBefore);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetTabActive([In] IntPtr hwndTab,
                         [In] IntPtr hwndMDI,
                         [In] TBATFLAG tbatFlags);

        //preliminary
        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void ThumbBarAddButtons([In] IntPtr hwnd,
                               [In] uint cButtons,
                               [In] IntPtr pButton);
        ///* [size_is][in] */ __RPC__in_ecount_full(cButtons) LPTHUMBBUTTON pButton);

        //preliminary
        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void ThumbBarUpdateButtons([In] IntPtr hwnd,
                                  [In] uint cButtons,
                                  [In] IntPtr pButton);
        ///* [size_is][in] */ __RPC__in_ecount_full(cButtons) LPTHUMBBUTTON pButton);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void ThumbBarSetImageList([In] IntPtr hwnd,
                                 [In] IntPtr himl);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetOverlayIcon([In] IntPtr hwnd,
                           [In] IntPtr hIcon,
                           [In, MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetThumbnailTooltip([In] IntPtr hwnd,
                                [In, MarshalAs(UnmanagedType.LPWStr)] string pszTip);

        //preliminary
        [MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void SetThumbnailClip([In] IntPtr hwnd,
                                [In] IntPtr prcClip);

    }


    [ComImport]
    [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
    internal class TaskbarList { }

}
