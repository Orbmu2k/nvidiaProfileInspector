namespace nvidiaProfileInspector.Native.WINAPI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Shell;
    using nvidiaProfileInspector.Common.Helper;
    using nvidiaProfileInspector.Native.NVAPI2;
    using nvidiaProfileInspector.UI.Controls;

    internal static class WindowBackdropHelper
    {
        private static readonly HashSet<IntPtr> InitialDisabledBypassHandles = new HashSet<IntPtr>();

        private const int AccentDisabled = 0;
        private const int AccentEnableGradient = 1;
        private const int AccentEnableBlurBehind = 3;

        private const int WcaAccentPolicy = 19;

        private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaSystemBackdropType = 38;

        private const int DwmsbtAuto = 0;
        private const int DwmsbtNone = 1;
        private const int DwmsbtMainWindow = 2;
        private const int DwmsbtTransientWindow = 3;
        private const int DwmsbtTabbedWindow = 4;

        private const int Windows10Build = 10240;
        private const int Windows10DarkModeBuild = 17763;
        private const int Windows11Build = 22000;
        private const int Windows11BackdropBuild = 22621;
        private const int TitleBarFrameHeight = 48;
        private const int MinimumWin11TitleBarFrameHeight = 54;

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
            var win11Mode = GetWin11BackdropMode();
            ApplyTitleBarBackground(window, version, win11Mode);
            SyncWindowChromeCaptionHeight(window, version);

            if (ShouldSkipNativeBackdropInitialization(window, handle, win11Mode))
                return;

            TrySetImmersiveDarkMode(handle, version, backdropConfig.UseDarkMode);

            if (version.Build >= Windows11Build)
            {
                ExtendFrameIntoTitleBar(window, handle, version);

                if (!TryEnableWindows11Mica(handle, version, win11Mode))
                {
                    if (win11Mode != Win11BackdropMode.Disabled)
                        TryEnableWindows10Backdrop(handle, backdropConfig.GradientColor);
                }

                return;
            }

            if (version.Build >= Windows10Build)
            {
                if (win11Mode == Win11BackdropMode.Disabled)
                {
                    TryDisableWindows10Backdrop(handle, backdropConfig.DisabledGradientColor);
                    return;
                }

                ExtendFrameIntoTitleBar(window, handle, version);
                TryEnableWindows10Backdrop(handle, backdropConfig.GradientColor);
            }
        }

        private static bool TryEnableWindows11Mica(IntPtr handle, Version version, Win11BackdropMode mode)
        {
            if (mode == Win11BackdropMode.Disabled)
            {
                return TrySetSystemBackdrop(handle, DwmsbtNone);
            }

            if (version.Build >= Windows11BackdropBuild)
            {
                var backdropType = GetSystemBackdropType(mode);
                if (TrySetSystemBackdrop(handle, backdropType))
                    return true;
            }

            return false;
        }

        private static bool TrySetSystemBackdrop(IntPtr handle, int backdropType)
        {
            return DwmSetWindowAttribute(handle, DwmwaSystemBackdropType, ref backdropType, Marshal.SizeOf(typeof(int))) == 0;
        }

        private static int GetSystemBackdropType(Win11BackdropMode mode)
        {
            switch (mode)
            {
                case Win11BackdropMode.Disabled:
                    return DwmsbtNone;
                case Win11BackdropMode.MainWindow:
                    return DwmsbtMainWindow;
                case Win11BackdropMode.Acrylic:
                    return DwmsbtTransientWindow;
                case Win11BackdropMode.Tabbed:
                case Win11BackdropMode.Default:
                default:
                    return DwmsbtTabbedWindow;
            }
        }

        private static Win11BackdropMode GetWin11BackdropMode()
        {
            try
            {
                var settings = UserSettings.LoadSettings();
                var configuredMode = NvapiDrsWrapper.Instance.IsMockMode
                                         ? NvapiDrsWrapper.Instance.GetMockWin11BackdropMode()
                                         : settings?.Win11BackdropMode;

                if (string.Equals(configuredMode, "MainWindow", StringComparison.OrdinalIgnoreCase))
                    return Win11BackdropMode.MainWindow;

                if (string.Equals(configuredMode, "Acrylic", StringComparison.OrdinalIgnoreCase))
                    return Win11BackdropMode.Acrylic;

                if (string.Equals(configuredMode, "Tabbed", StringComparison.OrdinalIgnoreCase))
                    return Win11BackdropMode.Tabbed;

                if (string.Equals(configuredMode, "Disabled", StringComparison.OrdinalIgnoreCase))
                    return Win11BackdropMode.Disabled;

            }
            catch
            {
            }

            return Win11BackdropMode.Default;
        }

        private static void TryEnableWindows10Backdrop(IntPtr handle, int gradientColor)
        {
            ApplyWindows10Accent(handle, AccentEnableBlurBehind, 2, gradientColor);
        }

        private static void TryDisableWindows10Backdrop(IntPtr handle, int gradientColor)
        {
            ApplyWindows10Accent(handle, AccentEnableGradient, 2, gradientColor);
        }

        private static void ApplyWindows10Accent(IntPtr handle, int accentState, int accentFlags, int gradientColor)
        {
            var accent = new AccentPolicy
            {
                AccentState = accentState,
                AccentFlags = accentFlags,
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

        private static void ExtendFrameIntoTitleBar(Window window, IntPtr handle, Version version)
        {
            var topMargin = GetTitleBarFrameHeight(window, version);
            var margins = new Margins
            {
                Left = 0,
                Right = 0,
                Top = topMargin,
                Bottom = 0
            };

            DwmExtendFrameIntoClientArea(handle, ref margins);
        }

        private static int GetTitleBarFrameHeight(Window window, Version version)
        {
            var frameHeight = TitleBarFrameHeight;

            if (window?.FindName("AppTitleBar") is FrameworkElement titleBar)
            {
                var actualHeight = titleBar.ActualHeight;
                if (actualHeight > 0)
                    frameHeight = Math.Max(frameHeight, (int)Math.Ceiling(actualHeight));
            }

            if (version.Build >= Windows11Build)
                frameHeight = Math.Max(frameHeight, MinimumWin11TitleBarFrameHeight);

            return frameHeight;
        }

        private static void ApplyTitleBarBackground(Window window, Version version, Win11BackdropMode mode)
        {
            if (!(window.FindName("AppTitleBar") is TitleBar titleBar))
                return;

            if (mode == Win11BackdropMode.Disabled)
            {
                ApplyTitleBarResourceBackground(titleBar, "TitleBarBackgroundBrush");
                return;
            }

            if (version.Build >= Windows11Build)
            {
                ApplyTitleBarTransparentBackground(titleBar);
                return;
            }

            if (window.TryFindResource("TitleBarBackdropBrush") != null)
            {
                ApplyTitleBarResourceBackground(titleBar, "TitleBarBackdropBrush");
                return;
            }

            ApplyTitleBarTransparentBackground(titleBar);
        }

        private static void ApplyTitleBarResourceBackground(TitleBar titleBar, string resourceKey)
        {
            titleBar.ClearValue(TitleBar.BackgroundProperty);
            titleBar.SetResourceReference(TitleBar.BackgroundProperty, resourceKey);
        }

        private static void ApplyTitleBarTransparentBackground(TitleBar titleBar)
        {
            titleBar.ClearValue(TitleBar.BackgroundProperty);
            titleBar.Background = Brushes.Transparent;
        }

        private static bool ShouldSkipNativeBackdropInitialization(Window window, IntPtr handle, Win11BackdropMode mode)
        {
            if (mode != Win11BackdropMode.Disabled)
                return false;

            if (window.IsLoaded)
                return false;

            lock (InitialDisabledBypassHandles)
            {
                if (InitialDisabledBypassHandles.Contains(handle))
                    return false;

                InitialDisabledBypassHandles.Add(handle);
                return true;
            }
        }

        private static void SyncWindowChromeCaptionHeight(Window window, Version version)
        {
            if (version.Build < Windows11Build)
                return;

            if (!(window?.FindName("AppTitleBar") is FrameworkElement titleBar))
                return;

            var actualHeight = titleBar.ActualHeight;
            if (actualHeight <= 0)
                return;

            var chrome = WindowChrome.GetWindowChrome(window);
            if (chrome == null)
                return;

            var captionHeight = Math.Ceiling(actualHeight);
            if (captionHeight <= 0 || Math.Abs(chrome.CaptionHeight - captionHeight) <= 0.5)
                return;

            var updatedChrome = chrome.CloneCurrentValue() as WindowChrome;
            if (updatedChrome == null)
                return;

            updatedChrome.CaptionHeight = captionHeight;

            WindowChrome.SetWindowChrome(window, updatedChrome);
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

            var disabledTitleBarColor = TryGetResourceColor(window, "TitleBarBackgroundBrush")
                                        ?? TryGetResourceColor(window, "Layer1BackgroundBrush")
                                        ?? titleBarColor;

            var referenceColor = TryGetResourceColor(window, "WindowBackgroundBrush")
                                 ?? TryGetResourceColor(window, "Layer1BackgroundBrush")
                                 ?? titleBarColor;

            var isDark = IsDark(referenceColor);
            var overlayAlpha = titleBarColor.A < byte.MaxValue
                                   ? titleBarColor.A
                                   : (isDark ? (byte)0xCC : (byte)0xE0);

            return new BackdropConfig
            {
                DisabledGradientColor = ToAbgrInt(Color.FromArgb(byte.MaxValue, disabledTitleBarColor.R, disabledTitleBarColor.G, disabledTitleBarColor.B)),
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
            public int DisabledGradientColor;
            public int GradientColor;
            public bool UseDarkMode;
        }

        private enum Win11BackdropMode
        {
            Default,
            MainWindow,
            Acrylic,
            Tabbed,
            Disabled
        }

    }
}
