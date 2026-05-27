using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
            if (d is Selector selector)
            {
                if ((bool)e.NewValue)
                {
                    selector.SelectionChanged += ListView_SelectionChanged;
                    Debug.WriteLine("Addded SelectionChanged Handler...");
                }
                else
                {
                    selector.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(1000);
                        selector.SelectionChanged -= ListView_SelectionChanged;
                        Debug.WriteLine("Removed SelectionChanged Handler...");
                    }));

                }
            }
        }

        private static void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        listBox.UpdateLayout();
                        if (listBox.SelectedItem != null)
                        {
                            listBox.ScrollIntoView(listBox.SelectedItem);
                            listBox.UpdateLayout();
                            Debug.WriteLine("ScrollIntoView + UpdateLayout...");
                        }
                    }));
                }
            }
        }
    }
}
