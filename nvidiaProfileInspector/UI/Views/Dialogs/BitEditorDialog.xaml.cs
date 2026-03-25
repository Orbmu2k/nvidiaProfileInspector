namespace nvidiaProfileInspector.UI.Views.Dialogs
{
    using Microsoft.Win32;
    using nvidiaProfileInspector.UI.ViewModels;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public partial class BitEditorDialog : Window
    {
        private readonly BitEditorViewModel _viewModel;
        private ScrollViewer _bitsScrollViewer;

        public Action<uint> OnValueChanged { get; set; }

        public BitEditorDialog()
        {
            InitializeComponent();
            _viewModel = DesignTimeData.BitEditorViewModel;
            InitializeViewModel();
            StatusTextBlock.Text = "Designer preview";
        }

        public BitEditorDialog(uint settingId, uint initialValue, string settingName)
        {
            InitializeComponent();

            _viewModel = BitEditorViewModel.Create(settingId, initialValue, settingName);
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            DataContext = _viewModel;
            _viewModel.OnValueChanged = (value) => OnValueChanged?.Invoke(value);

            Title = _viewModel.Title;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Applications|*.exe",
                Title = "Select Game"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.GamePath = dialog.FileName;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void BitsListView_Loaded(object sender, RoutedEventArgs e)
        {
            _bitsScrollViewer = FindVisualChild<ScrollViewer>(BitsListView);
            if (_bitsScrollViewer != null)
                _bitsScrollViewer.ScrollChanged += BitsScrollViewer_ScrollChanged;

            UpdateVisibleRange();
        }

        private void BitsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisibleRange();
        }

        private void BitsScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateVisibleRange();
        }

        private void UpdateVisibleRange()
        {
            if (_viewModel == null || BitsListView == null || BitsListView.Items.Count == 0)
                return;

            var firstVisibleIndex = int.MaxValue;
            var lastVisibleIndex = -1;

            for (int index = 0; index < BitsListView.Items.Count; index++)
            {
                if (!(BitsListView.ItemContainerGenerator.ContainerFromIndex(index) is ListViewItem item))
                    continue;

                var itemBounds = item.TransformToAncestor(BitsListView).TransformBounds(new Rect(0, 0, item.ActualWidth, item.ActualHeight));
                if (itemBounds.Bottom <= 0 || itemBounds.Top >= BitsListView.ActualHeight)
                    continue;

                firstVisibleIndex = Math.Min(firstVisibleIndex, index);
                lastVisibleIndex = Math.Max(lastVisibleIndex, index);
            }

            if (lastVisibleIndex < 0)
            {
                firstVisibleIndex = 0;
                lastVisibleIndex = BitsListView.Items.Count - 1;
            }

            _viewModel.UpdateVisibleRange(firstVisibleIndex, lastVisibleIndex);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
