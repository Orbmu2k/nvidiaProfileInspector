using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly DrsScannerService _ScannerService;
        private readonly DrsDecrypterService _DecrypterService;

        public DrsImportService(
            DrsSettingsMetaService metaService,
            DrsSettingsService settingService,
            DrsScannerService scannerService,
            DrsDecrypterService decrypterService)
            : base(metaService)
        {
            _SettingService = settingService;
            _ScannerService = scannerService;
            _DecrypterService = decrypterService;
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
            }, forceNonGlobalSession: true, preventLoadSettings: true);
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

                result.ProfileName = profileName;

                var apps = GetProfileApplications(hSession, hProfile);
                foreach (var app in apps)
                {
                    result.Executeables.Add(app.appName);
                }

                var settings = GetProfileSettings(hSession, hProfile);
                foreach (var setting in settings)
                {
                    var isPredefined = setting.isCurrentPredefined == 1;
                    var isCurrentProfile = setting.settingLocation ==
                                           NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;

                    if (isCurrentProfile && (!isPredefined || includePredefined))
                    {
                        var exportSetting = setting;
                        _DecrypterService.DecryptSettingIfNeeded(profileName, ref exportSetting);

                        var profileSetting = ImportExportUitl
                            .ConvertDrsSettingToProfileSetting(exportSetting);

                        result.Settings.Add(profileSetting);
                    }
                }

            }

            return result;
        }

        internal string ImportProfiles(string filename)
        {
            var sbFailedProfilesMessage = new StringBuilder();
            var appInUseHint = false;
            var profiles = XMLHelper<Profiles>.DeserializeFromXMLFile(filename);

            DrsSession((hSession) =>
            {
                foreach (Profile profile in profiles)
                {
                    var profileCreated = false;
                    var hProfile = GetProfileHandle(hSession, profile.ProfileName);
                    if (hProfile == IntPtr.Zero)
                    {
                        hProfile = CreateProfile(hSession, profile.ProfileName);
                        nvw.DRS_SaveSettings(hSession);
                        profileCreated = true;
                    }

                    if (hProfile != IntPtr.Zero)
                    {
                        var modified = false;
                        _SettingService.ResetProfile(profile.ProfileName, out modified);
                        try
                        {
                            UpdateApplications(hSession, hProfile, profile);
                            UpdateSettings(hSession, hProfile, profile, profile.ProfileName);
                        }
                        catch (NvapiException nex)
                        {
                            if (profileCreated)
                            {
                                nvw.DRS_DeleteProfile(hSession, hProfile);
                            }

                            sbFailedProfilesMessage.AppendLine(string.Format("Failed to import profile '{0}'", profile.ProfileName));
                            var appEx = nex as NvapiAddApplicationException;
                            if (appEx != null)
                            {
                                var profilesWithThisApp = _ScannerService.FindProfilesUsingApplication(appEx.ApplicationName);
                                sbFailedProfilesMessage.AppendLine(string.Format("- application '{0}' is already in use by profile '{1}'", appEx.ApplicationName, profilesWithThisApp));
                                appInUseHint = true;
                            }
                            else
                            {
                                sbFailedProfilesMessage.AppendLine(string.Format("- {0}", nex.Message));
                            }
                            sbFailedProfilesMessage.AppendLine("");
                        }
                        nvw.DRS_SaveSettings(hSession);
                    }
                }
            });

            if (appInUseHint)
            {
                sbFailedProfilesMessage.AppendLine("Hint: If just the profile name has been changed by nvidia, consider to manually modify the profile name inside the import file using a text editor.");
            }

            return sbFailedProfilesMessage.ToString();
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
                    try
                    {
                        AddApplication(hSession, hProfile, appName);
                    }
                    catch (NvapiException)
                    {
                        throw new NvapiAddApplicationException(appName);
                    }
                }
            }
        }

        private uint GetImportValue(uint settingId, Profile importProfile)
        {
            var setting = importProfile.Settings
                .FirstOrDefault(x => x.SettingId.Equals(settingId));

            if (setting != null)
                return uint.Parse(setting.SettingValue);

            return 0;
        }

        private ProfileSetting GetImportProfileSetting(uint settingId, Profile importProfile)
        {
            return importProfile.Settings
                .FirstOrDefault(x => x.SettingId.Equals(settingId));
        }

        private bool ExistsImportValue(uint settingId, Profile importProfile)
        {
            return importProfile.Settings
                .Any(x => x.SettingId.Equals(settingId));
        }

        private void UpdateSettings(IntPtr hSession, IntPtr hProfile, Profile importProfile, string profileName)
        {
            var alreadySet = new HashSet<uint>();

            var settings = GetProfileSettings(hSession, hProfile);
            foreach (var setting in settings)
            {
                var isCurrentProfile = setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;
                var isPredefined = setting.isCurrentPredefined == 1;

                if (isCurrentProfile)
                {
                    bool exitsValueInImport = ExistsImportValue(setting.settingId, importProfile);
                    var importSetting = GetImportProfileSetting(setting.settingId, importProfile);

                    var decryptedSetting = setting;
                    _DecrypterService.DecryptSettingIfNeeded(profileName, ref decryptedSetting);

                    if (isPredefined && exitsValueInImport && ImportExportUitl.AreDrsSettingEqualToProfileSetting(decryptedSetting, importSetting))
                    {
                        alreadySet.Add(setting.settingId);
                    }
                    else if (exitsValueInImport)
                    {
                        var updatedSetting = ImportExportUitl.ConvertProfileSettingToDrsSetting(importSetting);
                        StoreSetting(hSession, hProfile, updatedSetting);
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
                    var newSetting = ImportExportUitl.ConvertProfileSettingToDrsSetting(setting);
                    try
                    {
                        StoreSetting(hSession, hProfile, newSetting);
                    }
                    catch (NvapiException ex)
                    {
                        if (ex.Status != NvAPI_Status.NVAPI_SETTING_NOT_FOUND)
                            throw;
                    }
                }
            }
        }

    }
}
