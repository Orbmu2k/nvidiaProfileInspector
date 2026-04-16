namespace nvidiaProfileInspector.UI.ViewModels
{
    using Microsoft.Win32;
    using nvidiaProfileInspector.Common;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;

    public class ExportProfilesViewModel : ViewModelBase
    {
        private readonly DrsImportService _importService;
        private readonly DrsScannerService _scannerService;
        private readonly ObservableCollection<ProfileExportItem> _profiles = new ObservableCollection<ProfileExportItem>();

        public ExportProfilesViewModel(DrsImportService importService, DrsScannerService scannerService)
        {
            _importService = importService;
            _scannerService = scannerService;
            LoadProfiles();
        }

        public ObservableCollection<ProfileExportItem> Profiles => _profiles;

        public bool IncludePredefined { get; set; }

        public ICommand SelectAllCommand => new RelayCommand(_ => SelectAll());
        public ICommand SelectNoneCommand => new RelayCommand(_ => SelectNone());
        public ICommand InvertSelectionCommand => new RelayCommand(_ => InvertSelection());
        public ICommand ExportCommand => new RelayCommand(_ => Export(), _ => Profiles.Any(x => x.IsSelected));
        public ICommand CancelCommand { get; }

        public System.Action CloseAction { get; set; }
        public System.Action<string> OnShowMessage { get; set; }

        private void LoadProfiles()
        {
            foreach (var profile in _scannerService.ModifiedProfiles)
            {
                _profiles.Add(new ProfileExportItem { ProfileName = profile, IsSelected = true });
            }
        }

        private void SelectAll()
        {
            foreach (var profile in _profiles)
                profile.IsSelected = true;
        }

        private void SelectNone()
        {
            foreach (var profile in _profiles)
                profile.IsSelected = false;
        }

        private void InvertSelection()
        {
            foreach (var profile in _profiles)
                profile.IsSelected = !profile.IsSelected;
        }

        private void Export()
        {
            var selectedProfiles = _profiles.Where(x => x.IsSelected).Select(x => x.ProfileName).ToList();
            if (!selectedProfiles.Any())
            {
                OnShowMessage?.Invoke("Nothing to export");
                return;
            }

            var dialog = new SaveFileDialog
            {
                DefaultExt = "*.nip",
                Filter = "NVIDIA PROFILE INSPECTOR Profiles|*.nip"
            };

            if (dialog.ShowDialog() == true)
            {
                _importService.ExportProfiles(selectedProfiles, dialog.FileName, IncludePredefined);
                OnShowMessage?.Invoke("Export succeeded!");
                CloseAction?.Invoke();
            }
        }
    }

    public class ProfileExportItem : ViewModelBase
    {
        private bool _isSelected;

        public string ProfileName { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value, nameof(IsSelected));
        }
    }
}
