using System;
using System.IO;
using System.Linq;
using System.Windows;
using nvidiaProfileInspector.Common.Helper;

namespace nvidiaProfileInspector.Services
{
    /// <summary>
    /// Manages application themes including loading, saving, and switching themes.
    /// </summary>
    public class ThemeManager
    {
        private const string DarkTheme = "DarkTheme.xaml";
        private const string LightTheme = "LightTheme.xaml";
        private const string SlateLightTheme = "SlateLightTheme.xaml";
        
        private static readonly string[] ValidThemes = { DarkTheme, LightTheme, SlateLightTheme };

        /// <summary>
        /// Gets the current theme name.
        /// </summary>
        public string CurrentTheme { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ThemeManager class.
        /// </summary>
        public ThemeManager()
        {
            LoadSavedTheme();
        }

        /// <summary>
        /// Loads the saved theme from user settings or defaults to DarkTheme.
        /// </summary>
        public void LoadSavedTheme()
        {
            try
            {
                var settings = UserSettings.LoadSettings();
                var themeName = settings.Theme ?? DarkTheme;

                // Ensure themeName has .xaml extension
                if (!themeName.EndsWith(".xaml"))
                    themeName = DarkTheme;

                // Validate theme
                if (!ValidThemes.Contains(themeName))
                    themeName = DarkTheme;

                CurrentTheme = themeName;
                ApplyTheme(themeName);
            }
            catch
            {
                // Default to dark theme on error
                CurrentTheme = DarkTheme;
                ApplyTheme(DarkTheme);
            }
        }

        /// <summary>
        /// Toggles to the next theme in the sequence: Dark -> Light -> SlateLight -> Dark.
        /// </summary>
        public void ToggleTheme()
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                var mergedDicts = app.Resources.MergedDictionaries;
                var existingTheme = mergedDicts.FirstOrDefault(d =>
                    d.Source != null && (d.Source.OriginalString.Contains(DarkTheme) ||
                                         d.Source.OriginalString.Contains(LightTheme) ||
                                         d.Source.OriginalString.Contains(SlateLightTheme)));

                string newSource;
                if (existingTheme != null)
                {
                    if (existingTheme.Source.OriginalString.Contains(DarkTheme))
                        newSource = LightTheme;
                    else if (existingTheme.Source.OriginalString.Contains(LightTheme))
                        newSource = SlateLightTheme;
                    else if (existingTheme.Source.OriginalString.Contains(SlateLightTheme))
                        newSource = DarkTheme;
                    else
                        newSource = DarkTheme; // fallback

                    // Update the theme dictionary
                    mergedDicts.Remove(existingTheme);
                    mergedDicts.Add(new ResourceDictionary { Source = new Uri($"/UI/Themes/{newSource}", UriKind.Relative) });
                }
                else
                {
                    // No theme found, apply dark theme
                    newSource = DarkTheme;
                    ApplyTheme(newSource);
                }

                // Save the new theme preference
                CurrentTheme = newSource;
                SaveThemePreference(newSource);
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error toggling theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies the specified theme to the application resources.
        /// </summary>
        /// <param name="themeName">The name of the theme to apply.</param>
        private void ApplyTheme(string themeName)
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                var mergedDicts = app.Resources.MergedDictionaries;
                var themeDict = mergedDicts.FirstOrDefault(d =>
                    d.Source != null && (d.Source.OriginalString.Contains("DarkTheme.xaml") ||
                                         d.Source.OriginalString.Contains("LightTheme.xaml") ||
                                         d.Source.OriginalString.Contains("SlateLightTheme.xaml")));

                if (themeDict != null)
                {
                    string newSource = $"/UI/Themes/{themeName}";
                    mergedDicts.Remove(themeDict);
                    mergedDicts.Add(new ResourceDictionary { Source = new Uri(newSource, UriKind.Relative) });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the theme preference to user settings.
        /// </summary>
        /// <param name="themeName">The name of the theme to save.</param>
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
    }
}