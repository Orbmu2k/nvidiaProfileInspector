namespace nvidiaProfileInspector.UI.Views
{
    using nvidiaProfileInspector.Native.WINAPI;
    using nvidiaProfileInspector.Services;
    using nvidiaProfileInspector;
    using nvidiaProfileInspector.UI.ViewModels;
    using nvidiaProfileInspector.UI.Views.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ThemeManager _themeManager;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = App.Bootstrapper.Resolve<MainViewModel>();
            _themeManager = App.Bootstrapper.Resolve<ThemeManager>();
            _viewModel.OnOpenBitEditor += OnOpenBitEditor;
            _themeManager.ThemeChanged += OnThemeChanged;
            DataContext = _viewModel;
            ApplyMockTitle();
            SourceInitialized += MainWindow_SourceInitialized;
        }

        private void ApplyMockTitle()
        {
            if (!Native.NVAPI2.NvapiDrsWrapper.Instance.IsMockMode)
                return;

            const string mockTitle = "NVIDIA Profile Inspector - MOCK!";
            Title = mockTitle;
            AppTitleBar.Title = mockTitle;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            WindowBackdropHelper.TryApplyTo(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreWindowSettings();
            await _viewModel.InitializeAsync();
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
            _viewModel.SaveSettings(Left, Top, Width, Height, WindowState);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    var extension = System.IO.Path.GetExtension(files[0]).ToLowerInvariant();
                    if (extension == ".nip")
                    {
                        try
                        {
                            var report = _viewModel.ImportFile(files[0]);
                            if (string.IsNullOrEmpty(report))
                                MessageBoxViewModel.Show("Profile(s) imported successfully!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBoxViewModel.Show($"Some profile(s) could not be imported!\r\n\r\n{report}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxViewModel.Show($"Import Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
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
            if (sender is Button btn && btn.Tag is MainViewModel.ApplicationItem app)
            {
                _viewModel.RemoveApplication(app);
            }
        }

        private void ModifiedProfilesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is string profileName && combo.IsDropDownOpen)
            {
                _viewModel.SelectProfile(profileName);
                combo.SelectedIndex = -1;
            }
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
                    Title = "Import profile(s)",
                    Description = "Import one or more .nip files into the profile database.",
                    IconPath = (Geometry)Application.Current.Resources["IconImport"],
                    IconFill = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Command = () => ImportProfiles_Click(null, null)
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

        private void ExportAllCustomized_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportAllCustomizedProfiles();
        }

        private void ExportAllNVIDIA_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportAllProfilesNVIDIAFormat();
        }

        private void ImportProfiles_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ImportProfiles();
        }

        private void ImportAllNVIDIA_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ImportAllProfilesNVIDIAFormat();
        }

        private void ShowUnknownToggle_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RefreshCurrentProfileCommand.Execute(null);
        }

    }
}
