using nspector.Native.NVAPI2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;

namespace nspector.Common
{


    internal class DrsScannerService : DrsSettingsServiceBase
    {

        public DrsScannerService(DrsSettingsMetaService metaService, DrsDecrypterService decrpterService)
            : base(metaService, decrpterService)
        { }


        internal List<CachedSettings> CachedSettings = new List<CachedSettings>();
        internal List<string> ModifiedProfiles = new List<string>();
        internal HashSet<string> UserProfiles = new HashSet<string>();

        // most common setting ids as start pattern for the heuristic scan
        private readonly uint[] _commonSettingIds = new uint[] { 0x1095DEF8, 0x1033DCD2, 0x1033CEC1,
                            0x10930F46, 0x00A06946, 0x10ECDB82, 0x20EBD7B8, 0x0095DEF9, 0x00D55F7D,
                            0x1033DCD3, 0x1033CEC2, 0x2072F036, 0x00664339, 0x002C7F45, 0x209746C1,
                            0x0076E164, 0x20FF7493, 0x204CFF7B };


        private bool CheckCommonSetting(IntPtr hSession, IntPtr hProfile, NVDRS_PROFILE profile,
            ref int checkedSettingsCount, uint checkSettingId, bool addToScanResult,
            ref List<uint> alreadyCheckedSettingIds)
        {

            if (checkedSettingsCount >= profile.numOfSettings)
                return false;

            var setting = new NVDRS_SETTING();
            setting.version = nvw.NVDRS_SETTING_VER;

            if (nvw.DRS_GetSetting(hSession, hProfile, checkSettingId, ref setting) != NvAPI_Status.NVAPI_OK)
                return false;

            if (setting.settingLocation != NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                return false;

            if (!addToScanResult && setting.isCurrentPredefined == 1)
            {
                checkedSettingsCount++;
            }
            else if (addToScanResult)
            {
                if (decrypter != null)
                {
                    decrypter.DecryptSettingIfNeeded(profile.profileName, ref setting);
                }

                checkedSettingsCount++;
                AddScannedSettingToCache(profile, setting);
                alreadyCheckedSettingIds.Add(setting.settingId);
                return true;
            }
            else if (setting.isCurrentPredefined != 1)
            {
                return true;
            }

            return false;
        }


        private int CalcPercent(int current, int max)
        {
            return (current > 0) ? (int)Math.Round((current * 100f) / max) : 0; ;
        }

        public async Task ScanForModifiedProfilesAsync(IProgress<int> progress)
        {
            await Task.Run(() =>
            {

                ModifiedProfiles = new List<string>();
                UserProfiles = new HashSet<string>();
                var knownPredefines = new List<uint>(_commonSettingIds);

                DrsSession((hSession) =>
                {
                    IntPtr hBaseProfile = GetProfileHandle(hSession, "");
                    var profileHandles = EnumProfileHandles(hSession);

                    var maxProfileCount = profileHandles.Count;
                    int curProfilePos = 0;

                    foreach (IntPtr hProfile in profileHandles)
                    {
                        progress?.Report(CalcPercent(curProfilePos++, maxProfileCount));

                        var profile = GetProfileInfo(hSession, hProfile);

                        int checkedSettingsCount = 0;
                        bool foundModifiedProfile = false;

                        if (profile.isPredefined == 0)
                        {
                            ModifiedProfiles.Add(profile.profileName);
                            UserProfiles.Add(profile.profileName);
                            continue;
                        }

                        if ((hBaseProfile == hProfile || profile.numOfApps > 0) && profile.numOfSettings > 0)
                        {
                            var alreadyChecked = new List<uint>();

                            foreach (uint settingId in knownPredefines)
                            {
                                if (CheckCommonSetting(hSession, hProfile, profile,
                                    ref checkedSettingsCount, settingId, false, ref alreadyChecked))
                                {
                                    foundModifiedProfile = true;
                                    ModifiedProfiles.Add(profile.profileName);
                                    break;
                                }
                            }

                        // the detection if only applications has changed in a profile makes the scan process very slow, we leave it out!
                        //if (!foundModifiedProfile && profile.numOfApps > 0)
                        //{
                        //    var apps = GetProfileApplications(hSession, hProfile);
                        //    foreach (var app in apps)
                        //    {
                        //        if (app.isPredefined == 0)
                        //        {
                        //            foundModifiedProfile = true;
                        //            ModifiedProfiles.Add(profile.profileName);
                        //            break;
                        //        }
                        //    }
                        //}

                        if (foundModifiedProfile || checkedSettingsCount >= profile.numOfSettings)
                                continue;

                            var settings = GetProfileSettings(hSession, hProfile);
                            foreach (var setting in settings)
                            {
                                if (knownPredefines.IndexOf(setting.settingId) < 0)
                                    knownPredefines.Add(setting.settingId);

                                if (setting.isCurrentPredefined != 1)
                                {
                                    ModifiedProfiles.Add(profile.profileName);
                                    break;
                                }
                            }
                        }
                    }
                });


            });
        }

        public async Task ScanForPredefinedProfileSettingsAsync(IProgress<int> progress)
        {
            await Task.Run(() =>
            {

                var knownPredefines = new List<uint>(_commonSettingIds);

                DrsSession((hSession) =>
                {
                    var profileHandles = EnumProfileHandles(hSession);

                    var maxProfileCount = profileHandles.Count;
                    int curProfilePos = 0;

                    foreach (IntPtr hProfile in profileHandles)
                    {
                        progress?.Report(CalcPercent(curProfilePos++, maxProfileCount));

                        var profile = GetProfileInfo(hSession, hProfile);

                        int checkedSettingsCount = 0;
                        var alreadyChecked = new List<uint>();

                        foreach (uint kpd in knownPredefines)
                        {
                            CheckCommonSetting(hSession, hProfile, profile,
                                ref checkedSettingsCount, kpd, true, ref alreadyChecked);
                        }

                        if (checkedSettingsCount >= profile.numOfSettings)
                            continue;

                        var settings = GetProfileSettings(hSession, hProfile);
                        foreach (var setting in settings)
                        {
                            if (knownPredefines.IndexOf(setting.settingId) < 0)
                                knownPredefines.Add(setting.settingId);

                            if (alreadyChecked.IndexOf(setting.settingId) < 0)
                                AddScannedSettingToCache(profile, setting);
                        }
                    }
                });

            });
        }


        private void AddScannedSettingToCache(NVDRS_PROFILE profile, NVDRS_SETTING setting)
        {
            // Skip 3D Vision string values here for improved scan performance
            bool allowAddValue = setting.settingId < 0x70000000 || setting.settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;

            //bool allowAddValue = true;

            var cachedSetting = CachedSettings
                .FirstOrDefault(x => x.SettingId.Equals(setting.settingId));

            bool cacheEntryExists = true;
            if (cachedSetting == null)
            {
                cacheEntryExists = false;
                cachedSetting = new CachedSettings(setting.settingId, setting.settingType);
            }

            if (setting.isPredefinedValid == 1)
            {
                if (allowAddValue)
                {
                    if (setting.settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                        cachedSetting.AddStringValue(setting.predefinedValue.stringValue, profile.profileName);
                    else if (setting.settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
                        cachedSetting.AddDwordValue(setting.predefinedValue.dwordValue, profile.profileName);
                    else if (setting.settingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
                        cachedSetting.AddBinaryValue(setting.predefinedValue.binaryValue, profile.profileName);
                }

                if (!cacheEntryExists)
                    CachedSettings.Add(cachedSetting);
            }

        }

        public string FindProfilesUsingApplication(string applicationName)
        {
            string lowerApplicationName = applicationName.ToLower();
            string tmpfile = Path.GetTempFileName();
            var matchingProfiles = new List<string>();

            DrsSession((hSession) =>
            {
                SaveSettingsFileEx(hSession, tmpfile);
            });

            if (File.Exists(tmpfile))
            {
                string content = File.ReadAllText(tmpfile);
                string pattern = "\\sProfile\\s\\\"(?<profile>.*?)\\\"(?<scope>.*?Executable.*?)EndProfile";
                foreach (Match m in Regex.Matches(content, pattern, RegexOptions.Singleline))
                {
                    string scope = m.Result("${scope}");
                    foreach (Match ms in Regex.Matches(scope, "Executable\\s\\\"(?<app>.*?)\\\"", RegexOptions.Singleline))
                    {
                        if (ms.Result("${app}").ToLower() == lowerApplicationName)
                        {
                            matchingProfiles.Add(m.Result("${profile}"));
                        }
                    }
                }
            }

            if (File.Exists(tmpfile))
                File.Delete(tmpfile);

            return string.Join(";", matchingProfiles);
        }

    }
}
