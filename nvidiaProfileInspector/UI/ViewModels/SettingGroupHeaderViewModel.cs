namespace nvidiaProfileInspector.UI.ViewModels
{
    using System;

    /// <summary>
    /// Header row for a settings group in the flattened settings list. The settings list is
    /// rendered without WPF CollectionView grouping (hierarchical group virtualization is slow
    /// and breaks container recycling), so group headers are plain items in the flat list.
    /// </summary>
    public class SettingGroupHeaderViewModel : ViewModelBase
    {
        private readonly Action<SettingGroupHeaderViewModel> _onExpandedChanged;
        private bool _isExpanded;

        public SettingGroupHeaderViewModel(string name, bool isExpanded, Action<SettingGroupHeaderViewModel> onExpandedChanged)
        {
            Name = name ?? "";
            _isExpanded = isExpanded;
            _onExpandedChanged = onExpandedChanged;
        }

        public string Name { get; }

        // Discriminators for the shared item container style; SettingItemViewModel exposes the
        // same properties so style triggers bind cleanly on either item type.
        public bool IsGroupHeader => true;
        public bool IsModified => false;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value, nameof(IsExpanded)))
                    _onExpandedChanged?.Invoke(this);
            }
        }
    }
}
