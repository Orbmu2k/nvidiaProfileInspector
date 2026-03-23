namespace nvidiaProfileInspector.Native.WINAPI
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;

    internal static class WindowBackdropHelper
    {
        private const int AccentEnableBlurBehind = 3;

        private const int WcaAccentPolicy = 19;

        private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaMicaEffect = 1029;
        private const int DwmwaSystemBackdropType = 38;

        private const int DwmsbtAuto = 0;
        private const int DwmsbtMainWindow = 2;

        private const int Windows10Build = 10240;
        private const int Windows10DarkModeBuild = 17763;
        private const int Windows11Build = 22000;
        private const int Windows11BackdropBuild = 22621;
        private const int TitleBarFrameHeight = 48;

        public static void TryApplyTo(Window window)
        {
            if (window == null)
                return;

            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
                return;

            var version = Environment.OSVersion.Version;
            if (version.Major < 10)
                return;

            var backdropConfig = ResolveBackdropConfig(window);

            TrySetImmersiveDarkMode(handle, version, backdropConfig.UseDarkMode);
            ExtendFrameIntoTitleBar(handle);

            if (version.Build >= Windows11Build)
            {
                if (!TryEnableWindows11Mica(handle, version))
                {
                    TryEnableWindows10Backdrop(handle, backdropConfig.GradientColor);
                }

                return;
            }

            if (version.Build >= Windows10Build)
            {
                TryEnableWindows10Backdrop(handle, backdropConfig.GradientColor);
            }
        }

        private static bool TryEnableWindows11Mica(IntPtr handle, Version version)
        {
            if (version.Build >= Windows11BackdropBuild)
            {
                var backdropType = DwmsbtMainWindow;
                if (DwmSetWindowAttribute(handle, DwmwaSystemBackdropType, ref backdropType, Marshal.SizeOf(typeof(int))) == 0)
                    return true;
            }

            var enabled = 1;
            return DwmSetWindowAttribute(handle, DwmwaMicaEffect, ref enabled, Marshal.SizeOf(typeof(int))) == 0;
        }

        private static void TryEnableWindows10Backdrop(IntPtr handle, int gradientColor)
        {
            var accent = new AccentPolicy
            {
                AccentState = AccentEnableBlurBehind,
                AccentFlags = 2,
                GradientColor = gradientColor,
            };

            var accentSize = Marshal.SizeOf(typeof(AccentPolicy));
            var accentPtr = Marshal.AllocHGlobal(accentSize);

            try
            {
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = WcaAccentPolicy,
                    SizeOfData = accentSize,
                    Data = accentPtr
                };

                SetWindowCompositionAttribute(handle, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(accentPtr);
            }
        }

        private static void ExtendFrameIntoTitleBar(IntPtr handle)
        {
            var margins = new Margins
            {
                Left = 0,
                Right = 0,
                Top = TitleBarFrameHeight,
                Bottom = 0
            };

            DwmExtendFrameIntoClientArea(handle, ref margins);
        }

        private static void TrySetImmersiveDarkMode(IntPtr handle, Version version, bool enabledValue)
        {
            if (version.Build < Windows10DarkModeBuild)
                return;

            var enabled = enabledValue ? 1 : 0;
            var attribute = version.Build < 19041 ? DwmwaUseImmersiveDarkModeBefore20H1 : DwmwaUseImmersiveDarkMode;
            DwmSetWindowAttribute(handle, attribute, ref enabled, Marshal.SizeOf(typeof(int)));
        }

        private static BackdropConfig ResolveBackdropConfig(Window window)
        {
            var titleBarColor = TryGetResourceColor(window, "TitleBarBackdropBrush")
                                ?? TryGetResourceColor(window, "TitleBarBackgroundBrush")
                                ?? TryGetResourceColor(window, "Layer1BackgroundBrush")
                                ?? Colors.White;

            var referenceColor = TryGetResourceColor(window, "WindowBackgroundBrush")
                                 ?? TryGetResourceColor(window, "Layer1BackgroundBrush")
                                 ?? titleBarColor;

            var isDark = IsDark(referenceColor);
            var overlayAlpha = titleBarColor.A < byte.MaxValue
                                   ? titleBarColor.A
                                   : (isDark ? (byte)0xCC : (byte)0xE0);

            return new BackdropConfig
            {
                UseDarkMode = isDark,
                GradientColor = ToAbgrInt(Color.FromArgb(overlayAlpha, titleBarColor.R, titleBarColor.G, titleBarColor.B))
            };
        }

        private static Color? TryGetResourceColor(FrameworkElement element, string resourceKey)
        {
            var resource = element.TryFindResource(resourceKey);

            if (resource is SolidColorBrush brush)
                return brush.Color;

            if (resource is Color color)
                return color;

            return null;
        }

        private static bool IsDark(Color color)
        {
            var luminance = (0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B);
            return luminance < 140;
        }

        private static int ToAbgrInt(Color color)
        {
            return (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        private struct BackdropConfig
        {
            public int GradientColor;
            public bool UseDarkMode;
        }
    }
}
