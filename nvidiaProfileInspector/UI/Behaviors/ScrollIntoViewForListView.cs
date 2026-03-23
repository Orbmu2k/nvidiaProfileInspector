using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace nvidiaProfileInspector.UI.Behaviors
{
    public class ScrollIntoViewForListView
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(ScrollIntoViewForListView),
                new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject element, bool value)
        {
            element.SetValue(EnableProperty, value);
        }

        public static bool GetEnable(DependencyObject element)
        {
            return (bool)element.GetValue(EnableProperty);
        }

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView)
            {
                if ((bool)e.NewValue)
                {
                    listView.SelectionChanged += ListView_SelectionChanged;
                    Debug.WriteLine("Addded SelectionChanged Handler...");
                }
                else
                {
                    listView.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(1000);
                        listView.SelectionChanged -= ListView_SelectionChanged;
                        Debug.WriteLine("Removed SelectionChanged Handler...");
                    }));

                }
            }
        }

        private static void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                if (listView.SelectedItem != null)
                {
                    listView.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        listView.UpdateLayout();
                        if (listView.SelectedItem != null)
                        {
                            listView.ScrollIntoView(listView.SelectedItem);
                            listView.UpdateLayout();
                            Debug.WriteLine("ScrollIntoView + UpdateLayout...");
                        }
                    }));
                }
            }
        }
    }
}
