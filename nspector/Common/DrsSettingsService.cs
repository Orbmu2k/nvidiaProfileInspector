using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using nspector.Common.Helper;
using nspector.Common.Meta;
using nspector.Native.NVAPI2;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;

namespace nspector.Common
{
    internal class DrsSettingsService : DrsSettingsServiceBase
    {

        public DrsSettingsService(DrsSettingsMetaService metaService, DrsDecrypterService decrpterService)
            : base(metaService, decrpterService)
        {
            _baseProfileSettingIds = InitBaseProfileSettingIds();
        }

        private List<uint> InitBaseProfileSettingIds()
        {
            return DrsSession((hSession) =>
            {
                var hBaseProfile = GetProfileHandle(hSession, "");
                var baseProfileSettings = GetProfileSettings(hSession, hBaseProfile);

                return baseProfileSettings.Select(x => x.settingId).ToList();
            });
        }

        private readonly List<uint> _baseProfileSettingIds;

        private string GetDrsProgramPath()
        {
            var nvidiaInstallerFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"NVIDIA Corporation\Installer2");
            var driverFolders = Directory.EnumerateDirectories(nvidiaInstallerFolder, "Display.Driver.*");
            foreach (var folder in driverFolders)
            {
                var fiDbInstaller = new FileInfo(Path.Combine(folder, "dbInstaller.exe"));
                if (!fiDbInstaller.Exists) continue;

                var fviDbInstaller = FileVersionInfo.GetVersionInfo(fiDbInstaller.FullName);

                var fileversion = fviDbInstaller.FileVersion.Replace(".", "");
                var driverver = DriverVersion.ToString().Replace(",", "").Replace(".", "");

                if (fileversion.EndsWith(driverver))
                    return fiDbInstaller.DirectoryName;
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                @"NVIDIA Corporation\Drs");
        }

        private void RunDrsInitProcess()
        {
            var drsPath = GetDrsProgramPath();

            var si = new ProcessStartInfo();
            si.UseShellExecute = true;
            si.WorkingDirectory = drsPath;
            si.Arguments = "-init";
            si.FileName = Path.Combine(drsPath, "dbInstaller.exe");
            if (!AdminHelper.IsAdmin)
                si.Verb = "runas";
            var p = Process.Start(si);
            p.WaitForExit();
        }

        public void DeleteAllProfilesHard()
        {
            var tmpFile = TempFile.GetTempFileName();
            try
            {
                File.WriteAllText(tmpFile, "BaseProfile \"Base Profile\"\r\nSelectedGlobalProfile \"Base Profile\"\r\nProfile \"Base Profile\"\r\nShowOn All\r\nProfileType Global\r\nEndProfile\r\n");

                DrsSession((hSession) =>
                {
                    LoadSettingsFileEx(hSession, tmpFile);
                    SaveSettings(hSession);
                }, forceNonGlobalSession: true, preventLoadSettings: true);
            }
            finally
            {
                if (File.Exists(tmpFile))
                    File.Delete(tmpFile);
            }

        }

        public void DeleteProfileHard(string profileName)
        {
            var tmpFileName = TempFile.GetTempFileName();

            try
            {
                var tmpFileContent = "";

                DrsSession((hSession) =>
                {
                    SaveSettingsFileEx(hSession, tmpFileName);
                    tmpFileContent = File.ReadAllText(tmpFileName);
                    string pattern = "(?<rpl>\nProfile\\s\"" + Regex.Escape(profileName) + "\".*?EndProfile.*?\n)";
                    tmpFileContent = Regex.Replace(tmpFileContent, pattern, "", RegexOptions.Singleline);
                    File.WriteAllText(tmpFileName, tmpFileContent);
                });

                if (tmpFileContent != "")
                {
                    DrsSession((hSession) =>
                    {
                        LoadSettingsFileEx(hSession, tmpFileName);
                        SaveSettings(hSession);
                    });
                }
            }
            finally
            {
                if (File.Exists(tmpFileName))
                    File.Delete(tmpFileName);
            }
        }

        public void DeleteProfile(string profileName)
        {
            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);
                if (hProfile != IntPtr.Zero)
                {
                    var nvRes = nvw.DRS_DeleteProfile(hSession, hProfile);
                    if (nvRes != NvAPI_Status.NVAPI_OK)
                        throw new NvapiException("DRS_DeleteProfile", nvRes);

                    SaveSettings(hSession);
                }
            });

        }

        public List<string> GetProfileNames(ref string baseProfileName)
        {
            var lstResult = new List<string>();
            var tmpBaseProfileName = baseProfileName;

            DrsSession((hSession) =>
            {
                var hBase = GetProfileHandle(hSession, null);
                var baseProfile = GetProfileInfo(hSession, hBase);
                tmpBaseProfileName = baseProfile.profileName;

                lstResult.Add("_GLOBAL_DRIVER_PROFILE (" + tmpBaseProfileName + ")");

                var profileHandles = EnumProfileHandles(hSession);
                foreach (IntPtr hProfile in profileHandles)
                {
                    var profile = GetProfileInfo(hSession, hProfile);

                    if (profile.isPredefined == 0 || profile.numOfApps > 0)
                    {
                        lstResult.Add(profile.profileName);
                    }
                }
            });

            baseProfileName = tmpBaseProfileName;
            return lstResult;
        }

        public void CreateProfile(string profileName, string applicationName = null)
        {
            DrsSession((hSession) =>
            {
                var hProfile = CreateProfile(hSession, profileName);

                if (applicationName != null)
                    AddApplication(hSession, hProfile, applicationName);

                SaveSettings(hSession);
            });
        }

        public void ResetAllProfilesInternal()
        {
            RunDrsInitProcess();

            DrsSession((hSession) =>
            {
                var nvRes = nvw.DRS_RestoreAllDefaults(hSession);
                if (nvRes != NvAPI_Status.NVAPI_OK)
                    throw new NvapiException("DRS_RestoreAllDefaults", nvRes);

                SaveSettings(hSession);
            });
        }

        public void ResetProfile(string profileName, out bool removeFromModified)
        {
            bool tmpRemoveFromModified = false;
            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);
                var profile = GetProfileInfo(hSession, hProfile);

                if (profile.isPredefined == 1)
                {
                    var nvRes = nvw.DRS_RestoreProfileDefault(hSession, hProfile);
                    if (nvRes != NvAPI_Status.NVAPI_OK)
                        throw new NvapiException("DRS_RestoreProfileDefault", nvRes);

                    SaveSettings(hSession);
                    tmpRemoveFromModified = true;
                }
                else if (profile.numOfSettings > 0)
                {
                    int dropCount = 0;
                    var settings = GetProfileSettings(hSession, hProfile);

                    foreach (var setting in settings)
                    {
                        if (setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                        {
                            if (nvw.DRS_DeleteProfileSetting(hSession, hProfile, setting.settingId) == NvAPI_Status.NVAPI_OK)
                            {
                                dropCount++;
                            }
                        }
                    }
                    if (dropCount > 0)
                    {
                        SaveSettings(hSession);
                    }
                }
            });

            removeFromModified = tmpRemoveFromModified;
        }

        public void ResetValue(string profileName, uint settingId, out bool removeFromModified)
        {
            var tmpRemoveFromModified = false;

            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);

                if (hProfile != IntPtr.Zero)
                {
                    var nvRes = nvw.DRS_RestoreProfileDefaultSetting(hSession, hProfile, settingId);
                    if (nvRes != NvAPI_Status.NVAPI_OK)
                        throw new NvapiException("DRS_RestoreProfileDefaultSetting", nvRes);

                    SaveSettings(hSession);

                    var modifyCount = 0;
                    var settings = GetProfileSettings(hSession, hProfile);

                    foreach (var setting in settings)
                    {
                        if (setting.isCurrentPredefined == 0 && setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                        {
                            modifyCount++;
                        }
                    }
                    tmpRemoveFromModified = (modifyCount == 0);
                }
            });

            removeFromModified = tmpRemoveFromModified;
        }

        public void DeleteValue(string profileName, uint settingId, out bool removeFromModified)
        {
            var tmpRemoveFromModified = false;

            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);

                if (hProfile != IntPtr.Zero)
                {
                    int dropCount = 0;
                    var settings = GetProfileSettings(hSession, hProfile);
                    foreach (var setting in settings)
                    {
                        if (setting.settingId != settingId) continue;
                        
                        if (setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                        {
                            if (nvw.DRS_DeleteProfileSetting(hSession, hProfile, setting.settingId) == NvAPI_Status.NVAPI_OK)
                            {
                                dropCount++;
                                break;
                            }
                        }
                    }
                    tmpRemoveFromModified = (dropCount == 0);
                    SaveSettings(hSession);
                }
            });

            removeFromModified = tmpRemoveFromModified;
        }

        public uint GetDwordValueFromProfile(string profileName, uint settingId, bool returnDefaultValue = false, bool forceDedicatedScope = false)
        {
            return DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);

                var dwordValue = ReadDwordValue(hSession, hProfile, settingId);

                if (dwordValue != null)
                    return dwordValue.Value;
                else if (returnDefaultValue)
                    return meta.GetSettingMeta(settingId).DefaultDwordValue;

                throw new NvapiException("DRS_GetSetting", NvAPI_Status.NVAPI_SETTING_NOT_FOUND);
            });
        }

        public void SetDwordValueToProfile(string profileName, uint settingId, uint dwordValue)
        {
            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);
                StoreDwordValue(hSession, hProfile, settingId, dwordValue);
                SaveSettings(hSession);
            });
        }

        public int StoreSettingsToProfile(string profileName, List<KeyValuePair<uint, string>> settings)
        {
            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);

                foreach (var setting in settings)
                {
                    var settingMeta = meta.GetSettingMeta(setting.Key);
                    var settingType = settingMeta.SettingType;

                    if (settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
                    {
                        var dword = DrsUtil.ParseDwordSettingValue(settingMeta, setting.Value);
                        StoreDwordValue(hSession, hProfile, setting.Key, dword);
                    }
                    else if (settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                    {
                        var str = DrsUtil.ParseStringSettingValue(settingMeta, setting.Value);
                        StoreStringValue(hSession, hProfile, setting.Key, str);
                    }
                    else if (settingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
                    {
                        var bin = DrsUtil.ParseBinarySettingValue(settingMeta, setting.Value);
                        StoreBinaryValue(hSession, hProfile, setting.Key, bin);
                    }
                }

                SaveSettings(hSession);
            });

            return 0;
        }


        private SettingItem CreateSettingItem(NVDRS_SETTING setting, bool useDefault = false)
        {
            var settingMeta = meta.GetSettingMeta(setting.settingId);
            //settingMeta.SettingType = setting.settingType;

            if (settingMeta.DwordValues == null)
                settingMeta.DwordValues = new List<SettingValue<uint>>();


            if (settingMeta.StringValues == null)
                settingMeta.StringValues = new List<SettingValue<string>>();

            if (settingMeta.BinaryValues == null)
                settingMeta.BinaryValues = new List<SettingValue<byte[]>>();


            var settingState = SettingState.NotAssiged;
            string valueRaw = "";
            string valueText = "";

            if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
            {
                if (useDefault)
                {
                    valueRaw = DrsUtil.GetDwordString(settingMeta.DefaultDwordValue);
                    valueText = DrsUtil.GetDwordSettingValueName(settingMeta, settingMeta.DefaultDwordValue);
                }
                else if (setting.isCurrentPredefined == 1 && setting.isPredefinedValid == 1)
                {
                    valueRaw = DrsUtil.GetDwordString(setting.predefinedValue.dwordValue);
                    valueText = DrsUtil.GetDwordSettingValueName(settingMeta, setting.predefinedValue.dwordValue);

                    if (setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                        settingState = SettingState.NvidiaSetting;
                    else
                        settingState = SettingState.GlobalSetting;
                }
                else
                {
                    valueRaw = DrsUtil.GetDwordString(setting.currentValue.dwordValue);
                    valueText = DrsUtil.GetDwordSettingValueName(settingMeta, setting.currentValue.dwordValue);

                    if (setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                        settingState = SettingState.UserdefinedSetting;
                    else
                        settingState = SettingState.GlobalSetting;
                }
            }

            if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
            {
                if (useDefault)
                {
                    valueRaw = settingMeta.DefaultStringValue;
                    valueText = DrsUtil.GetStringSettingValueName(settingMeta, settingMeta.DefaultStringValue);
                }
                else if (setting.isCurrentPredefined == 1 && setting.isPredefinedValid == 1)
                {
                    valueRaw = setting.predefinedValue.stringValue;
                    valueText = DrsUtil.GetStringSettingValueName(settingMeta, setting.predefinedValue.stringValue);
                    settingState = SettingState.NvidiaSetting;
                }
                else
                {
                    valueRaw = setting.currentValue.stringValue;
                    valueText = DrsUtil.GetStringSettingValueName(settingMeta, setting.currentValue.stringValue);

                    if (setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                        settingState = SettingState.UserdefinedSetting;
                    else
                        settingState = SettingState.GlobalSetting;
                }
            }

            if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
            {
                if (useDefault)
                {
                    valueRaw = DrsUtil.GetBinaryString(settingMeta.DefaultBinaryValue);
                    valueText = DrsUtil.GetBinarySettingValueName(settingMeta, settingMeta.DefaultBinaryValue);
                }
                else if (setting.isCurrentPredefined == 1 && setting.isPredefinedValid == 1)
                {
                    valueRaw = DrsUtil.GetBinaryString(setting.predefinedValue.binaryValue);
                    valueText = DrsUtil.GetBinarySettingValueName(settingMeta, setting.predefinedValue.binaryValue);
                    settingState = SettingState.NvidiaSetting;
                }
                else
                {
                    valueRaw = DrsUtil.GetBinaryString(setting.currentValue.binaryValue);
                    valueText = DrsUtil.GetBinarySettingValueName(settingMeta, setting.currentValue.binaryValue);

                    if (setting.settingLocation == NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                        settingState = SettingState.UserdefinedSetting;
                    else
                        settingState = SettingState.GlobalSetting;
                }
            }

            return new SettingItem()
            {
                SettingId = setting.settingId,
                SettingText = settingMeta.SettingName,
                GroupName = settingMeta.GroupName,
                ValueRaw = valueRaw,
                ValueText = valueText,
                State = settingState,
                IsStringValue = settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE,
                IsApiExposed = settingMeta.IsApiExposed,
                IsSettingHidden = settingMeta.IsSettingHidden,
            };
        }


        public List<SettingItem> GetSettingsForProfile(string profileName, SettingViewMode viewMode, ref Dictionary<string, string> applications)
        {
            var result = new List<SettingItem>();
            var settingIds = meta.GetSettingIds(viewMode);
            settingIds.AddRange(_baseProfileSettingIds);
            settingIds = settingIds.Distinct().ToList();

            applications = DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);

                var profileSettings = GetProfileSettings(hSession, hProfile);
                foreach (var profileSetting in profileSettings)
                {
                    result.Add(CreateSettingItem(profileSetting));

                    if (settingIds.Contains(profileSetting.settingId))
                        settingIds.Remove(profileSetting.settingId);
                }

                foreach (var settingId in settingIds)
                {
                    if (settingId == 0) continue;

                    var setting = ReadSetting(hSession, hProfile, settingId);
                    if (setting != null)
                        result.Add(CreateSettingItem(setting.Value));
                    else
                    {
                        var dummySetting = new NVDRS_SETTING() { settingId = settingId };
                        result.Add(CreateSettingItem(dummySetting, true));
                    }
                }

                return GetProfileApplications(hSession, hProfile)
                    .Select(x => Tuple.Create(x.appName, GetApplicationFingerprint(x))).ToDictionary(x => x.Item2, x => x.Item1);

            });

            return result.OrderBy(x => x.SettingText).ThenBy(x => x.GroupName).ToList();
        }

        public void AddApplication(string profileName, string applicationName)
        {
            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);
                AddApplication(hSession, hProfile, applicationName);
                SaveSettings(hSession);
            });
        }

        public void RemoveApplication(string profileName, string applicationFingerprint)
        {
            DrsSession((hSession) =>
            {
                var hProfile = GetProfileHandle(hSession, profileName);
                var applications = GetProfileApplications(hSession, hProfile);
                foreach (var app in applications)
                {
                    if (GetApplicationFingerprint(app) != applicationFingerprint) continue;
                    DeleteApplication(hSession, hProfile, app);
                    break;
                }
                SaveSettings(hSession);
            });
        }

        private string GetApplicationFingerprint(NVDRS_APPLICATION_V4 application)
        {
            return $"{application.appName}|{application.fileInFolder}|{application.userFriendlyName}|{application.launcher}";
        }

    }

}

