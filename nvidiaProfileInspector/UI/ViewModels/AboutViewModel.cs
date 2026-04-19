namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common.Helper;
    using nvidiaProfileInspector.Common.Updates;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public class Contributor
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }
        public string ProfileUrl { get; set; }
    }

    public class AboutViewModel : ViewModelBase
    {
        private bool _isAutomaticUpdateCheckEnabled;
        private bool _isSplashScreenDisabled;
        private bool _isUpdateAvailable;
        private bool _isUpdating;
        private string _latestVersionText = "Not checked";
        private string _selectedUpdateChannel;
        private string _updateStatusText = "Choose a channel and check for updates.";
        private UpdateCheckResult _lastUpdateCheckResult;
        private readonly AppUpdateService _updateService = new AppUpdateService();
        private int _updateCheckGeneration;

        public string Version { get; }
        public ObservableCollection<Contributor> Contributors { get; } = new ObservableCollection<Contributor>();
        public ObservableCollection<string> UpdateChannels { get; } = new ObservableCollection<string> { "Release", "Pre-release" };
        public string GitHubUrl => "https://github.com/Orbmu2k/nvidiaProfileInspector/releases";
        public string GitHubRepositoryUrl => "https://github.com/Orbmu2k/nvidiaProfileInspector";

        public bool IsAutomaticUpdateCheckEnabled
        {
            get => _isAutomaticUpdateCheckEnabled;
            set
            {
                if (SetProperty(ref _isAutomaticUpdateCheckEnabled, value, nameof(IsAutomaticUpdateCheckEnabled)))
                {
                    var settings = UserSettings.LoadSettings();
                    settings.DisableUpdateCheck = !value;
                    settings.SaveSettings();
                }
            }
        }

        public bool IsSplashScreenDisabled
        {
            get => _isSplashScreenDisabled;
            set
            {
                if (SetProperty(ref _isSplashScreenDisabled, value, nameof(IsSplashScreenDisabled)))
                {
                    var settings = UserSettings.LoadSettings();
                    settings.DisableSplashScreen = value;
                    settings.SaveSettings();
                }
            }
        }

        public string SelectedUpdateChannel
        {
            get => _selectedUpdateChannel;
            set
            {
                if (SetProperty(ref _selectedUpdateChannel, value, nameof(SelectedUpdateChannel)))
                {
                    var settings = UserSettings.LoadSettings();
                    settings.UpdateChannel = UpdateChannelFormatter.ToSettingsValue(UpdateChannelFormatter.Parse(value));
                    settings.SaveSettings();
                    _lastUpdateCheckResult = null;
                    IsUpdateAvailable = false;
                    LatestVersionText = "Checking...";
                    UpdateStatusText = "Checking GitHub releases...";
                    OnPropertyChanged(nameof(CanInstallUpdate));
                    _ = RefreshUpdateStatusAsync();
                }
            }
        }

        public string LatestVersionText
        {
            get => _latestVersionText;
            set => SetProperty(ref _latestVersionText, value, nameof(LatestVersionText));
        }

        public string UpdateStatusText
        {
            get => _updateStatusText;
            set => SetProperty(ref _updateStatusText, value, nameof(UpdateStatusText));
        }

        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                if (SetProperty(ref _isUpdating, value, nameof(IsUpdating)))
                    OnPropertyChanged(nameof(CanInstallUpdate));
            }
        }

        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                if (SetProperty(ref _isUpdateAvailable, value, nameof(IsUpdateAvailable)))
                    OnPropertyChanged(nameof(CanInstallUpdate));
            }
        }

        public bool CanInstallUpdate => _lastUpdateCheckResult?.CanInstall == true && !IsUpdating;

        public ICommand OpenGitHubCommand { get; }
        public ICommand OpenGitHubRepositoryCommand { get; }
        public ICommand OpenContributorProfileCommand { get; }
        public ICommand CheckForUpdateCommand { get; }
        public ICommand InstallUpdateCommand { get; }

        public AboutViewModel(UpdateRelease latestAvailableRelease = null)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            Version = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            var settings = UserSettings.LoadSettings();
            _selectedUpdateChannel = UpdateChannelFormatter.ToDisplayName(UpdateChannelFormatter.Parse(settings.UpdateChannel));
            _isAutomaticUpdateCheckEnabled = !settings.DisableUpdateCheck;
            _isSplashScreenDisabled = settings.DisableSplashScreen;

            if (latestAvailableRelease?.Version != null)
            {
                _lastUpdateCheckResult = UpdateCheckResult.Available(latestAvailableRelease);
                ApplyUpdateCheckResult(_lastUpdateCheckResult);
            }
            else
            {
                LatestVersionText = "Checking...";
                UpdateStatusText = "Checking GitHub releases...";
                _ = RefreshUpdateStatusAsync();
            }

            OpenGitHubCommand = new RelayCommand(_ => OpenUrl(GitHubUrl));
            OpenGitHubRepositoryCommand = new RelayCommand(_ => OpenUrl(GitHubRepositoryUrl));
            OpenContributorProfileCommand = new RelayCommand(param => OpenUrl(param as string));
            CheckForUpdateCommand = new AsyncRelayCommand(CheckForUpdateAsync);
            InstallUpdateCommand = new AsyncRelayCommand(InstallUpdateAsync, () => CanInstallUpdate);

            LoadContributors();
        }

        private async Task CheckForUpdateAsync()
        {
            await RefreshUpdateStatusAsync();
        }

        private async Task RefreshUpdateStatusAsync()
        {
            IsUpdating = true;
            var generation = ++_updateCheckGeneration;
            var channel = GetSelectedUpdateChannel();
            try
            {
                UpdateStatusText = "Checking GitHub releases...";
                var result = await _updateService.CheckAsync(channel);
                if (generation != _updateCheckGeneration)
                    return;

                _lastUpdateCheckResult = result;
                ApplyUpdateCheckResult(_lastUpdateCheckResult);
            }
            finally
            {
                if (generation == _updateCheckGeneration)
                    IsUpdating = false;
            }
        }

        private async Task InstallUpdateAsync()
        {
            IsUpdating = true;
            try
            {
                UpdateStatusText = "Preparing update package...";
                if (_lastUpdateCheckResult == null || !_lastUpdateCheckResult.CanInstall)
                {
                    _lastUpdateCheckResult = await _updateService.CheckAsync(GetSelectedUpdateChannel());
                    ApplyUpdateCheckResult(_lastUpdateCheckResult);

                    if (!_lastUpdateCheckResult.CanInstall)
                        return;
                }

                await _updateService.PrepareInstallAsync(_lastUpdateCheckResult.LatestRelease);
                UpdateStatusText = "Update downloaded. Restarting application...";
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                UpdateStatusText = "Update failed.";
                MessageBoxViewModel.Show(
                    $"The update could not be installed.\r\n\r\n{ex.Message}",
                    "Update failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsUpdating = false;
            }
        }

        private UpdateChannel GetSelectedUpdateChannel()
        {
            return UpdateChannelFormatter.Parse(SelectedUpdateChannel);
        }

        private void ApplyUpdateCheckResult(UpdateCheckResult result)
        {
            IsUpdateAvailable = result?.IsUpdateAvailable == true;
            LatestVersionText = result?.LatestRelease?.DisplayText ?? "Unavailable";
            UpdateStatusText = result?.StatusMessage ?? "Could not read release information.";
            OnPropertyChanged(nameof(CanInstallUpdate));
        }

        private void LoadContributors()
        {
            Contributors.Add(new Contributor
            {
                Name = "Orbmu2k",
                Role = "Chief Architect (Emeritus)",
                AvatarUrl = "https://github.com/Orbmu2k.png",
                ProfileUrl = "https://github.com/Orbmu2k"
            });
            Contributors.Add(new Contributor
            {
                Name = "emoose",
                Role = "Lead Developer",
                AvatarUrl = "https://github.com/emoose.png",
                ProfileUrl = "https://github.com/emoose"
            });
            Contributors.Add(new Contributor
            {
                Name = "Warkratos",
                Role = "Developer & Driver Settings Researcher",
                AvatarUrl = "https://github.com/warkratos.png",
                ProfileUrl = "https://github.com/warkratos"
            });
            Contributors.Add(new Contributor
            {
                Name = "DarkStarSword",
                Role = "Contributor",
                AvatarUrl = "https://github.com/DarkStarSword.png",
                ProfileUrl = "https://github.com/DarkStarSword"
            });
        }

        private void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        }
    }
}
