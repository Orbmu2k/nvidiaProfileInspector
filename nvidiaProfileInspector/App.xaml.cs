namespace nvidiaProfileInspector
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Native.NVAPI2;
    using nvidiaProfileInspector.Native.WINAPI;
    using nvidiaProfileInspector.Services;
    using nvidiaProfileInspector.UI.ViewModels;
    using nvidiaProfileInspector.UI.Views;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    public partial class App : Application
    {
        private static Mutex _mutex;
        private static bool _mutexOwned;
        private static AppBootstrapper _bootstrapper;
        private static string _logFilePath;

        public static AppBootstrapper Bootstrapper => _bootstrapper;

        private bool HasArgument(StartupEventArgs e, string name)
        {
           return e.Args.Any(_ => string.Equals(_, name, StringComparison.InvariantCultureIgnoreCase));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!HasArgument(e, "-mock") && NvapiDrsWrapper.Instance.NvAPI_Initialize == null)
            {
                MessageBoxViewModel.Show("No compatible NVIDIA GPU was detected on your system.", "NVIDIA Profile Inspector", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }


            if (!Debugger.IsAttached)
            {
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            base.OnStartup(e);

            try
            {
                SafeNativeMethods.DeleteFile(Environment.GetCommandLineArgs()[0] + ":Zone.Identifier");
            }
            catch { }

            if (e.Args.Length > 0 && File.Exists(e.Args[0]))
            {
                _bootstrapper = new AppBootstrapper();
                _bootstrapper.Initialize();
                HandleFileArgument(e.Args[0]);
                Shutdown();
                return;
            }

            _mutex = new Mutex(true, "nvidiaProfileInspector", out _mutexOwned);

            if (!_mutexOwned)
            {
                MessageBoxViewModel.Show("NVIDIA Profile Inspector is already running.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            _bootstrapper = new AppBootstrapper();
            _bootstrapper.Initialize();

            // Load saved theme using ThemeManager
            var themeManager = _bootstrapper.Resolve<ThemeManager>();
            themeManager.LoadSavedTheme();

            // Defer MainWindow creation to ensure resources are loaded
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }), DispatcherPriority.Background);
        }

        private void HandleFileArgument(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (extension == ".nip")
                {
                    var importService = _bootstrapper.Resolve<DrsImportService>();
                    importService.ImportProfiles(filePath);
                    MessageBoxViewModel.Show("Profile(s) imported successfully!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBoxViewModel.Show($"Import Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, isShutdown: true);
            }
        }

        private void HandleException(Exception exception, bool isShutdown = false)
        {
            try
            {
                string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(
                        Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute))).Product;

                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), productName);
                Directory.CreateDirectory(path);

                // Create logs directory if it doesn't exist
                var logsDir = Path.Combine(path, "Logs");
                Directory.CreateDirectory(logsDir);

                // Generate log file name with timestamp
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                _logFilePath = Path.Combine(logsDir, $"Crash_{timestamp}.log");

                // Write detailed log
                var logContent = new System.Text.StringBuilder();
                logContent.AppendLine("=== NVIDIA Profile Inspector Crash Report ===");
                logContent.AppendLine($"Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                logContent.AppendLine($"Version: {GetType().Assembly.GetName().Version}");
                logContent.AppendLine($"OS: {Environment.OSVersion}");
                logContent.AppendLine($"64-bit: {Environment.Is64BitOperatingSystem}");
                logContent.AppendLine();
                logContent.AppendLine("=== Exception Details ===");
                logContent.AppendLine($"Type: {exception.GetType().FullName}");
                logContent.AppendLine($"Message: {exception.Message}");
                logContent.AppendLine($"Source: {exception.Source}");
                logContent.AppendLine($"TargetSite: {exception.TargetSite}");
                logContent.AppendLine($"StackTrace: {exception.StackTrace}");

                if (exception.InnerException != null)
                {
                    logContent.AppendLine();
                    logContent.AppendLine("=== Inner Exception ===");
                    logContent.AppendLine($"Type: {exception.InnerException.GetType().FullName}");
                    logContent.AppendLine($"Message: {exception.InnerException.Message}");
                    logContent.AppendLine($"StackTrace: {exception.InnerException.StackTrace}");
                }

                File.WriteAllText(_logFilePath, logContent.ToString());

                // Show error message with link to log file
                var message = $"An unexpected error occurred:\n\n{exception.Message}\n\n" +
                             $"A detailed crash log has been created at:\n{_logFilePath}\n\n" +
                             $"Would you like to open the log file location?";

                var result = MessageBoxViewModel.Show(message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Open folder containing the log file
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{_logFilePath}\"");
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                // If logging fails, show basic error
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{exception.Message}\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _bootstrapper?.Dispose();
            if (_mutexOwned)
                _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
