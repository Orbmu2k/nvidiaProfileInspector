namespace nvidiaProfileInspector.UI.Behaviors
{
    using nvidiaProfileInspector.Common.Helper;
    using System.Windows;
    using System.Windows.Controls;

    public static class GroupExpanderPersistBehavior
    {
        public static readonly DependencyProperty PersistGroupNameProperty =
            DependencyProperty.RegisterAttached(
                "PersistGroupName",
                typeof(string),
                typeof(GroupExpanderPersistBehavior),
                new PropertyMetadata(null, OnPersistGroupNameChanged));

        public static string GetPersistGroupName(DependencyObject obj) => (string)obj.GetValue(PersistGroupNameProperty);
        public static void SetPersistGroupName(DependencyObject obj, string value) => obj.SetValue(PersistGroupNameProperty, value);

        private static void OnPersistGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Expander expander))
                return;

            expander.Expanded -= OnExpandedChanged;
            expander.Collapsed -= OnExpandedChanged;

            if (e.NewValue != null)
            {
                expander.Expanded += OnExpandedChanged;
                expander.Collapsed += OnExpandedChanged;
            }
        }

        private static void OnExpandedChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is Expander expander))
                return;

            var groupName = GetPersistGroupName(expander);
            if (string.IsNullOrEmpty(groupName))
                return;

            var settings = UserSettings.LoadSettings();
            if (settings == null)
                return;

            if (expander.IsExpanded)
                settings.HiddenSettingGroups.Remove(groupName);
            else if (!settings.HiddenSettingGroups.Contains(groupName))
                settings.HiddenSettingGroups.Add(groupName);

            settings.SaveSettings();
        }
    }
}
