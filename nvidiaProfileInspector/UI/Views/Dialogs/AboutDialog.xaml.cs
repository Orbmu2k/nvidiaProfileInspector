using nvidiaProfileInspector.UI.ViewModels;
using System.Windows;

namespace nvidiaProfileInspector.UI.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog(bool isUpdateAvailable = false)
        {
            InitializeComponent();
            DataContext = new AboutViewModel(isUpdateAvailable);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
