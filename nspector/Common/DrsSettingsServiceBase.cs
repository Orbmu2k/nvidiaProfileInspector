#region

using Enumerable=System.Linq.Enumerable;
using nvw=nspector.Native.NVAPI2.NvapiDrsWrapper;

#endregion

namespace nspector.Common;

abstract class DrsSettingsServiceBase
{
    public readonly float               DriverVersion;
    protected       DrsDecrypterService decrypter;

    protected DrsSettingsMetaService meta;

    public DrsSettingsServiceBase(DrsSettingsMetaService metaService,DrsDecrypterService decrpterService=null)
    {
        this.meta         =metaService;
        this.decrypter    =decrpterService;
        this.DriverVersion=this.GetDriverVersionInternal();
    }

    float GetDriverVersionInternal()
    {
        var  result       =0f;
        uint sysDrvVersion=0;
        var  sysDrvBranch =new System.Text.StringBuilder((int)nvw.NVAPI_SHORT_STRING_MAX);

        if(nvw.SYS_GetDriverAndBranchVersion(ref sysDrvVersion,sysDrvBranch)
            ==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            try
            {
                result=sysDrvVersion/100f;
            }
            catch {}
        }

        return result;
    }

    protected void DrsSession(System.Action<System.IntPtr> action,bool forceNonGlobalSession=false,
        bool                                               preventLoadSettings=false)
    {
        DrsSessionScope.DrsSession(hSession=>
        {
            action(hSession);
            return true;
        },forceNonGlobalSession,preventLoadSettings);
    }

    protected T DrsSession<T>(System.Func<System.IntPtr,T> action,bool forceDedicatedScope=false)
        =>DrsSessionScope.DrsSession(action,forceDedicatedScope);

    protected System.IntPtr GetProfileHandle(System.IntPtr hSession,string profileName)
    {
        var hProfile=System.IntPtr.Zero;

        if(string.IsNullOrEmpty(profileName))
        {
            var nvRes=nvw.DRS_GetCurrentGlobalProfile(hSession,ref hProfile);

            if(hProfile==System.IntPtr.Zero)
            {
                throw new NvapiException("DRS_GetCurrentGlobalProfile",
                    nspector.Native.NVAPI2.NvAPI_Status.NVAPI_PROFILE_NOT_FOUND);
            }

            if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
            {
                throw new NvapiException("DRS_GetCurrentGlobalProfile",nvRes);
            }
        }
        else
        {
            var nvRes=nvw.DRS_FindProfileByName(hSession,new System.Text.StringBuilder(profileName),ref hProfile);

            if(nvRes==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_PROFILE_NOT_FOUND)
            {
                return System.IntPtr.Zero;
            }

            if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
            {
                throw new NvapiException("DRS_FindProfileByName",nvRes);
            }
        }

        return hProfile;
    }

    protected System.IntPtr CreateProfile(System.IntPtr hSession,string profileName)
    {
        if(string.IsNullOrEmpty(profileName))
        {
            throw new System.ArgumentNullException("profileName");
        }

        var hProfile=System.IntPtr.Zero;

        var newProfile=new nspector.Native.NVAPI2.NVDRS_PROFILE
        {
            version=nvw.NVDRS_PROFILE_VER,profileName=profileName,
        };

        var nvRes=nvw.DRS_CreateProfile(hSession,ref newProfile,ref hProfile);
        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_CreateProfile",nvRes);
        }

        return hProfile;
    }


    protected nspector.Native.NVAPI2.NVDRS_PROFILE GetProfileInfo(System.IntPtr hSession,System.IntPtr hProfile)
    {
        var tmpProfile=new nspector.Native.NVAPI2.NVDRS_PROFILE();
        tmpProfile.version=nvw.NVDRS_PROFILE_VER;

        var gpRes=nvw.DRS_GetProfileInfo(hSession,hProfile,ref tmpProfile);
        if(gpRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_GetProfileInfo",gpRes);
        }

        return tmpProfile;
    }

    protected void StoreSetting(System.IntPtr hSession,System.IntPtr hProfile,
        nspector.Native.NVAPI2.NVDRS_SETTING  newSetting)
    {
        var ssRes=nvw.DRS_SetSetting(hSession,hProfile,ref newSetting);
        if(ssRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_SetSetting",ssRes);
        }
    }

    protected void StoreDwordValue(System.IntPtr hSession,System.IntPtr hProfile,uint settingId,uint dwordValue)
    {
        var newSetting=new nspector.Native.NVAPI2.NVDRS_SETTING
        {
            version        =nvw.NVDRS_SETTING_VER,settingId=settingId,
            settingType    =nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE,
            settingLocation=nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,currentValue
                =new nspector.Native.NVAPI2.NVDRS_SETTING_UNION
                {
                    dwordValue=dwordValue,
                },
        };

        var ssRes=nvw.DRS_SetSetting(hSession,hProfile,ref newSetting);
        if(ssRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_SetSetting",ssRes);
        }
    }

    protected void StoreStringValue(System.IntPtr hSession,System.IntPtr hProfile,uint settingId,string stringValue)
    {
        var newSetting=new nspector.Native.NVAPI2.NVDRS_SETTING
        {
            version        =nvw.NVDRS_SETTING_VER,settingId=settingId,
            settingType    =nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE,
            settingLocation=nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,currentValue
                =new nspector.Native.NVAPI2.NVDRS_SETTING_UNION
                {
                    stringValue=stringValue,
                },
        };

        var ssRes=nvw.DRS_SetSetting(hSession,hProfile,ref newSetting);
        if(ssRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_SetSetting",ssRes);
        }
    }

    protected void StoreBinaryValue(System.IntPtr hSession,System.IntPtr hProfile,uint settingId,byte[] binValue)
    {
        var newSetting=new nspector.Native.NVAPI2.NVDRS_SETTING
        {
            version        =nvw.NVDRS_SETTING_VER,settingId=settingId,
            settingType    =nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE,
            settingLocation=nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,currentValue
                =new nspector.Native.NVAPI2.NVDRS_SETTING_UNION
                {
                    binaryValue=binValue,
                },
        };

        var ssRes=nvw.DRS_SetSetting(hSession,hProfile,ref newSetting);
        if(ssRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_SetSetting",ssRes);
        }
    }

    protected nspector.Native.NVAPI2.NVDRS_SETTING? ReadSetting(System.IntPtr hSession,System.IntPtr hProfile,
        uint                                                                  settingId)
    {
        var newSetting=new nspector.Native.NVAPI2.NVDRS_SETTING
        {
            version=nvw.NVDRS_SETTING_VER,
        };

        var ssRes=nvw.DRS_GetSetting(hSession,hProfile,settingId,ref newSetting);
        if(ssRes==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_SETTING_NOT_FOUND)
        {
            return null;
        }

        if(ssRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_GetSetting",ssRes);
        }

        if(this.decrypter!=null)
        {
            var profile=this.GetProfileInfo(hSession,hProfile);
            this.decrypter.DecryptSettingIfNeeded(profile.profileName,ref newSetting);
        }

        return newSetting;
    }

    protected uint? ReadDwordValue(System.IntPtr hSession,System.IntPtr hProfile,uint settingId)
    {
        var newSetting=this.ReadSetting(hSession,hProfile,settingId);
        if(newSetting==null)
        {
            return null;
        }

        return newSetting.Value.currentValue.dwordValue;
    }

    protected void AddApplication(System.IntPtr hSession,System.IntPtr hProfile,string applicationName)
    {
        var newApp=new nspector.Native.NVAPI2.NVDRS_APPLICATION_V3
        {
            version=nvw.NVDRS_APPLICATION_VER_V3,appName=applicationName,
        };

        var caRes=nvw.DRS_CreateApplication(hSession,hProfile,ref newApp);
        if(caRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_CreateApplication",caRes);
        }
    }

    protected void DeleteApplication(System.IntPtr  hSession,System.IntPtr hProfile,
        nspector.Native.NVAPI2.NVDRS_APPLICATION_V3 application)
    {
        var caRes=nvw.DRS_DeleteApplicationEx(hSession,hProfile,ref application);
        if(caRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_DeleteApplication",caRes);
        }
    }

    protected System.Collections.Generic.List<System.IntPtr> EnumProfileHandles(System.IntPtr hSession)
    {
        var  profileHandles=new System.Collections.Generic.List<System.IntPtr>();
        var  hProfile      =System.IntPtr.Zero;
        uint index         =0;

        nspector.Native.NVAPI2.NvAPI_Status nvRes;

        do
        {
            nvRes=nvw.DRS_EnumProfiles(hSession,index,ref hProfile);
            if(nvRes==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
            {
                profileHandles.Add(hProfile);
            }

            index++;
        }
        while(nvRes==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK);

        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_END_ENUMERATION)
        {
            throw new NvapiException("DRS_EnumProfiles",nvRes);
        }

        return profileHandles;
    }

    protected System.Collections.Generic.List<nspector.Native.NVAPI2.NVDRS_SETTING> GetProfileSettings(
        System.IntPtr hSession,System.IntPtr hProfile)
    {
        uint settingCount=512;
        var  settings    =new nspector.Native.NVAPI2.NVDRS_SETTING[settingCount];
        settings[0].version=nvw.NVDRS_SETTING_VER;

        var esRes=nvw.DRS_EnumSettings(hSession,hProfile,0,ref settingCount,ref settings);

        if(esRes==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_END_ENUMERATION)
        {
            return new System.Collections.Generic.List<nspector.Native.NVAPI2.NVDRS_SETTING>();
        }

        if(esRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_EnumSettings",esRes);
        }

        if(this.decrypter!=null)
        {
            var profile=this.GetProfileInfo(hSession,hProfile);
            for(var i=0;i<settingCount;i++)
            {
                this.decrypter.DecryptSettingIfNeeded(profile.profileName,ref settings[i]);
            }
        }

        return Enumerable.ToList(settings);
    }

    protected System.Collections.Generic.List<nspector.Native.NVAPI2.NVDRS_APPLICATION_V3> GetProfileApplications(
        System.IntPtr hSession,System.IntPtr hProfile)
    {
        uint appCount=512;
        var  apps    =new nspector.Native.NVAPI2.NVDRS_APPLICATION_V3[512];
        apps[0].version=nvw.NVDRS_APPLICATION_VER_V3;

        var esRes=nvw.DRS_EnumApplications(hSession,hProfile,0,ref appCount,ref apps);

        if(esRes==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_END_ENUMERATION)
        {
            return new System.Collections.Generic.List<nspector.Native.NVAPI2.NVDRS_APPLICATION_V3>();
        }

        if(esRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_EnumApplications",esRes);
        }

        return Enumerable.ToList(apps);
    }

    protected void SaveSettings(System.IntPtr hSession)
    {
        var nvRes=nvw.DRS_SaveSettings(hSession);
        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_SaveSettings",nvRes);
        }
    }

    protected void LoadSettingsFileEx(System.IntPtr hSession,string filename)
    {
        var nvRes=nvw.DRS_LoadSettingsFromFileEx(hSession,new System.Text.StringBuilder(filename));
        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_LoadSettingsFromFileEx",nvRes);
        }
    }

    protected void SaveSettingsFileEx(System.IntPtr hSession,string filename)
    {
        var nvRes=nvw.DRS_SaveSettingsToFileEx(hSession,new System.Text.StringBuilder(filename));
        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_SaveSettingsToFileEx",nvRes);
        }
    }
}