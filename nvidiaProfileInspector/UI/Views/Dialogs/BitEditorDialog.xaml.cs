namespace nvidiaProfileInspector.UI.Views.Dialogs
{
    using Microsoft.Win32;
    using nvidiaProfileInspector.UI.ViewModels;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public partial class BitEditorDialog : Window
    {
        private readonly BitEditorViewModel _viewModel;

        public Action<uint> OnValueChanged { get; set; }

        public BitEditorDialog(uint settingId, uint initialValue, string settingName)
        {
            InitializeComponent();

            _viewModel = BitEditorViewModel.Create(settingId, initialValue, settingName);
            _viewModel.OnValueChanged = (value) => OnValueChanged?.Invoke(value);
            _viewModel.OnShowMessage = (msg) => StatusTextBlock.Text = msg;

            Title = _viewModel.Title;
            CurrentValueTextBox.Text = _viewModel.CurrentValueHex;

            BitsDataGrid.ItemsSource = _viewModel.Bits;

            foreach (var bit in _viewModel.Bits)
            {
                bit.PropertyChanged += Bit_PropertyChanged;
            }

            CurrentValueTextBox.TextChanged += (s, e) => UpdateCurrentValueDisplay();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filter = FilterTextBox.Text.ToLowerInvariant();
            var filtered = _viewModel.Bits.Where(b =>
                string.IsNullOrEmpty(filter) ||
                b.MaskDescription.ToLowerInvariant().Contains(filter) ||
                b.BitLabel.Contains(filter));

            BitsDataGrid.ItemsSource = new ObservableCollection<BitItemViewModel>(filtered);
        }

        private void Bit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BitItemViewModel.IsChecked))
            {
                var bit = sender as BitItemViewModel;
                if (bit != null)
                {
                    uint mask = (uint)1 << bit.BitIndex;
                    if (bit.IsChecked)
                        _viewModel.CurrentValue |= mask;
                    else
                        _viewModel.CurrentValue &= ~mask;

                    UpdateCurrentValueDisplay();
                }
            }
        }

        private void UpdateCurrentValueDisplay()
        {
            CurrentValueTextBox.Text = $"0x{_viewModel.CurrentValue:X8}";
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ApplyToProfile();
            StatusTextBlock.Text = "Value applied to profile.";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Applications|*.exe",
                Title = "Select Game"
            };

            if (dialog.ShowDialog() == true)
            {
                GamePathTextBox.Text = dialog.FileName;
            }
        }

        private async void ApplyLaunch_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ApplyAndLaunchAsync();
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
    }
}
