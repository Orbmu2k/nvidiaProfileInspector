using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace nvidiaProfileInspector.UI.Behaviors
{
    public static class GridViewColumnResizeBehavior
    {
        public static readonly DependencyProperty EnableStarSizingProperty =
            DependencyProperty.RegisterAttached(
                "EnableStarSizing",
                typeof(bool),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(false, OnEnableStarSizingChanged));

        public static readonly DependencyProperty StarWidthProperty =
            DependencyProperty.RegisterAttached(
                "StarWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.RegisterAttached(
                "MinWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(0d));

        public static void SetEnableStarSizing(DependencyObject element, bool value)
        {
            element.SetValue(EnableStarSizingProperty, value);
        }

        public static bool GetEnableStarSizing(DependencyObject element)
        {
            return (bool)element.GetValue(EnableStarSizingProperty);
        }

        public static void SetStarWidth(DependencyObject element, double value)
        {
            element.SetValue(StarWidthProperty, value);
        }

        public static double GetStarWidth(DependencyObject element)
        {
            return (double)element.GetValue(StarWidthProperty);
        }

        public static void SetMinWidth(DependencyObject element, double value)
        {
            element.SetValue(MinWidthProperty, value);
        }

        public static double GetMinWidth(DependencyObject element)
        {
            return (double)element.GetValue(MinWidthProperty);
        }

        private static void OnEnableStarSizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ListView listView))
                return;

            if ((bool)e.NewValue)
            {
                listView.Loaded += ListView_LayoutChanged;
                listView.SizeChanged += ListView_LayoutChanged;
                QueueColumnResize(listView);
            }
            else
            {
                listView.Loaded -= ListView_LayoutChanged;
                listView.SizeChanged -= ListView_LayoutChanged;
            }
        }

        private static void ListView_LayoutChanged(object sender, EventArgs e)
        {
            if (sender is ListView listView)
                QueueColumnResize(listView);
        }

        private static void QueueColumnResize(ListView listView)
        {
            listView.Dispatcher.BeginInvoke(
                new Action(() => ResizeColumns(listView)),
                DispatcherPriority.Loaded);
        }

        private static void ResizeColumns(ListView listView)
        {
            if (!(listView.View is GridView gridView) || gridView.Columns.Count == 0)
                return;

            var starColumns = gridView.Columns
                .Cast<GridViewColumn>()
                .Where(column => GetStarWidth(column) > 0)
                .ToList();

            if (starColumns.Count == 0)
                return;

            var viewportWidth = GetViewportWidth(listView);
            if (viewportWidth <= 0)
                return;

            var fixedWidth = gridView.Columns
                .Cast<GridViewColumn>()
                .Where(column => GetStarWidth(column) <= 0)
                .Sum(column => double.IsNaN(column.Width) ? 0 : column.Width);

            var availableWidth = Math.Max(0, viewportWidth - fixedWidth);
            var totalStarWidth = starColumns.Sum(GetStarWidth);
            if (totalStarWidth <= 0)
                return;

            foreach (var column in starColumns)
            {
                var width = availableWidth * GetStarWidth(column) / totalStarWidth;
                column.Width = Math.Max(GetMinWidth(column), width);
            }
        }

        private static double GetViewportWidth(ListView listView)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(listView);
            if (scrollViewer != null && scrollViewer.ViewportWidth > 0)
                return scrollViewer.ViewportWidth;

            return Math.Max(0, listView.ActualWidth - SystemParameters.VerticalScrollBarWidth);
        }

        private static T FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            if (parent == null)
                return null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }
    }
}
