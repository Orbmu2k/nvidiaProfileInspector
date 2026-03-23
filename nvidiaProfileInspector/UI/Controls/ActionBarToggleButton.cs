using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace nvidiaProfileInspector.UI.Controls
{
    public class ActionBarToggleButton : ToggleButton
    {
        static ActionBarToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActionBarToggleButton), new FrameworkPropertyMetadata(typeof(ActionBarToggleButton)));
        }

        public static readonly DependencyProperty IconPathProperty = DependencyProperty.Register(
            nameof(IconPath),
            typeof(Geometry),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(null));

        public Geometry IconPath
        {
            get => (Geometry)GetValue(IconPathProperty);
            set => SetValue(IconPathProperty, value);
        }

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
            nameof(IconWidth),
            typeof(double),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(14.0));

        public double IconWidth
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
            nameof(IconHeight),
            typeof(double),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(14.0));

        public double IconHeight
        {
            get => (double)GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
        }

        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(
            nameof(IconMargin),
            typeof(Thickness),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(new Thickness(0, 0, 8, 0)));

        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }

        public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(null));

        public Brush IconFill
        {
            get => (Brush)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextFontSizeProperty = DependencyProperty.Register(
            nameof(TextFontSize),
            typeof(double),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(11.0));

        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        public static readonly DependencyProperty IsIconOnlyProperty = DependencyProperty.Register(
            nameof(IsIconOnly),
            typeof(bool),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(false));

        public bool IsIconOnly
        {
            get => (bool)GetValue(IsIconOnlyProperty);
            set => SetValue(IsIconOnlyProperty, value);
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
            nameof(ShowIcon),
            typeof(bool),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(true));

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register(
            nameof(TextForeground),
            typeof(Brush),
            typeof(ActionBarToggleButton),
            new PropertyMetadata(null));

        public Brush TextForeground
        {
            get => (Brush)GetValue(TextForegroundProperty);
            set => SetValue(TextForegroundProperty, value);
        }
    }
}
