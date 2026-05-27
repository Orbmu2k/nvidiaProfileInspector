using System;
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

        public static readonly DependencyProperty SettingColumnStarWidthProperty =
            DependencyProperty.RegisterAttached(
                "SettingColumnStarWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(0d, OnListBoxColumnConfigChanged));

        public static readonly DependencyProperty ValueColumnStarWidthProperty =
            DependencyProperty.RegisterAttached(
                "ValueColumnStarWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(0d, OnListBoxColumnConfigChanged));

        public static readonly DependencyProperty SettingColumnMinWidthProperty =
            DependencyProperty.RegisterAttached(
                "SettingColumnMinWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(0d, OnListBoxColumnConfigChanged));

        public static readonly DependencyProperty ValueColumnMinWidthProperty =
            DependencyProperty.RegisterAttached(
                "ValueColumnMinWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(0d, OnListBoxColumnConfigChanged));

        public static readonly DependencyProperty HexColumnWidthProperty =
            DependencyProperty.RegisterAttached(
                "HexColumnWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(200d, OnListBoxColumnConfigChanged));

        public static readonly DependencyProperty CalculatedSettingColumnWidthProperty =
            DependencyProperty.RegisterAttached(
                "CalculatedSettingColumnWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(450d));

        public static readonly DependencyProperty CalculatedValueColumnWidthProperty =
            DependencyProperty.RegisterAttached(
                "CalculatedValueColumnWidth",
                typeof(double),
                typeof(GridViewColumnResizeBehavior),
                new PropertyMetadata(450d));

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

        public static void SetSettingColumnStarWidth(DependencyObject element, double value)
        {
            element.SetValue(SettingColumnStarWidthProperty, value);
        }

        public static double GetSettingColumnStarWidth(DependencyObject element)
        {
            return (double)element.GetValue(SettingColumnStarWidthProperty);
        }

        public static void SetValueColumnStarWidth(DependencyObject element, double value)
        {
            element.SetValue(ValueColumnStarWidthProperty, value);
        }

        public static double GetValueColumnStarWidth(DependencyObject element)
        {
            return (double)element.GetValue(ValueColumnStarWidthProperty);
        }

        public static void SetSettingColumnMinWidth(DependencyObject element, double value)
        {
            element.SetValue(SettingColumnMinWidthProperty, value);
        }

        public static double GetSettingColumnMinWidth(DependencyObject element)
        {
            return (double)element.GetValue(SettingColumnMinWidthProperty);
        }

        public static void SetValueColumnMinWidth(DependencyObject element, double value)
        {
            element.SetValue(ValueColumnMinWidthProperty, value);
        }

        public static double GetValueColumnMinWidth(DependencyObject element)
        {
            return (double)element.GetValue(ValueColumnMinWidthProperty);
        }

        public static void SetHexColumnWidth(DependencyObject element, double value)
        {
            element.SetValue(HexColumnWidthProperty, value);
        }

        public static double GetHexColumnWidth(DependencyObject element)
        {
            return (double)element.GetValue(HexColumnWidthProperty);
        }

        public static double GetCalculatedSettingColumnWidth(DependencyObject element)
        {
            return (double)element.GetValue(CalculatedSettingColumnWidthProperty);
        }

        public static double GetCalculatedValueColumnWidth(DependencyObject element)
        {
            return (double)element.GetValue(CalculatedValueColumnWidthProperty);
        }

        private static void OnEnableStarSizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                if ((bool)e.NewValue)
                {
                    listBox.Loaded += ListBox_LayoutChanged;
                    listBox.SizeChanged += ListBox_LayoutChanged;
                    QueueColumnResize(listBox);
                }
                else
                {
                    listBox.Loaded -= ListBox_LayoutChanged;
                    listBox.SizeChanged -= ListBox_LayoutChanged;
                }
            }
        }

        private static void OnListBoxColumnConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox && GetEnableStarSizing(listBox))
                QueueColumnResize(listBox);
        }

        private static void ListBox_LayoutChanged(object sender, EventArgs e)
        {
            if (sender is ListBox listBox)
                QueueColumnResize(listBox);
        }

        private static void QueueColumnResize(ListBox listBox)
        {
            listBox.Dispatcher.BeginInvoke(
                new Action(() => ResizeColumns(listBox)),
                DispatcherPriority.Loaded);
        }

        private static void ResizeColumns(ListBox listBox)
        {
            var settingStarWidth = GetSettingColumnStarWidth(listBox);
            var valueStarWidth = GetValueColumnStarWidth(listBox);
            if (settingStarWidth <= 0 && valueStarWidth <= 0)
                return;

            var viewportWidth = GetViewportWidth(listBox);
            if (viewportWidth <= 0)
                return;

            const double iconColumnWidth = 32d;
            const double favoriteColumnWidth = 32d;
            var fixedWidth = iconColumnWidth + favoriteColumnWidth + GetHexColumnWidth(listBox);
            var availableWidth = Math.Max(0, viewportWidth - fixedWidth);
            var totalStarWidth = Math.Max(0, settingStarWidth) + Math.Max(0, valueStarWidth);
            if (totalStarWidth <= 0)
                return;

            var settingWidth = availableWidth * settingStarWidth / totalStarWidth;
            var valueWidth = availableWidth * valueStarWidth / totalStarWidth;

            listBox.SetValue(CalculatedSettingColumnWidthProperty, Math.Max(GetSettingColumnMinWidth(listBox), settingWidth));
            listBox.SetValue(CalculatedValueColumnWidthProperty, Math.Max(GetValueColumnMinWidth(listBox), valueWidth));
        }

        private static double GetViewportWidth(ListBox listBox)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(listBox);
            if (scrollViewer != null && scrollViewer.ViewportWidth > 0)
                return scrollViewer.ViewportWidth;

            return Math.Max(0, listBox.ActualWidth - SystemParameters.VerticalScrollBarWidth);
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
