using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace nvidiaProfileInspector.Common.Helper
{
    public class ComboBoxHelper
    {
        private ComboBoxHelper() { }

        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.RegisterAttached(
                "FilterText",
                typeof(string),
                typeof(ComboBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static string GetFilterText(DependencyObject obj)
        {
            return (string)obj.GetValue(FilterTextProperty);
        }

        public static void SetFilterText(DependencyObject obj, string value)
        {
            obj.SetValue(FilterTextProperty, value);
        }

        private static ICommand _clearCommand;
        public static ICommand ClearCommand => _clearCommand ?? (_clearCommand = new ClearCommandImpl());

        private class ClearCommandImpl : ICommand
        {
            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                if (parameter is DependencyObject obj)
                    SetFilterText(obj, string.Empty);
            }

            public event System.EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }

        public static readonly DependencyProperty AutoFocusSearchProperty =
            DependencyProperty.RegisterAttached(
                "AutoFocusSearch",
                typeof(bool),
                typeof(ComboBoxHelper),
                new PropertyMetadata(false, OnAutoFocusSearchChanged));

        public static bool GetAutoFocusSearch(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoFocusSearchProperty);
        }

        public static void SetAutoFocusSearch(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoFocusSearchProperty, value);
        }

        private static void OnAutoFocusSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox && (bool)e.NewValue)
            {
                comboBox.DropDownOpened += ComboBox_DropDownOpened;
                comboBox.DropDownClosed += ComboBox_DropDownClosed;
            }
        }

        private static void ComboBox_DropDownOpened(object sender, System.EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null)
                return;

            comboBox.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                var searchBox = FindSearchBox(comboBox);
                if (searchBox != null)
                {
                    searchBox.Text = string.Empty;
                    searchBox.Focus();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private static void ComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null)
                return;

            var searchBox = FindSearchBox(comboBox);
            if (searchBox != null)
            {
                searchBox.Text = string.Empty;
            }
        }

        private static TextBox FindSearchBox(ComboBox comboBox)
        {
            if (comboBox.Template == null)
                return null;

            var popup = comboBox.Template.FindName("Popup", comboBox) as Popup;
            if (popup == null)
                return null;

            var popupContent = popup.Child;
            if (popupContent == null)
                return null;

            return FindVisualChild<TextBox>(popupContent);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }
    }
}
