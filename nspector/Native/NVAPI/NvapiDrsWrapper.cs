#region

using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector.Native.NVAPI2;

public enum NvAPI_Status
{
    NVAPI_OK=0,NVAPI_ERROR=-1,NVAPI_LIBRARY_NOT_FOUND=-2,
    NVAPI_NO_IMPLEMENTATION=-3,NVAPI_API_NOT_INITIALIZED=-4,NVAPI_INVALID_ARGUMENT=-5,
    NVAPI_NVIDIA_DEVICE_NOT_FOUND=-6,NVAPI_END_ENUMERATION=-7,NVAPI_INVALID_HANDLE=-8,
    NVAPI_INCOMPATIBLE_STRUCT_VERSION=-9,NVAPI_HANDLE_INVALIDATED=-10,NVAPI_OPENGL_CONTEXT_NOT_CURRENT=-11,
    NVAPI_INVALID_POINTER=-14,NVAPI_NO_GL_EXPERT=-12,NVAPI_INSTRUMENTATION_DISABLED=-13,
    NVAPI_NO_GL_NSIGHT=-15,NVAPI_EXPECTED_LOGICAL_GPU_HANDLE=-100,NVAPI_EXPECTED_PHYSICAL_GPU_HANDLE=-101,
    NVAPI_EXPECTED_DISPLAY_HANDLE=-102,NVAPI_INVALID_COMBINATION=-103,NVAPI_NOT_SUPPORTED=-104,
    NVAPI_PORTID_NOT_FOUND=-105,NVAPI_EXPECTED_UNATTACHED_DISPLAY_HANDLE=-106,NVAPI_INVALID_PERF_LEVEL=-107,
    NVAPI_DEVICE_BUSY=-108,NVAPI_NV_PERSIST_FILE_NOT_FOUND=-109,NVAPI_PERSIST_DATA_NOT_FOUND=-110,
    NVAPI_EXPECTED_TV_DISPLAY=-111,NVAPI_EXPECTED_TV_DISPLAY_ON_DCONNECTOR=-112,NVAPI_NO_ACTIVE_SLI_TOPOLOGY=-113,
    NVAPI_SLI_RENDERING_MODE_NOTALLOWED=-114,NVAPI_EXPECTED_DIGITAL_FLAT_PANEL=-115,NVAPI_ARGUMENT_EXCEED_MAX_SIZE=-116,
    NVAPI_DEVICE_SWITCHING_NOT_ALLOWED=-117,NVAPI_TESTING_CLOCKS_NOT_SUPPORTED=-118,NVAPI_UNKNOWN_UNDERSCAN_CONFIG=-119,
    NVAPI_TIMEOUT_RECONFIGURING_GPU_TOPO=-120,NVAPI_DATA_NOT_FOUND=-121,NVAPI_EXPECTED_ANALOG_DISPLAY=-122,
    NVAPI_NO_VIDLINK=-123,NVAPI_REQUIRES_REBOOT=-124,NVAPI_INVALID_HYBRID_MODE=-125,
    NVAPI_MIXED_TARGET_TYPES=-126,NVAPI_SYSWOW64_NOT_SUPPORTED=-127,
    NVAPI_IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED=-128,NVAPI_REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS=-129,
    NVAPI_OUT_OF_MEMORY=-130,NVAPI_WAS_STILL_DRAWING=-131,NVAPI_FILE_NOT_FOUND=-132,
    NVAPI_TOO_MANY_UNIQUE_STATE_OBJECTS=-133,NVAPI_INVALID_CALL=-134,NVAPI_D3D10_1_LIBRARY_NOT_FOUND=-135,
    NVAPI_FUNCTION_NOT_FOUND=-136,NVAPI_INVALID_USER_PRIVILEGE=-137,NVAPI_EXPECTED_NON_PRIMARY_DISPLAY_HANDLE=-138,
    NVAPI_EXPECTED_COMPUTE_GPU_HANDLE=-139,NVAPI_STEREO_NOT_INITIALIZED=-140,NVAPI_STEREO_REGISTRY_ACCESS_FAILED=-141,
    NVAPI_STEREO_REGISTRY_PROFILE_TYPE_NOT_SUPPORTED=-142,NVAPI_STEREO_REGISTRY_VALUE_NOT_SUPPORTED=-143,
    NVAPI_STEREO_NOT_ENABLED=-144,NVAPI_STEREO_NOT_TURNED_ON=-145,NVAPI_STEREO_INVALID_DEVICE_INTERFACE=-146,
    NVAPI_STEREO_PARAMETER_OUT_OF_RANGE=-147,NVAPI_STEREO_FRUSTUM_ADJUST_MODE_NOT_SUPPORTED=-148,
    NVAPI_TOPO_NOT_POSSIBLE=-149,NVAPI_MODE_CHANGE_FAILED=-150,NVAPI_D3D11_LIBRARY_NOT_FOUND=-151,
    NVAPI_INVALID_ADDRESS=-152,NVAPI_STRING_TOO_SMALL=-153,NVAPI_MATCHING_DEVICE_NOT_FOUND=-154,
    NVAPI_DRIVER_RUNNING=-155,NVAPI_DRIVER_NOTRUNNING=-156,NVAPI_ERROR_DRIVER_RELOAD_REQUIRED=-157,
    NVAPI_SET_NOT_ALLOWED=-158,NVAPI_ADVANCED_DISPLAY_TOPOLOGY_REQUIRED=-159,NVAPI_SETTING_NOT_FOUND=-160,
    NVAPI_SETTING_SIZE_TOO_LARGE=-161,NVAPI_TOO_MANY_SETTINGS_IN_PROFILE=-162,NVAPI_PROFILE_NOT_FOUND=-163,
    NVAPI_PROFILE_NAME_IN_USE=-164,NVAPI_PROFILE_NAME_EMPTY=-165,NVAPI_EXECUTABLE_NOT_FOUND=-166,
    NVAPI_EXECUTABLE_ALREADY_IN_USE=-167,NVAPI_DATATYPE_MISMATCH=-168,NVAPI_PROFILE_REMOVED=-169,
    NVAPI_UNREGISTERED_RESOURCE=-170,NVAPI_ID_OUT_OF_RANGE=-171,NVAPI_DISPLAYCONFIG_VALIDATION_FAILED=-172,
    NVAPI_DPMST_CHANGED=-173,NVAPI_INSUFFICIENT_BUFFER=-174,NVAPI_ACCESS_DENIED=-175,
    NVAPI_MOSAIC_NOT_ACTIVE=-176,NVAPI_SHARE_RESOURCE_RELOCATED=-177,NVAPI_REQUEST_USER_TO_DISABLE_DWM=-178,
    NVAPI_D3D_DEVICE_LOST=-179,NVAPI_INVALID_CONFIGURATION=-180,NVAPI_STEREO_HANDSHAKE_NOT_DONE=-181,
    NVAPI_EXECUTABLE_PATH_IS_AMBIGUOUS=-182,NVAPI_DEFAULT_STEREO_PROFILE_IS_NOT_DEFINED=-183,
    NVAPI_DEFAULT_STEREO_PROFILE_DOES_NOT_EXIST=-184,NVAPI_CLUSTER_ALREADY_EXISTS=-185,
    NVAPI_DPMST_DISPLAY_ID_EXPECTED=-186,NVAPI_INVALID_DISPLAY_ID=-187,NVAPI_STREAM_IS_OUT_OF_SYNC=-188,
    NVAPI_INCOMPATIBLE_AUDIO_DRIVER=-189,NVAPI_VALUE_ALREADY_SET=-190,NVAPI_TIMEOUT=-191,
    NVAPI_GPU_WORKSTATION_FEATURE_INCOMPLETE=-192,NVAPI_STEREO_INIT_ACTIVATION_NOT_DONE=-193,NVAPI_SYNC_NOT_ACTIVE=-194,
    NVAPI_SYNC_MASTER_NOT_FOUND=-195,NVAPI_INVALID_SYNC_TOPOLOGY=-196,NVAPI_ECID_SIGN_ALGO_UNSUPPORTED=-197,
    NVAPI_ECID_KEY_VERIFICATION_FAILED=-198,
}

enum NVDRS_SETTING_TYPE
{
    NVDRS_DWORD_TYPE,NVDRS_BINARY_TYPE,NVDRS_STRING_TYPE,
    NVDRS_WSTRING_TYPE,
}

enum NVDRS_SETTING_LOCATION
{
    NVDRS_CURRENT_PROFILE_LOCATION,NVDRS_GLOBAL_PROFILE_LOCATION,NVDRS_BASE_PROFILE_LOCATION,
    NVDRS_DEFAULT_PROFILE_LOCATION,
}

[System.FlagsAttribute]
public enum NVDRS_GPU_SUPPORT:uint
{
    None,Geforce,Quadro,
    Nvs,
}

[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=8)]
struct NVDRS_SETTING_VALUES
{
    public uint                version;
    public uint                numSettingValues;
    public NVDRS_SETTING_TYPE  settingType;
    public NVDRS_SETTING_UNION defaultValue;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_SETTING_MAX_VALUES)]
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
[System.Runtime.InteropServices.StructLayoutAttribute(     System.Runtime.InteropServices.LayoutKind.Sequential,Pack=8,
    CharSet=System.Runtime.InteropServices.CharSet.Unicode,Size=4100)]
struct NVDRS_SETTING_UNION
{
    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray,
        SizeConst=4100)]
    public byte[] rawData;

    public byte[] binaryValue
    {
        get
        {
            var length =System.BitConverter.ToUInt32(this.rawData,0);
            var tmpData=new byte[length];
            System.Buffer.BlockCopy(this.rawData,4,tmpData,0,(int)length);
            return tmpData;
        }

        set
        {
            this.rawData=new byte[4100];
            if(value!=null)
            {
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(value.Length),0,this.rawData,0,4);
                System.Buffer.BlockCopy(value,                                     0,this.rawData,4,value.Length);
            }
        }
    }

    public uint dwordValue
    {
        get
        {
            return System.BitConverter.ToUInt32(this.rawData,0);
        }

        set
        {
            this.rawData=new byte[4100];
            System.Buffer.BlockCopy(System.BitConverter.GetBytes(value),0,this.rawData,0,4);
        }
    }

    public string stringValue
    {
        get
        {
            return System.Text.Encoding.Unicode.GetString(this.rawData).Split(new[]
            {
                '\0',
            },2)[0];
        }

        set
        {
            this.rawData=new byte[4100];
            var bytesRaw=System.Text.Encoding.Unicode.GetBytes(value);
            System.Buffer.BlockCopy(bytesRaw,0,this.rawData,0,bytesRaw.Length);
        }
    }

    public string ansiStringValue
    {
        get
        {
            return System.Text.Encoding.Default.GetString(this.rawData).Split(new[]
            {
                '\0',
            },2)[0];
        }

        set
        {
            this.rawData=new byte[4100];
            var bytesRaw=System.Text.Encoding.Default.GetBytes(value);
            System.Buffer.BlockCopy(bytesRaw,0,this.rawData,0,bytesRaw.Length);
        }
    }
}

[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=8,
    CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
struct NVDRS_SETTING
{
    public uint version;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string settingName;

    public uint                   settingId;
    public NVDRS_SETTING_TYPE     settingType;
    public NVDRS_SETTING_LOCATION settingLocation;
    public uint                   isCurrentPredefined;
    public uint                   isPredefinedValid;
    public NVDRS_SETTING_UNION    predefinedValue;
    public NVDRS_SETTING_UNION    currentValue;
}

[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=8,
    CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
struct NVDRS_APPLICATION_V1
{
    public uint version;
    public uint isPredefined;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string appName;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string userFriendlyName;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string launcher;
}

[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=8,
    CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
struct NVDRS_APPLICATION_V2
{
    public uint version;
    public uint isPredefined;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string appName;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string userFriendlyName;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string launcher;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string fileInFolder;
}

[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=8,
    CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
struct NVDRS_APPLICATION_V3
{
    public uint isMetro
    {
        get
        {
            return this.bitvector1&1;
        }
        set
        {
            this.bitvector1=value|this.bitvector1;
        }
    }

    public uint version;
    public uint isPredefined;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string appName;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string userFriendlyName;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string launcher;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string fileInFolder;

    uint bitvector1;
}

[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=8,
    CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
struct NVDRS_PROFILE
{
    public uint version;

    [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr,
        SizeConst=(int)NvapiDrsWrapper.NVAPI_UNICODE_STRING_MAX)]
    public string profileName;

    public NVDRS_GPU_SUPPORT gpuSupport;
    public uint              isPredefined;
    public uint              numOfApps;
    public uint              numOfSettings;
}

class NvapiDrsWrapper
{
    static NvapiDrsWrapper()
    {
        var lib=NvapiDrsWrapper.LoadLibrary(NvapiDrsWrapper.GetDllName());
        if(lib!=System.IntPtr.Zero)
        {
            NvapiDrsWrapper.nvapi_QueryInterface
                =NvapiDrsWrapper.GetDelegateOfFunction<nvapi_QueryInterfaceDelegate>(lib,"nvapi_QueryInterface");
            if(NvapiDrsWrapper.nvapi_QueryInterface!=null)
            {
                NvapiDrsWrapper.GetDelegate(0x0150E828,out NvapiDrsWrapper.NvAPI_Initialize);
                if(NvapiDrsWrapper.NvAPI_Initialize()==NvAPI_Status.NVAPI_OK)
                {
                #region FUNCTION IDs

                    NvapiDrsWrapper.GetDelegate(0x0150E828,out NvapiDrsWrapper.Initialize);
                    NvapiDrsWrapper.GetDelegate(0xD22BDD7E,out NvapiDrsWrapper.Unload);
                    NvapiDrsWrapper.GetDelegate(0x6C2D048C,out NvapiDrsWrapper.GetErrorMessage);
                    NvapiDrsWrapper.GetDelegate(0x01053FA5,out NvapiDrsWrapper.GetInterfaceVersionString);
                    NvapiDrsWrapper.GetDelegate(0x2926AAAD,out NvapiDrsWrapper.SYS_GetDriverAndBranchVersion);
                    NvapiDrsWrapper.GetDelegate(0x0694D52E,out NvapiDrsWrapper.DRS_CreateSession);
                    NvapiDrsWrapper.GetDelegate(0xDAD9CFF8,out NvapiDrsWrapper.DRS_DestroySession);
                    NvapiDrsWrapper.GetDelegate(0x375DBD6B,out NvapiDrsWrapper.DRS_LoadSettings);
                    NvapiDrsWrapper.GetDelegate(0xFCBC7E14,out NvapiDrsWrapper.DRS_SaveSettings);
                    NvapiDrsWrapper.GetDelegate(0xD3EDE889,out NvapiDrsWrapper.DRS_LoadSettingsFromFile);
                    NvapiDrsWrapper.GetDelegate(0x2BE25DF8,out NvapiDrsWrapper.DRS_SaveSettingsToFile);
                    NvapiDrsWrapper.GetDelegate(0xC63C045B,out NvapiDrsWrapper.DRS_LoadSettingsFromFileEx);
                    NvapiDrsWrapper.GetDelegate(0x1267818E,out NvapiDrsWrapper.DRS_SaveSettingsToFileEx);
                    NvapiDrsWrapper.GetDelegate(0xCC176068,out NvapiDrsWrapper.DRS_CreateProfile);
                    NvapiDrsWrapper.GetDelegate(0x17093206,out NvapiDrsWrapper.DRS_DeleteProfile);
                    NvapiDrsWrapper.GetDelegate(0x1C89C5DF,out NvapiDrsWrapper.DRS_SetCurrentGlobalProfile);
                    NvapiDrsWrapper.GetDelegate(0x617BFF9F,out NvapiDrsWrapper.DRS_GetCurrentGlobalProfile);
                    NvapiDrsWrapper.GetDelegate(0x61CD6FD6,out NvapiDrsWrapper.DRS_GetProfileInfo);
                    NvapiDrsWrapper.GetDelegate(0x16ABD3A9,out NvapiDrsWrapper.DRS_SetProfileInfo);
                    NvapiDrsWrapper.GetDelegate(0x7E4A9A0B,out NvapiDrsWrapper.DRS_FindProfileByName);
                    NvapiDrsWrapper.GetDelegate(0xBC371EE0,out NvapiDrsWrapper.DRS_EnumProfiles);
                    NvapiDrsWrapper.GetDelegate(0x1DAE4FBC,out NvapiDrsWrapper.DRS_GetNumProfiles);
                    NvapiDrsWrapper.GetDelegate(0x4347A9DE,out NvapiDrsWrapper.DRS_CreateApplication);
                    NvapiDrsWrapper.GetDelegate(0xC5EA85A1,out NvapiDrsWrapper.DRS_DeleteApplicationEx);
                    NvapiDrsWrapper.GetDelegate(0x2C694BC6,out NvapiDrsWrapper.DRS_DeleteApplication);
                    NvapiDrsWrapper.GetDelegate(0xED1F8C69,out NvapiDrsWrapper.DRS_GetApplicationInfo);
                    NvapiDrsWrapper.GetDelegate(0x7FA2173A,out NvapiDrsWrapper.DRS_EnumApplicationsInternal);
                    NvapiDrsWrapper.GetDelegate(0xEEE566B2,out NvapiDrsWrapper.DRS_FindApplicationByName);
                    NvapiDrsWrapper.GetDelegate(0x577DD202,out NvapiDrsWrapper.DRS_SetSetting);
                    NvapiDrsWrapper.GetDelegate(0x73BF8338,out NvapiDrsWrapper.DRS_GetSetting);
                    NvapiDrsWrapper.GetDelegate(0xAE3039DA,out NvapiDrsWrapper.DRS_EnumSettingsInternal);
                    NvapiDrsWrapper.GetDelegate(0xF020614A,out NvapiDrsWrapper.DRS_EnumAvailableSettingIdsInternal);
                    NvapiDrsWrapper.GetDelegate(0x2EC39F90,out NvapiDrsWrapper.DRS_EnumAvailableSettingValuesInternal);
                    NvapiDrsWrapper.GetDelegate(0xCB7309CD,out NvapiDrsWrapper.DRS_GetSettingIdFromName);
                    NvapiDrsWrapper.GetDelegate(0xD61CBE6E,out NvapiDrsWrapper.DRS_GetSettingNameFromId);
                    NvapiDrsWrapper.GetDelegate(0xE4A26362,out NvapiDrsWrapper.DRS_DeleteProfileSetting);
                    NvapiDrsWrapper.GetDelegate(0x5927B094,out NvapiDrsWrapper.DRS_RestoreAllDefaults);
                    NvapiDrsWrapper.GetDelegate(0xFA5F6134,out NvapiDrsWrapper.DRS_RestoreProfileDefault);
                    NvapiDrsWrapper.GetDelegate(0x53F0381E,out NvapiDrsWrapper.DRS_RestoreProfileDefaultSetting);
                    NvapiDrsWrapper.GetDelegate(0xDA8466A0,out NvapiDrsWrapper.DRS_GetBaseProfile);

                #endregion
                }
            }
        }
    }


    NvapiDrsWrapper() {}

    [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll")]
    static extern System.IntPtr LoadLibrary(string dllname);

    [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll")]
    static extern System.IntPtr GetProcAddress(System.IntPtr hModule,string procname);

    static uint MAKE_NVAPI_VERSION<T>(int version)
        =>(uint)(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))|version<<16);

    static string GetDllName()
    {
        if(System.IntPtr.Size==4)
        {
            return"nvapi.dll";
        }

        return"nvapi64.dll";
    }

    static void GetDelegate<T>(uint id,out T newDelegate) where T:class
    {
        var ptr=NvapiDrsWrapper.nvapi_QueryInterface(id);
        if(ptr!=System.IntPtr.Zero)
        {
            newDelegate=System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(ptr,typeof(T)) as T;
        }
        else
        {
            newDelegate=null;
        }
    }

    static T GetDelegateOfFunction<T>(System.IntPtr pLib,string signature)
    {
        var FuncT   =default(T);
        var FuncAddr=NvapiDrsWrapper.GetProcAddress(pLib,signature);
        if(FuncAddr!=System.IntPtr.Zero)
        {
            FuncT=(T)(object)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(FuncAddr,typeof(T));
        }

        return FuncT;
    }

#region CONSTANTS

    public const uint NVAPI_GENERIC_STRING_MAX=4096;
    public const uint NVAPI_LONG_STRING_MAX   =256;
    public const uint NVAPI_SHORT_STRING_MAX  =64;
    public const uint NVAPI_MAX_PHYSICAL_GPUS =64;
    public const uint NVAPI_UNICODE_STRING_MAX=2048;
    public const uint NVAPI_BINARY_DATA_MAX   =4096;

    public const  uint NVAPI_SETTING_MAX_VALUES=100;
    public static uint NVDRS_SETTING_VALUES_VER=NvapiDrsWrapper.MAKE_NVAPI_VERSION<NVDRS_SETTING_VALUES>(1);
    public static uint NVDRS_SETTING_VER       =NvapiDrsWrapper.MAKE_NVAPI_VERSION<NVDRS_SETTING>(1);
    public static uint NVDRS_APPLICATION_VER_V1=NvapiDrsWrapper.MAKE_NVAPI_VERSION<NVDRS_APPLICATION_V1>(1);
    public static uint NVDRS_APPLICATION_VER_V2=NvapiDrsWrapper.MAKE_NVAPI_VERSION<NVDRS_APPLICATION_V2>(2);
    public static uint NVDRS_APPLICATION_VER_V3=NvapiDrsWrapper.MAKE_NVAPI_VERSION<NVDRS_APPLICATION_V3>(3);
    public static uint NVDRS_APPLICATION_VER   =NvapiDrsWrapper.NVDRS_APPLICATION_VER_V3;
    public static uint NVDRS_PROFILE_VER       =NvapiDrsWrapper.MAKE_NVAPI_VERSION<NVDRS_PROFILE>(1);

    public const uint   OGL_IMPLICIT_GPU_AFFINITY_NUM_VALUES=1;
    public const uint   CUDA_EXCLUDED_GPUS_NUM_VALUES       =1;
    public const string D3DOGL_GPU_MAX_POWER_DEFAULTPOWER   ="0";
    public const uint   D3DOGL_GPU_MAX_POWER_NUM_VALUES     =1;
    public const string D3DOGL_GPU_MAX_POWER_DEFAULT        =NvapiDrsWrapper.D3DOGL_GPU_MAX_POWER_DEFAULTPOWER;

#endregion

#region DELEGATES

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    delegate System.IntPtr nvapi_QueryInterfaceDelegate(uint id);

    static readonly nvapi_QueryInterfaceDelegate nvapi_QueryInterface;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status NvAPI_InitializeDelegate();

    public static readonly NvAPI_InitializeDelegate NvAPI_Initialize;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status InitializeDelegate();

    public static readonly InitializeDelegate Initialize;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status UnloadDelegate();

    public static readonly UnloadDelegate Unload;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status GetErrorMessageDelegate(NvAPI_Status nr,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr,
            SizeConst=(int)NVAPI_SHORT_STRING_MAX)]
        System.Text.StringBuilder szDesc);

    public static readonly GetErrorMessageDelegate GetErrorMessage;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status GetInterfaceVersionStringDelegate(
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr,
            SizeConst=(int)NVAPI_SHORT_STRING_MAX)]
        System.Text.StringBuilder szDesc);

    public static readonly GetInterfaceVersionStringDelegate GetInterfaceVersionString;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status SYS_GetDriverAndBranchVersionDelegate(ref uint pDriverVersion,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr,
            SizeConst=(int)NVAPI_SHORT_STRING_MAX)]
        System.Text.StringBuilder szBuildBranchString);

    public static readonly SYS_GetDriverAndBranchVersionDelegate SYS_GetDriverAndBranchVersion;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_CreateSessionDelegate(ref System.IntPtr phSession);

    public static readonly DRS_CreateSessionDelegate DRS_CreateSession;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_DestroySessionDelegate(System.IntPtr hSession);

    public static readonly DRS_DestroySessionDelegate DRS_DestroySession;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_LoadSettingsDelegate(System.IntPtr hSession);

    public static readonly DRS_LoadSettingsDelegate DRS_LoadSettings;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_SaveSettingsDelegate(System.IntPtr hSession);

    public static readonly DRS_SaveSettingsDelegate DRS_SaveSettings;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_LoadSettingsFromFileDelegate(System.IntPtr hSession,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder fileName);

    public static readonly DRS_LoadSettingsFromFileDelegate DRS_LoadSettingsFromFile;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_SaveSettingsToFileDelegate(System.IntPtr hSession,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder fileName);

    public static readonly DRS_SaveSettingsToFileDelegate DRS_SaveSettingsToFile;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_LoadSettingsFromFileExDelegate(System.IntPtr hSession,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder fileName);

    public static readonly DRS_LoadSettingsFromFileExDelegate DRS_LoadSettingsFromFileEx;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_SaveSettingsToFileExDelegate(System.IntPtr hSession,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder fileName);

    public static readonly DRS_SaveSettingsToFileExDelegate DRS_SaveSettingsToFileEx;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_CreateProfileDelegate(System.IntPtr hSession,ref NVDRS_PROFILE pProfileInfo,
        ref System.IntPtr                                                phProfile);

    public static readonly DRS_CreateProfileDelegate DRS_CreateProfile;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_DeleteProfileDelegate(System.IntPtr hSession,System.IntPtr hProfile);

    public static readonly DRS_DeleteProfileDelegate DRS_DeleteProfile;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_SetCurrentGlobalProfileDelegate(System.IntPtr hSession,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder wszGlobalProfileName);

    public static readonly DRS_SetCurrentGlobalProfileDelegate DRS_SetCurrentGlobalProfile;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status
        DRS_GetCurrentGlobalProfileDelegate(System.IntPtr hSession,ref System.IntPtr phProfile);

    public static readonly DRS_GetCurrentGlobalProfileDelegate DRS_GetCurrentGlobalProfile;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_GetProfileInfoDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        ref NVDRS_PROFILE                                                 pProfileInfo);

    public static readonly DRS_GetProfileInfoDelegate DRS_GetProfileInfo;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_SetProfileInfoDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        ref NVDRS_PROFILE                                                 pProfileInfo);

    public static readonly DRS_SetProfileInfoDelegate DRS_SetProfileInfo;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_FindProfileByNameDelegate(System.IntPtr hSession,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder profileName,
        ref System.IntPtr phProfile);

    public static readonly DRS_FindProfileByNameDelegate DRS_FindProfileByName;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status
        DRS_EnumProfilesDelegate(System.IntPtr hSession,uint index,ref System.IntPtr phProfile);

    public static readonly DRS_EnumProfilesDelegate DRS_EnumProfiles;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_GetNumProfilesDelegate(System.IntPtr hSession,ref uint numProfiles);

    public static readonly DRS_GetNumProfilesDelegate DRS_GetNumProfiles;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_CreateApplicationDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        ref NVDRS_APPLICATION_V3                                             pApplication);

    public static readonly DRS_CreateApplicationDelegate DRS_CreateApplication;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_DeleteApplicationExDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        ref NVDRS_APPLICATION_V3                                               pApp);

    public static readonly DRS_DeleteApplicationExDelegate DRS_DeleteApplicationEx;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_DeleteApplicationDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder appName);

    public static readonly DRS_DeleteApplicationDelegate DRS_DeleteApplication;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_GetApplicationInfoDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder appName,
        ref NVDRS_APPLICATION_V3 pApplication);

    public static readonly DRS_GetApplicationInfoDelegate DRS_GetApplicationInfo;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    delegate NvAPI_Status DRS_EnumApplicationsDelegate(System.IntPtr hSession,System.IntPtr hProfile,uint startIndex,
        ref uint                                                     appCount,System.IntPtr pApplication);

    static readonly DRS_EnumApplicationsDelegate DRS_EnumApplicationsInternal;

    public static NvAPI_Status DRS_EnumApplications<TDrsAppVersion>(System.IntPtr hSession,System.IntPtr hProfile,
        uint                                                                      startIndex,
        ref uint                                                                  appCount,ref TDrsAppVersion[] apps)
    {
        NvAPI_Status res;

        System.IntPtr pSettings;
        NativeArrayHelper.SetArrayData(apps,out pSettings);
        try
        {
            res =NvapiDrsWrapper.DRS_EnumApplicationsInternal(hSession,hProfile,startIndex,ref appCount,pSettings);
            apps=NativeArrayHelper.GetArrayData<TDrsAppVersion>(pSettings,(int)appCount);
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pSettings);
        }

        return res;
    }


    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_FindApplicationByNameDelegate(System.IntPtr hSession,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder appName,
        ref System.IntPtr phProfile,ref NVDRS_APPLICATION_V3 pApplication);

    public static readonly DRS_FindApplicationByNameDelegate DRS_FindApplicationByName;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_SetSettingDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        ref NVDRS_SETTING                                             pSetting);

    public static readonly DRS_SetSettingDelegate DRS_SetSetting;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_GetSettingDelegate(System.IntPtr hSession,System.IntPtr hProfile,uint settingId,
        ref NVDRS_SETTING                                             pSetting);

    public static readonly DRS_GetSettingDelegate DRS_GetSetting;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    delegate NvAPI_Status DRS_EnumSettingsDelegate(System.IntPtr hSession,     System.IntPtr hProfile,uint startIndex,
        ref uint                                                 settingsCount,System.IntPtr pSetting);

    static readonly DRS_EnumSettingsDelegate DRS_EnumSettingsInternal;

    public static NvAPI_Status DRS_EnumSettings(System.IntPtr hSession,     System.IntPtr hProfile,uint startIndex,
        ref uint                                              settingsCount,ref NVDRS_SETTING[] settings)
    {
        NvAPI_Status res;

        System.IntPtr pSettings;
        NativeArrayHelper.SetArrayData(settings,out pSettings);
        try
        {
            res     =NvapiDrsWrapper.DRS_EnumSettingsInternal(hSession,hProfile,startIndex,ref settingsCount,pSettings);
            settings=NativeArrayHelper.GetArrayData<NVDRS_SETTING>(pSettings,(int)settingsCount);
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pSettings);
        }

        return res;
    }

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_EnumAvailableSettingIdsDelegate(System.IntPtr pSettingIds,ref uint pMaxCount);

    public static readonly DRS_EnumAvailableSettingIdsDelegate DRS_EnumAvailableSettingIdsInternal;

    public static NvAPI_Status DRS_EnumAvailableSettingIds(out System.Collections.Generic.List<uint> settingIds,
        uint                                                                                         maxCount)
    {
        NvAPI_Status res;
        var          settingIdArray=new uint[maxCount];
        var          pSettingIds   =System.IntPtr.Zero;
        NativeArrayHelper.SetArrayData(settingIdArray,out pSettingIds);
        try
        {
            res=NvapiDrsWrapper.DRS_EnumAvailableSettingIdsInternal(pSettingIds,ref maxCount);

            settingIdArray=NativeArrayHelper.GetArrayData<uint>(pSettingIds,(int)maxCount);
            settingIds    =Enumerable.ToList(settingIdArray);
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pSettingIds);
        }

        return res;
    }

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    delegate NvAPI_Status DRS_EnumAvailableSettingValuesDelegate(uint settingId,ref uint pMaxNumValues,
        System.IntPtr                                                 pSettingValues);

    static readonly DRS_EnumAvailableSettingValuesDelegate DRS_EnumAvailableSettingValuesInternal;

    public static NvAPI_Status DRS_EnumAvailableSettingValues(uint settingId,ref uint pMaxNumValues,
        ref NVDRS_SETTING_VALUES                                   settingValues)
    {
        var pSettingValues
            =System.Runtime.InteropServices.Marshal.AllocHGlobal(
                System.Runtime.InteropServices.Marshal.SizeOf(typeof(NVDRS_SETTING_VALUES)));
        NvAPI_Status res;
        try
        {
            settingValues.settingValues=new NVDRS_SETTING_UNION[(int)NvapiDrsWrapper.NVAPI_SETTING_MAX_VALUES];
            System.Runtime.InteropServices.Marshal.StructureToPtr(settingValues,pSettingValues,true);
            res=NvapiDrsWrapper.DRS_EnumAvailableSettingValuesInternal(settingId,ref pMaxNumValues,pSettingValues);
            settingValues
                =(NVDRS_SETTING_VALUES)System.Runtime.InteropServices.Marshal.PtrToStructure(pSettingValues,
                    typeof(NVDRS_SETTING_VALUES));
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pSettingValues);
        }

        return res;
    }

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_GetSettingIdFromNameDelegate(
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder settingName,
        ref uint pSettingId);

    public static readonly DRS_GetSettingIdFromNameDelegate DRS_GetSettingIdFromName;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_GetSettingNameFromIdDelegate(uint settingId,
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr,
            SizeConst=(int)NVAPI_UNICODE_STRING_MAX)]
        System.Text.StringBuilder pSettingName);

    public static readonly DRS_GetSettingNameFromIdDelegate DRS_GetSettingNameFromId;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_DeleteProfileSettingDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        uint                                                                    settingId);

    public static readonly DRS_DeleteProfileSettingDelegate DRS_DeleteProfileSetting;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_RestoreAllDefaultsDelegate(System.IntPtr hSession);

    public static readonly DRS_RestoreAllDefaultsDelegate DRS_RestoreAllDefaults;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_RestoreProfileDefaultDelegate(System.IntPtr hSession,System.IntPtr hProfile);

    public static readonly DRS_RestoreProfileDefaultDelegate DRS_RestoreProfileDefault;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_RestoreProfileDefaultSettingDelegate(System.IntPtr hSession,System.IntPtr hProfile,
        uint                                                                            settingId);

    public static readonly DRS_RestoreProfileDefaultSettingDelegate DRS_RestoreProfileDefaultSetting;

    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention
        .Cdecl)]
    public delegate NvAPI_Status DRS_GetBaseProfileDelegate(System.IntPtr hSession,ref System.IntPtr phProfile);

    public static readonly DRS_GetBaseProfileDelegate DRS_GetBaseProfile;

#endregion
}