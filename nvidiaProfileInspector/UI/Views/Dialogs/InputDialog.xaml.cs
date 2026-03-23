namespace nvidiaProfileInspector.UI.Views.Dialogs
{
    using nvidiaProfileInspector.UI.ViewModels;
    using System;
    using System.Windows;

    public partial class InputDialog : Window
    {
        private readonly InputViewModel _viewModel;

        public string InputValue => _viewModel.InputValue;

        public InputDialog(string title, string prompt, string defaultValue = "", bool allowBrowse = false, Func<string, string> validationFunc = null)
        {
            InitializeComponent();

            _viewModel = new InputViewModel(title, prompt, defaultValue, allowBrowse, validationFunc);
            DataContext = _viewModel;

            InputTextBox.SelectAll();
            InputTextBox.Focus();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select Application Absolute Path"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.InputValue = dialog.FileName;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.HasErrors)
                return;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
