using System.Windows;
using System.Windows.Controls;

namespace nvidiaProfileInspector.UI.Controls
{
    public class PlaceholderClearTextBox : TextBox
    {
        private Button? _clearButton;
        private TextBlock? _placeholderText;

        static PlaceholderClearTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlaceholderClearTextBox),
                new FrameworkPropertyMetadata(typeof(PlaceholderClearTextBox)));
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(PlaceholderClearTextBox),
                new PropertyMetadata(string.Empty));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clearButton = GetTemplateChild("ClearButton") as Button;
            _placeholderText = GetTemplateChild("PlaceholderText") as TextBlock;

            if (_clearButton != null)
            {
                _clearButton.Click += ClearButton_Click;
            }

            UpdatePlaceholderVisibility();
        }

        private void ClearButton_Click(object? sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdatePlaceholderVisibility();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdatePlaceholderVisibility();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            if (_placeholderText != null)
            {
                _placeholderText.Visibility = string.IsNullOrEmpty(Text) && !IsFocused
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}
