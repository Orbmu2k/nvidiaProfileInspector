using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace nvidiaProfileInspector.UI.Controls
{
    public class TitleBar : Control
    {
        static TitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBar), new FrameworkPropertyMetadata(typeof(TitleBar)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("MinimizeButton") is Button minimizeButton)
                minimizeButton.Click += (s, e) => OnMinimize();

            if (GetTemplateChild("MaximizeButton") is Button maximizeButton)
                maximizeButton.Click += (s, e) => OnMaximize();

            if (GetTemplateChild("CloseButton") is Button closeButton)
                closeButton.Click += (s, e) => OnClose();
        }

        private void OnMinimize()
        {
            if (ParentWindow is Window window)
                window.WindowState = WindowState.Minimized;
        }

        private void OnMaximize()
        {
            if (ParentWindow is Window window)
            {
                if (window.WindowState == WindowState.Maximized)
                    window.WindowState = WindowState.Normal;
                else
                    window.WindowState = WindowState.Maximized;
            }
        }

        private void OnClose()
        {
            if (ParentWindow is Window window)
                window.Close();
        }

        private Window ParentWindow
        {
            get
            {
                DependencyObject parent = this;
                while (parent != null && !(parent is Window))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                return parent as Window;
            }
        }

        #region Title Property
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(TitleBar),
            new PropertyMetadata("Application"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        #endregion

        #region TitleForeground Property
        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(
            nameof(TitleForeground),
            typeof(Brush),
            typeof(TitleBar),
            new PropertyMetadata(null));

        public Brush TitleForeground
        {
            get => (Brush)GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }
        #endregion

        #region TitleFontSize Property
        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
            nameof(TitleFontSize),
            typeof(double),
            typeof(TitleBar),
            new PropertyMetadata(16.0));

        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }
        #endregion

        #region IconContent Property (Left side content - logo)
        public static readonly DependencyProperty IconContentProperty = DependencyProperty.Register(
            nameof(IconContent),
            typeof(object),
            typeof(TitleBar),
            new PropertyMetadata(null));

        public object IconContent
        {
            get => GetValue(IconContentProperty);
            set => SetValue(IconContentProperty, value);
        }
        #endregion

        #region LeftContent Property
        public static readonly DependencyProperty LeftContentProperty = DependencyProperty.Register(
            nameof(LeftContent),
            typeof(object),
            typeof(TitleBar),
            new PropertyMetadata(null));

        public object LeftContent
        {
            get => GetValue(LeftContentProperty);
            set => SetValue(LeftContentProperty, value);
        }
        #endregion

        #region CenterContent Property
        public static readonly DependencyProperty CenterContentProperty = DependencyProperty.Register(
            nameof(CenterContent),
            typeof(object),
            typeof(TitleBar),
            new PropertyMetadata(null));

        public object CenterContent
        {
            get => GetValue(CenterContentProperty);
            set => SetValue(CenterContentProperty, value);
        }
        #endregion

        #region RightContent Property
        public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register(
            nameof(RightContent),
            typeof(object),
            typeof(TitleBar),
            new PropertyMetadata(null));

        public object RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }
        #endregion

        #region ShowMinimizeButton Property
        public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMinimizeButton),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true));

        public bool ShowMinimizeButton
        {
            get => (bool)GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }
        #endregion

        #region ShowMaximizeButton Property
        public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMaximizeButton),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true));

        public bool ShowMaximizeButton
        {
            get => (bool)GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }
        #endregion

        #region ShowCloseButton Property
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register(
            nameof(ShowCloseButton),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true));

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }
        #endregion

        #region Background Property
        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(TitleBar),
            new PropertyMetadata(null));

        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        #endregion

        #region Padding Property
        public static readonly new DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding),
            typeof(Thickness),
            typeof(TitleBar),
            new PropertyMetadata(new Thickness(16, 12, 16, 12)));

        public new Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }
        #endregion
    }
}
