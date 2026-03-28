using nvidiaProfileInspector.Common.Helper;
using nvidiaProfileInspector.Common.Import;
using nvidiaProfileInspector.Native.NVAPI2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nvw = nvidiaProfileInspector.Native.NVAPI2.NvapiDrsWrapper;

namespace nvidiaProfileInspector.Common
{
    public class DrsImportService : DrsSettingsServiceBase
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

        public void ExportAllCustomizedProfiles(string filename)
        {
            var profileNames = GetCustomizedProfileNames();
            ExportProfiles(profileNames, filename, includePredefined: false);
        }


        public void ExportAllProfilesToNvidiaTextFile(string filename)
        {
            DrsSession((hSession) =>
            {
                SaveSettingsFileEx(hSession, filename);
            });
        }

        public void ImportAllProfilesFromNvidiaTextFile(string filename)
        {
            DrsSession((hSession) =>
            {
                LoadSettingsFileEx(hSession, filename);
                SaveSettings(hSession);
            }, forceNonGlobalSession: true, preventLoadSettings: true);
        }

        public void ExportProfiles(List<string> profileNames, string filename, bool includePredefined)
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

        private List<string> GetCustomizedProfileNames()
        {
            var customizedProfiles = new List<string>();

            DrsSession((hSession) =>
            {
                foreach (var hProfile in EnumProfileHandles(hSession))
                {
                    var profile = GetProfileInfo(hSession, hProfile);
                    if (profile.profileName == null)
                        continue;

                    if (profile.isPredefined == 0)
                    {
                        customizedProfiles.Add(profile.profileName);
                        continue;
                    }

                    var settings = GetProfileSettings(hSession, hProfile);
                    if (settings.Any(_ =>
                        _.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION &&
                        _.isCurrentPredefined != 1))
                    {
                        customizedProfiles.Add(profile.profileName);
                    }
                }
            });

            return customizedProfiles
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(_ => _, StringComparer.InvariantCultureIgnoreCase)
                .ToList();
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

        public string ImportProfiles(string filename)
        {
            return ImportProfiles(new[] { filename });
        }

        public string ImportProfiles(IEnumerable<string> filenames)
        {
            var sbFailedProfilesMessage = new StringBuilder();
            var appInUseHint = false;
            var profiles = LoadAndMergeProfiles(filenames);

            DrsSession((hSession) =>
            {
                foreach (Profile profile in profiles)
                {
                    var profileCreated = false;
                    var hProfile = GetProfileHandle(hSession, profile.ProfileName);
                    if (hProfile == IntPtr.Zero)
                    {
                        hProfile = CreateProfile(hSession, profile.ProfileName);
                        nvw.Instance.DRS_SaveSettings(hSession);
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
                                nvw.Instance.DRS_DeleteProfile(hSession, hProfile);
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
                        nvw.Instance.DRS_SaveSettings(hSession);
                    }
                }
            });

            if (appInUseHint)
            {
                sbFailedProfilesMessage.AppendLine("Hint: If just the profile name has been changed by nvidia, consider to manually modify the profile name inside the import file using a text editor.");
            }

            return sbFailedProfilesMessage.ToString();
        }

        public string MergeProfiles(IEnumerable<string> filenames)
        {
            var sbFailedMessage = new StringBuilder();
            var appInUseHint = false;
            var profiles = LoadAndMergeProfiles(filenames);

            DrsSession((hSession) =>
            {
                foreach (var profile in profiles)
                {
                    var profileCreated = false;
                    var hProfile = GetProfileHandle(hSession, profile.ProfileName);
                    if (hProfile == IntPtr.Zero)
                    {
                        hProfile = CreateProfile(hSession, profile.ProfileName);
                        nvw.Instance.DRS_SaveSettings(hSession);
                        profileCreated = true;
                    }

                    try
                    {
                        MergeApplications(hSession, hProfile, profile);
                        MergeSettings(hSession, hProfile, profile);
                    }
                    catch (NvapiException nex)
                    {
                        if (profileCreated)
                            nvw.Instance.DRS_DeleteProfile(hSession, hProfile);

                        sbFailedMessage.AppendLine(string.Format("Failed to merge import into profile '{0}'", profile.ProfileName));
                        var appEx = nex as NvapiAddApplicationException;
                        if (appEx != null)
                        {
                            var profilesWithThisApp = _ScannerService.FindProfilesUsingApplication(appEx.ApplicationName);
                            sbFailedMessage.AppendLine(string.Format("- application '{0}' is already in use by profile '{1}'", appEx.ApplicationName, profilesWithThisApp));
                            appInUseHint = true;
                        }
                        else
                        {
                            sbFailedMessage.AppendLine(string.Format("- {0}", nex.Message));
                        }
                        sbFailedMessage.AppendLine("");
                    }

                    nvw.Instance.DRS_SaveSettings(hSession);
                }
            });

            if (appInUseHint)
            {
                sbFailedMessage.AppendLine("Hint: Remove conflicting applications from the other profile first, then retry the merge.");
            }

            return sbFailedMessage.ToString();
        }

        private Profiles LoadAndMergeProfiles(IEnumerable<string> filenames)
        {
            var mergedProfiles = new Dictionary<string, Profile>(StringComparer.InvariantCultureIgnoreCase);
            var profileOrder = new List<string>();

            foreach (var filename in (filenames ?? Enumerable.Empty<string>()).Where(_ => !string.IsNullOrWhiteSpace(_)))
            {
                var fileProfiles = XMLHelper<Profiles>.DeserializeFromXMLFile(filename);

                foreach (var profile in fileProfiles)
                {
                    if (!mergedProfiles.TryGetValue(profile.ProfileName, out var mergedProfile))
                    {
                        mergedProfile = new Profile
                        {
                            ProfileName = profile.ProfileName
                        };

                        mergedProfiles.Add(profile.ProfileName, mergedProfile);
                        profileOrder.Add(profile.ProfileName);
                    }

                    MergeExecutables(mergedProfile, profile);
                    MergeSettings(mergedProfile, profile);
                }
            }

            var result = new Profiles();
            foreach (var profileName in profileOrder)
                result.Add(mergedProfiles[profileName]);

            return result;
        }

        private void MergeExecutables(Profile targetProfile, Profile sourceProfile)
        {
            var executableIndexByName = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            for (var index = 0; index < targetProfile.Executeables.Count; index++)
                executableIndexByName[targetProfile.Executeables[index]] = index;

            foreach (var executable in sourceProfile.Executeables)
            {
                if (executableIndexByName.TryGetValue(executable, out var existingIndex))
                {
                    targetProfile.Executeables[existingIndex] = executable;
                }
                else
                {
                    executableIndexByName[executable] = targetProfile.Executeables.Count;
                    targetProfile.Executeables.Add(executable);
                }
            }
        }

        private void MergeSettings(Profile targetProfile, Profile sourceProfile)
        {
            var settingIndexById = new Dictionary<uint, int>();

            for (var index = 0; index < targetProfile.Settings.Count; index++)
                settingIndexById[targetProfile.Settings[index].SettingId] = index;

            foreach (var setting in sourceProfile.Settings)
            {
                var clonedSetting = CloneSetting(setting);

                if (settingIndexById.TryGetValue(clonedSetting.SettingId, out var existingIndex))
                {
                    targetProfile.Settings[existingIndex] = clonedSetting;
                }
                else
                {
                    settingIndexById[clonedSetting.SettingId] = targetProfile.Settings.Count;
                    targetProfile.Settings.Add(clonedSetting);
                }
            }
        }

        private ProfileSetting CloneSetting(ProfileSetting setting)
        {
            return new ProfileSetting
            {
                SettingId = setting.SettingId,
                SettingNameInfo = setting.SettingNameInfo,
                SettingValue = setting.SettingValue,
                ValueType = setting.ValueType
            };
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
                    nvw.Instance.DRS_DeleteApplication(hSession, hProfile, new StringBuilder(app.appName));
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

        private void MergeApplications(IntPtr hSession, IntPtr hProfile, Profile importProfile)
        {
            var existingApplications = new HashSet<string>(
                GetProfileApplications(hSession, hProfile).Select(_ => _.appName),
                StringComparer.InvariantCultureIgnoreCase);

            foreach (var appName in importProfile.Executeables)
            {
                if (existingApplications.Contains(appName))
                    continue;

                try
                {
                    AddApplication(hSession, hProfile, appName);
                    existingApplications.Add(appName);
                }
                catch (NvapiException)
                {
                    throw new NvapiAddApplicationException(appName);
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
                        nvw.Instance.DRS_DeleteProfileSetting(hSession, hProfile, setting.settingId);
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

        private void MergeSettings(IntPtr hSession, IntPtr hProfile, Profile importProfile)
        {
            var existingSettings = GetProfileSettings(hSession, hProfile)
                .Where(_ => _.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                .ToDictionary(_ => _.settingId, _ => _, EqualityComparer<uint>.Default);

            foreach (var setting in importProfile.Settings)
            {
                var newSetting = ImportExportUitl.ConvertProfileSettingToDrsSetting(setting);

                try
                {
                    StoreSetting(hSession, hProfile, newSetting);
                    existingSettings[setting.SettingId] = newSetting;
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
