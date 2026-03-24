using nvidiaProfileInspector.UI.ViewModels;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace nvidiaProfileInspector.UI.Controls
{
    public class SearchableComboBox : ComboBox
    {
        private IEnumerable? _originalSource;
        private bool _isApplyingFilter;

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
            DropDownOpened += SearchableComboBox_DropDownOpened;
            SyncDisplayedText();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SyncDisplayedText();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            SyncDisplayedText();
        }

        private async void SearchableComboBox_DropDownOpened(object? sender, EventArgs e)
        {
            FilterText = string.Empty;
            if (_searchTextBox == null)
            {
                _searchTextBox = GetTemplateChild("PART_SearchTextBox") as SearchableTextBox;
            }
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
    }
}
