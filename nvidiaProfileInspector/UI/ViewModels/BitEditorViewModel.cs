namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.CustomSettings;
    using nvidiaProfileInspector.TinyIoc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class BitEditorViewModel : ViewModelBase
    {
        private readonly uint _settingId;
        private readonly uint _initialValue;
        private readonly string _settingName;
        private readonly DrsSettingsService _settingService;
        private readonly DrsScannerService _scannerService;
        private readonly CustomSettingNames _referenceSettings;
        private string _filterText = "";
        private uint _currentValue;
        private string _gamePath = "";
        private List<BitItemViewModel> _bits = new List<BitItemViewModel>();

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
            InitializeBits();
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

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value, nameof(FilterText)))
                    UpdateBits();
            }
        }

        public uint CurrentValue
        {
            get => _currentValue;
            set
            {
                if (SetProperty(ref _currentValue, value, nameof(CurrentValue)))
                {
                    foreach (var bit in _bits)
                        bit.IsChecked = ((value >> bit.BitIndex) & 1) == 1;
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

        public Action<uint> OnValueChanged { get; set; }
        public Action<string> OnShowMessage { get; set; }

        private void InitializeBits()
        {
            _bits.Clear();
            for (int bit = 0; bit < 32; bit++)
            {
                var bitVm = new BitItemViewModel(bit)
                {
                    IsChecked = ((_currentValue >> bit) & 1) == 1,
                    MaskDescription = GetMaskDescription(bit)
                };

                var cachedSetting = _scannerService.CachedSettings.FirstOrDefault(x => x.SettingId == _settingId);
                if (cachedSetting != null)
                {
                    foreach (var cachedValue in cachedSetting.SettingValues)
                    {
                        if (((cachedValue.Value >> bit) & 1) == 1)
                            bitVm.ProfileCount += (int)cachedValue.ValueProfileCount;
                    }
                }

                _bits.Add(bitVm);
            }
            OnPropertyChanged(nameof(Bits));
        }

        private void UpdateBits()
        {
            OnPropertyChanged(nameof(Bits));
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

            uint mask = (uint)1 << bitIndex;
            if ((_currentValue & mask) == mask)
                _currentValue &= ~mask;
            else
                _currentValue |= mask;

            CurrentValue = _currentValue;
        }

        public void ApplyToProfile()
        {
            try
            {
                // Note: This would need the current profile from MainViewModel
                // For now, just update the display
                OnValueChanged?.Invoke(_currentValue);
            }
            catch (Exception ex)
            {
                OnShowMessage?.Invoke($"Error: {ex.Message}");
            }
        }

        public async Task ApplyAndLaunchAsync()
        {
            ApplyToProfile();

            if (!string.IsNullOrEmpty(GamePath) && System.IO.File.Exists(GamePath))
            {
                await Task.Delay(500);
                System.Diagnostics.Process.Start(GamePath);
            }
        }
    }

    public class BitItemViewModel : ViewModelBase
    {
        private bool _isChecked;
        private int _profileCount;
        private string _profileNames = "";
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

        public string MaskDescription
        {
            get => _maskDescription;
            set => SetProperty(ref _maskDescription, value, nameof(MaskDescription));
        }
    }
}
