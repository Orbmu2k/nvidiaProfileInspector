namespace nvidiaProfileInspector.UI.Views
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    public partial class SplashWindow : Window
    {
        private bool _isClosing;

        public SplashWindow()
        {
            InitializeComponent();
        }

        public void CloseWithFade()
        {
            if (_isClosing)
                return;

            _isClosing = true;

            var animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (sender, args) => Close();
            BeginAnimation(OpacityProperty, animation);
        }
    }
}
