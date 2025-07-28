using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace nspector.Native.NVAPI2
{

    public enum NvAPI_Status : int
    {
        NVAPI_OK = 0,
        NVAPI_ERROR = -1,
        NVAPI_LIBRARY_NOT_FOUND = -2,
        NVAPI_NO_IMPLEMENTATION = -3,
        NVAPI_API_NOT_INITIALIZED = -4,
        NVAPI_INVALID_ARGUMENT = -5,
        NVAPI_NVIDIA_DEVICE_NOT_FOUND = -6,
        NVAPI_END_ENUMERATION = -7,
        NVAPI_INVALID_HANDLE = -8,
        NVAPI_INCOMPATIBLE_STRUCT_VERSION = -9,
        NVAPI_HANDLE_INVALIDATED = -10,
        NVAPI_OPENGL_CONTEXT_NOT_CURRENT = -11,
        NVAPI_INVALID_POINTER = -14,
        NVAPI_NO_GL_EXPERT = -12,
        NVAPI_INSTRUMENTATION_DISABLED = -13,
        NVAPI_NO_GL_NSIGHT = -15,
        NVAPI_EXPECTED_LOGICAL_GPU_HANDLE = -100,
        NVAPI_EXPECTED_PHYSICAL_GPU_HANDLE = -101,
        NVAPI_EXPECTED_DISPLAY_HANDLE = -102,
        NVAPI_INVALID_COMBINATION = -103,
        NVAPI_NOT_SUPPORTED = -104,
        NVAPI_PORTID_NOT_FOUND = -105,
        NVAPI_EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106,
        NVAPI_INVALID_PERF_LEVEL = -107,
        NVAPI_DEVICE_BUSY = -108,
        NVAPI_NV_PERSIST_FILE_NOT_FOUND = -109,
        NVAPI_PERSIST_DATA_NOT_FOUND = -110,
        NVAPI_EXPECTED_TV_DISPLAY = -111,
        NVAPI_EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112,
        NVAPI_NO_ACTIVE_SLI_TOPOLOGY = -113,
        NVAPI_SLI_RENDERING_MODE_NOTALLOWED = -114,
        NVAPI_EXPECTED_DIGITAL_FLAT_PANEL = -115,
        NVAPI_ARGUMENT_EXCEED_MAX_SIZE = -116,
        NVAPI_DEVICE_SWITCHING_NOT_ALLOWED = -117,
        NVAPI_TESTING_CLOCKS_NOT_SUPPORTED = -118,
        NVAPI_UNKNOWN_UNDERSCAN_CONFIG = -119,
        NVAPI_TIMEOUT_RECONFIGURING_GPU_TOPO = -120,
        NVAPI_DATA_NOT_FOUND = -121,
        NVAPI_EXPECTED_ANALOG_DISPLAY = -122,
        NVAPI_NO_VIDLINK = -123,
        NVAPI_REQUIRES_REBOOT = -124,
        NVAPI_INVALID_HYBRID_MODE = -125,
        NVAPI_MIXED_TARGET_TYPES = -126,
        NVAPI_SYSWOW64_NOT_SUPPORTED = -127,
        NVAPI_IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128,
        NVAPI_REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129,
        NVAPI_OUT_OF_MEMORY = -130,
        NVAPI_WAS_STILL_DRAWING = -131,
        NVAPI_FILE_NOT_FOUND = -132,
        NVAPI_TOO_MANY_UNIQUE_STATE_OBJECTS = -133,
        NVAPI_INVALID_CALL = -134,
        NVAPI_D3D10_1_LIBRARY_NOT_FOUND = -135,
        NVAPI_FUNCTION_NOT_FOUND = -136,
        NVAPI_INVALID_USER_PRIVILEGE = -137,
        NVAPI_EXPECTED_NON_PRIMARY_DISPLAY_HANDLE = -138,
        NVAPI_EXPECTED_COMPUTE_GPU_HANDLE = -139,
        NVAPI_STEREO_NOT_INITIALIZED = -140,
        NVAPI_STEREO_REGISTRY_ACCESS_FAILED = -141,
        NVAPI_STEREO_REGISTRY_PROFILE_TYPE_NOT_SUPPORTED = -142,
        NVAPI_STEREO_REGISTRY_VALUE_NOT_SUPPORTED = -143,
        NVAPI_STEREO_NOT_ENABLED = -144,
        NVAPI_STEREO_NOT_TURNED_ON = -145,
        NVAPI_STEREO_INVALID_DEVICE_INTERFACE = -146,
        NVAPI_STEREO_PARAMETER_OUT_OF_RANGE = -147,
        NVAPI_STEREO_FRUSTUM_ADJUST_MODE_NOT_SUPPORTED = -148,
        NVAPI_TOPO_NOT_POSSIBLE = -149,
        NVAPI_MODE_CHANGE_FAILED = -150,
        NVAPI_D3D11_LIBRARY_NOT_FOUND = -151,
        NVAPI_INVALID_ADDRESS = -152,
        NVAPI_STRING_TOO_SMALL = -153,
        NVAPI_MATCHING_DEVICE_NOT_FOUND = -154,
        NVAPI_DRIVER_RUNNING = -155,
        NVAPI_DRIVER_NOTRUNNING = -156,
        NVAPI_ERROR_DRIVER_RELOAD_REQUIRED = -157,
        NVAPI_SET_NOT_ALLOWED = -158,
        NVAPI_ADVANCED_DISPLAY_TOPOLOGY_REQUIRED = -159,
        NVAPI_SETTING_NOT_FOUND = -160,
        NVAPI_SETTING_SIZE_TOO_LARGE = -161,
        NVAPI_TOO_MANY_SETTINGS_IN_PROFILE = -162,
        NVAPI_PROFILE_NOT_FOUND = -163,
        NVAPI_PROFILE_NAME_IN_USE = -164,
        NVAPI_PROFILE_NAME_EMPTY = -165,
        NVAPI_EXECUTABLE_NOT_FOUND = -166,
        NVAPI_EXECUTABLE_ALREADY_IN_USE = -167,
        NVAPI_DATATYPE_MISMATCH = -168,
        NVAPI_PROFILE_REMOVED = -169,
        NVAPI_UNREGISTERED_RESOURCE = -170,
        NVAPI_ID_OUT_OF_RANGE = -171,
        NVAPI_DISPLAYCONFIG_VALIDATION_FAILED = -172,
        NVAPI_DPMST_CHANGED = -173,
        NVAPI_INSUFFICIENT_BUFFER = -174,
        NVAPI_ACCESS_DENIED = -175,
        NVAPI_MOSAIC_NOT_ACTIVE = -176,
        NVAPI_SHARE_RESOURCE_RELOCATED = -177,
        NVAPI_REQUEST_USER_TO_DISABLE_DWM = -178,
        NVAPI_D3D_DEVICE_LOST = -179,
        NVAPI_INVALID_CONFIGURATION = -180,
        NVAPI_STEREO_HANDSHAKE_NOT_DONE = -181,
        NVAPI_EXECUTABLE_PATH_IS_AMBIGUOUS = -182,
        NVAPI_DEFAULT_STEREO_PROFILE_IS_NOT_DEFINED = -183,
        NVAPI_DEFAULT_STEREO_PROFILE_DOES_NOT_EXIST = -184,
        NVAPI_CLUSTER_ALREADY_EXISTS = -185,
        NVAPI_DPMST_DISPLAY_ID_EXPECTED = -186,
        NVAPI_INVALID_DISPLAY_ID = -187,
        NVAPI_STREAM_IS_OUT_OF_SYNC = -188,
        NVAPI_INCOMPATIBLE_AUDIO_DRIVER = -189,
        NVAPI_VALUE_ALREADY_SET = -190,
        NVAPI_TIMEOUT = -191,
        NVAPI_GPU_WORKSTATION_FEATURE_INCOMPLETE = -192,
        NVAPI_STEREO_INIT_ACTIVATION_NOT_DONE = -193,
        NVAPI_SYNC_NOT_ACTIVE = -194,
        NVAPI_SYNC_MASTER_NOT_FOUND = -195,
        NVAPI_INVALID_SYNC_TOPOLOGY = -196,
        NVAPI_ECID_SIGN_ALGO_UNSUPPORTED = -197,
        NVAPI_ECID_KEY_VERIFICATION_FAILED = -198,
    }

    internal enum NVDRS_SETTING_TYPE : int
    {
        NVDRS_DWORD_TYPE,
        NVDRS_BINARY_TYPE,
        NVDRS_STRING_TYPE,
        NVDRS_WSTRING_TYPE,
    }

    internal enum NVDRS_SETTING_LOCATION : int
    {
        NVDRS_CURRENT_PROFILE_LOCATION,
        NVDRS_GLOBAL_PROFILE_LOCATION,
        NVDRS_BASE_PROFILE_LOCATION,
        NVDRS_DEFAULT_PROFILE_LOCATION,
    }

    [Flags]
    public enum NVDRS_GPU_SUPPORT : uint
    {
        None,
        Geforce,
        Quadro,
        Nvs,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NVDRS_SETTING_VALUES
    {
        public uint version;
        public uint numSettingValues;
        public NVDRS_SETTING_TYPE settingType;
        public NVDRS_SETTING_UNION defaultValue;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)NvapiDrsWrapper.NVAPI_SETTING_MAX_VALUES)]
        public NVDRS_SETTING_UNION[] settingValues;
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 8)]
    //internal struct NVDRS_BINARY_SETTING
    //{
    //    public uint valueLength;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)NvapiDrsWrapper.NVAPI_BINARY_DATA_MAX)]
    //    public byte[] valueData;
    //}

    //[StructLayout(LayoutKind.Explicit, Pack = 8, CharSet = CharSet.Unicode)]
    //internal struct NVDRS_SETTING_UNION
    //{
    //    public uint dwordValue
    //    {
    //        get { return binaryValue.valueLength; }
    //        set { binaryValue.valueLength = value; }
    //    }

    //    [FieldOffsetAttribute(0)]
    //    public NVDRS_BINARY_SETTING binaryValue;

    //    [FieldOffsetAttribute(0)]
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    //    public string stringValue;
    //}

    //[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode, Size = 4100)]
    //internal struct NVDRS_SETTING_UNION
    //{
    //    public uint dwordValue;

    //    //massive hack for marshalling issues with unicode string overlapping
    //    public string stringValue
    //    {
    //        get
    //        {
    //            var firstPart = Encoding.Unicode.GetString(BitConverter.GetBytes(dwordValue)).Trim('\0');
    //            return firstPart + stringRaw;
    //        }

    //        set
    //        {
    //            var bytesRaw = Encoding.Unicode.GetBytes(value);
    //            var bytesFirst = new byte[4];
    //            var firstLength = bytesRaw.Length;
    //            if (firstLength > 4)
    //                firstLength = 4;
    //            Buffer.BlockCopy(bytesRaw, 0, bytesFirst, 0, firstLength);
    //            dwordValue = BitConverter.ToUInt32(bytesFirst, 0);

    //            if (value.Length > 2)
    //            {
    //                stringRaw = value.Substring(2);
    //            }
    //        }

    //    }

    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    //    private string stringRaw;

    //}

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode, Size = 4100)]
    internal struct NVDRS_SETTING_UNION
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4100)]
        public byte[] rawData;

        public byte[] binaryValue
        {
            get
            {
                var length = BitConverter.ToUInt32(rawData, 0);
                var tmpData = new byte[length];
                Buffer.BlockCopy(rawData, 4, tmpData, 0, (int)length);
                return tmpData;
            }

            set
            {
                rawData = new byte[4100];
                if (value != null)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(value.Length), 0, rawData, 0, 4);
                    Buffer.BlockCopy(value, 0, rawData, 4, value.Length);
                }
            }
        }

        public uint dwordValue
        {
            get
            {
                return BitConverter.ToUInt32(rawData, 0);
            }

            set
            {
                rawData = new byte[4100];
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, rawData, 0, 4);
            }
        }

        public string stringValue
        {
            get
            {
                return Encoding.Unicode.GetString(rawData).Split(new[] { '\0' }, 2)[0];
            }

            set
            {
                rawData = new byte[4100];
                var bytesRaw = Encoding.Unicode.GetBytes(value);
                Buffer.BlockCopy(bytesRaw, 0, rawData, 0, bytesRaw.Length);
            }
        }

        public string ansiStringValue
        {
            get
            {
                return Encoding.Default.GetString(rawData).Split(new[] { '\0' }, 2)[0];
            }

            set
            {
                rawData = new byte[4100];
                var bytesRaw = Encoding.Default.GetBytes(value);
                Buffer.BlockCopy(bytesRaw, 0, rawData, 0, bytesRaw.Length);
            }
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NVDRS_SETTING
    {
        public uint version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string settingName;
        public uint settingId;
        public NVDRS_SETTING_TYPE settingType;
        public NVDRS_SETTING_LOCATION settingLocation;
        public uint isCurrentPredefined;
        public uint isPredefinedValid;
        public NVDRS_SETTING_UNION predefinedValue;
        public NVDRS_SETTING_UNION currentValue;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NVDRS_APPLICATION_V1
    {
        public uint version;
        public uint isPredefined;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string appName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string userFriendlyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string launcher;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NVDRS_APPLICATION_V2
    {
        public uint version;
        public uint isPredefined;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string appName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string userFriendlyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string launcher;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string fileInFolder;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NVDRS_APPLICATION_V3
    {
        public bool isMetro { get { return (bitvector1 & 1) != 0; } set { if (value) bitvector1 |= 1; else bitvector1 &= ~1u; } }
        public bool isCommandLine { get { return (bitvector1 & 2) != 0; } set { if (value) bitvector1 |= 2; else bitvector1 &= ~2u; } }

        public uint version;
        public uint isPredefined;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string appName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string userFriendlyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string launcher;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string fileInFolder;
        private uint bitvector1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NVDRS_APPLICATION_V4
    {
        public bool isMetro { get { return (bitvector1 & 1) != 0; } set { if (value) bitvector1 |= 1; else bitvector1 &= ~1u; } }
        public bool isCommandLine { get { return (bitvector1 & 2) != 0; } set { if (value) bitvector1 |= 2; else bitvector1 &= ~2u; } }

        public uint version;
        public uint isPredefined;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string appName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string userFriendlyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string launcher;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string fileInFolder;
        private uint bitvector1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string commandLine;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct NVDRS_PROFILE
    {
        public uint version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
        public string profileName;
        public NVDRS_GPU_SUPPORT gpuSupport;
        public uint isPredefined;
        public uint numOfApps;
        public uint numOfSettings;
    }

    internal class NvapiDrsWrapper
    {

        #region CONSTANTS
        public const uint NVAPI_GENERIC_STRING_MAX = 4096;
        public const uint NVAPI_LONG_STRING_MAX = 256;
        public const uint NVAPI_SHORT_STRING_MAX = 64;
        public const uint NVAPI_MAX_PHYSICAL_GPUS = 64;
        public const uint NVAPI_UNICODE_STRING_MAX = 2048;
        public const uint NVAPI_BINARY_DATA_MAX = 4096;

        public const uint NVAPI_SETTING_MAX_VALUES = 100;
        public static uint NVDRS_SETTING_VALUES_VER = MAKE_NVAPI_VERSION<NVDRS_SETTING_VALUES>(1);
        public static uint NVDRS_SETTING_VER = MAKE_NVAPI_VERSION<NVDRS_SETTING>(1);
        public static uint NVDRS_APPLICATION_VER_V1 = MAKE_NVAPI_VERSION<NVDRS_APPLICATION_V1>(1);
        public static uint NVDRS_APPLICATION_VER_V2 = MAKE_NVAPI_VERSION<NVDRS_APPLICATION_V2>(2);
        public static uint NVDRS_APPLICATION_VER_V3 = MAKE_NVAPI_VERSION<NVDRS_APPLICATION_V3>(3);
        public static uint NVDRS_APPLICATION_VER_V4 = MAKE_NVAPI_VERSION<NVDRS_APPLICATION_V4>(4);
        public static uint NVDRS_APPLICATION_VER = NVDRS_APPLICATION_VER_V4;
        public static uint NVDRS_PROFILE_VER = MAKE_NVAPI_VERSION<NVDRS_PROFILE>(1);

        public const uint OGL_IMPLICIT_GPU_AFFINITY_NUM_VALUES = 1;
        public const uint CUDA_EXCLUDED_GPUS_NUM_VALUES = 1;
        public const string D3DOGL_GPU_MAX_POWER_DEFAULTPOWER = "0";
        public const uint D3DOGL_GPU_MAX_POWER_NUM_VALUES = 1;
        public const string D3DOGL_GPU_MAX_POWER_DEFAULT = D3DOGL_GPU_MAX_POWER_DEFAULTPOWER;
        #endregion


        private NvapiDrsWrapper() { }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(String dllname);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, String procname);

        private static uint MAKE_NVAPI_VERSION<T>(int version)
        {
            return (UInt32)((Marshal.SizeOf(typeof(T))) | (int)(version << 16));
        }

        private static string GetDllName()
        {
            if (IntPtr.Size == 4)
            {
                return "nvapi.dll";
            }
            else
            {
                return "nvapi64.dll";
            }
        }

        private static void GetDelegate<T>(uint id, out T newDelegate, uint? fallbackId = null) where T : class
        {
            IntPtr ptr = nvapi_QueryInterface(id);
            if (ptr != IntPtr.Zero)
            {
                newDelegate = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
            }
            else if (fallbackId.HasValue)
            {
                GetDelegate(fallbackId.Value, out newDelegate);
            }
            else
            {
                newDelegate = null;
            }
        }

        private static T GetDelegateOfFunction<T>(IntPtr pLib, string signature)
        {
            T FuncT = default(T);
            IntPtr FuncAddr = GetProcAddress(pLib, signature);
            if (FuncAddr != IntPtr.Zero)
                FuncT = (T)(object)Marshal.GetDelegateForFunctionPointer(FuncAddr, typeof(T));
            return FuncT;
        }

        #region DELEGATES

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr nvapi_QueryInterfaceDelegate(uint id);
        private static readonly nvapi_QueryInterfaceDelegate nvapi_QueryInterface;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status NvAPI_InitializeDelegate();
        public static readonly NvAPI_InitializeDelegate NvAPI_Initialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status InitializeDelegate();
        public static readonly InitializeDelegate Initialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status UnloadDelegate();
        public static readonly UnloadDelegate Unload;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status GetErrorMessageDelegate(NvAPI_Status nr, [MarshalAs(UnmanagedType.LPStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_SHORT_STRING_MAX)]StringBuilder szDesc);
        public static readonly GetErrorMessageDelegate GetErrorMessage;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status GetInterfaceVersionStringDelegate([MarshalAs(UnmanagedType.LPStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_SHORT_STRING_MAX)]StringBuilder szDesc);
        public static readonly GetInterfaceVersionStringDelegate GetInterfaceVersionString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status SYS_GetDriverAndBranchVersionDelegate(ref uint pDriverVersion, [MarshalAs(UnmanagedType.LPStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_SHORT_STRING_MAX)]StringBuilder szBuildBranchString);
        public static readonly SYS_GetDriverAndBranchVersionDelegate SYS_GetDriverAndBranchVersion;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_CreateSessionDelegate(ref IntPtr phSession);
        public static readonly DRS_CreateSessionDelegate DRS_CreateSession;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_DestroySessionDelegate(IntPtr hSession);
        public static readonly DRS_DestroySessionDelegate DRS_DestroySession;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_LoadSettingsDelegate(IntPtr hSession);
        public static readonly DRS_LoadSettingsDelegate DRS_LoadSettings;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_SaveSettingsDelegate(IntPtr hSession);
        public static readonly DRS_SaveSettingsDelegate DRS_SaveSettings;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_LoadSettingsFromFileDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder fileName);
        public static readonly DRS_LoadSettingsFromFileDelegate DRS_LoadSettingsFromFile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_SaveSettingsToFileDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder fileName);
        public static readonly DRS_SaveSettingsToFileDelegate DRS_SaveSettingsToFile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_LoadSettingsFromFileExDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder fileName);
        public static readonly DRS_LoadSettingsFromFileExDelegate DRS_LoadSettingsFromFileEx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_SaveSettingsToFileExDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder fileName);
        public static readonly DRS_SaveSettingsToFileExDelegate DRS_SaveSettingsToFileEx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_CreateProfileDelegate(IntPtr hSession, ref NVDRS_PROFILE pProfileInfo, ref IntPtr phProfile);
        public static readonly DRS_CreateProfileDelegate DRS_CreateProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_DeleteProfileDelegate(IntPtr hSession, IntPtr hProfile);
        public static readonly DRS_DeleteProfileDelegate DRS_DeleteProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_SetCurrentGlobalProfileDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder wszGlobalProfileName);
        public static readonly DRS_SetCurrentGlobalProfileDelegate DRS_SetCurrentGlobalProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetCurrentGlobalProfileDelegate(IntPtr hSession, ref IntPtr phProfile);
        public static readonly DRS_GetCurrentGlobalProfileDelegate DRS_GetCurrentGlobalProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetProfileInfoDelegate(IntPtr hSession, IntPtr hProfile, ref NVDRS_PROFILE pProfileInfo);
        public static readonly DRS_GetProfileInfoDelegate DRS_GetProfileInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_SetProfileInfoDelegate(IntPtr hSession, IntPtr hProfile, ref NVDRS_PROFILE pProfileInfo);
        public static readonly DRS_SetProfileInfoDelegate DRS_SetProfileInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_FindProfileByNameDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder profileName, ref IntPtr phProfile);
        public static readonly DRS_FindProfileByNameDelegate DRS_FindProfileByName;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_EnumProfilesDelegate(IntPtr hSession, uint index, ref IntPtr phProfile);
        public static readonly DRS_EnumProfilesDelegate DRS_EnumProfiles;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetNumProfilesDelegate(IntPtr hSession, ref uint numProfiles);
        public static readonly DRS_GetNumProfilesDelegate DRS_GetNumProfiles;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_CreateApplicationDelegate(IntPtr hSession, IntPtr hProfile, ref NVDRS_APPLICATION_V4 pApplication);
        public static readonly DRS_CreateApplicationDelegate DRS_CreateApplication;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_DeleteApplicationExDelegate(IntPtr hSession, IntPtr hProfile, ref NVDRS_APPLICATION_V4 pApp);
        public static readonly DRS_DeleteApplicationExDelegate DRS_DeleteApplicationEx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_DeleteApplicationDelegate(IntPtr hSession, IntPtr hProfile, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder appName);
        public static readonly DRS_DeleteApplicationDelegate DRS_DeleteApplication;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetApplicationInfoDelegate(IntPtr hSession, IntPtr hProfile, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder appName, ref NVDRS_APPLICATION_V4 pApplication);
        public static readonly DRS_GetApplicationInfoDelegate DRS_GetApplicationInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvAPI_Status DRS_EnumApplicationsDelegate(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint appCount, IntPtr pApplication);
        private static readonly DRS_EnumApplicationsDelegate DRS_EnumApplicationsInternal;
        public static NvAPI_Status DRS_EnumApplications<TDrsAppVersion>(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint appCount, ref TDrsAppVersion[] apps)
        {
            NvAPI_Status res;

            IntPtr pSettings;
            NativeArrayHelper.SetArrayData(apps, out pSettings);
            try
            {
                res = DRS_EnumApplicationsInternal(hSession, hProfile, startIndex, ref appCount, pSettings);
                apps = NativeArrayHelper.GetArrayData<TDrsAppVersion>(pSettings, (int)appCount);
            }
            finally
            {
                Marshal.FreeHGlobal(pSettings);
            }
            return res;
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_FindApplicationByNameDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder appName, ref IntPtr phProfile, ref NVDRS_APPLICATION_V4 pApplication);
        public static readonly DRS_FindApplicationByNameDelegate DRS_FindApplicationByName;

        public static NvAPI_Status DRS_SetSetting(IntPtr hSession, IntPtr hProfile, ref NVDRS_SETTING pSetting)
        {
            return _DRS_SetSetting(hSession, hProfile, ref pSetting, 0, 0);
        }

        public static NvAPI_Status DRS_GetSetting(IntPtr hSession, IntPtr hProfile, uint settingId, ref NVDRS_SETTING pSetting)
        {
            uint x = 0;
            return _DRS_GetSetting(hSession, hProfile, settingId, ref pSetting, ref x);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_SetSettingDelegate(IntPtr hSession, IntPtr hProfile, ref NVDRS_SETTING pSetting, uint x, uint y);
        private static readonly DRS_SetSettingDelegate _DRS_SetSetting;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId, ref NVDRS_SETTING pSetting, ref uint x);
        private static readonly DRS_GetSettingDelegate _DRS_GetSetting;
        

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvAPI_Status DRS_EnumSettingsDelegate(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint settingsCount, IntPtr pSetting);
        private static readonly DRS_EnumSettingsDelegate DRS_EnumSettingsInternal;
        public static NvAPI_Status DRS_EnumSettings(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint settingsCount, ref NVDRS_SETTING[] settings)
        {
            NvAPI_Status res;

            IntPtr pSettings;
            NativeArrayHelper.SetArrayData(settings, out pSettings);
            try
            {
                res = DRS_EnumSettingsInternal(hSession, hProfile, startIndex, ref settingsCount, pSettings);
                settings = NativeArrayHelper.GetArrayData<NVDRS_SETTING>(pSettings, (int)settingsCount);
            }
            finally
            {
                Marshal.FreeHGlobal(pSettings);
            }
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_EnumAvailableSettingIdsDelegate(IntPtr pSettingIds, ref uint pMaxCount);
        public static readonly DRS_EnumAvailableSettingIdsDelegate DRS_EnumAvailableSettingIdsInternal;
        public static NvAPI_Status DRS_EnumAvailableSettingIds(out List<uint> settingIds, uint maxCount)
        {
            NvAPI_Status res;
            var settingIdArray = new uint[maxCount];
            var pSettingIds = IntPtr.Zero;
            NativeArrayHelper.SetArrayData(settingIdArray, out pSettingIds);
            try
            {
                res = DRS_EnumAvailableSettingIdsInternal(pSettingIds, ref maxCount);

                settingIdArray = NativeArrayHelper.GetArrayData<uint>(pSettingIds, (int)maxCount);
                settingIds = settingIdArray.ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(pSettingIds);
            }
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvAPI_Status DRS_EnumAvailableSettingValuesDelegate(uint settingId, ref uint pMaxNumValues, IntPtr pSettingValues);
        private static readonly DRS_EnumAvailableSettingValuesDelegate DRS_EnumAvailableSettingValuesInternal;
        public static NvAPI_Status DRS_EnumAvailableSettingValues(uint settingId, ref uint pMaxNumValues, ref NVDRS_SETTING_VALUES settingValues)
        {
            var pSettingValues = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NVDRS_SETTING_VALUES)));
            NvAPI_Status res;
            try
            {
                settingValues.settingValues = new NVDRS_SETTING_UNION[(int)NVAPI_SETTING_MAX_VALUES];
                Marshal.StructureToPtr(settingValues, pSettingValues, true);
                res = DRS_EnumAvailableSettingValuesInternal(settingId, ref pMaxNumValues, pSettingValues);
                settingValues = (NVDRS_SETTING_VALUES)Marshal.PtrToStructure(pSettingValues, typeof(NVDRS_SETTING_VALUES));
            }
            finally
            {
                Marshal.FreeHGlobal(pSettingValues);
            }
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetSettingIdFromNameDelegate([MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder settingName, ref uint pSettingId);
        public static readonly DRS_GetSettingIdFromNameDelegate DRS_GetSettingIdFromName;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetSettingNameFromIdDelegate(uint settingId, [MarshalAs(UnmanagedType.LPWStr, SizeConst = (int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]StringBuilder pSettingName);
        public static readonly DRS_GetSettingNameFromIdDelegate DRS_GetSettingNameFromId;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_DeleteProfileSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId);
        public static readonly DRS_DeleteProfileSettingDelegate DRS_DeleteProfileSetting;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_RestoreAllDefaultsDelegate(IntPtr hSession);
        public static readonly DRS_RestoreAllDefaultsDelegate DRS_RestoreAllDefaults;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_RestoreProfileDefaultDelegate(IntPtr hSession, IntPtr hProfile);
        public static readonly DRS_RestoreProfileDefaultDelegate DRS_RestoreProfileDefault;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_RestoreProfileDefaultSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId);
        public static readonly DRS_RestoreProfileDefaultSettingDelegate DRS_RestoreProfileDefaultSetting;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvAPI_Status DRS_GetBaseProfileDelegate(IntPtr hSession, ref IntPtr phProfile);
        public static readonly DRS_GetBaseProfileDelegate DRS_GetBaseProfile;

        #endregion
        static NvapiDrsWrapper()
        {
            IntPtr lib = LoadLibrary(GetDllName());
            if (lib != IntPtr.Zero)
            {
                nvapi_QueryInterface = GetDelegateOfFunction<nvapi_QueryInterfaceDelegate>(lib, "nvapi_QueryInterface");
                if (nvapi_QueryInterface != null)
                {
                    GetDelegate(0x0150E828, out NvAPI_Initialize);
                    if (NvAPI_Initialize() == NvAPI_Status.NVAPI_OK)
                    {
                        #region FUNCTION IDs
                        GetDelegate(0x0150E828, out Initialize);
                        GetDelegate(0xD22BDD7E, out Unload);
                        GetDelegate(0x6C2D048C, out GetErrorMessage);
                        GetDelegate(0x01053FA5, out GetInterfaceVersionString);
                        GetDelegate(0x2926AAAD, out SYS_GetDriverAndBranchVersion);
                        GetDelegate(0x0694D52E, out DRS_CreateSession);
                        GetDelegate(0xDAD9CFF8, out DRS_DestroySession);
                        GetDelegate(0x375DBD6B, out DRS_LoadSettings);
                        GetDelegate(0xFCBC7E14, out DRS_SaveSettings);
                        GetDelegate(0xD3EDE889, out DRS_LoadSettingsFromFile);
                        GetDelegate(0x2BE25DF8, out DRS_SaveSettingsToFile);
                        GetDelegate(0xC63C045B, out DRS_LoadSettingsFromFileEx);
                        GetDelegate(0x1267818E, out DRS_SaveSettingsToFileEx);
                        GetDelegate(0xCC176068, out DRS_CreateProfile);
                        GetDelegate(0x17093206, out DRS_DeleteProfile);
                        GetDelegate(0x1C89C5DF, out DRS_SetCurrentGlobalProfile);
                        GetDelegate(0x617BFF9F, out DRS_GetCurrentGlobalProfile);
                        GetDelegate(0x61CD6FD6, out DRS_GetProfileInfo);
                        GetDelegate(0x16ABD3A9, out DRS_SetProfileInfo);
                        GetDelegate(0x7E4A9A0B, out DRS_FindProfileByName);
                        GetDelegate(0xBC371EE0, out DRS_EnumProfiles);
                        GetDelegate(0x1DAE4FBC, out DRS_GetNumProfiles);
                        GetDelegate(0x4347A9DE, out DRS_CreateApplication);
                        GetDelegate(0xC5EA85A1, out DRS_DeleteApplicationEx);
                        GetDelegate(0x2C694BC6, out DRS_DeleteApplication);
                        GetDelegate(0xED1F8C69, out DRS_GetApplicationInfo);
                        GetDelegate(0x7FA2173A, out DRS_EnumApplicationsInternal);
                        GetDelegate(0xEEE566B2, out DRS_FindApplicationByName);
                        GetDelegate(0x8A2CF5F5, out _DRS_SetSetting, 0x577DD202);
                        GetDelegate(0xEA99498D, out _DRS_GetSetting, 0x73BF8338);
                        GetDelegate(0xCFD6983E, out DRS_EnumSettingsInternal, 0xAE3039DA);
                        GetDelegate(0xE5DE48E5, out DRS_EnumAvailableSettingIdsInternal, 0xF020614A);
                        GetDelegate(0x2EC39F90, out DRS_EnumAvailableSettingValuesInternal);
                        GetDelegate(0xCB7309CD, out DRS_GetSettingIdFromName);
                        GetDelegate(0x1EB13791, out DRS_GetSettingNameFromId, 0xD61CBE6E);
                        GetDelegate(0xD20D29DF, out DRS_DeleteProfileSetting, 0xE4A26362);
                        GetDelegate(0x5927B094, out DRS_RestoreAllDefaults);
                        GetDelegate(0xFA5F6134, out DRS_RestoreProfileDefault);
                        GetDelegate(0x7DD5B261, out DRS_RestoreProfileDefaultSetting, 0x53F0381E);
                        GetDelegate(0xDA8466A0, out DRS_GetBaseProfile);
                        #endregion
                    }
                }
            }
        }
    }

}
