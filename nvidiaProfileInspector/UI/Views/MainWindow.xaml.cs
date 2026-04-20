namespace nvidiaProfileInspector.UI.Views
{
    using nvidiaProfileInspector;
    using nvidiaProfileInspector.Native.WINAPI;
    using nvidiaProfileInspector.Services;
    using nvidiaProfileInspector.UI.ViewModels;
    using nvidiaProfileInspector.UI.Views.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Threading;

    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ThemeManager _themeManager;
        private readonly bool _disableInitialScan;
        private readonly bool _showOnlyCustomizedSettings;
        private HwndSource _windowSource;
        private bool _mainWindowNativeDropRegistered;

        public MainWindow(bool showOnlyCustomizedSettings = false, bool disableInitialScan = false)
        {
            InitializeComponent();
            _showOnlyCustomizedSettings = showOnlyCustomizedSettings;
            _disableInitialScan = disableInitialScan;
            _viewModel = App.Bootstrapper.Resolve<MainViewModel>();
            _themeManager = App.Bootstrapper.Resolve<ThemeManager>();
            _viewModel.OnOpenBitEditor += OnOpenBitEditor;
            _viewModel.OnFocusFilter += () => FilterTextBox.Focus();
            _themeManager.ThemeChanged += OnThemeChanged;
            DataContext = _viewModel;

            if (_showOnlyCustomizedSettings)
                _viewModel.FilterTypeIndex = 0;

            ApplyMockTitle();
            SourceInitialized += MainWindow_SourceInitialized;
            Dispatcher.BeginInvoke(new Action(PreloadAccessibilityAssembly), DispatcherPriority.ApplicationIdle);
        }

        private static void PreloadAccessibilityAssembly()
        {
            try
            {
                Assembly.Load(new AssemblyName("Accessibility, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
            }
            catch
            {
            }
        }

        private void ApplyMockTitle()
        {
            if (!Native.NVAPI2.NvapiDrsWrapper.Instance.IsMockMode)
                return;

            const string mockTitle = "NVIDIA PROFILE INSPECTOR - MOCK!";
            Title = mockTitle;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            WindowBackdropHelper.TryApplyTo(this);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var handle = new WindowInteropHelper(this).Handle;
            RegisterMainWindowNativeDrop(handle);
            var exStyle = DragAcceptNativeHelper.GetWindowLongPtr(handle, DragAcceptNativeHelper.GWL_EXSTYLE).ToInt64();
            if ((exStyle & DragAcceptNativeHelper.WS_EX_ACCEPTFILES) == 0)
            {
                DragAcceptNativeHelper.SetWindowLongPtr(
                    handle,
                    DragAcceptNativeHelper.GWL_EXSTYLE,
                    new IntPtr(exStyle | DragAcceptNativeHelper.WS_EX_ACCEPTFILES));
            }

            DragAcceptNativeHelper.DragAcceptFiles(handle, true);
            _windowSource = HwndSource.FromHwnd(handle);
            _windowSource?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == DragAcceptNativeHelper.WM_DROPFILES)
            {
                try
                {
                    var files = DragAcceptNativeHelper.GetDroppedFiles(wParam);
                    if (files.Length > 0)
                    {
                        HandleDroppedFiles(files);
                        handled = true;
                    }
                }
                finally
                {
                    DragAcceptNativeHelper.DragFinish(wParam);
                }

                return IntPtr.Zero;
            }

            if (msg == MessageHelper.WM_COPYDATA)
            {
                var data = Marshal.PtrToStructure<MessageHelper.COPYDATASTRUCT>(lParam);
                var message = data.lpData ?? string.Empty;

                if (HandleCopyDataMessage(message))
                {
                    handled = true;
                    return new IntPtr(1);
                }
            }

            // Fix controls jumping/flickering during resize
            // Based on https://github.com/sourcechord/FluentWPF/issues/102#issuecomment-903709242
            if (msg == MessageHelper.WM_NCCALCSIZE && wParam != IntPtr.Zero)
            {
                var param = Marshal.PtrToStructure<MessageHelper.NCCALCSIZE_PARAMS>(lParam);
                param.rgrc[0].Bottom -= 1;
                Marshal.StructureToPtr(param, lParam, false);
            }

            // Constrain maximized window to the monitor's work area so it doesn't cover the taskbar.
            if (msg == MessageHelper.WM_GETMINMAXINFO)
            {
                var monitor = MessageHelper.MonitorFromWindow(hwnd, MessageHelper.MONITOR_DEFAULTTONEAREST);
                if (monitor != IntPtr.Zero)
                {
                    var monitorInfo = new MessageHelper.MONITORINFO { cbSize = Marshal.SizeOf<MessageHelper.MONITORINFO>() };
                    if (MessageHelper.GetMonitorInfo(monitor, ref monitorInfo))
                    {
                        var work = monitorInfo.rcWork;
                        var monitor_rect = monitorInfo.rcMonitor;
                        var mmi = Marshal.PtrToStructure<MessageHelper.MINMAXINFO>(lParam);
                        var dpiScale = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
                        mmi.ptMinTrackSize = new MessageHelper.POINT
                        {
                            X = (int)Math.Ceiling(MinWidth * dpiScale.M11),
                            Y = (int)Math.Ceiling(MinHeight * dpiScale.M22)
                        };
                        mmi.ptMaxPosition = new MessageHelper.POINT
                        {
                            X = work.Left - monitor_rect.Left,
                            Y = work.Top - monitor_rect.Top
                        };
                        mmi.ptMaxSize = new MessageHelper.POINT
                        {
                            X = work.Right - work.Left,
                            Y = work.Bottom - work.Top
                        };
                        Marshal.StructureToPtr(mmi, lParam, false);
                    }
                }
                handled = true;
            }


            return IntPtr.Zero;
        }

        private void HandleDroppedFiles(string[] files)
        {
            if (files == null || files.Length == 0)
                return;

            if (files.All(file => string.Equals(Path.GetExtension(file), ".nip", StringComparison.InvariantCultureIgnoreCase)))
            {
                ImportNipFiles(files, showSuccessNotification: true);
                return;
            }

            if (files.Length != 1)
                return;

            var droppedFile = files[0];
            string profileName;
            var applicationName = Common.Helper.ShortcutResolver.ResolveExecuteable(droppedFile, out profileName);
            if (string.IsNullOrEmpty(applicationName))
                return;

            var profiles = _viewModel.FindProfilesUsingApplication(applicationName);
            if (!string.IsNullOrEmpty(profiles))
            {
                var profile = profiles.Split(';')[0];
                _viewModel.SelectProfile(profile);
                _viewModel.ShowSnackbar($"Profile for '{applicationName}' has been selected!", "Success");
                return;
            }

            var result = MessageBoxViewModel.Show(
                "Would you like to create a new profile for this application?",
                "Profile not found!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.ShowCreateProfileDialog(profileName, applicationName);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreWindowSettings();
            await _viewModel.InitializeAsync(_disableInitialScan);
        }

        private void OnThemeChanged(string themeName)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                WindowBackdropHelper.TryApplyTo(this);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void RestoreWindowSettings()
        {
            var settings = Common.Helper.UserSettings.LoadSettings();

            if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
            {
                Width = Math.Max(settings.WindowWidth, MinWidth);
                Height = Math.Max(settings.WindowHeight, MinHeight);
            }

            if (settings.WindowLeft > 0 && settings.WindowTop > 0)
            {
                Left = settings.WindowLeft;
                Top = settings.WindowTop;
            }

            if (settings.WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _themeManager.ThemeChanged -= OnThemeChanged;

            var restoreBounds = WindowState == WindowState.Maximized ? RestoreBounds : new Rect(Left, Top, Width, Height);
            _viewModel.SaveSettings(restoreBounds.Left, restoreBounds.Top, restoreBounds.Width, restoreBounds.Height, WindowState);
        }

        private void CustomSettingsOverrideChip_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxEx.Show(
                "CustomSettingNames.xml is being loaded from the application directory instead of the embedded default resource.\r\n\r\n" +
                "This means the custom setting metadata has been overridden locally. That can be intentional, but names, groups, descriptions, and values may no longer match the embedded defaults shipped with this build.\r\n\r\n" +
                "Possible consequences:\r\n" +
                "- outdated or mismatched setting names\r\n" +
                "- stale descriptions or value labels\r\n" +
                "- missing newer embedded settings or metadata fixes\r\n" +
                "- grouping differences compared to the embedded defaults\r\n\r\n" +
                "If that is not intended, remove or rename the external CustomSettingNames.xml in the application folder so the embedded resource is used again.",
                "Custom Settings Override",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void RegisterMainWindowNativeDrop(IntPtr handle)
        {
            if (_mainWindowNativeDropRegistered || handle == IntPtr.Zero)
                return;

            DragAcceptNativeHelper.RevokeDragDrop(handle);

            DragAcceptNativeHelper.ChangeWindowMessageFilter(DragAcceptNativeHelper.WM_DROPFILES, DragAcceptNativeHelper.MSGFLT_ADD);
            DragAcceptNativeHelper.ChangeWindowMessageFilter(DragAcceptNativeHelper.WM_COPYDATA, DragAcceptNativeHelper.MSGFLT_ADD);
            DragAcceptNativeHelper.ChangeWindowMessageFilter(DragAcceptNativeHelper.WM_COPYGLOBALDATA, DragAcceptNativeHelper.MSGFLT_ADD);
            DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle, DragAcceptNativeHelper.WM_DROPFILES, DragAcceptNativeHelper.MSGFLT_ALLOW, IntPtr.Zero);
            DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle, DragAcceptNativeHelper.WM_COPYDATA, DragAcceptNativeHelper.MSGFLT_ALLOW, IntPtr.Zero);
            DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle, DragAcceptNativeHelper.WM_COPYGLOBALDATA, DragAcceptNativeHelper.MSGFLT_ALLOW, IntPtr.Zero);
            DragAcceptNativeHelper.DragAcceptFiles(handle, true);
            _mainWindowNativeDropRegistered = true;
        }

        private bool HandleCopyDataMessage(string message)
        {
            if (string.Equals(message, App.LegacyProfilesImportedMessage, StringComparison.InvariantCulture))
            {
                _viewModel.RefreshCommand.Execute(null);
                _viewModel.ShowSnackbar("Profile(s) imported successfully!", "Success");
                return true;
            }

            var files = App.ParseImportFilesMessage(message);
            if (files.Count == 0)
                return false;

            ImportNipFiles(files, showSuccessNotification: true);
            return true;
        }

        private void ImportNipFiles(IEnumerable<string> files, bool showSuccessNotification)
        {
            try
            {
                var report = _viewModel.ImportFiles(files);
                _viewModel.RefreshCommand.Execute(null);

                if (string.IsNullOrWhiteSpace(report))
                {
                    if (showSuccessNotification)
                        _viewModel.ShowSnackbar("Profile(s) imported successfully!", "Success");
                }
                else
                {
                    MessageBoxViewModel.Show($"Some profile(s) could not be imported!\r\n\r\n{report}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBoxViewModel.Show($"Import Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnOpenBitEditor(uint settingId, uint value, string settingName)
        {
            var dialog = new UI.Views.Dialogs.BitEditorDialog(settingId, value, settingName);
            dialog.Owner = this;
            dialog.OnValueChanged = (newValue) => _viewModel.SetDwordValue(settingId, newValue);
            dialog.ShowDialog();
        }

        private void AddApplication_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AddApplication();
        }

        private void RemoveApplication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApplicationItem app)
            {
                _viewModel.RemoveApplication(app);
            }
        }

        private void ModifiedProfilesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is ModifiedProfileItem profile && combo.IsDropDownOpen)
            {
                _viewModel.SelectProfile(profile.ProfileName);
                combo.SelectedIndex = -1;
            }
        }

        private void ModifiedProfilesCombo_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter
                || sender is not ComboBox combo
                || !combo.IsDropDownOpen)
            {
                return;
            }

            var focusedProfile = GetFocusedModifiedProfileItem() ?? combo.SelectedItem as ModifiedProfileItem;
            if (focusedProfile == null)
            {
                return;
            }

            e.Handled = true;
            combo.IsDropDownOpen = false;
            _viewModel.SelectProfile(focusedProfile.ProfileName);
            combo.SelectedIndex = -1;
        }

        private ModifiedProfileItem GetFocusedModifiedProfileItem()
        {
            DependencyObject current = Keyboard.FocusedElement as DependencyObject;

            while (current != null)
            {
                if (current is ComboBoxItem comboBoxItem && comboBoxItem.DataContext is ModifiedProfileItem profile)
                {
                    return profile;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var items = new List<ActionCardsDialog.ActionCardItem>
            {
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Export current profile only",
                    Description = "Export only the settings you have changed for the currently selected profile.",
                    IconPath = (Geometry)Application.Current.Resources["IconExport"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => ExportCurrentOnly_Click(null, null)
                },
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Export current profile including predefined",
                    Description = "Export the current profile with all its settings, including those not modified by you.",
                    IconPath = (Geometry)Application.Current.Resources["IconExport"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => ExportCurrentWithPredefined_Click(null, null)
                },
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Select profiles to export",
                    Description = "Select multiple modified profiles from a list to export them into a single file.",
                    IconPath = (Geometry)Application.Current.Resources["IconUser"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    ShowProfileSelection = true
                },
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Export all customized profiles",
                    Description = "Export all profiles that contain user-defined or modified settings.",
                    IconPath = (Geometry)Application.Current.Resources["IconUser"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => ExportAllCustomized_Click(null, null)
                },
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Export all profiles (NVIDIA Text Format)",
                    Description = "Export the entire profile database in the official NVIDIA text format.",
                    IconPath = (Geometry)Application.Current.Resources["IconNvidia"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => ExportAllNVIDIA_Click(null, null)
                }
            };

            var dialog = new ActionCardsDialog("Export Profiles", items, "Choose how you want to export your profiles:");
            dialog.Owner = this;
            if (dialog.ShowDialog() == true && dialog.SelectedItem != null)
            {
                dialog.SelectedItem.Command?.Invoke();
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var items = new List<ActionCardsDialog.ActionCardItem>
            {
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Replace imported profile(s)",
                    Description = "Import one or more .nip files and replace each imported profile's current apps and settings with the file contents.",
                    IconPath = (Geometry)Application.Current.Resources["IconImport"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => ImportProfiles_Click(null, null)
                },
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Merge imported profile(s)",
                    Description = "Import one or more .nip files and merge them into the profile targets named inside the files. Existing target values stay unless the import contains the same setting, in which case the imported value wins.",
                    IconPath = (Geometry)Application.Current.Resources["IconUser"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => MergeImportedProfiles_Click(null, null)
                },
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Import (replace) all driver profiles",
                    Description = "Restore or replace all profiles from an NVIDIA Text Format file.",
                    IconPath = (Geometry)Application.Current.Resources["IconNvidia"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => ImportAllNVIDIA_Click(null, null)
                }
            };

            var dialog = new ActionCardsDialog("Import Profiles", items, "Choose how you want to import profiles:");
            dialog.Owner = this;
            if (dialog.ShowDialog() == true && dialog.SelectedItem != null)
            {
                dialog.SelectedItem.Command?.Invoke();
            }
        }

        private void ExportCurrentOnly_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportCurrentProfile(includePredefined: false);
        }

        private void ExportCurrentWithPredefined_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportCurrentProfile(includePredefined: true);
        }

        private async void ExportAllCustomized_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ExportAllCustomizedProfilesAsync();
        }

        private void ExportAllNVIDIA_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportAllProfilesNVIDIAFormat();
        }

        private void ImportProfiles_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ImportProfiles();
        }

        private void MergeImportedProfiles_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.MergeImportedProfiles();
        }

        private void ImportAllNVIDIA_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ImportAllProfilesNVIDIAFormat();
        }

        private void ShowUnknownToggle_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RefreshCurrentProfileCommand.Execute(null);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_windowSource != null)
            {
                _windowSource.RemoveHook(WndProc);
                _windowSource = null;
            }

            base.OnClosed(e);
        }
    }
}
