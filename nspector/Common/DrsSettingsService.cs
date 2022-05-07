#region

using Enumerable=System.Linq.Enumerable;
using nvw=nspector.Native.NVAPI2.NvapiDrsWrapper;

#endregion

namespace nspector.Common;

class DrsSettingsService:DrsSettingsServiceBase
{
    readonly System.Collections.Generic.List<uint> _baseProfileSettingIds;

    public DrsSettingsService(DrsSettingsMetaService metaService,DrsDecrypterService decrpterService)
        :base(metaService,decrpterService)=>this._baseProfileSettingIds=this.InitBaseProfileSettingIds();

    System.Collections.Generic.List<uint> InitBaseProfileSettingIds()
    {
        return this.DrsSession(hSession=>
        {
            var hBaseProfile       =this.GetProfileHandle(hSession,"");
            var baseProfileSettings=this.GetProfileSettings(hSession,hBaseProfile);

            return Enumerable.ToList(Enumerable.Select(baseProfileSettings,x=>x.settingId));
        });
    }

    string GetDrsProgramPath()
    {
        var nvidiaInstallerFolder=System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
            @"NVIDIA Corporation\Installer2");
        var driverFolders=System.IO.Directory.EnumerateDirectories(nvidiaInstallerFolder,"Display.Driver.*");
        foreach(var folder in driverFolders)
        {
            var fiDbInstaller=new System.IO.FileInfo(System.IO.Path.Combine(folder,"dbInstaller.exe"));
            if(!fiDbInstaller.Exists)
            {
                continue;
            }

            var fviDbInstaller=System.Diagnostics.FileVersionInfo.GetVersionInfo(fiDbInstaller.FullName);

            var fileversion=fviDbInstaller.FileVersion.Replace(".","");
            var driverver  =this.DriverVersion.ToString().Replace(",","").Replace(".","");

            if(fileversion.EndsWith(driverver))
            {
                return fiDbInstaller.DirectoryName;
            }
        }

        return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
            @"NVIDIA Corporation\Drs");
    }

    void RunDrsInitProcess()
    {
        var drsPath=this.GetDrsProgramPath();

        var si=new System.Diagnostics.ProcessStartInfo();
        si.UseShellExecute =true;
        si.WorkingDirectory=drsPath;
        si.Arguments       ="-init";
        si.FileName        =System.IO.Path.Combine(drsPath,"dbInstaller.exe");
        if(!nspector.Common.Helper.AdminHelper.IsAdmin)
        {
            si.Verb="runas";
        }

        var p=System.Diagnostics.Process.Start(si);
        p.WaitForExit();
    }

    public void DeleteAllProfilesHard()
    {
        var tmpFile=nspector.Common.Helper.TempFile.GetTempFileName();
        try
        {
            System.IO.File.WriteAllText(tmpFile,
                "BaseProfile \"Base Profile\"\r\nSelectedGlobalProfile \"Base Profile\"\r\nProfile \"Base Profile\"\r\nShowOn All\r\nProfileType Global\r\nEndProfile\r\n");

            this.DrsSession(hSession=>
            {
                this.LoadSettingsFileEx(hSession,tmpFile);
                this.SaveSettings(hSession);
            },true,true);
        }
        finally
        {
            if(System.IO.File.Exists(tmpFile))
            {
                System.IO.File.Delete(tmpFile);
            }
        }
    }

    public void DeleteProfileHard(string profileName)
    {
        var tmpFileName=nspector.Common.Helper.TempFile.GetTempFileName();

        try
        {
            var tmpFileContent="";

            this.DrsSession(hSession=>
            {
                this.SaveSettingsFileEx(hSession,tmpFileName);
                tmpFileContent=System.IO.File.ReadAllText(tmpFileName);
                var pattern="(?<rpl>\nProfile\\s\""+System.Text.RegularExpressions.Regex.Escape(profileName)
                    +"\".*?EndProfile.*?\n)";
                tmpFileContent=System.Text.RegularExpressions.Regex.Replace(tmpFileContent,pattern,"",
                    System.Text.RegularExpressions.RegexOptions.Singleline);
                System.IO.File.WriteAllText(tmpFileName,tmpFileContent);
            });

            if(tmpFileContent!="")
            {
                this.DrsSession(hSession=>
                {
                    this.LoadSettingsFileEx(hSession,tmpFileName);
                    this.SaveSettings(hSession);
                });
            }
        }
        finally
        {
            if(System.IO.File.Exists(tmpFileName))
            {
                System.IO.File.Delete(tmpFileName);
            }
        }
    }

    public void DeleteProfile(string profileName)
    {
        this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);
            if(hProfile!=System.IntPtr.Zero)
            {
                var nvRes=nvw.DRS_DeleteProfile(hSession,hProfile);
                if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
                {
                    throw new NvapiException("DRS_DeleteProfile",nvRes);
                }

                this.SaveSettings(hSession);
            }
        });
    }

    public System.Collections.Generic.List<string> GetProfileNames(ref string baseProfileName)
    {
        var lstResult         =new System.Collections.Generic.List<string>();
        var tmpBaseProfileName=baseProfileName;

        this.DrsSession(hSession=>
        {
            var hBase      =this.GetProfileHandle(hSession,null);
            var baseProfile=this.GetProfileInfo(hSession,hBase);
            tmpBaseProfileName=baseProfile.profileName;

            lstResult.Add("_GLOBAL_DRIVER_PROFILE ("+tmpBaseProfileName+")");

            var profileHandles=this.EnumProfileHandles(hSession);
            foreach(var hProfile in profileHandles)
            {
                var profile=this.GetProfileInfo(hSession,hProfile);

                if(profile.isPredefined==0||profile.numOfApps>0)
                {
                    lstResult.Add(profile.profileName);
                }
            }
        });

        baseProfileName=tmpBaseProfileName;
        return lstResult;
    }

    public void CreateProfile(string profileName,string applicationName=null)
    {
        this.DrsSession(hSession=>
        {
            var hProfile=this.CreateProfile(hSession,profileName);

            if(applicationName!=null)
            {
                this.AddApplication(hSession,hProfile,applicationName);
            }

            this.SaveSettings(hSession);
        });
    }

    public void ResetAllProfilesInternal()
    {
        this.RunDrsInitProcess();

        this.DrsSession(hSession=>
        {
            var nvRes=nvw.DRS_RestoreAllDefaults(hSession);
            if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
            {
                throw new NvapiException("DRS_RestoreAllDefaults",nvRes);
            }

            this.SaveSettings(hSession);
        });
    }

    public void ResetProfile(string profileName,out bool removeFromModified)
    {
        var tmpRemoveFromModified=false;
        this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);
            var profile =this.GetProfileInfo(hSession,hProfile);

            if(profile.isPredefined==1)
            {
                var nvRes=nvw.DRS_RestoreProfileDefault(hSession,hProfile);
                if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
                {
                    throw new NvapiException("DRS_RestoreProfileDefault",nvRes);
                }

                this.SaveSettings(hSession);
                tmpRemoveFromModified=true;
            }
            else if(profile.numOfSettings>0)
            {
                var dropCount=0;
                var settings =this.GetProfileSettings(hSession,hProfile);

                foreach(var setting in settings)
                {
                    if(setting.settingLocation
                        ==nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                    {
                        if(nvw.DRS_DeleteProfileSetting(hSession,hProfile,setting.settingId)==
                            nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
                        {
                            dropCount++;
                        }
                    }
                }

                if(dropCount>0)
                {
                    this.SaveSettings(hSession);
                }
            }
        });

        removeFromModified=tmpRemoveFromModified;
    }

    public void ResetValue(string profileName,uint settingId,out bool removeFromModified)
    {
        var tmpRemoveFromModified=false;

        this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);

            if(hProfile!=System.IntPtr.Zero)
            {
                var nvRes=nvw.DRS_RestoreProfileDefaultSetting(hSession,hProfile,settingId);
                if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
                {
                    throw new NvapiException("DRS_RestoreProfileDefaultSetting",nvRes);
                }

                this.SaveSettings(hSession);

                var modifyCount=0;
                var settings   =this.GetProfileSettings(hSession,hProfile);

                foreach(var setting in settings)
                {
                    if(setting.isCurrentPredefined==0&&setting.settingLocation==
                        nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                    {
                        modifyCount++;
                    }
                }

                tmpRemoveFromModified=modifyCount==0;
            }
        });

        removeFromModified=tmpRemoveFromModified;
    }

    public uint GetDwordValueFromProfile(string profileName,uint settingId,bool returnDefaultValue=false,
        bool                                    forceDedicatedScope=false)
    {
        return this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);

            var dwordValue=this.ReadDwordValue(hSession,hProfile,settingId);

            if(dwordValue!=null)
            {
                return dwordValue.Value;
            }

            if(returnDefaultValue)
            {
                return this.meta.GetSettingMeta(settingId).DefaultDwordValue;
            }

            throw new NvapiException("DRS_GetSetting",nspector.Native.NVAPI2.NvAPI_Status.NVAPI_SETTING_NOT_FOUND);
        });
    }

    public void SetDwordValueToProfile(string profileName,uint settingId,uint dwordValue)
    {
        this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);
            this.StoreDwordValue(hSession,hProfile,settingId,dwordValue);
            this.SaveSettings(hSession);
        });
    }

    public int StoreSettingsToProfile(string                                                  profileName,
        System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<uint,string>> settings)
    {
        this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);

            foreach(var setting in settings)
            {
                var settingMeta=this.meta.GetSettingMeta(setting.Key);
                var settingType=settingMeta.SettingType;

                if(settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
                {
                    var dword=DrsUtil.ParseDwordSettingValue(settingMeta,setting.Value);
                    this.StoreDwordValue(hSession,hProfile,setting.Key,dword);
                }
                else if(settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                {
                    var str=DrsUtil.ParseStringSettingValue(settingMeta,setting.Value);
                    this.StoreStringValue(hSession,hProfile,setting.Key,str);
                }
                else if(settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
                {
                    var bin=DrsUtil.ParseBinarySettingValue(settingMeta,setting.Value);
                    this.StoreBinaryValue(hSession,hProfile,setting.Key,bin);
                }
            }

            this.SaveSettings(hSession);
        });

        return 0;
    }


    SettingItem CreateSettingItem(nspector.Native.NVAPI2.NVDRS_SETTING setting,bool useDefault=false)
    {
        var settingMeta=this.meta.GetSettingMeta(setting.settingId);
        //settingMeta.SettingType = setting.settingType;

        if(settingMeta.DwordValues==null)
        {
            settingMeta.DwordValues=new System.Collections.Generic.List<nspector.Common.Meta.SettingValue<uint>>();
        }


        if(settingMeta.StringValues==null)
        {
            settingMeta.StringValues=new System.Collections.Generic.List<nspector.Common.Meta.SettingValue<string>>();
        }

        if(settingMeta.BinaryValues==null)
        {
            settingMeta.BinaryValues=new System.Collections.Generic.List<nspector.Common.Meta.SettingValue<byte[]>>();
        }


        var settingState=SettingState.NotAssiged;
        var valueRaw    ="";
        var valueText   ="";

        if(settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
        {
            if(useDefault)
            {
                valueRaw =DrsUtil.GetDwordString(settingMeta.DefaultDwordValue);
                valueText=DrsUtil.GetDwordSettingValueName(settingMeta,settingMeta.DefaultDwordValue);
            }
            else if(setting.isCurrentPredefined==1&&setting.isPredefinedValid==1)
            {
                valueRaw =DrsUtil.GetDwordString(setting.predefinedValue.dwordValue);
                valueText=DrsUtil.GetDwordSettingValueName(settingMeta,setting.predefinedValue.dwordValue);

                if(setting.settingLocation
                    ==nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                {
                    settingState=SettingState.NvidiaSetting;
                }
                else
                {
                    settingState=SettingState.GlobalSetting;
                }
            }
            else
            {
                valueRaw =DrsUtil.GetDwordString(setting.currentValue.dwordValue);
                valueText=DrsUtil.GetDwordSettingValueName(settingMeta,setting.currentValue.dwordValue);

                if(setting.settingLocation
                    ==nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                {
                    settingState=SettingState.UserdefinedSetting;
                }
                else
                {
                    settingState=SettingState.GlobalSetting;
                }
            }
        }

        if(settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
        {
            if(useDefault)
            {
                valueRaw =settingMeta.DefaultStringValue;
                valueText=DrsUtil.GetStringSettingValueName(settingMeta,settingMeta.DefaultStringValue);
            }
            else if(setting.isCurrentPredefined==1&&setting.isPredefinedValid==1)
            {
                valueRaw    =setting.predefinedValue.stringValue;
                valueText   =DrsUtil.GetStringSettingValueName(settingMeta,setting.predefinedValue.stringValue);
                settingState=SettingState.NvidiaSetting;
            }
            else
            {
                valueRaw =setting.currentValue.stringValue;
                valueText=DrsUtil.GetStringSettingValueName(settingMeta,setting.currentValue.stringValue);

                if(setting.settingLocation
                    ==nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                {
                    settingState=SettingState.UserdefinedSetting;
                }
                else
                {
                    settingState=SettingState.GlobalSetting;
                }
            }
        }

        if(settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
        {
            if(useDefault)
            {
                valueRaw =DrsUtil.GetBinaryString(settingMeta.DefaultBinaryValue);
                valueText=DrsUtil.GetBinarySettingValueName(settingMeta,settingMeta.DefaultBinaryValue);
            }
            else if(setting.isCurrentPredefined==1&&setting.isPredefinedValid==1)
            {
                valueRaw    =DrsUtil.GetBinaryString(setting.predefinedValue.binaryValue);
                valueText   =DrsUtil.GetBinarySettingValueName(settingMeta,setting.predefinedValue.binaryValue);
                settingState=SettingState.NvidiaSetting;
            }
            else
            {
                valueRaw =DrsUtil.GetBinaryString(setting.currentValue.binaryValue);
                valueText=DrsUtil.GetBinarySettingValueName(settingMeta,setting.currentValue.binaryValue);

                if(setting.settingLocation
                    ==nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
                {
                    settingState=SettingState.UserdefinedSetting;
                }
                else
                {
                    settingState=SettingState.GlobalSetting;
                }
            }
        }

        return new SettingItem
        {
            SettingId    =setting.settingId,SettingText=settingMeta.SettingName,GroupName=settingMeta.GroupName,
            ValueRaw     =valueRaw,ValueText           =valueText,State                  =settingState,
            IsStringValue=settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE,
            IsApiExposed =settingMeta.IsApiExposed,
        };
    }


    public System.Collections.Generic.List<SettingItem> GetSettingsForProfile(string profileName,
        SettingViewMode                                                              viewMode,
        ref System.Collections.Generic.Dictionary<string,string>                     applications)
    {
        var result    =new System.Collections.Generic.List<SettingItem>();
        var settingIds=this.meta.GetSettingIds(viewMode);
        settingIds.AddRange(this._baseProfileSettingIds);
        settingIds=Enumerable.ToList(Enumerable.Distinct(settingIds));

        applications=this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);

            var profileSettings=this.GetProfileSettings(hSession,hProfile);
            foreach(var profileSetting in profileSettings)
            {
                result.Add(this.CreateSettingItem(profileSetting));

                if(settingIds.Contains(profileSetting.settingId))
                {
                    settingIds.Remove(profileSetting.settingId);
                }
            }

            foreach(var settingId in settingIds)
            {
                var setting=this.ReadSetting(hSession,hProfile,settingId);
                if(setting!=null)
                {
                    result.Add(this.CreateSettingItem(setting.Value));
                }
                else
                {
                    var dummySetting=new nspector.Native.NVAPI2.NVDRS_SETTING
                    {
                        settingId=settingId,
                    };
                    result.Add(this.CreateSettingItem(dummySetting,true));
                }
            }

            return Enumerable.ToDictionary(
                Enumerable.Select(this.GetProfileApplications(hSession,hProfile),
                    x=>System.Tuple.Create(x.appName,this.GetApplicationFingerprint(x))),x=>x.Item2,x=>x.Item1);
        });

        return Enumerable.ToList(Enumerable.ThenBy(Enumerable.OrderBy(result,x=>x.SettingText),x=>x.GroupName));
    }

    public void AddApplication(string profileName,string applicationName)
    {
        this.DrsSession(hSession=>
        {
            var hProfile=this.GetProfileHandle(hSession,profileName);
            this.AddApplication(hSession,hProfile,applicationName);
            this.SaveSettings(hSession);
        });
    }

    public void RemoveApplication(string profileName,string applicationFingerprint)
    {
        this.DrsSession(hSession=>
        {
            var hProfile    =this.GetProfileHandle(hSession,profileName);
            var applications=this.GetProfileApplications(hSession,hProfile);
            foreach(var app in applications)
            {
                if(this.GetApplicationFingerprint(app)!=applicationFingerprint)
                {
                    continue;
                }

                this.DeleteApplication(hSession,hProfile,app);
                break;
            }

            this.SaveSettings(hSession);
        });
    }

    string GetApplicationFingerprint(nspector.Native.NVAPI2.NVDRS_APPLICATION_V3 application)
        =>$"{application.appName}|{application.fileInFolder}|{application.userFriendlyName}|{application.launcher}";
}