namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.Meta;
    using System.Collections.Generic;
    using System.Linq;

    public class SettingValueItem
    {
        public string ValueName { get; set; }
        public SettingMetaSource Source { get; set; }

        public override string ToString()
        {
            return ValueName;
        }
    }

    public class SettingItemViewModel : ViewModelBase
    {
        private SettingItem _item;
        private string _selectedValue;
        private bool _isModified;
        private string _originalValue;
        private bool _isFavorite;
        private List<SettingValueItem> _cachedValueNameItems;
        private string _cachedGroupNameForDisplay;

        public SettingItemViewModel(SettingItem item)
        {
            _item = item;
            _originalValue = item.ValueText;
            _selectedValue = item.ValueText;
            UpdateGroupNameForDisplay();
            UpdateDisplayName();
        }

        private void UpdateGroupNameForDisplay()
        {
            if (_isFavorite)
                _cachedGroupNameForDisplay = "⭐ Favorites";
            else
                _cachedGroupNameForDisplay = GroupName;
        }

        private void UpdateDisplayName()
        {
            if (IsSettingHidden)
                DisplayName = "[H] " + SettingText;
            else
                DisplayName = SettingText;
        }

        private void InvalidateValueNameItems()
        {
            _cachedValueNameItems = null;
            OnPropertyChanged(nameof(ValueNameItems));
        }

        public void UpdateValueSources(
            List<SettingValue<uint>> dwordValues,
            List<SettingValue<string>> stringValues,
            List<SettingValue<byte[]>> binaryValues)
        {
            DwordValues = dwordValues;
            StringValues = stringValues;
            BinaryValues = binaryValues;
            InvalidateValueNameItems();
        }

        public List<SettingValueItem> ValueNameItems
        {
            get
            {
                if (_cachedValueNameItems == null)
                {
                    var items = new List<SettingValueItem>();
                    if (DwordValues != null)
                    {
                        items.AddRange(DwordValues.Where(v => !string.IsNullOrEmpty(v.ValueName))
                        .Select(v => new SettingValueItem { ValueName = v.ValueName, Source = v.ValueSource }));
                    }
                    else if (StringValues != null)
                    {
                        items.AddRange(StringValues.Where(v => !string.IsNullOrEmpty(v.ValueName))
                        .Select(v => new SettingValueItem { ValueName = v.ValueName, Source = v.ValueSource }));
                    }
                    else if (BinaryValues != null)
                    {
                        items.AddRange(BinaryValues.Where(v => !string.IsNullOrEmpty(v.ValueName))
                        .Select(v => new SettingValueItem { ValueName = v.ValueName, Source = v.ValueSource }));
                    }
                    _cachedValueNameItems = items;
                }
                return _cachedValueNameItems;
            }
        }

        public SettingItemViewModel() : this(new SettingItem
        {
            SettingId = 0x12345678,
            SettingText = "Sample Setting",
            ValueText = "Enabled",
            ValueRaw = "0x00000001",
            GroupName = "Design Time",
            State = SettingState.NvidiaSetting
        })
        {
        }

        public uint SettingId => _item.SettingId;
        public string SettingIdHex => string.Format("0x{0:X8}", _item.SettingId);
        public string SettingText => _item.SettingText;
        public string ValueText => _item.ValueText;
        public string ValueRaw => _item.ValueRaw;
        public string GroupName => _item.GroupName;

        // Sort key for group name - empty names sort last using Unicode max value
        public string GroupNameSortKey => string.IsNullOrEmpty(GroupName) ? "Z" : GroupName;

        public string AlternateNames => _item.AlternateNames;
        public SettingState State => _item.State;
        public bool IsStringValue => _item.IsStringValue;
        public bool IsApiExposed => _item.IsApiExposed;
        public bool IsSettingHidden => _item.IsSettingHidden;

        public string DisplayName { get; private set; }

        public bool IsUserDefined => State == SettingState.UserdefinedSetting;
        public bool IsNvidiaSetting => State == SettingState.NvidiaSetting;
        public bool IsGlobalSetting => State == SettingState.GlobalSetting;

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (SetProperty(ref _isFavorite, value, nameof(IsFavorite)))
                {
                    UpdateGroupNameForDisplay();
                    OnPropertyChanged(nameof(FavoriteStarVisibility));
                    OnPropertyChanged(nameof(GroupNameForDisplay));
                }
            }
        }

        public string FavoriteStarVisibility => _isFavorite ? "Visible" : "Hidden";

        public string GroupNameForDisplay => _cachedGroupNameForDisplay;

        public string SelectedValue
        {
            get => _selectedValue;
            set
            {
                if (SetProperty(ref _selectedValue, value, nameof(SelectedValue)))
                {
                    IsModified = !string.IsNullOrEmpty(value) && value != _originalValue;
                    OnPropertyChanged(nameof(HasChanged));
                }
            }
        }

        public bool HasChanged => !string.IsNullOrEmpty(_selectedValue) && _selectedValue != _originalValue;

        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value, nameof(IsModified));
        }

        public SettingItem OriginalItem
        {
            get => _item;
            set => SetProperty(ref _item, value, nameof(OriginalItem));
        }
        public List<SettingValue<uint>> DwordValues { get; set; }
        public List<SettingValue<string>> StringValues { get; set; }
        public List<SettingValue<byte[]>> BinaryValues { get; set; }
    }
}
