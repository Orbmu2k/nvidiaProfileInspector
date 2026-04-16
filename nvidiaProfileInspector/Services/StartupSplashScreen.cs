namespace nvidiaProfileInspector.Services
{
    using nvidiaProfileInspector.Common.Helper;
    using nvidiaProfileInspector.UI.Views;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    internal sealed class StartupSplashScreen : IDisposable
    {
        private const string DarkTheme = "DarkTheme.xaml";

        private static readonly string[] ValidThemes = {
            DarkTheme,
            "SlateLightTheme.xaml",
            "MidnightTheme.xaml",
            "CleanWhiteTheme.xaml"
        };

        private readonly ManualResetEventSlim _readyEvent = new ManualResetEventSlim(false);
        private Dispatcher _dispatcher;
        private SplashWindow _window;
        private bool _disposed;

        public static StartupSplashScreen Show()
        {
            var splashScreen = new StartupSplashScreen();
            splashScreen.Start();
            return splashScreen;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Close();
            _disposed = true;
        }

        public void Close()
        {
            var dispatcher = _dispatcher;
            if (dispatcher == null || dispatcher.HasShutdownStarted)
                return;

            try
            {
                dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_window == null || !_window.IsVisible)
                    {
                        dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                        return;
                    }

                    _window.CloseWithFade();
                }), DispatcherPriority.Send);
            }
            catch
            {
            }
        }

        private void Start()
        {
            var thread = new Thread(RunSplashThread)
            {
                IsBackground = true,
                Name = "Startup splash screen"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            _readyEvent.Wait(TimeSpan.FromSeconds(2));
        }

        private void RunSplashThread()
        {
            try
            {
                _dispatcher = Dispatcher.CurrentDispatcher;
                _window = new SplashWindow();
                ApplySavedTheme(_window);
                _window.Closed += (sender, args) =>
                {
                    _window = null;
                    _dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                };

                _window.Show();
                _readyEvent.Set();

                Dispatcher.Run();
            }
            catch
            {
                _readyEvent.Set();
            }
        }

        private static void ApplySavedTheme(Window window)
        {
            var themeName = GetSavedThemeName();
            window.Resources.MergedDictionaries.Insert(0, new ResourceDictionary
            {
                Source = new Uri($"/UI/Themes/{themeName}", UriKind.Relative)
            });
        }

        private static string GetSavedThemeName()
        {
            try
            {
                var themeName = Path.GetFileName(UserSettings.LoadSettings().Theme);
                return ValidThemes.FirstOrDefault(theme =>
                           string.Equals(theme, themeName, StringComparison.OrdinalIgnoreCase))
                       ?? DarkTheme;
            }
            catch
            {
                return DarkTheme;
            }
        }
    }
}
