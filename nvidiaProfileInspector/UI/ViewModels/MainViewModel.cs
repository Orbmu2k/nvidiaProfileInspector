namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.Helper;
    using nvidiaProfileInspector.Native.WINAPI;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;

    public static class MessageBoxEx
    {
        public static MessageBoxResult Show(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            return MessageBoxViewModel.Show(message, title, button, icon);
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly DrsSettingsMetaService _metaService;
        private readonly DrsSettingsService _settingService;
        private readonly DrsScannerService _scannerService;
        private readonly DrsImportService _importService;

        private ObservableCollection<string> _profileNames = new ObservableCollection<string>();
        private string _currentProfile;
        private string _baseProfileName = "";
        private string _filterText = "";
        private string _applicationsText = "";
        private string _settingDescription = "";
        private string _scanStatus = "";
        private bool _showCustomizedSettingsOnly;
        private bool _showScannedUnknownSettings;
        private bool _isDevMode;
        private int _scanProgress;
        private bool _isScanning;
        private bool _isUpdateAvailable;
        private SettingItemViewModel _selectedSetting;
        private ListCollectionView _groupedSettingsView;
        private CancellationTokenSource _scanCancellationTokenSource;
        private int _filterTypeIndex;
        private string _profileFilterText = "";
        private string _modifiedProfileFilterText = "";
        private bool _isFiltering;
        private bool _isInitializing;
        private ListCollectionView _profilesView;
        private ListCollectionView _modifiedProfilesView;
        private SynchronizationContext _uiContext;
        private ITaskbarList3 _taskbarList;
        private IntPtr _windowHandle;
        private bool _scrollIntoViewEnabled = false;
        private string _snackbarMessage;
        private string _snackbarType;
        private bool _isSnackbarActive;
        private int _snackbarToken;

        public MainViewModel(
            DrsSettingsMetaService metaService,
            DrsSettingsService settingService,
            DrsScannerService scannerService,
            DrsImportService importService)
        {
            _metaService = metaService;
            _settingService = settingService;
            _scannerService = scannerService;
            _importService = importService;

            if (_metaService == null && _settingService == null)
            {
                IsDesignMode = true;
                LoadDesignTimeData();
            }
            else
            {
                InitializeCommands();
                LoadSettings();
            }

            OnShowMessage += (msg) => ShowSnackbar(msg, "Information");
            OnShowError += (msg) => ShowSnackbar(msg, "Error");
        }

        public bool IsDesignMode { get; private set; }

        private void LoadDesignTimeData()
        {
            IsDesignMode = true;
            _profileNames.Add("Global Profile");
            _profileNames.Add("Sample Game Profile");
            _currentProfile = "Sample Game Profile";
            _showCustomizedSettingsOnly = true;
            _filterTypeIndex = 0;

            foreach (var item in DesignTimeData.SampleSettings)
            {
                Settings.Add(item);
            }

            if (_groupedSettingsView != null)
            {
                _groupedSettingsView = new ListCollectionView(Settings);
                _groupedSettingsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SettingItemViewModel.GroupName)));
                _groupedSettingsView.Filter = FilterPredicate;
            }

            InitializeCommands();
        }

        public ObservableCollection<string> ProfileNames => _profileNames;


        public bool IsGlobalProfile => string.IsNullOrEmpty(_currentProfile) || _currentProfile == _baseProfileName;

        public bool ShowApplicationsArea
        {
            get
            {
                var isGlobal = string.IsNullOrEmpty(_currentProfile) || _currentProfile == _baseProfileName;
                return !isGlobal;
            }
        }

        public string CurrentProfile
        {
            get
            {
                if (string.IsNullOrEmpty(_currentProfile) || _currentProfile == _baseProfileName)
                {
                    return DrsSettingsService.GlobalProfileName;
                }
                return _currentProfile;
            }
            set
            {
                var profileNameToCheck = value;
                if (profileNameToCheck == DrsSettingsService.GlobalProfileName)
                {
                    profileNameToCheck = null;
                }

                if (SetProperty(ref _currentProfile, profileNameToCheck, nameof(CurrentProfile)))
                {
                    if (!_isFiltering && !_isInitializing)
                        OnCurrentProfileChanged();
                }
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value, nameof(FilterText)))
                    OnFilterTextChanged();
            }
        }

        public string ProfileFilterText
        {
            get => _profileFilterText;
            set
            {
                if (SetProperty(ref _profileFilterText, value, nameof(ProfileFilterText)))
                {
                    _isFiltering = value.Length > 0;
                    ProfilesView?.Refresh();
                    _isFiltering = false;
                }
            }
        }

        public string ModifiedProfileFilterText
        {
            get => _modifiedProfileFilterText;
            set
            {
                if (SetProperty(ref _modifiedProfileFilterText, value, nameof(ModifiedProfileFilterText)))
                {
                    _isFiltering = value.Length > 0;
                    ModifiedProfilesView?.Refresh();
                    _isFiltering = false;
                }
            }
        }



        public int FilterTypeIndex
        {
            get => _filterTypeIndex;
            set
            {
                if (SetProperty(ref _filterTypeIndex, value, nameof(FilterTypeIndex)))
                {
                    _showCustomizedSettingsOnly = value == 0;
                    RefreshCurrentProfileCommand.Execute(null);
                }
            }
        }

        public string ApplicationsText
        {
            get => _applicationsText;
            set => SetProperty(ref _applicationsText, value, nameof(ApplicationsText));
        }

        public string SettingDescription
        {
            get => _settingDescription;
            set => SetProperty(ref _settingDescription, value, nameof(SettingDescription));
        }

        public string ScanStatus
        {
            get => _scanStatus;
            set => SetProperty(ref _scanStatus, value, nameof(ScanStatus));
        }

        public bool ShowScannedUnknownSettings
        {
            get => _showScannedUnknownSettings;
            set
            {
                if (SetProperty(ref _showScannedUnknownSettings, value, nameof(ShowScannedUnknownSettings)))
                    RefreshCurrentProfileCommand.Execute(null);
            }
        }

        public bool IsDevMode
        {
            get => _isDevMode;
            set => SetProperty(ref _isDevMode, value, nameof(IsDevMode));
        }

        public int ScanProgress
        {
            get => _scanProgress;
            set => SetProperty(ref _scanProgress, value, nameof(ScanProgress));
        }

        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value, nameof(IsScanning));
        }

        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set => SetProperty(ref _isUpdateAvailable, value, nameof(IsUpdateAvailable));
        }

        public SettingItemViewModel SelectedSetting
        {
            get => _selectedSetting;
            set
            {
                if (SetProperty(ref _selectedSetting, value, nameof(SelectedSetting)))
                    OnSelectedSettingChanged();
            }
        }

        public bool ScrollIntoViewEnabled
        {
            get => _scrollIntoViewEnabled;
            set => SetProperty(ref _scrollIntoViewEnabled, value, nameof(ScrollIntoViewEnabled));
        }

        public string SnackbarMessage
        {
            get => _snackbarMessage;
            set => SetProperty(ref _snackbarMessage, value, nameof(SnackbarMessage));
        }

        public string SnackbarType
        {
            get => _snackbarType;
            set => SetProperty(ref _snackbarType, value, nameof(SnackbarType));
        }

        public bool IsSnackbarActive
        {
            get => _isSnackbarActive;
            set => SetProperty(ref _isSnackbarActive, value, nameof(IsSnackbarActive));
        }

        public ListCollectionView GroupedSettingsView => _groupedSettingsView;
        public ICollectionView ProfilesView => _profilesView;
        public ICollectionView ModifiedProfilesView => _modifiedProfilesView;

        public ObservableCollection<SettingItemViewModel> Settings { get; } = new ObservableCollection<SettingItemViewModel>();
        public ObservableCollection<string> ModifiedProfiles { get; } = new ObservableCollection<string>();
        public ObservableCollection<ApplicationItem> Applications { get; } = new ObservableCollection<ApplicationItem>();

        public ICommand RefreshCommand { get; private set; }
        public ICommand ApplyCommand { get; private set; }
        public ICommand RestoreProfileCommand { get; private set; }
        public ICommand CreateProfileCommand { get; private set; }
        public ICommand DeleteProfileCommand { get; private set; }
        public ICommand AddApplicationCommand { get; private set; }
        public ICommand RemoveApplicationCommand { get; private set; }
        public ICommand ExportProfileCommand { get; private set; }
        public ICommand ImportProfileCommand { get; private set; }
        public ICommand OpenBitEditorCommand { get; private set; }
        public ICommand ResetValueCommand { get; private set; }
        public ICommand CopySettingsCommand { get; private set; }
        public ICommand ToggleDevModeCommand { get; private set; }
        public ICommand NavigateToGlobalCommand { get; private set; }
        public ICommand RefreshCurrentProfileCommand { get; private set; }
        public ICommand ClearFilterCommand { get; private set; }
        public ICommand ToggleFavoriteCommand { get; private set; }
        public AsyncRelayCommand ScanCommand { get; private set; }
        public AsyncRelayCommand CheckUpdateCommand { get; private set; }
        public ICommand ShowAboutCommand { get; private set; }

        public event Action<uint, uint, string> OnOpenBitEditor;
        public event Action<string> OnShowMessage;
        public event Action<string> OnShowError;

        public async void ShowSnackbar(string message, string type = "Information")
        {
            SnackbarMessage = message;
            SnackbarType = type;
            IsSnackbarActive = true;

            var currentToken = ++_snackbarToken;
            await Task.Delay(4000);

            if (currentToken == _snackbarToken)
            {
                IsSnackbarActive = false;
            }
        }

        private void InitializeCommands()
        {
            RefreshCommand = new RelayCommand(_ => RefreshAll());
            RefreshCurrentProfileCommand = new RelayCommand(_ => RefreshCurrentProfile());
            ApplyCommand = new RelayCommand(ApplyChanges);
            RestoreProfileCommand = new AsyncRelayCommand(RestoreProfileAsync);
            CreateProfileCommand = new RelayCommand(CreateProfile);
            DeleteProfileCommand = new RelayCommand(DeleteProfile);
            AddApplicationCommand = new RelayCommand(_ => AddApplication());
            RemoveApplicationCommand = new RelayCommand(param => RemoveApplication(param as ApplicationItem));
            ExportProfileCommand = new RelayCommand(ExportProfile);
            ImportProfileCommand = new RelayCommand(ImportProfile);
            OpenBitEditorCommand = new RelayCommand(OpenBitEditor, () => SelectedSetting != null);
            ResetValueCommand = new RelayCommand(ResetValue, () => SelectedSetting?.IsUserDefined == true);
            CopySettingsCommand = new RelayCommand(CopySettingsToClipboard);
            ToggleDevModeCommand = new RelayCommand(ToggleDevMode);
            NavigateToGlobalCommand = new RelayCommand(_ => NavigateToGlobalProfile());
            ClearFilterCommand = new RelayCommand(_ => FilterText = "");
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
            ScanCommand = new AsyncRelayCommand(async () => await ScanProfilesAsync());
            CheckUpdateCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
            ShowAboutCommand = new RelayCommand(_ => ShowAbout());
        }

        public async Task InitializeAsync()
        {
            _isInitializing = true;
            RefreshProfilesCombo();
            RefreshCurrentProfile();
            await ScanProfilesAsync();
            _isInitializing = false;
            await CheckForUpdatesAsync();
            RefreshCurrentProfile();
            CurrentProfile = _profileNames.FirstOrDefault();
            OnPropertyChanged(nameof(IsGlobalProfile));
            OnPropertyChanged(nameof(ShowApplicationsArea));
        }

        private void LoadSettings()
        {
            var settings = Common.Helper.UserSettings.LoadSettings();
            _showCustomizedSettingsOnly = settings.ShowCustomizedSettingNamesOnly;
            _showScannedUnknownSettings = settings.ShowScannedUnknownSettings;
            _filterTypeIndex = _showCustomizedSettingsOnly ? 0 : 1;
        }

        private void SaveFavorites()
        {
            var settings = Common.Helper.UserSettings.LoadSettings();
            settings.FavoriteSettingIds.Clear();
            foreach (var setting in Settings.Where(s => s.IsFavorite))
            {
                if (!settings.FavoriteSettingIds.Contains(setting.SettingId))
                    settings.FavoriteSettingIds.Add(setting.SettingId);
            }
            settings.SaveSettings();
        }

        public void SaveSettings(double left, double top, double width, double height, WindowState state)
        {
            var settings = Common.Helper.UserSettings.LoadSettings();
            settings.WindowLeft = (int)left;
            settings.WindowTop = (int)top;
            settings.WindowWidth = (int)width;
            settings.WindowHeight = (int)height;
            settings.WindowState = state;
            settings.ShowCustomizedSettingNamesOnly = _showCustomizedSettingsOnly;
            settings.ShowScannedUnknownSettings = _showScannedUnknownSettings;
            settings.SaveSettings();
        }

        private async void OnCurrentProfileChanged()
        {
            OnPropertyChanged(nameof(IsGlobalProfile));
            OnPropertyChanged(nameof(ShowApplicationsArea));

            // Defer to avoid blocking UI during selection
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => RefreshCurrentProfile()));
        }

        private void OnFilterTextChanged()
        {
            if (_groupedSettingsView != null)
            {
                _groupedSettingsView.Filter = FilterPredicate;
                //_groupedSettingsView.Refresh();
            }
        }

        private bool FilterPredicate(object obj)
        {
            if (string.IsNullOrWhiteSpace(_filterText))
                return true;

            if (obj is SettingItemViewModel item)
            {
                string nameToCheck = item.DisplayName ?? "";
                string altNamesToCheck = item.AlternateNames ?? "";

                if (nameToCheck.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(altNamesToCheck) &&
                    altNamesToCheck.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private void OnSelectedSettingChanged()
        {
            if (_selectedSetting == null)
            {
                SettingDescription = "";
                return;
            }

            var meta = _metaService.GetSettingMeta(_selectedSetting.SettingId, GetSettingViewMode());
            var description = DlssHelper.ReplaceDlssVersions(meta?.Description ?? "");
            if (!string.IsNullOrEmpty(_selectedSetting.AlternateNames))
                description = $"Alternate names: {_selectedSetting.AlternateNames}\r\n{description}";

            SettingDescription = description?.Replace("\\r\\n", "\r\n") ?? "";
        }

        private SettingViewMode GetSettingViewMode()
        {
            if (_showCustomizedSettingsOnly)
                return SettingViewMode.CustomSettingsOnly;
            if (_showScannedUnknownSettings)
                return SettingViewMode.IncludeScannedSetttings;
            return SettingViewMode.Normal;
        }

        private async void RefreshAll()
        {
            DrsSessionScope.DestroyGlobalSession();
            FilterText = "";
            await ScanProfilesAsync(true);
        }

        private void RefreshProfilesCombo()
        {
            _profileNames.Clear();
            var names = _settingService.GetProfileNames(ref _baseProfileName);
            foreach (var name in names)
                _profileNames.Add(name);

            OnPropertyChanged(nameof(IsGlobalProfile));
            OnPropertyChanged(nameof(ShowApplicationsArea));

            if (_profilesView == null)
            {
                _profilesView = new ListCollectionView(_profileNames);
                _profilesView.Filter = (obj) =>
                {
                    if (string.IsNullOrWhiteSpace(_profileFilterText)) return true;
                    return obj.ToString().IndexOf(_profileFilterText, StringComparison.OrdinalIgnoreCase) >= 0;
                };
                OnPropertyChanged(nameof(ProfilesView));
            }

            if (string.IsNullOrEmpty(_currentProfile) || !_profileNames.Contains(_currentProfile))
                CurrentProfile = _profileNames.FirstOrDefault();
            else
            {
                OnPropertyChanged(nameof(CurrentProfile));
            }
        }

        private void RefreshCurrentProfile()
        {
            Applications.Clear();

            var applications = new Dictionary<string, string>();
            var items = _settingService.GetSettingsForProfile(_currentProfile, GetSettingViewMode(), ref applications);

            ApplicationsText = string.Join(", ", applications.Select(x => x.Value));
            foreach (var app in applications)
                Applications.Add(new ApplicationItem { Key = app.Key, Name = app.Value });

            OnPropertyChanged(nameof(IsGlobalProfile));
            OnPropertyChanged(nameof(ShowApplicationsArea));

            var tempSettings = new List<SettingItemViewModel>();
            foreach (var item in items)
            {
                if (item.IsSettingHidden && !_isDevMode)
                    continue;

                var vm = new SettingItemViewModel(item);
                var meta = _metaService.GetSettingMeta(item.SettingId, GetSettingViewMode());
                vm.DwordValues = meta?.DwordValues;
                vm.StringValues = meta?.StringValues;
                vm.BinaryValues = meta?.BinaryValues;
                tempSettings.Add(vm);
            }

            var settings = UserSettings.LoadSettings();
            if (settings.FavoriteSettingIds != null)
            {
                var favoriteIds = new HashSet<uint>(settings.FavoriteSettingIds);
                foreach (var setting in tempSettings)
                {
                    setting.IsFavorite = favoriteIds.Contains(setting.SettingId);
                }
            }

            var sortedSettings = tempSettings
                .OrderByDescending(x => x.IsFavorite)
                .ThenBy(x => string.IsNullOrEmpty(x.GroupNameForDisplay) ? 1 : 0)
                .ThenBy(x => x.GroupNameForDisplay)
                .ThenBy(x => x.DisplayName)
                .ToList();

            Settings.IncrementalPatchSettingsListOrdered(sortedSettings, (s1, s2) => s1.SettingId == s2.SettingId);

            if (_groupedSettingsView == null)
            {
                _groupedSettingsView = new ListCollectionView(Settings);
                _groupedSettingsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SettingItemViewModel.GroupNameForDisplay)));
                _groupedSettingsView.Filter = FilterPredicate;
                OnPropertyChanged(nameof(GroupedSettingsView));
                Debug.WriteLine("Initialized GroupedSettingsView");
            }
        }

        private void ApplyChanges()
        {
            var oldSelectedSettingId = _selectedSetting?.SettingId;

            var settingsToStore = new List<KeyValuePair<uint, string>>();

            foreach (var item in Settings.Where(x => x.HasChanged))
            {
                var original = item.OriginalItem;
                var currentValue = item.SelectedValue;

                if (original.ValueText != currentValue)
                    settingsToStore.Add(new KeyValuePair<uint, string>(item.SettingId, currentValue));
            }

            if (settingsToStore.Count > 0)
            {
                _settingService.StoreSettingsToProfile(_currentProfile, settingsToStore);
                RefreshCurrentProfile();
            }
        }

        private async Task RestoreProfileAsync()
        {
            var result = MessageBoxEx.Show(
                "Restore profile to NVIDIA driver defaults?",
                "Restore Profile",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool removeFromModified;
                _settingService.ResetProfile(_currentProfile, out removeFromModified);
                RefreshCurrentProfile();
            }
        }

        private void CreateProfile()
        {
            var dialog = new Views.Dialogs.InputDialog("Create Profile",
                "Enter a unique name for your new custom driver profile.",
                "", false, (val) =>
                {
                    if (string.IsNullOrWhiteSpace(val)) return "Expected is a unique profile name.";
                    if (_profileNames.Any(p => p.Equals(val, StringComparison.OrdinalIgnoreCase)))
                        return "Profile name must be unique.";
                    return null;
                });

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
            {
                try
                {
                    _settingService.CreateProfile(dialog.InputValue);
                    RefreshProfilesCombo();
                    CurrentProfile = dialog.InputValue;
                }
                catch (Exception ex)
                {
                    OnShowError?.Invoke(ex.Message);
                }
            }
        }

        private void DeleteProfile()
        {
            var result = MessageBoxEx.Show(
                $"Really delete this profile?\r\n\r\nNote: NVIDIA predefined profiles can not be restored until next driver installation!",
                "Delete Profile",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _settingService.DeleteProfile(_currentProfile);
                    RefreshProfilesCombo();
                    CurrentProfile = _profileNames.FirstOrDefault();
                    ShowSnackbar("Profile successfully deleted.", "Success");
                }
                catch (Exception ex)
                {
                    OnShowError?.Invoke(ex.Message);
                }
            }
        }

        public void AddApplication()
        {
            var dialog = new Views.Dialogs.InputDialog("Add Application",
                "To link a new application, enter its filename (e.g. game.exe) or UWP ID. If you need to link a specific file location, use the browse button for an absolute path.",
                "", true, (val) =>
                {
                    if (string.IsNullOrWhiteSpace(val)) return "Expected a filename, UWP ID, or absolute path.";
                    return null;
                });

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
            {
                try
                {
                    _settingService.AddApplication(_currentProfile, dialog.InputValue);
                    RefreshCurrentProfile();
                }
                catch (Exception ex)
                {
                    OnShowError?.Invoke(ex.Message);
                }
            }
        }

        public void RemoveApplication(ApplicationItem app)
        {
            if (app != null)
            {
                var result = MessageBoxEx.Show(
                    $"Remove application '{app.Name}' from profile?",
                    "Remove Application",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _settingService.RemoveApplication(_currentProfile, app.Key);
                    RefreshCurrentProfile();
                }
            }
        }

        private void ExportProfile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = "*.nip",
                Filter = "NVIDIA Profile Inspector Profiles|*.nip",
                FileName = _currentProfile + ".nip"
            };

            if (dialog.ShowDialog() == true)
            {
                var profiles = new List<string> { _currentProfile };
                _importService.ExportProfiles(profiles, dialog.FileName, false);
            }
        }

        public void ExportCurrentProfile(bool includePredefined)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = "*.nip",
                Filter = "NVIDIA Profile Inspector Profiles|*.nip",
                FileName = _currentProfile + ".nip"
            };

            if (dialog.ShowDialog() == true)
            {
                var profiles = new List<string> { _currentProfile };
                _importService.ExportProfiles(profiles, dialog.FileName, includePredefined);
                ShowSnackbar("Current Profile exported successfully!", "Success");
            }
        }

        public void ExportAllCustomizedProfiles()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = "*.nip",
                Filter = "NVIDIA Profile Inspector Profiles|*.nip",
                FileName = "all_profiles.nip"
            };

            if (dialog.ShowDialog() == true)
            {
                _importService.ExportAllCustomizedProfiles(dialog.FileName);
                ShowSnackbar("All customized profiles exported successfully!", "Success");
            }
        }

        public void ExportAllProfilesNVIDIAFormat()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = "*.txt",
                Filter = "NVIDIA Text Files|*.txt",
                FileName = "nvidia_profiles.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                _importService.ExportAllProfilesToNvidiaTextFile(dialog.FileName);
                ShowSnackbar("All profiles exported in NVIDIA text format!", "Success");
            }
        }

        private void ImportProfile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.nip",
                Filter = "NVIDIA Profile Inspector Profiles|*.nip"
            };

            if (dialog.ShowDialog() == true)
            {
                ImportFile(dialog.FileName);
            }
        }

        public string ImportFile(string filePath)
        {
            try
            {
                var report = _importService.ImportProfiles(filePath);
                RefreshCurrentProfile();
                return report ?? "";
            }
            catch (Exception ex)
            {
                OnShowError?.Invoke($"Import Error: {ex.Message}");
                return ex.Message;
            }
        }

        public void ImportProfiles()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.nip",
                Filter = "NVIDIA Profile Inspector Profiles|*.nip"
            };

            if (dialog.ShowDialog() == true)
            {
                var report = ImportFile(dialog.FileName);
                if (string.IsNullOrEmpty(report))
                    ShowSnackbar("Profile(s) successfully imported!", "Success");
                else
                    ShowSnackbar($"Some profile(s) could not imported!\r\n\r\n{report}", "Warning");
            }
        }

        public void ImportAllProfilesNVIDIAFormat()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.txt",
                Filter = "NVIDIA Text Files|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _importService.ImportAllProfilesFromNvidiaTextFile(dialog.FileName);
                    RefreshCurrentProfile();
                    ShowSnackbar("All profiles imported successfully!", "Success");
                }
                catch (Exception ex)
                {
                    OnShowError?.Invoke($"Import Error: {ex.Message}");
                }
            }
        }

        public void SelectProfile(string profileName)
        {
            if (_profileNames.Contains(profileName))
                CurrentProfile = profileName;
        }

        private void OpenBitEditor()
        {
            if (_selectedSetting == null)
                return;

            var rawValue = _selectedSetting.ValueRaw;
            if (rawValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                rawValue = rawValue.Substring(2);

            if (uint.TryParse(rawValue, System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint value))
            {
                OnOpenBitEditor?.Invoke(_selectedSetting.SettingId, value, _selectedSetting.DisplayName);
            }
        }

        public void SetDwordValue(uint settingId, uint value)
        {
            var item = Settings.FirstOrDefault(x => x.SettingId == settingId);
            if (item != null)
            {
                item.SelectedValue = $"0x{value:X8}";
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void ResetValue()
        {

            if (_selectedSetting == null || !_selectedSetting.IsUserDefined)
                return;

            var selectedSettingId = _selectedSetting.SettingId;

            bool removeFromModified;
            _settingService.ResetValue(_currentProfile, selectedSettingId, out removeFromModified);
            RefreshCurrentProfile();
        }

        private void CopySettingsToClipboard()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"### NVIDIA Profile Inspector ###");
            sb.AppendLine($"Profile: {_currentProfile}");
            sb.AppendLine();

            foreach (var item in Settings.Where(x => x.IsUserDefined))
            {
                sb.AppendLine($"{item.DisplayName,-40} {item.SelectedValue}");
            }

            Clipboard.SetText(sb.ToString());
            ShowSnackbar("Settings copied to clipboard!", "Success");
        }

        private void ToggleDevMode()
        {
            IsDevMode = !IsDevMode;
            RefreshCurrentProfile();
        }

        private void NavigateToGlobalProfile()
        {
            if (_profileNames.Count > 0)
                CurrentProfile = _profileNames[0];
        }

        private void ToggleFavorite(object parameter)
        {
            if (parameter == null)
                return;

            var setting = parameter as SettingItemViewModel;
            if (setting == null)
                return;

            var toggledSettingId = setting.SettingId;


            setting.IsFavorite = !setting.IsFavorite;

            SaveFavorites();

            ReorderSettingsWithPosition();
        }

        public void ReorderSettingsWithPosition()
        {
            _groupedSettingsView.IsLiveGrouping = false;
            _groupedSettingsView.IsLiveFiltering = false;

            var sortedSettings = Settings
                .OrderByDescending(x => x.IsFavorite)
                .ThenBy(x => string.IsNullOrEmpty(x.GroupNameForDisplay) ? 1 : 0)
                .ThenBy(x => x.GroupNameForDisplay)
                .ThenBy(x => x.DisplayName)
                .ToList();

            Settings.IncrementalPatchSettingsListOrdered(sortedSettings, (s1, s2) => s1.SettingId == s2.SettingId);

            _groupedSettingsView.IsLiveGrouping = true;
            _groupedSettingsView.IsLiveFiltering = true;
        }

        private void ShowAbout()
        {
            var dialog = new Views.Dialogs.AboutDialog(IsUpdateAvailable);
            var owner = Application.Current.Windows.Cast<Window>()
                .FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;

            if (owner != null)
                dialog.Owner = owner;

            dialog.ShowDialog();
        }

        private async Task ScanProfilesAsync(bool onlyModified = false)
        {
            if (_isScanning)
                return;

            _uiContext = SynchronizationContext.Current;
            var window = Application.Current?.MainWindow;
            if (window != null)
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(window);
                _windowHandle = helper.Handle;
                if (_windowHandle != IntPtr.Zero)
                {
                    _taskbarList = (ITaskbarList3)new TaskbarList();
                    _taskbarList.HrInit();
                }
            }

            IsScanning = true;
            ScanProgress = 0;
            ScanStatus = "Scanning profiles...";

            _scanCancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<int>(value =>
            {
                if (_uiContext != null)
                {
                    _uiContext.Post(_ =>
                    {
                        ScanProgress = value;
                        ScanStatus = $"Scanning... {value}%";
                        if (_taskbarList != null && _windowHandle != IntPtr.Zero)
                        {
                            _taskbarList.SetProgressState(_windowHandle, TBPFLAG.TBPF_NORMAL);
                            _taskbarList.SetProgressValue(_windowHandle, (ulong)value, 100);
                        }
                    }, null);
                }
            });

            try
            {
                await _scannerService.ScanProfileSettingsAsync(onlyModified, progress, _scanCancellationTokenSource.Token);

                ModifiedProfiles.Clear();
                foreach (var profile in _scannerService.ModifiedProfiles)
                    ModifiedProfiles.Add(profile);

                if (_modifiedProfilesView == null)
                {
                    _modifiedProfilesView = new ListCollectionView(ModifiedProfiles);
                    _modifiedProfilesView.Filter = (obj) =>
                    {
                        if (string.IsNullOrWhiteSpace(_modifiedProfileFilterText)) return true;
                        return obj.ToString().IndexOf(_modifiedProfileFilterText, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                    OnPropertyChanged(nameof(ModifiedProfilesView));
                }

                _metaService.ResetMetaCache();
                RefreshCurrentProfile();
                ScanStatus = "";

                if (_taskbarList != null && _windowHandle != IntPtr.Zero)
                {
                    _taskbarList.SetProgressState(_windowHandle, TBPFLAG.TBPF_NOPROGRESS);
                }
            }
            finally
            {
                IsScanning = false;
                ScanProgress = 0;
                _taskbarList = null;
                _windowHandle = IntPtr.Zero;
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                if (nvidiaProfileInspector.Common.Helper.UserSettings.LoadSettings().DisableUpdateCheck)
                    return;

                if (System.IO.File.Exists(System.IO.Path.Combine(AppContext.BaseDirectory, "DisableUpdateCheck.txt")))
                    return;

                IsUpdateAvailable = await GithubVersionHelper.IsUpdateAvailableAsync();
            }
            catch { }
        }

        public class ApplicationItem
        {
            public string Key { get; set; }
            public string Name { get; set; }
        }
    }
}
