using System.Windows;
using System.Windows.Input;

namespace nvidiaProfileInspector.Common.Helper
{
    public class TextBoxHelper
    {
        public static readonly DependencyProperty ClearCommandProperty =
            DependencyProperty.RegisterAttached(
                "ClearCommand",
                typeof(ICommand),
                typeof(TextBoxHelper),
                new PropertyMetadata(null));

        public static ICommand GetClearCommand(DependencyObject obj) => (ICommand)obj.GetValue(ClearCommandProperty);
        public static void SetClearCommand(DependencyObject obj, ICommand value) => obj.SetValue(ClearCommandProperty, value);

        public static readonly DependencyProperty ClearCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "ClearCommandParameter",
                typeof(object),
                typeof(TextBoxHelper),
                new PropertyMetadata(null));

        public static object GetClearCommandParameter(DependencyObject obj) => obj.GetValue(ClearCommandParameterProperty);
        public static void SetClearCommandParameter(DependencyObject obj, object value) => obj.SetValue(ClearCommandParameterProperty, value);

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxHelper),
                new PropertyMetadata(string.Empty));

        public static string GetPlaceholder(DependencyObject obj) => (string)obj.GetValue(PlaceholderProperty);
        public static void SetPlaceholder(DependencyObject obj, string value) => obj.SetValue(PlaceholderProperty, value);
    }
}
