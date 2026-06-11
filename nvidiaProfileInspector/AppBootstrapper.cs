namespace nvidiaProfileInspector
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.CustomSettings;
    using nvidiaProfileInspector.Services;
    using nvidiaProfileInspector.TinyIoc;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public class AppBootstrapper : IDisposable
    {
        private TinyIoCContainer _container;
        private bool _disposed;

        public bool IsExternalCustomSettings { get; private set; }

        public TinyIoCContainer Container => _container;

        public void Initialize()
        {
            //RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            _container = new TinyIoCContainer();

            var customSettings = LoadCustomSettings();
            var referenceLocalizationPath = GetReferenceLocalizationPath();
            var referenceSettings = LoadReferenceSettings(referenceLocalizationPath);
            var referenceGroupTranslations = LoadReferenceGroupTranslations(referenceLocalizationPath);

            var metaService = new DrsSettingsMetaService(
                customSettings,
                referenceSettings,
                referenceGroupTranslations);
            var decrypterService = new DrsDecrypterService(metaService);
            var scannerService = new DrsScannerService(metaService, decrypterService);
            var settingService = new DrsSettingsService(metaService, decrypterService);
            var importService = new DrsImportService(metaService, settingService, scannerService, decrypterService);

            _container.RegisterInstance<DrsSettingsMetaService>(metaService);
            _container.RegisterInstance<DrsDecrypterService>(decrypterService);
            _container.RegisterInstance<DrsScannerService>(scannerService);
            _container.RegisterInstance<DrsSettingsService>(settingService);
            _container.RegisterInstance<DrsImportService>(importService);

            metaService.ResetMetaCache();

            _container.Register<BitEditorService>();
            _container.RegisterSingleton<DialogService>();
            _container.RegisterSingleton<ThemeManager>();

            _container.Register<UI.ViewModels.MainViewModel>(autoResolve: true);
            _container.Register<UI.ViewModels.SettingsListViewModel>(autoResolve: true);
            _container.Register<UI.ViewModels.ExportProfilesViewModel>(autoResolve: true);
            _container.Register<UI.ViewModels.AboutViewModel>(autoResolve: true);

            TinyIoC.Setup(_container);
        }

        public T Resolve<T>() where T : class => _container.Resolve<T>();
        public T TryResolve<T>() where T : class => _container.TryResolve<T>();
        public object Resolve(Type type) => _container.Resolve(type);
        public object TryResolve(Type type) => _container.TryResolve(type);

        private CustomSettingNames LoadCustomSettings()
        {
            string csnDefaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CustomSettingNames.xml";

            if (File.Exists(csnDefaultPath))
            {
                try
                {
                    IsExternalCustomSettings = true;
                    return CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);
                }
                catch
                {
                    return CreateDefaultCustomSettings();
                }
            }

            return CreateDefaultCustomSettings();
        }

        private CustomSettingNames CreateDefaultCustomSettings()
        {
            try
            {
                var defaultXml = EmbeddedResourceHelper.GetFileAsString("nvidiaProfileInspector.CustomSettingNames.xml");
                return CustomSettingNames.FactoryLoadFromString(defaultXml);
            }
            catch
            {
                return new CustomSettingNames();
            }
        }

        private string GetApplicationDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private string GetReferenceLocalizationPath()
        {
            var localizationPath = Path.Combine(
                GetApplicationDirectory(),
                $"Reference.{CultureInfo.CurrentUICulture.Name}.xml");

            return File.Exists(localizationPath) ? localizationPath : null;
        }

        private IReadOnlyDictionary<string, string> LoadReferenceGroupTranslations(string localizationPath)
        {
            if (localizationPath == null)
                return null;

            try
            {
                return ReferenceLocalization.LoadGroupTranslations(localizationPath);
            }
            catch
            {
                return null;
            }
        }

        private CustomSettingNames LoadReferenceSettings(string localizationPath)
        {
            var referencePath = Path.Combine(GetApplicationDirectory(), "Reference.xml");

            try
            {
                if (!File.Exists(referencePath))
                    return null;

                var referenceSettings = CustomSettingNames.FactoryLoadFromFile(referencePath);

                if (localizationPath != null)
                {
                    try
                    {
                        ReferenceLocalization.Apply(referenceSettings, localizationPath);
                    }
                    catch
                    {
                        // A missing or outdated localization must never prevent the base reference from loading.
                        referenceSettings = CustomSettingNames.FactoryLoadFromFile(referencePath);
                    }
                }

                return referenceSettings;
            }
            catch { }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _container?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
