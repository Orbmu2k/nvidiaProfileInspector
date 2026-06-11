using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace nvidiaProfileInspector.Common.CustomSettings
{
    internal static class ReferenceLocalization
    {
        public static void Apply(CustomSettingNames referenceSettings, string localizationFile)
        {
            if (referenceSettings?.Settings == null)
                return;

            InitializeSearchTerms(referenceSettings);

            var document = XDocument.Load(localizationFile, LoadOptions.PreserveWhitespace);
            var root = document.Root;
            if (root == null || root.Name.LocalName != "ReferenceLocalization")
                throw new InvalidOperationException("Invalid reference localization root element.");

            ApplyGroupTranslations(referenceSettings, root);
            ApplySettingTranslations(referenceSettings, root);
        }

        public static IReadOnlyDictionary<string, string> LoadGroupTranslations(string localizationFile)
        {
            var document = XDocument.Load(localizationFile, LoadOptions.PreserveWhitespace);
            var root = document.Root;
            if (root == null || root.Name.LocalName != "ReferenceLocalization")
                throw new InvalidOperationException("Invalid reference localization root element.");

            return ReadGroupTranslations(root);
        }

        private static void InitializeSearchTerms(CustomSettingNames referenceSettings)
        {
            foreach (var setting in referenceSettings.Settings)
            {
                setting.SearchTerms = JoinSearchTerms(
                    setting.SearchTerms,
                    setting.UserfriendlyName,
                    setting.AlternateNames,
                    setting.GroupName);

                if (setting.SettingValues == null)
                    continue;

                foreach (var value in setting.SettingValues)
                    value.SearchTerms = JoinSearchTerms(value.SearchTerms, value.UserfriendlyName);
            }
        }

        private static void ApplyGroupTranslations(CustomSettingNames referenceSettings, XElement root)
        {
            var translations = ReadGroupTranslations(root);

            foreach (var setting in referenceSettings.Settings)
            {
                if (setting.GroupName == null ||
                    !translations.TryGetValue(setting.GroupName, out var translatedGroup))
                {
                    continue;
                }

                setting.SearchTerms = JoinSearchTerms(
                    setting.SearchTerms,
                    setting.GroupName,
                    translatedGroup);
                setting.GroupName = translatedGroup;
            }
        }

        private static IReadOnlyDictionary<string, string> ReadGroupTranslations(XElement root)
        {
            return root
                .Element("Groups")?
                .Elements("Group")
                .Select(group => new
                {
                    Source = (string)group.Attribute("source"),
                    Translation = NormalizeText(group.Value)
                })
                .Where(group => !string.IsNullOrWhiteSpace(group.Source) &&
                    !string.IsNullOrWhiteSpace(group.Translation))
                .ToDictionary(
                    group => group.Source,
                    group => group.Translation,
                    StringComparer.Ordinal) ?? new Dictionary<string, string>();
        }

        private static void ApplySettingTranslations(CustomSettingNames referenceSettings, XElement root)
        {
            var settingsById = referenceSettings.Settings
                .Where(setting => TryParseHex(setting.HexSettingId, out _))
                .ToDictionary(setting => setting.SettingId);

            var translatedSettings = root.Element("Settings")?.Elements("Setting");
            if (translatedSettings == null)
                return;

            foreach (var translatedSetting in translatedSettings)
            {
                if (!TryParseHex((string)translatedSetting.Attribute("id"), out var settingId) ||
                    !settingsById.TryGetValue(settingId, out var setting))
                {
                    continue;
                }

                ApplyText(
                    translatedSetting.Element("Name"),
                    setting.UserfriendlyName,
                    value =>
                    {
                        setting.UserfriendlyName = value;
                        setting.HasLocalizedName = true;
                    },
                    value => setting.SearchTerms = JoinSearchTerms(setting.SearchTerms, value));
                ApplyText(
                    translatedSetting.Element("Description"),
                    setting.Description,
                    value => setting.Description = value);
                ApplyText(
                    translatedSetting.Element("AlternateNames"),
                    setting.AlternateNames,
                    value => setting.AlternateNames = value,
                    value => setting.SearchTerms = JoinSearchTerms(setting.SearchTerms, value));

                ApplyValueTranslations(setting, translatedSetting);
            }
        }

        private static void ApplyValueTranslations(CustomSetting setting, XElement translatedSetting)
        {
            if (setting.SettingValues == null)
                return;

            var valuesById = setting.SettingValues
                .Where(value => TryParseHex(value.HexValue, out _))
                .GroupBy(value => ParseHex(value.HexValue))
                .ToDictionary(group => group.Key, group => group.First());

            var translatedValues = translatedSetting.Element("Values")?.Elements("Value");
            if (translatedValues == null)
                return;

            foreach (var translatedValue in translatedValues)
            {
                if (!TryParseHex((string)translatedValue.Attribute("id"), out var valueId) ||
                    !valuesById.TryGetValue(valueId, out var value) ||
                    !SourceMatches(translatedValue, value.UserfriendlyName))
                {
                    continue;
                }

                var localizedName = NormalizeText(translatedValue.Value);
                if (string.IsNullOrWhiteSpace(localizedName))
                    continue;

                value.SearchTerms = JoinSearchTerms(
                    value.SearchTerms,
                    value.UserfriendlyName,
                    localizedName);
                value.UserfriendlyName = localizedName;
            }
        }

        private static void ApplyText(
            XElement element,
            string originalValue,
            Action<string> setValue,
            Action<string> addSearchTerm = null)
        {
            if (element == null || !SourceMatches(element, originalValue))
                return;

            var localizedValue = NormalizeText(element.Value);
            if (string.IsNullOrWhiteSpace(localizedValue))
                return;

            addSearchTerm?.Invoke(originalValue);
            addSearchTerm?.Invoke(localizedValue);
            setValue(localizedValue);
        }

        private static bool SourceMatches(XElement element, string originalValue)
        {
            var expectedSource = (string)element.Attribute("source");
            return expectedSource == null ||
                string.Equals(expectedSource, originalValue, StringComparison.Ordinal);
        }

        private static string NormalizeText(string value)
        {
            return value?.Replace("\r\n", "\n").Replace("\n", "\\r\\n");
        }

        private static string JoinSearchTerms(params string[] values)
        {
            return string.Join(
                "\n",
                values
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .SelectMany(value => value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static bool TryParseHex(string value, out uint parsed)
        {
            value = value?.Trim();
            if (value != null && value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(2);

            return uint.TryParse(
                value,
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out parsed);
        }

        private static uint ParseHex(string value)
        {
            TryParseHex(value, out var parsed);
            return parsed;
        }
    }
}
