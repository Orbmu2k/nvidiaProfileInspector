using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using nvidiaProfileInspector.Common.Helper;
using nvidiaProfileInspector.Native.WINAPI;

namespace nvidiaProfileInspector.Native.NVAPI2
{
    public partial class NvapiDrsWrapper
    {
        public const uint MockWin11BackdropSettingId = 0x00F01111;

        private static readonly string[] MockWin11BackdropModes =
        {
            "Default",
            "MainWindow",
            "Tabbed",
            "Acrylic",
            "Disabled"
        };

        private readonly object _mockSync = new object();
        private readonly Dictionary<IntPtr, MockSessionState> _mockSessions = new Dictionary<IntPtr, MockSessionState>();
        private readonly Dictionary<IntPtr, MockProfileState> _mockProfilesByHandle = new Dictionary<IntPtr, MockProfileState>();
        private readonly Dictionary<uint, MockSettingDefinition> _mockSettingDefinitions = new Dictionary<uint, MockSettingDefinition>();

        private IntPtr _mockCurrentGlobalProfileHandle = IntPtr.Zero;
        private int _mockNextSessionId = 1;
        private int _mockNextProfileHandle = 1;
        private string _mockWin11BackdropMode = "Default";

        private static bool ShouldUseMock()
        {
            return Environment.GetCommandLineArgs()
                .Any(x => x.Equals("-mock", StringComparison.OrdinalIgnoreCase));
        }

        private void InitializeMockState()
        {
            lock (_mockSync)
            {
                _mockSessions.Clear();
                _mockProfilesByHandle.Clear();
                _mockSettingDefinitions.Clear();
                _mockCurrentGlobalProfileHandle = IntPtr.Zero;
                _mockNextSessionId = 1;
                _mockNextProfileHandle = 1;
                _mockWin11BackdropMode = GetConfiguredMockBackdropMode();

                var baseProfile = CreateMockProfile("Base Profile", isPredefined: true);
                _mockCurrentGlobalProfileHandle = baseProfile.Handle;
                RegisterMockBackdropSettingDefinition();
                baseProfile.Settings[MockWin11BackdropSettingId] = CreateMockBackdropSetting();
                baseProfile.RefreshCounts();
            }
        }

        private NvAPI_Status MockNvAPI_Initialize()
        {
            return NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockInitialize()
        {
            return NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockUnload()
        {
            return NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockGetErrorMessage(NvAPI_Status nr, StringBuilder szDesc)
        {
            if (szDesc != null)
            {
                szDesc.Clear();
                szDesc.Append("Mock NVAPI status: " + nr);
            }

            return NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockGetInterfaceVersionString(StringBuilder szDesc)
        {
            if (szDesc != null)
            {
                szDesc.Clear();
                szDesc.Append("Mock NVAPI");
            }

            return NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockSYS_GetDriverAndBranchVersion(ref uint pDriverVersion, StringBuilder szBuildBranchString)
        {
            pDriverVersion = 0;
            if (szBuildBranchString != null)
            {
                szBuildBranchString.Clear();
                szBuildBranchString.Append("mock");
            }

            return NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockDRS_CreateSession(ref IntPtr phSession)
        {
            lock (_mockSync)
            {
                phSession = new IntPtr(_mockNextSessionId++);
                _mockSessions[phSession] = new MockSessionState(phSession);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_DestroySession(IntPtr hSession)
        {
            lock (_mockSync)
            {
                _mockSessions.Remove(hSession);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_LoadSettings(IntPtr hSession)
        {
            return EnsureMockSession(hSession);
        }

        private NvAPI_Status MockDRS_SaveSettings(IntPtr hSession)
        {
            return EnsureMockSession(hSession);
        }

        private NvAPI_Status MockDRS_LoadSettingsFromFile(IntPtr hSession, StringBuilder fileName)
        {
            return MockDRS_LoadSettingsFromFileEx(hSession, fileName);
        }

        private NvAPI_Status MockDRS_SaveSettingsToFile(IntPtr hSession, StringBuilder fileName)
        {
            return MockDRS_SaveSettingsToFileEx(hSession, fileName);
        }

        private NvAPI_Status MockDRS_LoadSettingsFromFileEx(IntPtr hSession, StringBuilder fileName)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            var path = fileName?.ToString();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return NvAPI_Status.NVAPI_FILE_NOT_FOUND;

            try
            {
                var content = File.ReadAllText(path);
                var profiles = ParseMockProfiles(content);

                lock (_mockSync)
                {
                    _mockProfilesByHandle.Clear();
                    _mockNextProfileHandle = 1;

                    var baseProfile = CreateMockProfile("Base Profile", isPredefined: true);
                    _mockCurrentGlobalProfileHandle = baseProfile.Handle;

                    foreach (var parsed in profiles.Where(p => !p.ProfileName.Equals(baseProfile.ProfileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var profile = CreateMockProfile(parsed.ProfileName, isPredefined: false);
                        profile.Applications = parsed.Applications.Select(CloneApplication).ToList();
                        profile.RefreshCounts();
                    }
                }

                return NvAPI_Status.NVAPI_OK;
            }
            catch
            {
                return NvAPI_Status.NVAPI_ERROR;
            }
        }

        private NvAPI_Status MockDRS_SaveSettingsToFileEx(IntPtr hSession, StringBuilder fileName)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            var path = fileName?.ToString();
            if (string.IsNullOrWhiteSpace(path))
                return NvAPI_Status.NVAPI_INVALID_ARGUMENT;

            try
            {
                File.WriteAllText(path, BuildMockExportText(), Encoding.Unicode);
                return NvAPI_Status.NVAPI_OK;
            }
            catch
            {
                return NvAPI_Status.NVAPI_ERROR;
            }
        }

        private NvAPI_Status MockDRS_CreateProfile(IntPtr hSession, ref NVDRS_PROFILE pProfileInfo, ref IntPtr phProfile)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            if (string.IsNullOrWhiteSpace(pProfileInfo.profileName))
                return NvAPI_Status.NVAPI_PROFILE_NAME_EMPTY;

            lock (_mockSync)
            {
                if (TryGetProfileByName(pProfileInfo.profileName, out _))
                    return NvAPI_Status.NVAPI_PROFILE_NAME_IN_USE;

                var profile = CreateMockProfile(pProfileInfo.profileName, isPredefined: false);
                phProfile = profile.Handle;
                pProfileInfo = profile.ToProfile();
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_DeleteProfile(IntPtr hSession, IntPtr hProfile)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.ContainsKey(hProfile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                if (hProfile == _mockCurrentGlobalProfileHandle)
                    return NvAPI_Status.NVAPI_ACCESS_DENIED;

                _mockProfilesByHandle.Remove(hProfile);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_SetCurrentGlobalProfile(IntPtr hSession, StringBuilder wszGlobalProfileName)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!TryGetProfileByName(wszGlobalProfileName?.ToString(), out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                _mockCurrentGlobalProfileHandle = profile.Handle;
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_GetCurrentGlobalProfile(IntPtr hSession, ref IntPtr phProfile)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            phProfile = _mockCurrentGlobalProfileHandle;
            return phProfile == IntPtr.Zero ? NvAPI_Status.NVAPI_PROFILE_NOT_FOUND : NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockDRS_GetProfileInfo(IntPtr hSession, IntPtr hProfile, ref NVDRS_PROFILE pProfileInfo)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                pProfileInfo = profile.ToProfile();
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_SetProfileInfo(IntPtr hSession, IntPtr hProfile, ref NVDRS_PROFILE pProfileInfo)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            if (string.IsNullOrWhiteSpace(pProfileInfo.profileName))
                return NvAPI_Status.NVAPI_PROFILE_NAME_EMPTY;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                if (!profile.ProfileName.Equals(pProfileInfo.profileName, StringComparison.OrdinalIgnoreCase)
                    && TryGetProfileByName(pProfileInfo.profileName, out _))
                    return NvAPI_Status.NVAPI_PROFILE_NAME_IN_USE;

                profile.ProfileName = pProfileInfo.profileName;
                profile.IsPredefined = pProfileInfo.isPredefined == 1;
                profile.RefreshCounts();
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_FindProfileByName(IntPtr hSession, StringBuilder profileName, ref IntPtr phProfile)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!TryGetProfileByName(profileName?.ToString(), out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                phProfile = profile.Handle;
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_EnumProfiles(IntPtr hSession, uint index, ref IntPtr phProfile)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                var profiles = _mockProfilesByHandle.Values.OrderBy(x => x.ProfileName).ToList();
                if (index >= profiles.Count)
                    return NvAPI_Status.NVAPI_END_ENUMERATION;

                phProfile = profiles[(int)index].Handle;
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_GetNumProfiles(IntPtr hSession, ref uint numProfiles)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                numProfiles = (uint)_mockProfilesByHandle.Count;
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_CreateApplication(IntPtr hSession, IntPtr hProfile, ref NVDRS_APPLICATION_V4 pApplication)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            var applicationName = pApplication.appName;
            if (string.IsNullOrWhiteSpace(applicationName))
                return NvAPI_Status.NVAPI_EXECUTABLE_NOT_FOUND;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                if (TryFindApplicationOwner(applicationName, out var owner) && owner.Handle != hProfile)
                    return NvAPI_Status.NVAPI_EXECUTABLE_ALREADY_IN_USE;

                profile.Applications.RemoveAll(x => x.appName.Equals(applicationName, StringComparison.OrdinalIgnoreCase));
                profile.Applications.Add(CloneApplication(pApplication));
                profile.RefreshCounts();
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_DeleteApplicationEx(IntPtr hSession, IntPtr hProfile, ref NVDRS_APPLICATION_V4 pApp)
        {
            return DeleteMockApplication(hSession, hProfile, pApp.appName);
        }

        private NvAPI_Status MockDRS_DeleteApplication(IntPtr hSession, IntPtr hProfile, StringBuilder appName)
        {
            return DeleteMockApplication(hSession, hProfile, appName?.ToString());
        }

        private NvAPI_Status MockDRS_GetApplicationInfo(IntPtr hSession, IntPtr hProfile, StringBuilder appName, ref NVDRS_APPLICATION_V4 pApplication)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                var app = profile.Applications.FirstOrDefault(x => x.appName.Equals(appName?.ToString(), StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrWhiteSpace(app.appName))
                    return NvAPI_Status.NVAPI_EXECUTABLE_NOT_FOUND;

                pApplication = CloneApplication(app);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_EnumApplications(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint appCount, IntPtr pApplication)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                var apps = profile.Applications.OrderBy(x => x.appName).ToList();
                if (startIndex >= apps.Count)
                    return NvAPI_Status.NVAPI_END_ENUMERATION;

                var count = (int)Math.Min(appCount, (uint)(apps.Count - startIndex));
                for (int i = 0; i < count; i++)
                {
                    Marshal.StructureToPtr(CloneApplication(apps[(int)startIndex + i]), IntPtr.Add(pApplication, i * Marshal.SizeOf(typeof(NVDRS_APPLICATION_V4))), false);
                }

                appCount = (uint)count;
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_FindApplicationByName(IntPtr hSession, StringBuilder appName, ref IntPtr phProfile, ref NVDRS_APPLICATION_V4 pApplication)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!TryFindApplicationOwner(appName?.ToString(), out var owner))
                    return NvAPI_Status.NVAPI_EXECUTABLE_NOT_FOUND;

                var application = owner.Applications.First(x => x.appName.Equals(appName.ToString(), StringComparison.OrdinalIgnoreCase));
                phProfile = owner.Handle;
                pApplication = CloneApplication(application);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_SetSetting(IntPtr hSession, IntPtr hProfile, ref NVDRS_SETTING pSetting, uint x, uint y)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            if (pSetting.settingId == MockWin11BackdropSettingId)
                return ApplyMockBackdropModeSetting(hProfile, ref pSetting);

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                var setting = CloneSetting(pSetting);
                setting.version = NVDRS_SETTING_VER;
                setting.settingLocation = NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;
                setting.isCurrentPredefined = 0;
                setting.isPredefinedValid = 0;
                if (string.IsNullOrWhiteSpace(setting.settingName))
                    setting.settingName = GetMockSettingName(setting.settingId);

                profile.Settings[setting.settingId] = setting;
                profile.RefreshCounts();
                RememberSettingDefinition(setting);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_GetSetting(IntPtr hSession, IntPtr hProfile, uint settingId, ref NVDRS_SETTING pSetting, ref uint x)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                if (!profile.Settings.TryGetValue(settingId, out var setting))
                    return NvAPI_Status.NVAPI_SETTING_NOT_FOUND;

                pSetting = CloneSetting(setting);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_EnumSettings(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint settingsCount, IntPtr pSetting)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                var settings = profile.Settings.Values.OrderBy(x => x.settingId).ToList();
                if (startIndex >= settings.Count)
                    return NvAPI_Status.NVAPI_END_ENUMERATION;

                var count = (int)Math.Min(settingsCount, (uint)(settings.Count - startIndex));
                for (int i = 0; i < count; i++)
                {
                    Marshal.StructureToPtr(CloneSetting(settings[(int)startIndex + i]), IntPtr.Add(pSetting, i * Marshal.SizeOf(typeof(NVDRS_SETTING))), false);
                }

                settingsCount = (uint)count;
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_EnumAvailableSettingIds(IntPtr pSettingIds, ref uint pMaxCount)
        {
            lock (_mockSync)
            {
                var settingIds = GetAllKnownSettingIds().ToList();
                var count = (int)Math.Min(pMaxCount, (uint)settingIds.Count);
                for (int i = 0; i < count; i++)
                {
                    Marshal.WriteInt32(pSettingIds, i * sizeof(uint), unchecked((int)settingIds[i]));
                }

                pMaxCount = (uint)count;
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_EnumAvailableSettingValues(uint settingId, ref uint pMaxNumValues, IntPtr pSettingValues)
        {
            lock (_mockSync)
            {
                var definition = GetMockSettingDefinition(settingId);
                if (definition == null)
                    return NvAPI_Status.NVAPI_SETTING_NOT_FOUND;

                var valueList = definition.Values.Take((int)Math.Min((uint)definition.Values.Count, pMaxNumValues)).ToList();
                var values = new NVDRS_SETTING_VALUES
                {
                    version = NVDRS_SETTING_VALUES_VER,
                    numSettingValues = (uint)valueList.Count,
                    settingType = definition.SettingType,
                    defaultValue = CloneUnion(definition.DefaultValue),
                    settingValues = new NVDRS_SETTING_UNION[(int)NVAPI_SETTING_MAX_VALUES],
                };

                for (int i = 0; i < valueList.Count; i++)
                {
                    values.settingValues[i] = CloneUnion(valueList[i]);
                }

                pMaxNumValues = (uint)valueList.Count;
                Marshal.StructureToPtr(values, pSettingValues, true);
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_GetSettingIdFromName(StringBuilder settingName, ref uint pSettingId)
        {
            lock (_mockSync)
            {
                var targetName = settingName?.ToString();
                foreach (var pair in _mockSettingDefinitions)
                {
                    if (pair.Value.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        pSettingId = pair.Key;
                        return NvAPI_Status.NVAPI_OK;
                    }
                }

                return NvAPI_Status.NVAPI_SETTING_NOT_FOUND;
            }
        }

        private NvAPI_Status MockDRS_GetSettingNameFromId(uint settingId, StringBuilder pSettingName)
        {
            if (pSettingName == null)
                return NvAPI_Status.NVAPI_INVALID_POINTER;

            pSettingName.Clear();
            pSettingName.Append(GetMockSettingName(settingId));
            return NvAPI_Status.NVAPI_OK;
        }

        private NvAPI_Status MockDRS_DeleteProfileSetting(IntPtr hSession, IntPtr hProfile, uint settingId)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            if (settingId == MockWin11BackdropSettingId)
                return ResetMockBackdropMode(hProfile);

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                if (!profile.Settings.Remove(settingId))
                    return NvAPI_Status.NVAPI_SETTING_NOT_FOUND;

                profile.RefreshCounts();
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_RestoreAllDefaults(IntPtr hSession)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                foreach (var profile in _mockProfilesByHandle.Values)
                {
                    profile.Settings.Clear();
                    if (!profile.IsPredefined)
                        profile.Applications.Clear();
                    profile.RefreshCounts();
                }

                var userProfiles = _mockProfilesByHandle.Values.Where(x => !x.IsPredefined).Select(x => x.Handle).ToList();
                foreach (var handle in userProfiles)
                {
                    _mockProfilesByHandle.Remove(handle);
                }

                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_RestoreProfileDefault(IntPtr hSession, IntPtr hProfile)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                profile.Settings.Clear();
                profile.Applications.Clear();
                profile.RefreshCounts();
                return NvAPI_Status.NVAPI_OK;
            }
        }

        private NvAPI_Status MockDRS_RestoreProfileDefaultSetting(IntPtr hSession, IntPtr hProfile, uint settingId)
        {
            if (settingId == MockWin11BackdropSettingId)
                return ResetMockBackdropMode(hProfile);

            return MockDRS_DeleteProfileSetting(hSession, hProfile, settingId);
        }

        private NvAPI_Status MockDRS_GetBaseProfile(IntPtr hSession, ref IntPtr phProfile)
        {
            return MockDRS_GetCurrentGlobalProfile(hSession, ref phProfile);
        }

        private NvAPI_Status EnsureMockSession(IntPtr hSession)
        {
            lock (_mockSync)
            {
                return _mockSessions.ContainsKey(hSession) ? NvAPI_Status.NVAPI_OK : NvAPI_Status.NVAPI_INVALID_HANDLE;
            }
        }

        private MockProfileState CreateMockProfile(string profileName, bool isPredefined)
        {
            var handle = new IntPtr(_mockNextProfileHandle++);
            var profile = new MockProfileState(handle, profileName, isPredefined);
            _mockProfilesByHandle[handle] = profile;
            return profile;
        }

        private bool TryGetProfileByName(string profileName, out MockProfileState profile)
        {
            profile = _mockProfilesByHandle.Values.FirstOrDefault(x => x.ProfileName.Equals(profileName ?? string.Empty, StringComparison.OrdinalIgnoreCase));
            return profile != null;
        }

        private bool TryFindApplicationOwner(string appName, out MockProfileState owner)
        {
            owner = _mockProfilesByHandle.Values.FirstOrDefault(x => x.Applications.Any(a => a.appName.Equals(appName ?? string.Empty, StringComparison.OrdinalIgnoreCase)));
            return owner != null;
        }

        private NvAPI_Status DeleteMockApplication(IntPtr hSession, IntPtr hProfile, string appName)
        {
            if (EnsureMockSession(hSession) != NvAPI_Status.NVAPI_OK)
                return NvAPI_Status.NVAPI_INVALID_HANDLE;

            lock (_mockSync)
            {
                if (!_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    return NvAPI_Status.NVAPI_PROFILE_NOT_FOUND;

                var removedCount = profile.Applications.RemoveAll(x => x.appName.Equals(appName ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                profile.RefreshCounts();
                return removedCount > 0 ? NvAPI_Status.NVAPI_OK : NvAPI_Status.NVAPI_EXECUTABLE_NOT_FOUND;
            }
        }

        private IEnumerable<uint> GetAllKnownSettingIds()
        {
            return _mockSettingDefinitions.Keys
                .Concat(_mockProfilesByHandle.Values.SelectMany(x => x.Settings.Keys))
                .Distinct()
                .OrderBy(x => x);
        }

        private MockSettingDefinition GetMockSettingDefinition(uint settingId)
        {
            if (_mockSettingDefinitions.TryGetValue(settingId, out var definition))
                return definition;

            var firstSetting = _mockProfilesByHandle.Values
                .SelectMany(x => x.Settings.Values)
                .FirstOrDefault(x => x.settingId == settingId);

            if (firstSetting.settingId == 0)
                return null;

            definition = new MockSettingDefinition
            {
                Name = firstSetting.settingName,
                SettingType = firstSetting.settingType,
                DefaultValue = CloneUnion(firstSetting.currentValue),
                Values = new List<NVDRS_SETTING_UNION> { CloneUnion(firstSetting.currentValue) },
            };

            _mockSettingDefinitions[settingId] = definition;
            return definition;
        }

        private void RememberSettingDefinition(NVDRS_SETTING setting)
        {
            if (!_mockSettingDefinitions.TryGetValue(setting.settingId, out var definition))
            {
                definition = new MockSettingDefinition
                {
                    Name = setting.settingName,
                    SettingType = setting.settingType,
                    DefaultValue = CloneUnion(setting.currentValue),
                    Values = new List<NVDRS_SETTING_UNION>(),
                };

                _mockSettingDefinitions[setting.settingId] = definition;
            }

            if (string.IsNullOrWhiteSpace(definition.Name) && !string.IsNullOrWhiteSpace(setting.settingName))
                definition.Name = setting.settingName;

            if (!definition.Values.Any(x => AreEqual(x, setting.currentValue, setting.settingType)))
                definition.Values.Add(CloneUnion(setting.currentValue));
        }

        private string GetMockSettingName(uint settingId)
        {
            if (_mockSettingDefinitions.TryGetValue(settingId, out var definition) && !string.IsNullOrWhiteSpace(definition.Name))
                return definition.Name;

            return string.Format("0x{0:X8}", settingId);
        }

        private void RegisterMockBackdropSettingDefinition()
        {
            _mockSettingDefinitions[MockWin11BackdropSettingId] = new MockSettingDefinition
            {
                Name = "Win11 Backdrop Mode",
                SettingType = NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE,
                DefaultValue = CreateStringUnion(_mockWin11BackdropMode),
                Values = MockWin11BackdropModes.Select(CreateStringUnion).ToList(),
            };
        }

        private string GetConfiguredMockBackdropMode()
        {
            var configuredMode = UserSettings.LoadSettings().Win11BackdropMode;
            if (MockWin11BackdropModes.Any(x => x.Equals(configuredMode, StringComparison.OrdinalIgnoreCase)))
                return MockWin11BackdropModes.First(x => x.Equals(configuredMode, StringComparison.OrdinalIgnoreCase));

            return "Default";
        }

        private NVDRS_SETTING CreateMockBackdropSetting()
        {
            return new NVDRS_SETTING
            {
                version = NVDRS_SETTING_VER,
                settingId = MockWin11BackdropSettingId,
                settingName = "Win11 Backdrop Mode",
                settingType = NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE,
                settingLocation = NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,
                isCurrentPredefined = 0,
                isPredefinedValid = 1,
                predefinedValue = CreateStringUnion(_mockWin11BackdropMode),
                currentValue = CreateStringUnion(_mockWin11BackdropMode),
            };
        }

        private NvAPI_Status ApplyMockBackdropModeSetting(IntPtr hProfile, ref NVDRS_SETTING setting)
        {
            try
            {
                var mode = setting.currentValue.stringValue;
                if (!MockWin11BackdropModes.Any(x => x.Equals(mode, StringComparison.OrdinalIgnoreCase)))
                    mode = "Default";

                _mockWin11BackdropMode = mode;

                RegisterMockBackdropSettingDefinition();

                if (Application.Current?.MainWindow != null)
                    WindowBackdropHelper.TryApplyTo(Application.Current.MainWindow);

                setting.version = NVDRS_SETTING_VER;
                setting.settingName = GetMockSettingName(setting.settingId);
                setting.settingType = NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE;
                setting.settingLocation = NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;
                setting.isCurrentPredefined = 0;
                setting.isPredefinedValid = 1;
                setting.predefinedValue = CreateStringUnion(mode);
                setting.currentValue = CreateStringUnion(mode);

                lock (_mockSync)
                {
                    if (_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    {
                        profile.Settings[MockWin11BackdropSettingId] = CloneSetting(setting);
                        profile.RefreshCounts();
                    }
                }

                return NvAPI_Status.NVAPI_OK;
            }
            catch
            {
                return NvAPI_Status.NVAPI_ERROR;
            }
        }

        private NvAPI_Status ResetMockBackdropMode(IntPtr hProfile)
        {
            try
            {
                _mockWin11BackdropMode = GetConfiguredMockBackdropMode();
                RegisterMockBackdropSettingDefinition();

                lock (_mockSync)
                {
                    if (_mockProfilesByHandle.TryGetValue(hProfile, out var profile))
                    {
                        profile.Settings[MockWin11BackdropSettingId] = CreateMockBackdropSetting();
                        profile.RefreshCounts();
                    }
                }

                if (Application.Current?.MainWindow != null)
                    WindowBackdropHelper.TryApplyTo(Application.Current.MainWindow);

                return NvAPI_Status.NVAPI_OK;
            }
            catch
            {
                return NvAPI_Status.NVAPI_ERROR;
            }
        }

        private static NVDRS_SETTING_UNION CreateStringUnion(string value)
        {
            var union = new NVDRS_SETTING_UNION();
            union.stringValue = value ?? string.Empty;
            return union;
        }

        public string GetMockWin11BackdropMode()
        {
            lock (_mockSync)
            {
                return string.IsNullOrWhiteSpace(_mockWin11BackdropMode) ? "Default" : _mockWin11BackdropMode;
            }
        }

        public void SetMockWin11BackdropMode(string mode)
        {
            lock (_mockSync)
            {
                if (!MockWin11BackdropModes.Any(x => x.Equals(mode, StringComparison.OrdinalIgnoreCase)))
                    mode = "Default";

                _mockWin11BackdropMode = mode;
                RegisterMockBackdropSettingDefinition();

                if (_mockProfilesByHandle.TryGetValue(_mockCurrentGlobalProfileHandle, out var profile))
                {
                    profile.Settings[MockWin11BackdropSettingId] = CreateMockBackdropSetting();
                    profile.RefreshCounts();
                }
            }
        }

        public bool TryGetMockSettingValueType(uint settingId, out NVDRS_SETTING_TYPE settingType)
        {
            lock (_mockSync)
            {
                if (_mockSettingDefinitions.TryGetValue(settingId, out var definition))
                {
                    settingType = definition.SettingType;
                    return true;
                }
            }

            settingType = default;
            return false;
        }

        public bool TryGetMockSettingName(uint settingId, out string name)
        {
            lock (_mockSync)
            {
                if (_mockSettingDefinitions.TryGetValue(settingId, out var definition) && !string.IsNullOrWhiteSpace(definition.Name))
                {
                    name = definition.Name;
                    return true;
                }
            }

            name = null;
            return false;
        }

        public IReadOnlyList<string> GetMockStringSettingValues(uint settingId)
        {
            lock (_mockSync)
            {
                if (_mockSettingDefinitions.TryGetValue(settingId, out var definition)
                    && definition.SettingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                {
                    return definition.Values
                        .Select(x => x.stringValue)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }

            return Array.Empty<string>();
        }
        private static bool AreEqual(NVDRS_SETTING_UNION left, NVDRS_SETTING_UNION right, NVDRS_SETTING_TYPE type)
        {
            switch (type)
            {
                case NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE:
                    return (left.binaryValue ?? Array.Empty<byte>()).SequenceEqual(right.binaryValue ?? Array.Empty<byte>());
                case NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE:
                    return string.Equals(left.ansiStringValue, right.ansiStringValue, StringComparison.Ordinal);
                case NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE:
                    return string.Equals(left.stringValue, right.stringValue, StringComparison.Ordinal);
                default:
                    return left.dwordValue == right.dwordValue;
            }
        }

        private static NVDRS_SETTING CloneSetting(NVDRS_SETTING setting)
        {
            setting.currentValue = CloneUnion(setting.currentValue);
            setting.predefinedValue = CloneUnion(setting.predefinedValue);
            return setting;
        }

        private static NVDRS_SETTING_UNION CloneUnion(NVDRS_SETTING_UNION value)
        {
            return new NVDRS_SETTING_UNION
            {
                rawData = value.rawData != null ? (byte[])value.rawData.Clone() : new byte[4100],
            };
        }

        private static NVDRS_APPLICATION_V4 CloneApplication(NVDRS_APPLICATION_V4 application)
        {
            return application;
        }

        private string BuildMockExportText()
        {
            var sb = new StringBuilder();
            var globalProfile = _mockProfilesByHandle[_mockCurrentGlobalProfileHandle];

            sb.AppendLine("BaseProfile \"Base Profile\"");
            sb.AppendLine("SelectedGlobalProfile \"" + globalProfile.ProfileName + "\"");

            foreach (var profile in _mockProfilesByHandle.Values.OrderBy(x => x.ProfileName))
            {
                sb.AppendLine("Profile \"" + profile.ProfileName + "\"");
                sb.AppendLine("ShowOn All");
                sb.AppendLine(profile.IsPredefined ? "ProfileType Global" : "ProfileType Application");
                foreach (var app in profile.Applications.OrderBy(x => x.appName))
                {
                    sb.AppendLine("Executable \"" + app.appName + "\"");
                }
                sb.AppendLine("EndProfile");
            }

            return sb.ToString();
        }

        private static List<ParsedMockProfile> ParseMockProfiles(string content)
        {
            var result = new List<ParsedMockProfile>();
            foreach (Match match in Regex.Matches(content ?? string.Empty, "Profile\\s+\"(?<name>[^\"]+)\"(?<body>.*?)EndProfile", RegexOptions.Singleline | RegexOptions.IgnoreCase))
            {
                var profile = new ParsedMockProfile
                {
                    ProfileName = match.Groups["name"].Value,
                };

                foreach (Match appMatch in Regex.Matches(match.Groups["body"].Value, "Executable\\s+\"(?<app>[^\"]+)\"", RegexOptions.IgnoreCase))
                {
                    profile.Applications.Add(new NVDRS_APPLICATION_V4
                    {
                        version = NVDRS_APPLICATION_VER_V4,
                        appName = appMatch.Groups["app"].Value,
                    });
                }

                result.Add(profile);
            }

            return result;
        }

        private sealed class MockSessionState
        {
            public MockSessionState(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; }
        }

        private sealed class MockProfileState
        {
            public MockProfileState(IntPtr handle, string profileName, bool isPredefined)
            {
                Handle = handle;
                ProfileName = profileName;
                IsPredefined = isPredefined;
                Settings = new Dictionary<uint, NVDRS_SETTING>();
                Applications = new List<NVDRS_APPLICATION_V4>();
            }

            public IntPtr Handle { get; }
            public string ProfileName { get; set; }
            public bool IsPredefined { get; set; }
            public Dictionary<uint, NVDRS_SETTING> Settings { get; set; }
            public List<NVDRS_APPLICATION_V4> Applications { get; set; }
            public uint NumSettings { get; private set; }
            public uint NumApps { get; private set; }

            public void RefreshCounts()
            {
                NumSettings = (uint)Settings.Count;
                NumApps = (uint)Applications.Count;
            }

            public NVDRS_PROFILE ToProfile()
            {
                RefreshCounts();
                return new NVDRS_PROFILE
                {
                    version = NVDRS_PROFILE_VER,
                    profileName = ProfileName,
                    gpuSupport = NVDRS_GPU_SUPPORT.Geforce,
                    isPredefined = IsPredefined ? 1u : 0u,
                    numOfApps = NumApps,
                    numOfSettings = NumSettings,
                };
            }
        }

        private sealed class MockSettingDefinition
        {
            public string Name { get; set; }
            public NVDRS_SETTING_TYPE SettingType { get; set; }
            public NVDRS_SETTING_UNION DefaultValue { get; set; }
            public List<NVDRS_SETTING_UNION> Values { get; set; }
        }

        private sealed class ParsedMockProfile
        {
            public string ProfileName { get; set; }
            public List<NVDRS_APPLICATION_V4> Applications { get; } = new List<NVDRS_APPLICATION_V4>();
        }
    }
}
