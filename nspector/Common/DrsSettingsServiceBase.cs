using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using nspector.Common.Helper;

using nspector.Native.NVAPI2;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;

namespace nspector.Common
{
    internal abstract class DrsSettingsServiceBase
    {
        public static readonly float DriverVersion = GetDriverVersionInternal();

        protected DrsSettingsMetaService meta;
        protected DrsDecrypterService decrypter;

        public DrsSettingsServiceBase(DrsSettingsMetaService metaService, DrsDecrypterService decrpterService = null)
        {
            meta = metaService;
            decrypter = decrpterService;
        }

        private static float GetDriverVersionInternal()
        {
            float result = 0f;
            uint sysDrvVersion = 0;
            var sysDrvBranch = new StringBuilder((int)NvapiDrsWrapper.NVAPI_SHORT_STRING_MAX);

            if (nvw.SYS_GetDriverAndBranchVersion(ref sysDrvVersion, sysDrvBranch) == NvAPI_Status.NVAPI_OK)
            {
                try { result = (float)(sysDrvVersion / 100f); }
                catch { }
            }

            return result;
        }

        protected void DrsSession(Action<IntPtr> action, bool forceNonGlobalSession = false, bool preventLoadSettings = false)
        {
            DrsSessionScope.DrsSession<bool>((hSession) =>
            {
                action(hSession);
                return true;
            }, forceNonGlobalSession: forceNonGlobalSession, preventLoadSettings: preventLoadSettings);
        }

        protected T DrsSession<T>(Func<IntPtr, T> action, bool forceDedicatedScope = false)
        {
            return DrsSessionScope.DrsSession<T>(action, forceDedicatedScope);
        }

        protected IntPtr GetProfileHandle(IntPtr hSession, string profileName)
        {
            var hProfile = IntPtr.Zero;

            if (string.IsNullOrEmpty(profileName))
            {
                var nvRes = nvw.DRS_GetCurrentGlobalProfile(hSession, ref hProfile);

                if (hProfile == IntPtr.Zero)
                    throw new NvapiException("DRS_GetCurrentGlobalProfile", NvAPI_Status.NVAPI_PROFILE_NOT_FOUND);

                if (nvRes != NvAPI_Status.NVAPI_OK)
                    throw new NvapiException("DRS_GetCurrentGlobalProfile", nvRes);
            }
            else
            {
                var nvRes = nvw.DRS_FindProfileByName(hSession, new StringBuilder(profileName), ref hProfile);

                if (nvRes == NvAPI_Status.NVAPI_PROFILE_NOT_FOUND)
                    return IntPtr.Zero;

                if (nvRes != NvAPI_Status.NVAPI_OK)
                    throw new NvapiException("DRS_FindProfileByName", nvRes);
            }
            return hProfile;
        }

        protected IntPtr CreateProfile(IntPtr hSession, string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
                throw new ArgumentNullException("profileName");

            var hProfile = IntPtr.Zero;

            var newProfile = new NVDRS_PROFILE()
            {
                version = nvw.NVDRS_PROFILE_VER,
                profileName = profileName,
            };

            var nvRes = nvw.DRS_CreateProfile(hSession, ref newProfile, ref hProfile);
            if (nvRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_CreateProfile", nvRes);

            return hProfile;
        }


        protected NVDRS_PROFILE GetProfileInfo(IntPtr hSession, IntPtr hProfile)
        {
            var tmpProfile = new NVDRS_PROFILE();
            tmpProfile.version = nvw.NVDRS_PROFILE_VER;

            var gpRes = nvw.DRS_GetProfileInfo(hSession, hProfile, ref tmpProfile);
            if (gpRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_GetProfileInfo", gpRes);

            return tmpProfile;
        }

        protected void StoreSetting(IntPtr hSession, IntPtr hProfile, NVDRS_SETTING newSetting)
        {
            var ssRes = nvw.DRS_SetSetting(hSession, hProfile, ref newSetting);
            if (ssRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_SetSetting", ssRes);
        }

        protected void StoreDwordValue(IntPtr hSession, IntPtr hProfile, uint settingId, uint dwordValue)
        {
            var newSetting = new NVDRS_SETTING()
            {
                version = nvw.NVDRS_SETTING_VER,
                settingId = settingId,
                settingType = NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE,
                settingLocation = NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,
                currentValue = new NVDRS_SETTING_UNION()
                {
                    dwordValue = dwordValue,
                },
            };

            var ssRes = nvw.DRS_SetSetting(hSession, hProfile, ref newSetting);
            if (ssRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_SetSetting", ssRes);

        }

        protected void StoreStringValue(IntPtr hSession, IntPtr hProfile, uint settingId, string stringValue)
        {
            var newSetting = new NVDRS_SETTING()
            {
                version = nvw.NVDRS_SETTING_VER,
                settingId = settingId,
                settingType = NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE,
                settingLocation = NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,
                currentValue = new NVDRS_SETTING_UNION()
                {
                    stringValue = stringValue,
                },
            };

            var ssRes = nvw.DRS_SetSetting(hSession, hProfile, ref newSetting);
            if (ssRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_SetSetting", ssRes);

        }

        protected void StoreBinaryValue(IntPtr hSession, IntPtr hProfile, uint settingId, byte[] binValue)
        {
            var newSetting = new NVDRS_SETTING()
            {
                version = nvw.NVDRS_SETTING_VER,
                settingId = settingId,
                settingType = NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE,
                settingLocation = NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,
                currentValue = new NVDRS_SETTING_UNION()
                {
                    binaryValue = binValue,
                },
            };

            var ssRes = nvw.DRS_SetSetting(hSession, hProfile, ref newSetting);
            if (ssRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_SetSetting", ssRes);

        }

        protected NVDRS_SETTING? ReadSetting(IntPtr hSession, IntPtr hProfile, uint settingId)
        {
            var newSetting = new NVDRS_SETTING()
            {
                version = nvw.NVDRS_SETTING_VER,
            };

            var ssRes = nvw.DRS_GetSetting(hSession, hProfile, settingId, ref newSetting);
            if (ssRes == NvAPI_Status.NVAPI_SETTING_NOT_FOUND)
                return null;

            if (ssRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_GetSetting", ssRes);

            if (decrypter != null)
            {
                var profile = GetProfileInfo(hSession, hProfile);
                decrypter.DecryptSettingIfNeeded(profile.profileName, ref newSetting);
            }

            return newSetting;
        }

        protected uint? ReadDwordValue(IntPtr hSession, IntPtr hProfile, uint settingId)
        {
            var newSetting = ReadSetting(hSession, hProfile, settingId);
            if (newSetting == null)
                return null;
            return newSetting.Value.currentValue.dwordValue;
        }

        protected void AddApplication(IntPtr hSession, IntPtr hProfile, string applicationName)
        {
            var newApp = new NVDRS_APPLICATION_V4()
            {
                version = nvw.NVDRS_APPLICATION_VER_V4,
                appName = applicationName,
            };

            var caRes = nvw.DRS_CreateApplication(hSession, hProfile, ref newApp);
            if (caRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_CreateApplication", caRes);

        }

        protected void DeleteApplication(IntPtr hSession, IntPtr hProfile, NVDRS_APPLICATION_V4 application)
        {
            var caRes = nvw.DRS_DeleteApplicationEx(hSession, hProfile, ref application);
            if (caRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_DeleteApplication", caRes);
        }

        protected List<IntPtr> EnumProfileHandles(IntPtr hSession)
        {
            var profileHandles = new List<IntPtr>();
            var hProfile = IntPtr.Zero;
            uint index = 0;

            NvAPI_Status nvRes;

            do
            {
                nvRes = nvw.DRS_EnumProfiles(hSession, index, ref hProfile);
                if (nvRes == NvAPI_Status.NVAPI_OK)
                {
                    profileHandles.Add(hProfile);
                }
                index++;
            }
            while (nvRes == NvAPI_Status.NVAPI_OK);

            if (nvRes != NvAPI_Status.NVAPI_END_ENUMERATION)
                throw new NvapiException("DRS_EnumProfiles", nvRes);

            return profileHandles;
        }

        protected List<NVDRS_SETTING> GetProfileSettings(IntPtr hSession, IntPtr hProfile)
        {
            uint settingCount = 512;
            var settings = new NVDRS_SETTING[settingCount];
            settings[0].version = NvapiDrsWrapper.NVDRS_SETTING_VER;

            var esRes = NvapiDrsWrapper.DRS_EnumSettings(hSession, hProfile, 0, ref settingCount, ref settings);

            if (esRes == NvAPI_Status.NVAPI_END_ENUMERATION)
                return new List<NVDRS_SETTING>();

            if (esRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_EnumSettings", esRes);

            if (decrypter != null)
            {
                var profile = GetProfileInfo(hSession, hProfile);
                for (int i = 0; i < settingCount; i++)
                {
                    decrypter.DecryptSettingIfNeeded(profile.profileName, ref settings[i]);
                }
            }

            return settings.ToList();
        }

        protected List<NVDRS_APPLICATION_V4> GetProfileApplications(IntPtr hSession, IntPtr hProfile)
        {
            uint appCount = 512;
            var apps = new NVDRS_APPLICATION_V4[512];
            apps[0].version = NvapiDrsWrapper.NVDRS_APPLICATION_VER_V4;

            var esRes = NvapiDrsWrapper.DRS_EnumApplications(hSession, hProfile, 0, ref appCount, ref apps);

            if (esRes == NvAPI_Status.NVAPI_END_ENUMERATION)
                return new List<NVDRS_APPLICATION_V4>();

            if (esRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_EnumApplications", esRes);

            return apps.ToList();
        }

        protected IntPtr FindApplicationByName(IntPtr hSession, string appName)
        {
            IntPtr hProfile = IntPtr.Zero;
            NVDRS_APPLICATION_V4 app = new NVDRS_APPLICATION_V4();
            app.version = NvapiDrsWrapper.NVDRS_APPLICATION_VER_V4;

            var res = NvapiDrsWrapper.DRS_FindApplicationByName(hSession, new StringBuilder(appName), ref hProfile, ref app);

            if (res != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_FindApplicationByName", res);

            return hProfile;
        }

        protected void SaveSettings(IntPtr hSession)
        {
            var nvRes = nvw.DRS_SaveSettings(hSession);
            if (nvRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_SaveSettings", nvRes);
        }

        protected void LoadSettingsFileEx(IntPtr hSession, string filename)
        {
            var nvRes = nvw.DRS_LoadSettingsFromFileEx(hSession, new StringBuilder(filename));
            if (nvRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_LoadSettingsFromFileEx", nvRes);
        }

        protected void SaveSettingsFileEx(IntPtr hSession, string filename)
        {
            var nvRes = nvw.DRS_SaveSettingsToFileEx(hSession, new StringBuilder(filename));
            if (nvRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_SaveSettingsToFileEx", nvRes);
        }

    }

}

