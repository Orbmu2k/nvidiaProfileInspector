namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;

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

        public bool HasSelectedProfiles => Profiles.Any(x => x.IsSelected);

        public List<string> SelectedProfileNames => Profiles
            .Where(x => x.IsSelected)
            .Select(x => x.ProfileName)
            .ToList();

        private void LoadProfiles()
        {
            foreach (var profile in _scannerService.ModifiedProfiles)
            {
                _profiles.Add(new ProfileExportItem { ProfileName = profile, IsSelected = true });
            }
        }

        public void SelectAll()
        {
            foreach (var profile in _profiles)
                profile.IsSelected = true;
        }

        public void SelectNone()
        {
            foreach (var profile in _profiles)
                profile.IsSelected = false;
        }

        public void InvertSelection()
        {
            foreach (var profile in _profiles)
                profile.IsSelected = !profile.IsSelected;
        }

        public void ExportSelectedProfiles(string filename)
        {
            _importService.ExportProfiles(SelectedProfileNames, filename, IncludePredefined);
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
