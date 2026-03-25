namespace nvidiaProfileInspector.UI.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;

    public class MinThumbTrack : Track
    {
        public static readonly DependencyProperty MinThumbLengthProperty =
            DependencyProperty.Register(
                nameof(MinThumbLength),
                typeof(double),
                typeof(MinThumbTrack),
                new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double MinThumbLength
        {
            get => (double)GetValue(MinThumbLengthProperty);
            set => SetValue(MinThumbLengthProperty, value);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var result = base.ArrangeOverride(arrangeSize);

            if (Thumb != null && Orientation == System.Windows.Controls.Orientation.Vertical)
            {
                var thumbRect = Thumb.RenderSize;
                if (thumbRect.Height < MinThumbLength && arrangeSize.Height > MinThumbLength)
                {
                    var currentTop = Thumb.TranslatePoint(new Point(0, 0), this).Y;
                    var trackLength = arrangeSize.Height;
                    var newThumbHeight = MinThumbLength;
                    var maxTop = trackLength - newThumbHeight;
                    var clampedTop = Math.Max(0, Math.Min(currentTop, maxTop));

                    Thumb.Arrange(new Rect(0, clampedTop, arrangeSize.Width, newThumbHeight));
                }
            }

            return result;
        }
    }
}
