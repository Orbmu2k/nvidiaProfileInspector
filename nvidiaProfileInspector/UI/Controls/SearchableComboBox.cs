using nvidiaProfileInspector.UI.ViewModels;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace nvidiaProfileInspector.UI.Controls
{
    public class SearchableComboBox : ComboBox
    {
        private IEnumerable? _originalSource;
        private bool _isApplyingFilter;
        private bool _allowFocusOnItems;
        private int _navigationVersion;
        private object _pendingSelectedItem;

        public event EventHandler DeferredSelectionCommitted;

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
            DropDownOpened -= SearchableComboBox_DropDownOpened;
            DropDownOpened += SearchableComboBox_DropDownOpened;
            DropDownClosed -= SearchableComboBox_DropDownClosed;
            DropDownClosed += SearchableComboBox_DropDownClosed;
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
            if (HandleDropDownNavigationKey(e))
                return;

            base.OnPreviewKeyDown(e);
        }

        private bool HandleDropDownNavigationKey(KeyEventArgs e)
        {
            if (!IsDropDownOpen)
                return false;

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;

                if (Items.Count == 0)
                    return true;

                var currentIndex = GetFocusedItemIndex();
                var targetIndex = currentIndex;

                if (currentIndex < 0)
                {
                    var selectedIndex = SelectedItem == null ? -1 : Items.IndexOf(SelectedItem);
                    targetIndex = selectedIndex >= 0
                        ? selectedIndex
                        : e.Key == Key.Down ? 0 : Items.Count - 1;
                }
                else
                {
                    targetIndex += e.Key == Key.Down ? 1 : -1;
                }

                if (targetIndex >= 0 && targetIndex < Items.Count)
                    _ = FocusItemAsync(Items[targetIndex]);

                return true;
            }

            if (e.Key == Key.Enter)
            {
                var itemToCommit = GetFocusedOrPendingItem();
                if (itemToCommit != null)
                {
                    e.Handled = true;
                    if (DeferSelectionUntilCommit)
                        CommitDeferredSelection(itemToCommit);
                    else
                        SelectedItem = itemToCommit;

                    IsDropDownOpen = false;
                    return true;
                }
            }

            return false;
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

            // Track the unfiltered source whenever the change comes from outside (binding pushed
            // a new value list, e.g. the shared editor moved to another setting row) so the
            // filter never resurrects the previous row's items.
            if (!_isApplyingFilter)
            {
                _originalSource = newValue;
                InvalidatePendingNavigation();
            }

            SyncDisplayedText();
        }

        private void SearchableComboBox_DropDownClosed(object? sender, EventArgs e)
        {
            // Reset the search state so the next use (possibly on another setting row)
            // always starts with the full, unfiltered value list.
            FilterText = string.Empty;
            InvalidatePendingNavigation();
        }

        private async void SearchableComboBox_DropDownOpened(object? sender, EventArgs e)
        {
            FilterText = string.Empty;
            InvalidatePendingNavigation();
            AttachSearchTextBox();
            if (_searchTextBox != null)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await Task.Delay(50);
                    _searchTextBox.CaptureMouse();
                    _searchTextBox.Focus();
                    _searchTextBox.SelectAll();
                    if (Mouse.Captured == _searchTextBox)
                        _searchTextBox.ReleaseMouseCapture();
                }, DispatcherPriority.Input);
            }
        }

        private void AttachSearchTextBox()
        {
            if (_searchTextBox != null)
            {
                _searchTextBox.PreviewKeyDown -= SearchTextBox_PreviewKeyDown;
                _searchTextBox.LostMouseCapture -= _searchTextBox_LostMouseCapture;
                _searchTextBox.LostTouchCapture -= _searchTextBox_LostTouchCapture;
                _searchTextBox.LostStylusCapture -= _searchTextBox_LostStylusCapture;
            }

            _searchTextBox = GetTemplateChild("PART_SearchTextBox") as SearchableTextBox;
            if (_searchTextBox != null)
            {
                _searchTextBox.PreviewKeyDown += SearchTextBox_PreviewKeyDown;
                _searchTextBox.LostMouseCapture += _searchTextBox_LostMouseCapture;
                _searchTextBox.LostTouchCapture += _searchTextBox_LostTouchCapture;
                _searchTextBox.LostStylusCapture += _searchTextBox_LostStylusCapture;
            }
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleDropDownNavigationKey(e);
        }

        private void _searchTextBox_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (IsDropDownOpen)
                e.Handled = true;
        }

        private void _searchTextBox_LostStylusCapture(object sender, StylusEventArgs e)
        {
            if (IsDropDownOpen)
                e.Handled = true;
        }

        private void _searchTextBox_LostTouchCapture(object sender, TouchEventArgs e)
        {
            if (IsDropDownOpen)
                e.Handled = true;
        }

        private void ApplyFilter()
        {
            if (ItemsSource == null || _isApplyingFilter)
                return;

            InvalidatePendingNavigation();
            _isApplyingFilter = true;

            try
            {
                if (_originalSource == null)
                {
                    _originalSource = ItemsSource;
                }

                // SetCurrentValue changes the effective value without discarding the ItemsSource
                // binding; a plain assignment would permanently detach it, leaving this shared
                // editor stuck on the value list of the row it was first filtered on.
                if (string.IsNullOrEmpty(FilterText))
                {
                    if (!ReferenceEquals(ItemsSource, _originalSource))
                    {
                        SetCurrentValue(ItemsSourceProperty, _originalSource);
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

                    SetCurrentValue(ItemsSourceProperty, filteredItems);
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
            if (_pendingSelectedItem != null && Items.Contains(_pendingSelectedItem))
                return _pendingSelectedItem;

            if (Keyboard.FocusedElement is DependencyObject focusedElement)
            {
                var comboBoxItem = ContainerFromElement(focusedElement) as ComboBoxItem;
                if (comboBoxItem?.DataContext != null)
                    return comboBoxItem.DataContext;
            }

            return null;
        }

        private void CommitDeferredSelection(object itemToCommit)
        {
            SelectedItem = itemToCommit;
            _pendingSelectedItem = null;
            DeferredSelectionCommitted?.Invoke(this, EventArgs.Empty);
        }

        private int GetFocusedItemIndex()
        {
            var focusedItem = GetFocusedOrPendingItem();
            return focusedItem == null ? -1 : Items.IndexOf(focusedItem);
        }

        private async Task FocusItemAsync(object targetItem)
        {
            if (targetItem == null || !Items.Contains(targetItem))
                return;

            _pendingSelectedItem = targetItem;
            var navigationVersion = ++_navigationVersion;

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var comboBoxItem = await Dispatcher.InvokeAsync(() =>
                {
                    if (navigationVersion != _navigationVersion)
                        return null;

                    var currentIndex = Items.IndexOf(targetItem);
                    if (currentIndex < 0)
                        return null;

                    UpdateLayout();

                    if (ItemContainerGenerator.ContainerFromItem(targetItem) is ComboBoxItem realizedItem)
                        return realizedItem;

                    if (ItemContainerGenerator.ContainerFromIndex(currentIndex) is ComboBoxItem indexedItem)
                        return indexedItem;

                    var dropDown = GetTemplateChild("DropDown") as DependencyObject;
                    var virtualizingPanel = FindVisualChild<VirtualizingPanel>(dropDown);
                    virtualizingPanel?.BringIndexIntoViewPublic(currentIndex);
                    UpdateLayout();

                    return ItemContainerGenerator.ContainerFromIndex(currentIndex) as ComboBoxItem;
                }, DispatcherPriority.Input);

                if (navigationVersion != _navigationVersion)
                    return;

                if (comboBoxItem != null)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (navigationVersion != _navigationVersion || !Items.Contains(targetItem))
                            return;

                        if (!Equals(ItemContainerGenerator.ItemFromContainer(comboBoxItem), targetItem))
                            return;

                        _allowFocusOnItems = true;
                        try
                        {
                            comboBoxItem.BringIntoView();
                            comboBoxItem.Focus();
                        }
                        finally
                        {
                            _allowFocusOnItems = false;
                        }
                    }, DispatcherPriority.Input);
                    return;
                }

                await Task.Delay(25);
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                if (child is T match)
                    return match;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }

        private void InvalidatePendingNavigation()
        {
            _navigationVersion++;
            _pendingSelectedItem = null;
        }
    }
}
