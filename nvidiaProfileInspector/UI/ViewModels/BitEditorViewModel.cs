namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.CustomSettings;
    using nvidiaProfileInspector.TinyIoc;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class BitEditorViewModel : ViewModelBase
    {
        private readonly uint _settingId;
        private readonly uint _initialValue;
        private readonly string _settingName;
        private readonly DrsSettingsService _settingService;
        private readonly DrsScannerService _scannerService;
        private readonly CustomSettingNames _referenceSettings;
        private string _filterText = "";
        private string _statusMessage = "";
        private uint _currentValue;
        private string _gamePath = "";
        private List<BitItemViewModel> _bits = new List<BitItemViewModel>();
        private readonly ObservableCollection<BitItemViewModel> _filteredBits = new ObservableCollection<BitItemViewModel>();
        private bool _isSynchronizingBits;
        private int _firstVisibleRowIndex;
        private int _lastVisibleRowIndex = 31;
        private int _matchesAboveVisibleRange;
        private int _matchesBelowVisibleRange;

        public BitEditorViewModel(
            uint settingId,
            uint initialValue,
            string settingName,
            DrsSettingsService settingService,
            DrsScannerService scannerService,
            CustomSettingNames referenceSettings)
        {
            _settingId = settingId;
            _initialValue = initialValue;
            _settingName = settingName;
            _currentValue = initialValue;
            _settingService = settingService;
            _scannerService = scannerService;
            _referenceSettings = referenceSettings;

            Title = $"Bit Value Editor - {settingName}";

            if (_scannerService == null)
                LoadDesignTimeData();
            else
                InitializeBits();

            ApplyCommand = new RelayCommand(ApplyToProfile);
            ApplyAndLaunchCommand = new AsyncRelayCommand(ApplyAndLaunchAsync);
        }

        public static BitEditorViewModel Create(uint settingId, uint initialValue, string settingName)
        {
            return new BitEditorViewModel(
                settingId,
                initialValue,
                settingName,
                TinyIoC.Resolve<DrsSettingsService>(),
                TinyIoC.Resolve<DrsScannerService>(),
                TinyIoC.Resolve<CustomSettingNames>());
        }

        public string Title { get; }
        public List<BitItemViewModel> Bits => _bits;
        public ObservableCollection<BitItemViewModel> FilteredBits => _filteredBits;
        public ICommand ApplyCommand { get; }
        public ICommand ApplyAndLaunchCommand { get; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value, nameof(FilterText)))
                    RefreshFilteredBits();
            }
        }

        public uint CurrentValue
        {
            get => _currentValue;
            set
            {
                if (SetProperty(ref _currentValue, value, nameof(CurrentValue)))
                {
                    _isSynchronizingBits = true;
                    foreach (var bit in _bits)
                        bit.IsChecked = ((value >> bit.BitIndex) & 1) == 1;
                    _isSynchronizingBits = false;
                    OnPropertyChanged(nameof(CurrentValueHex));
                }
            }
        }

        public string CurrentValueHex => "0x" + _currentValue.ToString("X8");

        public string GamePath
        {
            get => _gamePath;
            set => SetProperty(ref _gamePath, value, nameof(GamePath));
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value, nameof(StatusMessage));
        }

        public Action<uint> OnValueChanged { get; set; }
        public Action<string> OnShowMessage { get; set; }

        public int MatchesAboveVisibleRange
        {
            get => _matchesAboveVisibleRange;
            private set
            {
                if (SetProperty(ref _matchesAboveVisibleRange, value, nameof(MatchesAboveVisibleRange)))
                    OnPropertyChanged(nameof(HasMatchesAboveVisibleRange));
            }
        }

        public int MatchesBelowVisibleRange
        {
            get => _matchesBelowVisibleRange;
            private set
            {
                if (SetProperty(ref _matchesBelowVisibleRange, value, nameof(MatchesBelowVisibleRange)))
                    OnPropertyChanged(nameof(HasMatchesBelowVisibleRange));
            }
        }

        public bool HasMatchesAboveVisibleRange => MatchesAboveVisibleRange > 0;
        public bool HasMatchesBelowVisibleRange => MatchesBelowVisibleRange > 0;

        private void InitializeBits()
        {
            _bits.Clear();
            var cachedSetting = _scannerService.CachedSettings.FirstOrDefault(x => x.SettingId == _settingId);
            for (int bit = 0; bit < 32; bit++)
            {
                var bitVm = new BitItemViewModel(bit)
                {
                    IsChecked = ((_currentValue >> bit) & 1) == 1,
                    MaskDescription = GetMaskDescription(bit)
                };

                if (cachedSetting != null)
                {
                    var profileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var cachedValue in cachedSetting.SettingValues)
                    {
                        if (((cachedValue.Value >> bit) & 1) == 1)
                        {
                            bitVm.ProfileCount += (int)cachedValue.ValueProfileCount;
                            foreach (var profileName in SplitProfileNames(cachedValue.ProfileNames?.ToString()))
                                profileNames.Add(profileName);
                        }
                    }

                    bitVm.ProfileNames = string.Join(", ", profileNames.OrderBy(x => x));
                }

                _bits.Add(bitVm);
                bitVm.PropertyChanged += Bit_PropertyChanged;
            }
            RefreshFilteredBits();
        }

        private static IEnumerable<string> SplitProfileNames(string profileNames)
        {
            if (string.IsNullOrWhiteSpace(profileNames))
                yield break;

            foreach (var profileName in profileNames
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                yield return profileName;
            }
        }

        private void LoadDesignTimeData()
        {
            _bits = new List<BitItemViewModel>
            {
                new BitItemViewModel(0)
                {
                    IsChecked = true,
                    MaskDescription = "Enable compatibility profile override",
                    ProfileCount = 18,
                    ProfileNames = "Cyberpunk 2077, Control, Alan Wake 2"
                },
                new BitItemViewModel(2)
                {
                    IsChecked = true,
                    MaskDescription = "Force alternate depth resolve path",
                    ProfileCount = 9,
                    ProfileNames = "Red Dead Redemption 2, Starfield"
                },
                new BitItemViewModel(3)
                {
                    IsChecked = true,
                    MaskDescription = "Use driver AA heuristic for deferred renderer",
                    ProfileCount = 14,
                    ProfileNames = "The Witcher 3, Hogwarts Legacy, Dragon Age"
                },
                new BitItemViewModel(5)
                {
                    IsChecked = false,
                    MaskDescription = "Preserve transparency supersampling behavior",
                    ProfileCount = 6,
                    ProfileNames = "Batman Arkham Knight, Fallout 4"
                },
                new BitItemViewModel(7)
                {
                    IsChecked = false,
                    MaskDescription = "Skip engine-specific post-process workaround",
                    ProfileCount = 3,
                    ProfileNames = "Sample UE4 Profile"
                },
                new BitItemViewModel(12)
                {
                    IsChecked = true,
                    MaskDescription = "Prefer legacy SLI compatibility branch",
                    ProfileCount = 11,
                    ProfileNames = "Crysis 3, Metro Exodus, Watch Dogs 2"
                }
            };

            for (int bit = 0; bit < 32; bit++)
            {
                if (_bits.Any(x => x.BitIndex == bit))
                    continue;

                _bits.Add(new BitItemViewModel(bit)
                {
                    IsChecked = ((_currentValue >> bit) & 1) == 1,
                    MaskDescription = bit % 2 == 0 ? "Reserved / unknown" : string.Empty,
                    ProfileCount = 0,
                    ProfileNames = string.Empty
                });
            }

            _bits = _bits.OrderBy(x => x.BitIndex).ToList();
            foreach (var bit in _bits)
                bit.PropertyChanged += Bit_PropertyChanged;

            _gamePath = @"C:\Games\SampleGame\SampleGame.exe";
            RefreshFilteredBits();
            OnPropertyChanged(nameof(CurrentValueHex));
            OnPropertyChanged(nameof(GamePath));
        }

        private void RefreshFilteredBits()
        {
            var filter = (_filterText ?? string.Empty).Trim();
            foreach (var bit in _bits)
                bit.VisibleProfileNames = GetVisibleProfileNames(bit, filter);

            _filteredBits.Clear();
            foreach (var bit in _bits)
                _filteredBits.Add(bit);

            OnPropertyChanged(nameof(Bits));
            OnPropertyChanged(nameof(FilteredBits));
            UpdateViewportMatchIndicators();
        }

        private static string GetVisibleProfileNames(BitItemViewModel bit, string filter)
        {
            if (bit == null)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(filter))
                return bit.ProfileNames ?? string.Empty;

            var matches = SplitProfileNames(bit.ProfileNames)
                .Where(profileName => profileName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            return string.Join(", ", matches);
        }

        private void Bit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isSynchronizingBits || e.PropertyName != nameof(BitItemViewModel.IsChecked))
                return;

            if (sender is BitItemViewModel bit)
                SetBitState(bit.BitIndex, bit.IsChecked);
        }

        public void UpdateVisibleRange(int firstVisibleRowIndex, int lastVisibleRowIndex)
        {
            var maxIndex = Math.Max(0, _filteredBits.Count - 1);
            _firstVisibleRowIndex = Math.Max(0, Math.Min(firstVisibleRowIndex, maxIndex));
            _lastVisibleRowIndex = Math.Max(_firstVisibleRowIndex, Math.Min(lastVisibleRowIndex, maxIndex));
            UpdateViewportMatchIndicators();
        }

        private void UpdateViewportMatchIndicators()
        {
            if (string.IsNullOrWhiteSpace(_filterText))
            {
                MatchesAboveVisibleRange = 0;
                MatchesBelowVisibleRange = 0;
                return;
            }

            var above = 0;
            var below = 0;

            for (int index = 0; index < _filteredBits.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(_filteredBits[index].VisibleProfileNames))
                    continue;

                if (index < _firstVisibleRowIndex)
                    above++;
                else if (index > _lastVisibleRowIndex)
                    below++;
            }

            MatchesAboveVisibleRange = above;
            MatchesBelowVisibleRange = below;
        }

        private string GetMaskDescription(int bit)
        {
            if (_referenceSettings?.Settings == null)
                return "";

            var setting = _referenceSettings.Settings.FirstOrDefault(s => s.SettingId == _settingId);
            if (setting?.SettingValues == null)
                return "";

            uint mask = (uint)1 << bit;
            foreach (var sv in setting.SettingValues)
            {
                if (sv.SettingValue == mask)
                {
                    var name = sv.UserfriendlyName;
                    if (name.Contains("("))
                        name = name.Substring(0, name.IndexOf("(") - 1);
                    return name;
                }
            }

            return "";
        }

        public void ToggleBit(int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 32)
                return;

            var bit = _bits.FirstOrDefault(x => x.BitIndex == bitIndex);
            if (bit != null)
                bit.IsChecked = !bit.IsChecked;
        }

        private void SetBitState(int bitIndex, bool isEnabled)
        {
            if (bitIndex < 0 || bitIndex >= 32)
                return;

            uint mask = (uint)1 << bitIndex;
            var updatedValue = isEnabled
                ? (_currentValue | mask)
                : (_currentValue & ~mask);

            if (updatedValue != _currentValue)
                CurrentValue = updatedValue;
        }

        public void ApplyToProfile()
        {
            try
            {
                // Note: This would need the current profile from MainViewModel
                // For now, just update the display
                OnValueChanged?.Invoke(_currentValue);
                StatusMessage = "Value applied to profile.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                OnShowMessage?.Invoke(StatusMessage);
            }
        }

        public async Task ApplyAndLaunchAsync()
        {
            ApplyToProfile();

            if (!string.IsNullOrEmpty(GamePath) && System.IO.File.Exists(GamePath))
            {
                await Task.Delay(500);
                System.Diagnostics.Process.Start(GamePath);
                return;
            }

            if (!string.IsNullOrWhiteSpace(GamePath))
                StatusMessage = "Game executable not found.";
        }
    }

    public class BitItemViewModel : ViewModelBase
    {
        private bool _isChecked;
        private int _profileCount;
        private string _profileNames = "";
        private string _visibleProfileNames = "";
        private string _maskDescription;

        public BitItemViewModel(int bitIndex)
        {
            BitIndex = bitIndex;
        }

        public int BitIndex { get; }
        public string BitLabel => $"#{BitIndex:D2}";

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value, nameof(IsChecked));
        }

        public int ProfileCount
        {
            get => _profileCount;
            set => SetProperty(ref _profileCount, value, nameof(ProfileCount));
        }

        public string ProfileNames
        {
            get => _profileNames;
            set => SetProperty(ref _profileNames, value, nameof(ProfileNames));
        }

        public string VisibleProfileNames
        {
            get => _visibleProfileNames;
            set => SetProperty(ref _visibleProfileNames, value, nameof(VisibleProfileNames));
        }

        public string MaskDescription
        {
            get => _maskDescription;
            set => SetProperty(ref _maskDescription, value, nameof(MaskDescription));
        }
    }
}
