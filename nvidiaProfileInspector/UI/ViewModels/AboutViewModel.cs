namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common.Helper;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
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

        public string Version { get; }
        public ObservableCollection<Contributor> Contributors { get; } = new ObservableCollection<Contributor>();
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

        public ICommand OpenGitHubCommand { get; }
        public ICommand OpenGitHubRepositoryCommand { get; }
        public ICommand OpenContributorProfileCommand { get; }
        public bool IsUpdateAvailable { get; set; }

        public AboutViewModel(bool isUpdateAvailable = false)
        {
            IsUpdateAvailable = isUpdateAvailable;
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            Version = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            IsAutomaticUpdateCheckEnabled = !UserSettings.LoadSettings().DisableUpdateCheck;

            OpenGitHubCommand = new RelayCommand(_ => OpenUrl(GitHubUrl));
            OpenGitHubRepositoryCommand = new RelayCommand(_ => OpenUrl(GitHubRepositoryUrl));
            OpenContributorProfileCommand = new RelayCommand(param => OpenUrl(param as string));

            LoadContributors();
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
                Name = "renannmp",
                Role = "Developer & Driver Settings Researcher",
                AvatarUrl = "https://github.com/renannmp.png",
                ProfileUrl = "https://github.com/renannmp"
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
