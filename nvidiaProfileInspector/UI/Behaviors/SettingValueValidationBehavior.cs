namespace nvidiaProfileInspector.UI.Behaviors
{
    using nvidiaProfileInspector.UI.Controls;
    using nvidiaProfileInspector.UI.ViewModels;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    public static class SettingValueValidationBehavior
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached(
                "Enabled",
                typeof(bool),
                typeof(SettingValueValidationBehavior),
                new PropertyMetadata(false, OnEnabledChanged));

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = d as SearchableComboBox;
            if (comboBox == null)
                return;

            if ((bool)e.OldValue)
            {
                comboBox.PreviewLostKeyboardFocus -= ComboBox_PreviewLostKeyboardFocus;
                comboBox.RemoveHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(ComboBox_TextChanged));
            }

            if ((bool)e.NewValue)
            {
                comboBox.PreviewLostKeyboardFocus += ComboBox_PreviewLostKeyboardFocus;
                comboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(ComboBox_TextChanged), true);
            }
        }

        private static void ComboBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var comboBox = sender as SearchableComboBox;
            if (comboBox == null)
                return;

            if (IsFocusWithinComboBox(comboBox, e.NewFocus as DependencyObject))
                return;

            if (Validate(comboBox))
                return;

            e.Handled = true;
            comboBox.IsDropDownOpen = false;
            comboBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                comboBox.Focus();
                FocusEditableTextBox(comboBox);
            }), DispatcherPriority.Input);
        }

        private static void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var comboBox = sender as SearchableComboBox;
            if (comboBox != null)
                Validate(comboBox);
        }

        private static bool Validate(SearchableComboBox comboBox)
        {
            var setting = comboBox.DataContext as SettingItemViewModel;
            if (setting == null)
                return true;

            var viewModel = FindDataContext<MainViewModel>(comboBox);
            if (viewModel == null)
                return true;

            string errorMessage;
            if (viewModel.TryValidateSettingValue(setting, out errorMessage))
            {
                ClearValidationError(comboBox);
                return true;
            }

            MarkValidationError(comboBox, errorMessage);
            return false;
        }

        private static void MarkValidationError(SearchableComboBox comboBox, string errorMessage)
        {
            var bindingExpression = comboBox.GetBindingExpression(ComboBox.TextProperty);
            if (bindingExpression == null)
                return;

            var validationError = new ValidationError(new ExceptionValidationRule(), bindingExpression, errorMessage, null);
            Validation.ClearInvalid(bindingExpression);
            Validation.MarkInvalid(bindingExpression, validationError);
            comboBox.ToolTip = errorMessage;
        }

        private static void ClearValidationError(SearchableComboBox comboBox)
        {
            var bindingExpression = comboBox.GetBindingExpression(ComboBox.TextProperty);
            if (bindingExpression == null)
                return;

            Validation.ClearInvalid(bindingExpression);
            comboBox.ClearValue(FrameworkElement.ToolTipProperty);
        }

        private static void FocusEditableTextBox(SearchableComboBox comboBox)
        {
            var editableTextBox = comboBox.Template?.FindName("PART_EditableTextBox", comboBox) as TextBox;
            if (editableTextBox == null)
                return;

            editableTextBox.Focus();
            editableTextBox.SelectAll();
        }

        private static bool IsFocusWithinComboBox(SearchableComboBox comboBox, DependencyObject newFocus)
        {
            if (newFocus == null)
                return false;

            var comboBoxItem = FindAncestor<ComboBoxItem>(newFocus);
            if (comboBoxItem != null && ReferenceEquals(ItemsControl.ItemsControlFromItemContainer(comboBoxItem), comboBox))
                return true;

            var current = newFocus;
            while (current != null)
            {
                if (ReferenceEquals(current, comboBox))
                    return true;

                var element = current as FrameworkElement;
                if (element != null && ReferenceEquals(element.TemplatedParent, comboBox))
                    return true;

                var popup = current as Popup;
                if (popup != null && ReferenceEquals(popup.PlacementTarget, comboBox))
                    return true;

                current = GetParent(current);
            }

            return false;
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                var match = current as T;
                if (match != null)
                    return match;

                current = GetParent(current);
            }

            return null;
        }

        private static T FindDataContext<T>(DependencyObject current) where T : class
        {
            while (current != null)
            {
                var element = current as FrameworkElement;
                var dataContext = element?.DataContext as T;
                if (dataContext != null)
                    return dataContext;

                current = GetParent(current);
            }

            return null;
        }

        private static DependencyObject GetParent(DependencyObject current)
        {
            if (current == null)
                return null;

            var visualParent = VisualTreeHelper.GetParent(current);
            if (visualParent != null)
                return visualParent;

            return LogicalTreeHelper.GetParent(current);
        }
    }
}
