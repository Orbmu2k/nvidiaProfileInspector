using System.Windows;
using System.Windows.Controls;

namespace nvidiaProfileInspector.UI.Controls
{
    public class SearchableTextBox : TextBox
    {
        private Button? _clearButton;

        static SearchableTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableTextBox),
                new FrameworkPropertyMetadata(typeof(SearchableTextBox)));
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchableTextBox),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ShowClearButtonProperty =
            DependencyProperty.Register(nameof(ShowClearButton), typeof(bool), typeof(SearchableTextBox),
                new PropertyMetadata(true));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public bool ShowClearButton
        {
            get => (bool)GetValue(ShowClearButtonProperty);
            set => SetValue(ShowClearButtonProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clearButton = GetTemplateChild("PART_ClearButton") as Button;
            if (_clearButton != null)
            {
                _clearButton.Click += ClearButton_Click;
            }

            UpdateClearButtonVisibility();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdateClearButtonVisibility();
        }

        private void UpdateClearButtonVisibility()
        {
            if (_clearButton != null)
            {
                _clearButton.Visibility = ShowClearButton && !string.IsNullOrEmpty(Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
