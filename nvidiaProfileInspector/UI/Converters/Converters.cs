namespace nvidiaProfileInspector.UI.Converters
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.Meta;
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;


    public class StateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SettingState)
            {
                SettingState state = (SettingState)value;
                if (state == SettingState.NvidiaSetting)
                    return Application.Current.TryFindResource("IconGearNvidia");
                if (state == SettingState.UserdefinedSetting)
                    return Application.Current.TryFindResource("IconUser");
                if (state == SettingState.GlobalSetting)
                    return Application.Current.TryFindResource("IconGlobe");

                return Application.Current.TryFindResource("IconSettings");
            }
            return Application.Current.TryFindResource("IconSettings");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SettingState)
            {
                SettingState state = (SettingState)value;
                if (state == SettingState.UserdefinedSetting)
                    return Application.Current.FindResource("TextPrimaryBrush");
                return Application.Current.FindResource("TextSecondaryBrush");
            }
            return Application.Current.FindResource("TextSecondaryBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToSettingIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SettingState)
            {
                SettingState state = (SettingState)value;
                if (state == SettingState.UserdefinedSetting)
                    return Application.Current.FindResource("TextPrimaryBrush");
                if (state == SettingState.NvidiaSetting)
                    return Application.Current.FindResource("NvidiaGreenBrush");
                if (state == SettingState.GlobalSetting)
                    return Application.Current.FindResource("AccentBrush");
                return Application.Current.FindResource("TextSecondaryBrush");
            }
            return Application.Current.FindResource("TextSecondaryBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SettingState)
            {
                SettingState state = (SettingState)value;
                switch (state)
                {
                    case SettingState.NvidiaSetting:
                        return "Nvidia Setting - Defined by NVIDIA driver";
                    case SettingState.UserdefinedSetting:
                        return "User-defined Setting - Custom setting added by user";
                    case SettingState.GlobalSetting:
                        return "Global Setting - Applies to all applications";
                    default:
                        return "Setting";
                }
            }
            return "Setting";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter != null && parameter.ToString() == "Invert";
            bool boolValue = value is bool && (bool)value;
            if (invert) boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && !(bool)value;
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool && (bool)value;
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Collapsed;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter != null && parameter.ToString() == "Invert";
            bool isNull = value == null;
            if (invert) isNull = !isNull;
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value as string;
            string param = parameter as string;

            if (param == "ProfileCombo")
            {
                return val == "ProfileCombo" ? Visibility.Visible : Visibility.Collapsed;
            }

            bool invert = param == "Invert";
            bool hasContent = !string.IsNullOrEmpty(val);
            if (invert) hasContent = !hasContent;
            return hasContent ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModifiedToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isModified = false;
            if (value is bool)
                isModified = (bool)value;
            else if (value is int)
                isModified = (int)value > 0;

            if (isModified)
                return Application.Current.FindResource("TextPrimaryBrush");

            return Application.Current.FindResource("TextSecondaryBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageBoxButtonToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageBoxButton buttons && parameter is string buttonName)
            {
                bool isVisible = buttons switch
                {
                    MessageBoxButton.OK => buttonName == "OK",
                    MessageBoxButton.OKCancel => buttonName == "OK" || buttonName == "Cancel",
                    MessageBoxButton.YesNo => buttonName == "Yes" || buttonName == "No",
                    MessageBoxButton.YesNoCancel => buttonName == "Yes" || buttonName == "No" || buttonName == "Cancel",
                    _ => false
                };
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double value && values[1] is double maximum && values[2] is double width)
            {
                if (maximum <= 0) return 0;
                double ratio = value / maximum;
                return width * ratio;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SettingMetaSourceToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SettingMetaSource source)
            {
                switch (source)
                {
                    case SettingMetaSource.CustomSettings:
                        return Application.Current.TryFindResource("IconUser");
                    case SettingMetaSource.DriverSettings:
                        return Application.Current.TryFindResource("IconNvidia");
                    case SettingMetaSource.ConstantSettings:
                        return Application.Current.TryFindResource("IconSettings");
                    case SettingMetaSource.ReferenceSettings:
                        return Application.Current.TryFindResource("IconLab");
                    case SettingMetaSource.ScannedSettings:
                        return Application.Current.TryFindResource("IconSearch");
                }
            }
            return Application.Current.TryFindResource("IconSettings");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GroupExpandedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var groupName = value as string;
            if (string.IsNullOrEmpty(groupName))
                return true;

            var settings = Common.Helper.UserSettings.LoadSettings();
            return settings?.HiddenSettingGroups == null || !settings.HiddenSettingGroups.Contains(groupName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FavoriteToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                return isFavorite
                ? Application.Current.FindResource("WarningBrush")
                : Application.Current.FindResource("TextSecondaryBrush");
            }
            return Application.Current.FindResource("TextSecondaryBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
