using nvidiaProfileInspector.UI.ViewModels;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace nvidiaProfileInspector.UI.Controls
{
    public class SearchableComboBox : ComboBox
    {
        private IEnumerable? _originalSource;
        private bool _isApplyingFilter;
        private bool _allowFocusOnItems;
        private object _pendingSelectedItem;

        static SearchableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableComboBox),
            new FrameworkPropertyMetadata(typeof(SearchableComboBox)));
        }

        public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchableComboBox),
        new PropertyMetadata("Search..."));

        public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(SearchableComboBox),
        new PropertyMetadata(string.Empty, OnFilterTextChanged));

        public static readonly DependencyProperty SyncSelectedItemToTextProperty =
        DependencyProperty.Register(nameof(SyncSelectedItemToText), typeof(bool), typeof(SearchableComboBox),
        new PropertyMetadata(false));

        public static readonly DependencyProperty PreserveSelectionOnKeyboardFocusProperty =
        DependencyProperty.Register(nameof(PreserveSelectionOnKeyboardFocus), typeof(bool), typeof(SearchableComboBox),
        new PropertyMetadata(false));

        public static readonly DependencyProperty DeferSelectionUntilCommitProperty =
        DependencyProperty.Register(nameof(DeferSelectionUntilCommit), typeof(bool), typeof(SearchableComboBox),
        new PropertyMetadata(false));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
        }

        public bool SyncSelectedItemToText
        {
            get => (bool)GetValue(SyncSelectedItemToTextProperty);
            set => SetValue(SyncSelectedItemToTextProperty, value);
        }

        public bool PreserveSelectionOnKeyboardFocus
        {
            get => (bool)GetValue(PreserveSelectionOnKeyboardFocusProperty);
            set => SetValue(PreserveSelectionOnKeyboardFocusProperty, value);
        }

        public bool DeferSelectionUntilCommit
        {
            get => (bool)GetValue(DeferSelectionUntilCommitProperty);
            set => SetValue(DeferSelectionUntilCommitProperty, value);
        }

        private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control)
            {
                control.ApplyFilter();
            }
        }

        private SearchableTextBox? _searchTextBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            IsTextSearchEnabled = false;
            DropDownOpened += SearchableComboBox_DropDownOpened;
            AttachSearchTextBox();
            SyncDisplayedText();
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            // When an item gets focus (e.g. on hover), redirect focus back to the search textbox
            if (IsDropDownOpen && _searchTextBox != null
                && e.NewFocus is ComboBoxItem
                && !_allowFocusOnItems
                && e.OldFocus is not ComboBoxItem)
            {
                e.Handled = true;
                _searchTextBox.Focus();
                return;
            }

            _allowFocusOnItems = false;
            base.OnPreviewGotKeyboardFocus(e);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SyncDisplayedText();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (DeferSelectionUntilCommit && IsDropDownOpen && (e.Key == Key.Up || e.Key == Key.Down))
            {
                var currentIndex = GetFocusedItemIndex();
                if (currentIndex >= 0)
                {
                    var direction = e.Key == Key.Down ? 1 : -1;
                    var nextIndex = currentIndex + direction;
                    if (nextIndex >= 0 && nextIndex < Items.Count)
                    {
                        e.Handled = true;
                        _ = FocusItemByIndexAsync(nextIndex);
                        return;
                    }
                }
            }

            if (DeferSelectionUntilCommit && IsDropDownOpen && e.Key == Key.Enter)
            {
                var itemToCommit = GetFocusedOrPendingItem();
                if (itemToCommit != null)
                {
                    e.Handled = true;
                    CommitDeferredSelection(itemToCommit);
                    IsDropDownOpen = false;
                    return;
                }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (DeferSelectionUntilCommit && IsDropDownOpen)
            {
                var itemToCommit = ContainerFromElement((DependencyObject)e.OriginalSource) as ComboBoxItem;
                if (itemToCommit?.DataContext != null)
                {
                    CommitDeferredSelection(itemToCommit.DataContext);
                    IsDropDownOpen = false;
                    e.Handled = true;
                    return;
                }
            }

            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            SyncDisplayedText();
        }

        private async void SearchableComboBox_DropDownOpened(object? sender, EventArgs e)
        {
            FilterText = string.Empty;
            _pendingSelectedItem = null;
            AttachSearchTextBox();
            if (_searchTextBox != null)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await Task.Delay(50);
                    _searchTextBox.Focus();
                    _searchTextBox.SelectAll();
                }, DispatcherPriority.Input);
            }
        }

        private void AttachSearchTextBox()
        {
            if (_searchTextBox != null)
            {
                _searchTextBox.PreviewKeyDown -= SearchTextBox_PreviewKeyDown;
            }

            _searchTextBox = GetTemplateChild("PART_SearchTextBox") as SearchableTextBox;
            if (_searchTextBox != null)
            {
                _searchTextBox.PreviewKeyDown += SearchTextBox_PreviewKeyDown;
            }
        }

        private async void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Down || !IsDropDownOpen || Items.Count == 0)
                return;

            e.Handled = true;
            await FocusCurrentOrFirstItemAsync();
        }

        private async Task FocusCurrentOrFirstItemAsync()
        {
            var targetItem = GetFocusedOrPendingItem() ?? SelectedItem;
            if (targetItem == null || !Items.Cast<object>().Contains(targetItem))
            {
                targetItem = Items.Cast<object>().FirstOrDefault();
            }

            if (targetItem == null)
                return;

            var targetIndex = Items.IndexOf(targetItem);
            if (targetIndex < 0)
                targetIndex = 0;

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var comboBoxItem = await Dispatcher.InvokeAsync(() =>
                {
                    UpdateLayout();

                    if (ItemContainerGenerator.ContainerFromItem(targetItem) is ComboBoxItem realizedItem)
                    {
                        return realizedItem;
                    }

                    if (!PreserveSelectionOnKeyboardFocus)
                    {
                        SelectedItem = targetItem;
                        UpdateLayout();
                        return ItemContainerGenerator.ContainerFromItem(targetItem) as ComboBoxItem;
                    }

                    return ItemContainerGenerator.ContainerFromIndex(targetIndex) as ComboBoxItem;
                }, DispatcherPriority.Input);

                if (comboBoxItem != null)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        _pendingSelectedItem = comboBoxItem.DataContext;
                        _allowFocusOnItems = true;
                        comboBoxItem.BringIntoView();
                        comboBoxItem.Focus();
                    }, DispatcherPriority.Input);
                    return;
                }

                await Task.Delay(25);
            }
        }

        private void ApplyFilter()
        {
            if (ItemsSource == null || _isApplyingFilter)
                return;

            _isApplyingFilter = true;

            try
            {
                if (_originalSource == null)
                {
                    _originalSource = ItemsSource;
                }

                if (string.IsNullOrEmpty(FilterText))
                {
                    if (ItemsSource != _originalSource)
                    {
                        ItemsSource = _originalSource;
                    }
                }
                else
                {
                    var filteredItems = _originalSource
                        .Cast<object>()
                        .Where(item =>
                        {
                            if (item is SettingValueItem svi)
                            {
                                return !string.IsNullOrEmpty(svi.ValueName) &&
                                    svi.ValueName.ToLower().Contains(FilterText.ToLower());
                            }
                            return item?.ToString()?.ToLower().Contains(FilterText.ToLower()) ?? false;
                        })
                        .ToList();

                    ItemsSource = filteredItems;
                }
            }
            finally
            {
                _isApplyingFilter = false;
            }
        }

        private void SyncDisplayedText()
        {
            if (!IsEditable && SyncSelectedItemToText)
                Text = SelectedItem?.ToString() ?? SelectedValue?.ToString() ?? string.Empty;
        }

        private object GetFocusedOrPendingItem()
        {
            if (Keyboard.FocusedElement is DependencyObject focusedElement)
            {
                var comboBoxItem = ContainerFromElement(focusedElement) as ComboBoxItem;
                if (comboBoxItem?.DataContext != null)
                    return comboBoxItem.DataContext;
            }

            return _pendingSelectedItem;
        }

        private void CommitDeferredSelection(object itemToCommit)
        {
            SelectedItem = itemToCommit;
            _pendingSelectedItem = null;
        }

        private int GetFocusedItemIndex()
        {
            var focusedItem = GetFocusedOrPendingItem();
            return focusedItem == null ? -1 : Items.IndexOf(focusedItem);
        }

        private async Task FocusItemByIndexAsync(int index)
        {
            if (index < 0 || index >= Items.Count)
                return;

            var targetItem = Items[index];
            for (var attempt = 0; attempt < 3; attempt++)
            {
                var comboBoxItem = await Dispatcher.InvokeAsync(() =>
                {
                    UpdateLayout();
                    return ItemContainerGenerator.ContainerFromIndex(index) as ComboBoxItem;
                }, DispatcherPriority.Input);

                if (comboBoxItem != null)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        _pendingSelectedItem = targetItem;
                        _allowFocusOnItems = true;
                        comboBoxItem.BringIntoView();
                        comboBoxItem.Focus();
                    }, DispatcherPriority.Input);
                    return;
                }

                await Task.Delay(25);
            }
        }
    }
}
