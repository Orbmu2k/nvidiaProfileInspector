namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector;
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.Helper;
    using nvidiaProfileInspector.Native.NVAPI2;
    using nvidiaProfileInspector.Native.WINAPI;
    using nvidiaProfileInspector.Services;
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

        private ObservableCollection<ProfileListItem> _profileNames = new ObservableCollection<ProfileListItem>();
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
        private bool _isInitializing;
        private SynchronizationContext _uiContext;
        private ITaskbarList3 _taskbarList;
        private IntPtr _windowHandle;
        private bool _scrollIntoViewEnabled = false;
        private string _snackbarMessage;
        private string _snackbarType;
        private bool _isSnackbarActive;
        private int _snackbarToken;
        private bool _isAppearanceMenuOpen;
        private DateTime _appearanceMenuClosedAt = DateTime.MinValue;
        private bool _isDarkTheme = true;
        private bool _isCompactDensity;
        private string _currentBackdropMode = "Tabbed";
        private readonly bool _isWindows11 = Environment.OSVersion.Version.Build >= 22000;

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
            _profileNames.Add(new ProfileListItem(DrsSettingsService.GlobalProfileName, false));
            _profileNames.Add(new ProfileListItem("Sample Game Profile", false));
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

        public ObservableCollection<ProfileListItem> ProfileNames => _profileNames;


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
                SetCurrentProfile(value);
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
            set
            {
                if (SetProperty(ref _isUpdateAvailable, value, nameof(IsUpdateAvailable)))
                    OnPropertyChanged(nameof(VersionText));
            }
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

        public string VersionText
        {
            get
            {
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var ver = v != null ? $"v{v.Major}.{v.Minor}.{v.Build}.{v.Revision}" : "";
                return _isUpdateAvailable ? $"{ver} (update available)" : ver;
            }
        }

        public bool IsAppearanceMenuOpen
        {
            get => _isAppearanceMenuOpen;
            set
            {
                if (SetProperty(ref _isAppearanceMenuOpen, value, nameof(IsAppearanceMenuOpen)))
                {
                    if (!value)
                        _appearanceMenuClosedAt = DateTime.UtcNow;
                }
            }
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set => SetProperty(ref _isDarkTheme, value, nameof(IsDarkTheme));
        }

        public bool IsCompactDensity
        {
            get => _isCompactDensity;
            set => SetProperty(ref _isCompactDensity, value, nameof(IsCompactDensity));
        }

        public bool IsBackdropMica => string.Equals(_currentBackdropMode, "MainWindow", StringComparison.OrdinalIgnoreCase);

        public bool IsBackdropMicaVariant =>
            string.Equals(_currentBackdropMode, "Tabbed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_currentBackdropMode, "Default", StringComparison.OrdinalIgnoreCase);

        public bool IsBackdropAcrylic => string.Equals(_currentBackdropMode, "Acrylic", StringComparison.OrdinalIgnoreCase);

        public bool IsBackdropDisabled => string.Equals(_currentBackdropMode, "Disabled", StringComparison.OrdinalIgnoreCase);

        public bool IsBackdropGlass => !_isWindows11 && !IsBackdropDisabled;

        public bool IsWindows11 => _isWindows11;

        public bool IsWindows10 => !_isWindows11;

        public ListCollectionView GroupedSettingsView => _groupedSettingsView;

        public ObservableCollection<SettingItemViewModel> Settings { get; } = new ObservableCollection<SettingItemViewModel>();
        public ObservableCollection<ModifiedProfileItem> ModifiedProfiles { get; } = new ObservableCollection<ModifiedProfileItem>();
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
        public ICommand FocusFilterCommand { get; private set; }
        public ICommand ToggleFavoriteCommand { get; private set; }
        public AsyncRelayCommand ScanCommand { get; private set; }
        public AsyncRelayCommand CheckUpdateCommand { get; private set; }
        public ICommand ShowAboutCommand { get; private set; }
        public ICommand ToggleAppearanceMenuCommand { get; private set; }
        public ICommand SetThemeCommand { get; private set; }
        public ICommand SetDensityCommand { get; private set; }
        public ICommand SetBackdropModeCommand { get; private set; }

        public event Action OnFocusFilter;
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
            DeleteProfileCommand = new AsyncRelayCommand(DeleteProfileAsync);
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
            FocusFilterCommand = new RelayCommand(_ => OnFocusFilter?.Invoke());
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
            ScanCommand = new AsyncRelayCommand(async () => await ScanProfilesAsync());
            CheckUpdateCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
            ShowAboutCommand = new RelayCommand(_ => ShowAbout());
            ToggleAppearanceMenuCommand = new RelayCommand(_ => ToggleAppearanceMenu());
            SetThemeCommand = new RelayCommand(param => ApplyTheme(param as string));
            SetDensityCommand = new RelayCommand(param => ApplyDensity(param as string));
            SetBackdropModeCommand = new RelayCommand(param => ApplyBackdropMode(param as string));
        }

        public async Task InitializeAsync()
        {
            _isInitializing = true;
            RefreshProfilesCombo(null);
            RefreshCurrentProfile();
            await ScanProfilesAsync();
            _isInitializing = false;
            await CheckForUpdatesAsync();
            RefreshCurrentProfile();
            CurrentProfile = _profileNames.FirstOrDefault()?.ProfileName;
            OnPropertyChanged(nameof(IsGlobalProfile));
            OnPropertyChanged(nameof(ShowApplicationsArea));
        }

        private void LoadSettings()
        {
            var settings = Common.Helper.UserSettings.LoadSettings();
            _showCustomizedSettingsOnly = settings.ShowCustomizedSettingNamesOnly;
            _showScannedUnknownSettings = settings.ShowScannedUnknownSettings;
            _filterTypeIndex = _showCustomizedSettingsOnly ? 0 : 1;

            if (App.Bootstrapper != null)
            {
                var themeManager = App.Bootstrapper.Resolve<ThemeManager>();
                _isDarkTheme = themeManager.IsDarkTheme;
                _isCompactDensity = themeManager.IsCompactDensity;
            }

            RefreshBackdropMode();
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

            if (NvapiDrsWrapper.Instance.IsMockMode)
                settings.Win11BackdropMode = NvapiDrsWrapper.Instance.GetMockWin11BackdropMode();

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
            if (IsScanning)
                return;
            DrsSessionScope.DestroyGlobalSession();
            FilterText = "";
            await ScanProfilesAsync(true);
        }

        private void RefreshProfilesCombo(string lastCurrentProfile)
        {
            _profileNames.Clear();
            var names = _settingService.GetProfileNames(ref _baseProfileName);
            foreach (var name in names)
                _profileNames.Add(new ProfileListItem(name, _scannerService.UserProfiles.Contains(name)));

            ModifiedProfiles.Clear();
            foreach (var profile in _scannerService.ModifiedProfiles.OrderBy(x => x))
            {
                ModifiedProfiles.Add(new ModifiedProfileItem(
                    profile,
                    _scannerService.UserProfiles.Contains(profile)));
            }

            OnPropertyChanged(nameof(IsGlobalProfile));
            OnPropertyChanged(nameof(ShowApplicationsArea));

            if (string.IsNullOrEmpty(lastCurrentProfile) || !_profileNames.Any(x => x.ProfileName == lastCurrentProfile))
                SetCurrentProfile(_profileNames.FirstOrDefault()?.ProfileName, forceNotify: true);
            else
            {
                SetCurrentProfile(lastCurrentProfile, forceNotify: true);
            }
        }

        private void SetCurrentProfile(string profileName, bool forceNotify = false)
        {
            var profileNameToCheck = profileName;
            if (profileNameToCheck == DrsSettingsService.GlobalProfileName)
                profileNameToCheck = null;

            if (SetProperty(ref _currentProfile, profileNameToCheck, nameof(CurrentProfile)))
            {
                if (!_isInitializing)
                    OnCurrentProfileChanged();
            }
            else if (forceNotify)
            {
                OnPropertyChanged(nameof(CurrentProfile));
                OnPropertyChanged(nameof(IsGlobalProfile));
                OnPropertyChanged(nameof(ShowApplicationsArea));
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

        private void RemoveModifiedProfileFromCache(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return;

            var modifiedProfile = ModifiedProfiles.FirstOrDefault(x =>
                x.ProfileName.Equals(profileName, StringComparison.OrdinalIgnoreCase));
            if (modifiedProfile != null)
                ModifiedProfiles.Remove(modifiedProfile);

            _scannerService.ModifiedProfiles.RemoveAll(x =>
                x.Equals(profileName, StringComparison.OrdinalIgnoreCase));

            _scannerService.UserProfiles.RemoveWhere(x =>
                x.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        }

        private void AddModifiedProfileToCache(string profileName, bool isUserDefined)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return;

            if (!ModifiedProfiles.Any(x => x.ProfileName.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
            {
                var insertIndex = 0;
                while (insertIndex < ModifiedProfiles.Count &&
                       string.Compare(ModifiedProfiles[insertIndex].ProfileName, profileName, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    insertIndex++;
                }

                ModifiedProfiles.Insert(insertIndex, new ModifiedProfileItem(profileName, isUserDefined));
            }

            if (!_scannerService.ModifiedProfiles.Any(x => x.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
            {
                _scannerService.ModifiedProfiles.Add(profileName);
                _scannerService.ModifiedProfiles = _scannerService.ModifiedProfiles
                    .OrderBy(x => x)
                    .ToList();
            }

            if (isUserDefined)
                _scannerService.UserProfiles.Add(profileName);
        }

        private Task RestoreProfileAsync()
        {
            var result = MessageBoxEx.Show(
                "Restore profile to NVIDIA driver defaults?",
                "Restore Profile",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var lastProfile = _currentProfile;

                bool removeFromModified;
                _settingService.ResetProfile(_currentProfile, out removeFromModified);

                if (removeFromModified)
                    RemoveModifiedProfileFromCache(_currentProfile);

                
                RefreshProfilesCombo(lastProfile);
                
                RefreshCurrentProfile();

                ShowSnackbar($"Profile successfully restored to driver defaults!", "Success"); 
            }

            return Task.CompletedTask;
        }

        private void CreateProfile()
        {
            ShowCreateProfileDialog("");
        }

        public bool ShowCreateProfileDialog(string nameProposal, string applicationName = null)
        {
            var dialog = new Views.Dialogs.InputDialog("Create Profile",
            "Enter a unique name for your new custom driver profile.",
            nameProposal ?? "", false, (val) =>
            {
                if (string.IsNullOrWhiteSpace(val)) return "Expected is a unique profile name.";
                if (_profileNames.Any(p => p.ProfileName.Equals(val, StringComparison.OrdinalIgnoreCase)))
                    return "Profile name must be unique.";
                return null;
            });
            dialog.Owner = App.Current.MainWindow;

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
            {
                try
                {
                    _settingService.CreateProfile(dialog.InputValue, applicationName);
                    AddModifiedProfileToCache(dialog.InputValue, isUserDefined: true);
                    RefreshProfilesCombo(dialog.InputValue);
                    return true;
                }
                catch (Exception ex)
                {
                    OnShowError?.Invoke(ex.Message);
                }
            }

            return false;
        }

        private Task DeleteProfileAsync()
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
                    var deletedProfileName = _currentProfile;
                    _settingService.DeleteProfile(_currentProfile);
                    RemoveModifiedProfileFromCache(deletedProfileName);
                    RefreshProfilesCombo(null);
                    SetCurrentProfile(DrsSettingsService.GlobalProfileName, forceNotify: true);
                    ShowSnackbar("Profile successfully deleted.", "Success");
                }
                catch (Exception ex)
                {
                    OnShowError?.Invoke(ex.Message);
                }
            }

            return Task.CompletedTask;
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
            dialog.Owner = App.Current.MainWindow;

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
            if (_profileNames.Any(x => x.ProfileName == profileName))
                SetCurrentProfile(profileName, forceNotify: true);
        }

        public string FindProfilesUsingApplication(string applicationName)
        {
            return _scannerService.FindProfilesUsingApplication(applicationName);
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
                SetCurrentProfile(_profileNames[0].ProfileName, forceNotify: true);
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

        private void ToggleAppearanceMenu()
        {
            if (IsAppearanceMenuOpen)
            {
                IsAppearanceMenuOpen = false;
            }
            else if ((DateTime.UtcNow - _appearanceMenuClosedAt).TotalMilliseconds > 300)
            {
                IsAppearanceMenuOpen = true;
            }
        }

        private void ApplyTheme(string theme)
        {
            if (string.IsNullOrEmpty(theme) || App.Bootstrapper == null)
                return;

            var themeManager = App.Bootstrapper.Resolve<ThemeManager>();
            var themeName = theme == "Dark" ? "DarkTheme.xaml" : "SlateLightTheme.xaml";
            themeManager.SetTheme(themeName);
            IsDarkTheme = themeManager.IsDarkTheme;
            RefreshCurrentProfile();
        }

        private void ApplyDensity(string density)
        {
            if (string.IsNullOrEmpty(density) || App.Bootstrapper == null)
                return;

            var themeManager = App.Bootstrapper.Resolve<ThemeManager>();
            themeManager.SetDensity(density);
            IsCompactDensity = themeManager.IsCompactDensity;
        }

        private void ApplyBackdropMode(string mode)
        {
            var normalizedMode = NormalizeBackdropMode(mode);
            if (string.IsNullOrEmpty(normalizedMode))
                return;

            if (NvapiDrsWrapper.Instance.IsMockMode)
            {
                NvapiDrsWrapper.Instance.SetMockWin11BackdropMode(normalizedMode);
            }
            else
            {
                var settings = UserSettings.LoadSettings();
                settings.Win11BackdropMode = normalizedMode;
                settings.SaveSettings();
            }

            _currentBackdropMode = normalizedMode;
            NotifyBackdropModeChanged();

            if (Application.Current?.MainWindow != null)
                WindowBackdropHelper.TryApplyTo(Application.Current.MainWindow);
        }

        private void RefreshBackdropMode()
        {
            var configuredMode = NvapiDrsWrapper.Instance.IsMockMode
                ? NvapiDrsWrapper.Instance.GetMockWin11BackdropMode()
                : UserSettings.LoadSettings().Win11BackdropMode;

            _currentBackdropMode = NormalizeBackdropMode(configuredMode);
            NotifyBackdropModeChanged();
        }

        private static string NormalizeBackdropMode(string mode)
        {
            if (string.Equals(mode, "Default", StringComparison.OrdinalIgnoreCase))
                return "Default";

            if (string.Equals(mode, "MainWindow", StringComparison.OrdinalIgnoreCase))
                return "MainWindow";

            if (string.Equals(mode, "Acrylic", StringComparison.OrdinalIgnoreCase))
                return "Acrylic";

            if (string.Equals(mode, "Disabled", StringComparison.OrdinalIgnoreCase))
                return "Disabled";

            return "Tabbed";
        }

        private void NotifyBackdropModeChanged()
        {
            OnPropertyChanged(nameof(IsBackdropGlass));
            OnPropertyChanged(nameof(IsBackdropMica));
            OnPropertyChanged(nameof(IsBackdropMicaVariant));
            OnPropertyChanged(nameof(IsBackdropAcrylic));
            OnPropertyChanged(nameof(IsBackdropDisabled));
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
            if (IsScanning)
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

                _metaService.ResetMetaCache();
                RefreshProfilesCombo(_currentProfile);
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

    }
}
