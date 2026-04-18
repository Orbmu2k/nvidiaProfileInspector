using nvidiaProfileInspector.Common.Updates;
using nvidiaProfileInspector.UI.ViewModels;
using System.Windows;

namespace nvidiaProfileInspector.UI.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog(UpdateRelease latestAvailableRelease = null)
        {
            InitializeComponent();
            DataContext = new AboutViewModel(latestAvailableRelease);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
