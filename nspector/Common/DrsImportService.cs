using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using nspector.Common.Import;
using nspector.Native.NVAPI2;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;
using nspector.Common.Helper;

namespace nspector.Common
{
    internal class DrsImportService : DrsSettingsServiceBase
    {

        private readonly DrsSettingsService _SettingService;

        public DrsImportService(DrsSettingsMetaService metaService, DrsSettingsService settingService, IntPtr? hSession = null)
            : base(metaService, hSession)
        {
            _SettingService = settingService;
        }

        internal void ExportAllProfilesToNvidiaTextFile(string filename)
        {
            DrsSession((hSession) =>
            {
                SaveSettingsFileEx(hSession, filename);
            });
        }

        internal void ImportAllProfilesFromNvidiaTextFile(string filename)
        {
            DrsSession((hSession) =>
            {
                LoadSettingsFileEx(hSession, filename);
                SaveSettings(hSession);
            });
        }

        internal void ExportProfiles(List<string> profileNames, string filename, bool includePredefined)
        {
            var exports = new Profiles();

            DrsSession((hSession) =>
            {
                foreach (var profileName in profileNames)
                {
                    var profile = CreateProfileForExport(hSession, profileName, includePredefined);
                    exports.Add(profile);
                }
            });

            XMLHelper<Profiles>.SerializeToXmlFile(exports, filename, Encoding.Unicode, true);
        }

        private Profile CreateProfileForExport(IntPtr hSession, string profileName, bool includePredefined)
        {
            var result = new Profile();

            var hProfile = GetProfileHandle(hSession, profileName);
            if (hProfile != IntPtr.Zero)
            {

                var settings = GetProfileSettings(hSession, hProfile);
                if (settings.Count > 0)
                {
                    result.ProfileName = profileName;

                    var apps = GetProfileApplications(hSession, hProfile);
                    foreach (var app in apps)
                    {
                        result.Executeables.Add(app.appName);
                    }

                    foreach (var setting in settings)
                    {
                        var isPredefined = setting.isCurrentPredefined == 1;
                        var isDwordSetting = setting.settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;
                        var isCurrentProfile = setting.settingLocation ==
                                               NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;

                        if (isDwordSetting && isCurrentProfile)
                        {
                            if (!isPredefined || includePredefined)
                            {
                                var profileSetting = new ProfileSetting()
                                {
                                    SettingNameInfo = setting.settingName,
                                    SettingId = setting.settingId,
                                    SettingValue = setting.currentValue.dwordValue,
                                };
                                result.Settings.Add(profileSetting);
                            }
                        }
                    }
                }
            }

            return result;
        }

        internal void ImportProfiles(string filename)
        {
            var profiles = XMLHelper<Profiles>.DeserializeFromXMLFile(filename);

            DrsSession((hSession) =>
            {
                foreach (Profile profile in profiles)
                {
                    var hProfile = GetProfileHandle(hSession, profile.ProfileName);
                    if (hProfile == IntPtr.Zero)
                    {
                        hProfile = CreateProfile(hSession, profile.ProfileName);
                        nvw.DRS_SaveSettings(hSession);
                    }

                    if (hProfile != IntPtr.Zero)
                    {
                        var modified = false;
                        _SettingService.ResetProfile(profile.ProfileName, out modified);

                        UpdateApplications(hSession, hProfile, profile);
                        UpdateSettings(hSession, hProfile, profile);
                        nvw.DRS_SaveSettings(hSession);
                    }
                }
            });
        }

        private bool ExistsImportApp(string appName, Profile importProfile)
        {
            return importProfile.Executeables.Any(x => x.Equals(appName));
        }

        private void UpdateApplications(IntPtr hSession, IntPtr hProfile, Profile importProfile)
        {
            var alreadySet = new HashSet<string>();

            var apps = GetProfileApplications(hSession, hProfile);
            foreach (var app in apps)
            {
                if (ExistsImportApp(app.appName, importProfile) && !alreadySet.Contains(app.appName))
                    alreadySet.Add(app.appName);
                else
                    nvw.DRS_DeleteApplication(hSession, hProfile, new StringBuilder(app.appName));
            }

            foreach (string appName in importProfile.Executeables)
            {
                if (!alreadySet.Contains(appName))
                {
                    AddApplication(hSession, hProfile, appName);
                }
            }
        }

        private uint GetImportValue(uint settingId, Profile importProfile)
        {
            var setting = importProfile.Settings
                .FirstOrDefault(x => x.SettingId.Equals(settingId));

            if (setting != null)
                return setting.SettingValue;

            return 0;
        }

        private bool ExistsImportValue(uint settingId, Profile importProfile)
        {
            return importProfile.Settings
                .Any(x => x.SettingId.Equals(settingId));
        }

        private void UpdateSettings(IntPtr hSession, IntPtr hProfile, Profile importProfile)
        {
            var alreadySet = new HashSet<uint>();

            var settings = GetProfileSettings(hSession, hProfile);
            foreach (var setting in settings)
            {
                var isCurrentProfile = setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;
                var isDwordSetting = setting.settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;
                var isPredefined = setting.isCurrentPredefined == 1;

                if (isCurrentProfile && isDwordSetting)
                {
                    bool exitsValue = ExistsImportValue(setting.settingId, importProfile);
                    uint importValue = GetImportValue(setting.settingId, importProfile);
                    if (isPredefined && exitsValue && importValue == setting.currentValue.dwordValue)
                    {
                        alreadySet.Add(setting.settingId);
                    }
                    else if (exitsValue)
                    {
                        StoreDwordValue(hSession, hProfile, setting.settingId, importValue);
                        alreadySet.Add(setting.settingId);
                    }
                    else if (!isPredefined)
                    {
                        nvw.DRS_DeleteProfileSetting(hSession, hProfile, setting.settingId);
                    }
                }
            }

            foreach (var setting in importProfile.Settings)
            {
                if (!alreadySet.Contains(setting.SettingId))
                {
                    StoreDwordValue(hSession, hProfile, setting.SettingId, setting.SettingValue);
                }
            }
        }

    }
}
