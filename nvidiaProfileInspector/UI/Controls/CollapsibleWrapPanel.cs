namespace nvidiaProfileInspector.UI.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A horizontal wrap panel that can collapse to a single row.
    /// When collapsed only the first row is shown; additional rows are clipped.
    /// <see cref="HasOverflow"/> reports whether more than one row is needed, so a
    /// caller can show or hide an expand/collapse toggle accordingly.
    /// </summary>
    public class CollapsibleWrapPanel : Panel
    {
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(CollapsibleWrapPanel),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty HasOverflowProperty =
            DependencyProperty.Register(
                nameof(HasOverflow),
                typeof(bool),
                typeof(CollapsibleWrapPanel),
                new FrameworkPropertyMetadata(false));

        public CollapsibleWrapPanel()
        {
            // Clip the additional rows away while collapsed.
            ClipToBounds = true;
        }

        /// <summary>When false (default) the panel is constrained to its first row.</summary>
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        /// <summary>True when the children do not fit in a single row at the current width.</summary>
        public bool HasOverflow
        {
            get => (bool)GetValue(HasOverflowProperty);
            set => SetValue(HasOverflowProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double maxWidth = constraint.Width;
            bool wrap = !double.IsInfinity(maxWidth) && !double.IsNaN(maxWidth);

            var childConstraint = new Size(
                wrap ? maxWidth : double.PositiveInfinity,
                double.PositiveInfinity);

            double lineWidth = 0;       // accumulated width of the current line
            double lineHeight = 0;      // height of the current line
            double totalHeight = 0;     // height of all committed lines
            double firstRowHeight = 0;  // height of the very first line
            double widestLine = 0;
            int rowCount = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null)
                    continue;

                child.Measure(childConstraint);
                var sz = child.DesiredSize;

                if (wrap && lineWidth > 0 && lineWidth + sz.Width > maxWidth)
                {
                    // current line is full, commit it and start a new one
                    totalHeight += lineHeight;
                    if (rowCount == 0)
                        firstRowHeight = lineHeight;
                    rowCount++;
                    widestLine = Math.Max(widestLine, lineWidth);

                    lineWidth = sz.Width;
                    lineHeight = sz.Height;
                }
                else
                {
                    lineWidth += sz.Width;
                    lineHeight = Math.Max(lineHeight, sz.Height);
                }
            }

            // commit the trailing line
            if (lineWidth > 0 || lineHeight > 0)
            {
                totalHeight += lineHeight;
                if (rowCount == 0)
                    firstRowHeight = lineHeight;
                rowCount++;
                widestLine = Math.Max(widestLine, lineWidth);
            }

            bool overflow = rowCount > 1;
            if (HasOverflow != overflow)
                HasOverflow = overflow;

            double desiredWidth = wrap ? Math.Min(widestLine, maxWidth) : widestLine;
            double desiredHeight = (IsExpanded || !overflow) ? totalHeight : firstRowHeight;

            return new Size(desiredWidth, desiredHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double maxWidth = finalSize.Width;
            double x = 0, y = 0, lineHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null)
                    continue;

                var sz = child.DesiredSize;

                if (x > 0 && x + sz.Width > maxWidth)
                {
                    x = 0;
                    y += lineHeight;
                    lineHeight = 0;
                }

                child.Arrange(new Rect(x, y, sz.Width, sz.Height));
                x += sz.Width;
                lineHeight = Math.Max(lineHeight, sz.Height);
            }

            return finalSize;
        }
    }
}
