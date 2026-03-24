using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using nvidiaProfileInspector.Common.Helper;

namespace nvidiaProfileInspector.Services
{
    public class ThemeManager
    {
        private const string DarkTheme = "DarkTheme.xaml";
        private const string LightTheme = "LightTheme.xaml";
        private const string SlateLightTheme = "SlateLightTheme.xaml";
        
        private static readonly string[] ValidThemes = { 
            DarkTheme,
            LightTheme,
            SlateLightTheme 
        };

        public string CurrentTheme { get; private set; }

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
            }
            catch
            {
                ApplyAndPersistTheme(DarkTheme, savePreference: false);
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
