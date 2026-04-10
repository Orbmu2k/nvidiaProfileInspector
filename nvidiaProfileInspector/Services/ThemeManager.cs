using nvidiaProfileInspector.Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace nvidiaProfileInspector.Services
{
    public class ThemeManager
    {
        private const string DarkTheme = "DarkTheme.xaml";
        private const string SlateLightTheme = "SlateLightTheme.xaml";
        private const string MidnightTheme = "MidnightTheme.xaml";
        private const string CleanWhiteTheme = "CleanWhiteTheme.xaml";

        private static readonly string[] ValidThemes = {
            DarkTheme,
            SlateLightTheme,
            MidnightTheme,
            CleanWhiteTheme
        };

        private static readonly string[] ValidDensities = { "Modern", "Compact" };

        public string CurrentTheme { get; private set; }
        public string CurrentDensity { get; private set; } = "Modern";

        public bool IsDarkTheme => string.Equals(CurrentTheme, DarkTheme, StringComparison.OrdinalIgnoreCase);
        public bool IsCompactDensity => string.Equals(CurrentDensity, "Compact", StringComparison.OrdinalIgnoreCase);

        public event Action<string> ThemeChanged;

        public ThemeManager()
        {
            LoadSavedTheme();
        }

        public void LoadSavedTheme()
        {
            try
            {
                var settings = UserSettings.LoadSettings();
                ApplyAndPersistTheme(NormalizeThemeName(settings.Theme), savePreference: false);
                ApplyDensity(NormalizeDensity(settings.DisplayDensity), savePreference: false);
            }
            catch
            {
                ApplyAndPersistTheme(DarkTheme, savePreference: false);
                ApplyDensity("Modern", savePreference: false);
            }
        }

        public void ToggleTheme()
        {
            try
            {
                var currentTheme = GetCurrentAppliedThemeName();
                ApplyAndPersistTheme(GetNextThemeName(currentTheme), savePreference: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling theme: {ex.Message}");
            }
        }

        public void SetTheme(string themeName)
        {
            try
            {
                var normalized = NormalizeThemeName(themeName);
                ApplyAndPersistTheme(normalized, savePreference: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting theme: {ex.Message}");
            }
        }

        public void SetDensity(string density)
        {
            ApplyDensity(NormalizeDensity(density), savePreference: true);
        }

        private void ApplyDensity(string density, bool savePreference)
        {
            CurrentDensity = density;

            var app = Application.Current;
            if (app == null)
                return;

            bool compact = string.Equals(density, "Compact", StringComparison.OrdinalIgnoreCase);

            app.Resources["ListItemPadding"] = compact ? new Thickness(2, 1, 2, 1) : new Thickness(4, 4, 4, 4);
            app.Resources["ListItemMargin"] = compact ? new Thickness(0, 1, 0, 1) : new Thickness(0, 2, 0, 2);
            app.Resources["GroupHeaderOuterMargin"] = compact ? new Thickness(0, 4, 0, 2) : new Thickness(0, 8, 0, 4);
            app.Resources["GroupHeaderPadding"] = compact ? new Thickness(4, 2, 4, 4) : new Thickness(4, 4, 4, 8);
            app.Resources["GroupItemsPresenterMargin"] = compact ? new Thickness(8, 2, 8, 2) : new Thickness(8, 4, 8, 4);

            if (savePreference)
                SaveDensityPreference(density);
        }

        private static string NormalizeDensity(string density)
        {
            if (string.IsNullOrWhiteSpace(density))
                return "Modern";

            return ValidDensities.FirstOrDefault(d =>
                       string.Equals(d, density, StringComparison.OrdinalIgnoreCase))
                   ?? "Modern";
        }

        private void SaveThemePreference(string themeName)
        {
            try
            {
                var settings = UserSettings.LoadSettings();
                settings.Theme = themeName;
                settings.SaveSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme preference: {ex.Message}");
            }
        }

        private void SaveDensityPreference(string density)
        {
            try
            {
                var settings = UserSettings.LoadSettings();
                settings.DisplayDensity = density;
                settings.SaveSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving density preference: {ex.Message}");
            }
        }

        private static ResourceDictionary GetThemeDictionary(IList<ResourceDictionary> mergedDicts)
        {
            return mergedDicts.FirstOrDefault(d => GetThemeName(d.Source) != null);
        }

        private static string GetCurrentAppliedThemeName()
        {
            var app = Application.Current;
            if (app == null)
                return null;

            return GetThemeName(GetThemeDictionary(app.Resources.MergedDictionaries)?.Source);
        }

        private static string GetThemeName(Uri source)
        {
            if (source == null)
                return null;

            var themeName = Path.GetFileName(source.OriginalString);
            return ValidThemes.FirstOrDefault(validTheme =>
                string.Equals(validTheme, themeName, StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeThemeName(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
                return DarkTheme;

            if (!themeName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                return DarkTheme;

            return ValidThemes.FirstOrDefault(validTheme =>
                       string.Equals(validTheme, themeName, StringComparison.OrdinalIgnoreCase))
                   ?? DarkTheme;
        }

        private static string GetNextThemeName(string currentTheme)
        {
            if (ValidThemes.Length == 0)
                return DarkTheme;

            if (string.IsNullOrWhiteSpace(currentTheme))
                return ValidThemes[0];

            var currentThemeIndex = Array.FindIndex(
                ValidThemes,
                theme => string.Equals(theme, currentTheme, StringComparison.OrdinalIgnoreCase));

            if (currentThemeIndex < 0)
                return ValidThemes[0];

            var nextThemeIndex = (currentThemeIndex + 1) % ValidThemes.Length;
            return ValidThemes[nextThemeIndex];
        }

        private void ApplyAndPersistTheme(string themeName, bool savePreference)
        {
            var app = Application.Current;
            if (app == null)
                return;

            var normalizedThemeName = NormalizeThemeName(themeName);
            var mergedDicts = app.Resources.MergedDictionaries;
            var themeDict = GetThemeDictionary(mergedDicts);

            ReplaceThemeDictionary(mergedDicts, themeDict, normalizedThemeName);

            CurrentTheme = normalizedThemeName;

            ThemeChanged?.Invoke(normalizedThemeName);

            if (savePreference)
                SaveThemePreference(normalizedThemeName);
        }

        private static void ReplaceThemeDictionary(IList<ResourceDictionary> mergedDicts, ResourceDictionary existingTheme, string themeName)
        {
            var newThemeDictionary = new ResourceDictionary
            {
                Source = new Uri($"/UI/Themes/{themeName}", UriKind.Relative)
            };

            if (existingTheme == null)
            {
                mergedDicts.Add(newThemeDictionary);
                return;
            }

            var index = mergedDicts.IndexOf(existingTheme);
            mergedDicts.RemoveAt(index);
            mergedDicts.Insert(index, newThemeDictionary);
        }
    }
}
