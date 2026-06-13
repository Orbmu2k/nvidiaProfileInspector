namespace nvidiaProfileInspector
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.Helper;
    using nvidiaProfileInspector.Native.NVAPI2;
    using nvidiaProfileInspector.Native.WINAPI;
    using nvidiaProfileInspector.Services;
    using nvidiaProfileInspector.UI.ViewModels;
    using nvidiaProfileInspector.UI.Views;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Threading;

    public partial class App : Application
    {
        private static Mutex _mutex;
        private static bool _mutexOwned;
        private static AppBootstrapper _bootstrapper;
        private static string _logFilePath;
        private StartupSplashScreen _startupSplashScreen;
        private const string SingleInstanceMutexName = "nvidiaProfileInspector";
        internal const string ImportFilesMessagePrefix = "ImportNipFiles:";
        internal const string LegacyProfilesImportedMessage = "ProfilesImported";

        public static AppBootstrapper Bootstrapper => _bootstrapper;

        private bool HasArgument(IEnumerable<string> args, string name)
        {
            return args.Any(_ => string.Equals(_, name, StringComparison.InvariantCultureIgnoreCase));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var startupOptions = ParseStartupOptions(e.Args);

            RegisterUnhandledExceptionHandlers();
            base.OnStartup(e);

            if (!startupOptions.RequiresSingleInstanceMutex)
            {
                HandleStartupCommandWithoutMutex(startupOptions);
                return;
            }

            //if (!TryAcquireSingleInstanceMutex())
            //{
            //    ActivateRunningInstance();
            //    Shutdown();
            //    return;
            //}

            ShowStartupSplashScreen();
            //await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle); // Ensure UI is responsive
            //await System.Threading.Tasks.Task.Delay(15000); // Allow splash screen to be visible for a moment
            if (!EnsureCompatibleDriver(startupOptions))
                return;

            RunFullApplication(startupOptions);
        }

        private void RegisterUnhandledExceptionHandlers()
        {
            if (Debugger.IsAttached)
                return;

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void HandleStartupCommandWithoutMutex(StartupOptions startupOptions)
        {
            if (startupOptions.CreateCustomSettingNames)
            {
                WriteCustomSettingNamesFile();
                Shutdown();
                return;
            }

            if (startupOptions.HasImportFiles)
            {
                HandleImportStartupCommand(startupOptions);
                Shutdown();
                return;
            }

            if (startupOptions.ExportCustomized)
            {
                if (EnsureCompatibleDriver(startupOptions))
                    ExportCustomizedProfiles();

                Shutdown();
                return;
            }
        }

        private void HandleImportStartupCommand(StartupOptions startupOptions)
        {
            // This headless flow may open the import-mode prompt as the very first window.
            // Under the default OnMainWindowClose mode WPF would adopt that prompt as the
            // MainWindow and shut the app down when it closes - before the result message box
            // can show. The branch ends with an explicit Shutdown(), so opt out of auto-shutdown.
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Forward the explicit mode (or "prompt") so the running instance handles it
            // the same way this process would have.
            if (TryForwardImportFilesToRunningInstance(startupOptions.NipFiles, ResolveForcedImportMode(startupOptions), bringToFront: !startupOptions.IsSilentMode))
                return;

            if (!EnsureCompatibleDriver(startupOptions))
                return;

            InitializeBootstrapper();
            HandleImportArguments(startupOptions);
        }

        private bool TryAcquireSingleInstanceMutex()
        {
            _mutex = new Mutex(true, SingleInstanceMutexName, out _mutexOwned);
            return _mutexOwned;
        }

        private bool EnsureCompatibleDriver(StartupOptions startupOptions)
        {
            if (startupOptions.UseMockDriver)
                return true;

            if (NvapiDrsWrapper.Instance.NvAPI_Initialize != null && NvapiDrsWrapper.Instance.SYS_GetDriverAndBranchVersion != null)
                return true;

            CloseStartupSplashScreen();
            MessageBoxViewModel.Show("No compatible NVIDIA Driver was detected on your system. Your NVIDIA GPU might be disabled.", "NVIDIA PROFILE INSPECTOR", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return false;
        }

        private void ShowStartupSplashScreen()
        {
            if (Debugger.IsAttached)
                return;

            if (UserSettings.LoadSettings().DisableSplashScreen)
                return;

            _startupSplashScreen = StartupSplashScreen.Show();
        }

        private void RunFullApplication(StartupOptions startupOptions)
        {
            RunCommonStartupTasks();
            InitializeBootstrapper();
            PrewarmPopupCreateWindow();

            // Load saved theme using ThemeManager
            var themeManager = _bootstrapper.Resolve<ThemeManager>();
            themeManager.LoadSavedTheme();

            // Defer MainWindow creation to ensure resources are loaded
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var mainWindow = new MainWindow(startupOptions.ShowOnlyCustomizedSettings, startupOptions.DisableInitialScan);
                mainWindow.ContentRendered += MainWindow_ContentRendered;
                mainWindow.Closed += MainWindow_Closed;
                MainWindow = mainWindow;
                mainWindow.Show();
            }), DispatcherPriority.Background);
        }

        private void PrewarmPopupCreateWindow()
        {
            var dummyPopup = new System.Windows.Controls.Primitives.Popup
            {
                Width = 1,
                Height = 1,
                AllowsTransparency = true,
                Opacity = 0,
                Child = new System.Windows.Controls.Border()
            };
            dummyPopup.IsOpen = true;
            dummyPopup.IsOpen = false;
        }

        private void RunCommonStartupTasks()
        {
            try
            {
                SafeNativeMethods.DeleteFile(Environment.GetCommandLineArgs()[0] + ":Zone.Identifier");
            }
            catch { }

            try
            {
                FileAssociationHelper.RegisterNipAssociation();
            }
            catch { }
        }

        private void InitializeBootstrapper()
        {
            _bootstrapper = new AppBootstrapper();
            _bootstrapper.Initialize();
        }

        private void ExportCustomizedProfiles()
        {
            InitializeBootstrapper();
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"CustomProfiles_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.nip");
            _bootstrapper.Resolve<nvidiaProfileInspector.Common.DrsImportService>().ExportAllCustomizedProfiles(path);
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.ContentRendered -= MainWindow_ContentRendered;
                BringMainWindowToFront(window);
            }

            CloseStartupSplashScreen();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (sender is Window window)
                window.Closed -= MainWindow_Closed;

            CloseStartupSplashScreen();
        }

        private void CloseStartupSplashScreen()
        {
            _startupSplashScreen?.Dispose();
            _startupSplashScreen = null;
        }

        private void BringMainWindowToFront(Window window)
        {
            if (window == null)
                return;

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            window.Activate();
            window.Focus();

            var handle = new WindowInteropHelper(window).Handle;
            if (handle != IntPtr.Zero)
                new MessageHelper().bringAppToFront(handle.ToInt32());
        }

        private void HandleImportArguments(StartupOptions startupOptions)
        {
            try
            {
                var mode = ResolveForcedImportMode(startupOptions);
                if (mode == null)
                {
                    // Interactive launch (e.g. file-association double-click) without a running
                    // instance and without an explicit switch - ask before touching profiles.
                    CloseStartupSplashScreen();
                    mode = UI.Views.Dialogs.ProfileImportModePrompt.Ask(null, startupOptions.NipFiles.Length);
                    if (mode == null)
                        return; // cancelled
                }

                var importService = _bootstrapper.Resolve<DrsImportService>();
                var report = ImportNipFiles(importService, startupOptions.NipFiles, mode.Value);

                if (startupOptions.IsSilentMode)
                    return;

                if (string.IsNullOrWhiteSpace(report))
                    MessageBoxViewModel.Show("Profile(s) imported successfully!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBoxViewModel.Show($"Some profile(s) could not be imported!\r\n\r\n{report}", "Import Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBoxViewModel.Show($"Import Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private StartupOptions ParseStartupOptions(IEnumerable<string> args)
        {
            var arguments = args?.ToArray() ?? Array.Empty<string>();
            var nipFiles = arguments
                .Where(File.Exists)
                .Where(path => string.Equals(Path.GetExtension(path), ".nip", StringComparison.InvariantCultureIgnoreCase))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            return new StartupOptions
            {
                NipFiles = nipFiles,
                CreateCustomSettingNames = HasArgument(arguments, "-createCSN"),
                ShowOnlyCustomizedSettings = HasArgument(arguments, "-showOnlyCSN"),
                DisableInitialScan = HasArgument(arguments, "-disableScan"),
                IsSilentMode = HasArgument(arguments, "-silentImport") || HasArgument(arguments, "-silent"),
                ExportCustomized = HasArgument(arguments, "-exportCustomized"),
                UseMockDriver = HasArgument(arguments, "-mock"),
                ImportMode = ParseImportMode(arguments)
            };
        }

        private ProfileImportMode? ParseImportMode(IEnumerable<string> arguments)
        {
            if (HasArgument(arguments, "-mergeImport"))
                return ProfileImportMode.Merge;
            if (HasArgument(arguments, "-replaceImport"))
                return ProfileImportMode.Replace;
            return null;
        }

        /// <summary>
        /// Resolves the import mode for a non-interactive context: an explicit switch wins,
        /// silent mode falls back to Replace (legacy behavior), otherwise null = ask the user.
        /// </summary>
        private ProfileImportMode? ResolveForcedImportMode(StartupOptions startupOptions)
        {
            if (startupOptions.ImportMode.HasValue)
                return startupOptions.ImportMode.Value;
            if (startupOptions.IsSilentMode)
                return ProfileImportMode.Replace;
            return null;
        }

        private void WriteCustomSettingNamesFile()
        {
            var xml = EmbeddedResourceHelper.GetFileAsString("nvidiaProfileInspector.CustomSettingNames.xml") ?? string.Empty;
            File.WriteAllText("CustomSettingNames.xml", xml, Encoding.UTF8);
        }

        private string ImportNipFiles(DrsImportService importService, IEnumerable<string> filePaths, ProfileImportMode mode)
        {
            var files = (filePaths ?? Array.Empty<string>()).ToArray();
            var report = importService.ImportProfiles(files, mode);

            GC.Collect();
            NotifyRunningInstanceAboutImportedProfiles();

            return report;
        }

        private void NotifyRunningInstanceAboutImportedProfiles()
        {
            var current = Process.GetCurrentProcess();
            var processName = current.ProcessName.Replace(".vshost", string.Empty);
            var messageHelper = new MessageHelper();

            foreach (var process in Process.GetProcessesByName(processName))
            {
                if (process.Id == current.Id || process.MainWindowHandle == IntPtr.Zero)
                    continue;

                messageHelper.sendWindowsStringMessage((int)process.MainWindowHandle, 0, LegacyProfilesImportedMessage);
            }
        }

        private bool TryForwardImportFilesToRunningInstance(IEnumerable<string> filePaths, ProfileImportMode? forcedMode, bool bringToFront)
        {
            var files = filePaths?.ToArray() ?? Array.Empty<string>();
            if (files.Length == 0)
                return false;

            var current = Process.GetCurrentProcess();
            var processName = current.ProcessName.Replace(".vshost", string.Empty);
            var messageHelper = new MessageHelper();
            var payload = BuildImportFilesMessage(files, forcedMode);

            foreach (var process in Process.GetProcessesByName(processName))
            {
                if (process.Id == current.Id || process.MainWindowHandle == IntPtr.Zero)
                    continue;

                messageHelper.sendWindowsStringMessage((int)process.MainWindowHandle, 0, payload);
                if (bringToFront)
                    messageHelper.bringAppToFront((int)process.MainWindowHandle);

                return true;
            }

            return false;
        }

        private void ActivateRunningInstance()
        {
            var current = Process.GetCurrentProcess();
            var processName = current.ProcessName.Replace(".vshost", string.Empty);
            var messageHelper = new MessageHelper();

            foreach (var process in Process.GetProcessesByName(processName))
            {
                if (process.Id == current.Id || process.MainWindowHandle == IntPtr.Zero)
                    continue;

                messageHelper.bringAppToFront((int)process.MainWindowHandle);
                return;
            }

            MessageBoxViewModel.Show("NVIDIA PROFILE INSPECTOR is already running.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal static string BuildImportFilesMessage(IEnumerable<string> filePaths, ProfileImportMode? forcedMode)
        {
            var files = filePaths?.ToArray() ?? Array.Empty<string>();
            // First line carries the mode token (prompt|merge|replace), the rest are file paths.
            var modeToken = forcedMode.HasValue ? forcedMode.Value.ToString().ToLowerInvariant() : "prompt";
            return ImportFilesMessagePrefix + string.Join("\n", new[] { modeToken }.Concat(files));
        }

        internal sealed class ImportFilesRequest
        {
            public ProfileImportMode? Mode { get; set; }
            public IReadOnlyList<string> Files { get; set; } = Array.Empty<string>();
        }

        internal static ImportFilesRequest ParseImportFilesMessage(string message)
        {
            var result = new ImportFilesRequest();
            if (string.IsNullOrWhiteSpace(message) || !message.StartsWith(ImportFilesMessagePrefix, StringComparison.InvariantCulture))
                return result;

            var tokens = message.Substring(ImportFilesMessagePrefix.Length)
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return result;

            result.Mode = ParseModeToken(tokens[0]);

            result.Files = tokens.Skip(1)
                .Where(File.Exists)
                .Where(path => string.Equals(Path.GetExtension(path), ".nip", StringComparison.InvariantCultureIgnoreCase))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToArray();
            return result;
        }

        private static ProfileImportMode? ParseModeToken(string token)
        {
            if (string.Equals(token, "merge", StringComparison.OrdinalIgnoreCase))
                return ProfileImportMode.Merge;
            if (string.Equals(token, "replace", StringComparison.OrdinalIgnoreCase))
                return ProfileImportMode.Replace;
            return null; // "prompt" or anything unexpected -> ask interactively
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

        private static string GetDriverVersionSafe()
        {
            // NVAPI access itself may be the crash cause (e.g. no NVIDIA driver installed),
            // so the crash logger must never die on it.
            try
            {
                var version = Common.DrsSettingsServiceBase.DriverVersion;
                var branch = Common.DrsSettingsServiceBase.DriverBranch;
                return version > 0
                    ? version.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                        (string.IsNullOrEmpty(branch) ? "" : $" ({branch})")
                    : "unknown";
            }
            catch
            {
                return "unknown";
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
                logContent.AppendLine("=== NVIDIA PROFILE INSPECTOR Crash Report ===");
                logContent.AppendLine($"Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                logContent.AppendLine($"Version: {GetType().Assembly.GetName().Version}");
                logContent.AppendLine($"OS: {Environment.OSVersion}");
                logContent.AppendLine($"64-bit: {Environment.Is64BitOperatingSystem}");
                logContent.AppendLine($"Driver: {GetDriverVersionSafe()}");
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
            CloseStartupSplashScreen();
            _bootstrapper?.Dispose();
            if (_mutexOwned)
                _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }

        private sealed class StartupOptions
        {
            public bool CreateCustomSettingNames { get; set; }
            public bool DisableInitialScan { get; set; }
            public bool IsSilentMode { get; set; }
            public string[] NipFiles { get; set; } = Array.Empty<string>();
            public bool ShowOnlyCustomizedSettings { get; set; }
            public bool ExportCustomized { get; set; }
            public bool UseMockDriver { get; set; }

            /// <summary>Explicit import mode from the command line; null = ask interactively.</summary>
            public ProfileImportMode? ImportMode { get; set; }

            public bool HasImportFiles => NipFiles.Length > 0;
            public bool RequiresSingleInstanceMutex => !CreateCustomSettingNames && !HasImportFiles && !ExportCustomized;
        }
    }
}
