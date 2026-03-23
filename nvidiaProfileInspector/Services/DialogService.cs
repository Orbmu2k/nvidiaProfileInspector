namespace nvidiaProfileInspector.Services
{
    using nvidiaProfileInspector.UI.ViewModels;
    using System;
    using System.Windows;

    public interface IDialogService
    {
        MessageBoxResult ShowMessage(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information);
        bool? ShowInputDialog(string title, string prompt, ref string value, bool allowBrowse = false, Func<string, string> validationFunc = null);
        string ShowSaveFileDialog(string filter, string defaultExt, string fileName);
        string ShowOpenFileDialog(string filter, string defaultExt);
    }

    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowMessage(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            return MessageBoxViewModel.Show(message, title, button, icon);
        }

        public bool? ShowInputDialog(string title, string prompt, ref string value, bool allowBrowse = false, Func<string, string> validationFunc = null)
        {
            var dialog = new UI.Views.Dialogs.InputDialog(title, prompt, value, allowBrowse, validationFunc);
            var mainWindow = App.Current.MainWindow;
            if (mainWindow != null)
                dialog.Owner = mainWindow;
            var result = dialog.ShowDialog();
            if (result == true)
                value = dialog.InputValue;
            return result;
        }

        public string ShowSaveFileDialog(string filter, string defaultExt, string fileName)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                FileName = fileName
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string ShowOpenFileDialog(string filter, string defaultExt)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
