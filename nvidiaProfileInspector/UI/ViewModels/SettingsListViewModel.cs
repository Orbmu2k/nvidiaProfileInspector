namespace nvidiaProfileInspector.UI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Data;

    public class SettingsListViewModel : ViewModelBase
    {
        private readonly ObservableCollection<SettingItemViewModel> _allItems = new ObservableCollection<SettingItemViewModel>();
        private readonly CollectionViewSource _collectionViewSource;
        private string _filterText = "";
        private SettingItemViewModel _selectedItem;

        public SettingsListViewModel()
        {
            _collectionViewSource = new CollectionViewSource { Source = _allItems };
            _collectionViewSource.Filter += CollectionViewSource_Filter;
        }

        public ICollectionView ItemsView => _collectionViewSource.View;

        public ObservableCollection<SettingItemViewModel> AllItems => _allItems;

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value, nameof(FilterText)))
                    RefreshFilter();
            }
        }

        public SettingItemViewModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value, nameof(SelectedItem));
        }

        public void AddItem(SettingItemViewModel item)
        {
            _allItems.Add(item);
        }

        public void Clear()
        {
            _allItems.Clear();
        }

        private void RefreshFilter()
        {
            ItemsView.Refresh();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_filterText))
            {
                e.Accepted = true;
                return;
            }

            if (e.Item is SettingItemViewModel item)
            {
                var matches = item.DisplayName.IndexOf(_filterText, System.StringComparison.OrdinalIgnoreCase) >= 0;
                if (!matches && !string.IsNullOrEmpty(item.AlternateNames))
                    matches = item.AlternateNames.IndexOf(_filterText, System.StringComparison.OrdinalIgnoreCase) >= 0;
                e.Accepted = matches;
            }
        }

        public SettingItemViewModel FindBySettingId(uint settingId)
        {
            return _allItems.FirstOrDefault(x => x.SettingId == settingId);
        }

    }
}
