namespace nvidiaProfileInspector
{
    using nvidiaProfileInspector.Common;
    using nvidiaProfileInspector.Common.CustomSettings;
    using nvidiaProfileInspector.Services;
    using nvidiaProfileInspector.TinyIoc;
    using System;
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
            _container = new TinyIoCContainer();

            var customSettings = LoadCustomSettings();
            var referenceSettings = LoadReferenceSettings();

            var metaService = new DrsSettingsMetaService(customSettings, referenceSettings);
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
            _container.RegisterSingleton<UpdateService>();
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

        private CustomSettingNames LoadReferenceSettings()
        {
            string csnDefaultPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Reference.xml";

            try
            {
                if (File.Exists(csnDefaultPath))
                    return CustomSettingNames.FactoryLoadFromFile(csnDefaultPath);
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
