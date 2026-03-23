using System.Windows;
using System.Windows.Controls;

namespace nvidiaProfileInspector.UI.Controls
{
    public class NotificationSnackbar : ContentControl
    {
        static NotificationSnackbar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationSnackbar), new FrameworkPropertyMetadata(typeof(NotificationSnackbar)));
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(NotificationSnackbar),
            new PropertyMetadata(string.Empty));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(NotificationSnackbar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsActiveChanged));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type),
            typeof(string),
            typeof(NotificationSnackbar),
            new PropertyMetadata("Information"));

        public string Type
        {
            get => (string)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NotificationSnackbar snackbar)
            {
                snackbar.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
