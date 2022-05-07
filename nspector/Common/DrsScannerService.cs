#region

using Enumerable=System.Linq.Enumerable;
using nvw=nspector.Native.NVAPI2.NvapiDrsWrapper;

#endregion

namespace nspector.Common;

class DrsScannerService:DrsSettingsServiceBase
{
    // most common setting ids as start pattern for the heuristic scan
    readonly uint[] _commonSettingIds=
    {
        0x1095DEF8,0x1033DCD2,0x1033CEC1,0x10930F46,0x00A06946,0x10ECDB82,0x20EBD7B8,0x0095DEF9,0x00D55F7D,0x1033DCD3,
        0x1033CEC2,0x2072F036,0x00664339,0x002C7F45,0x209746C1,0x0076E164,0x20FF7493,0x204CFF7B,
    };


    internal System.Collections.Generic.List<CachedSettings> CachedSettings
        =new System.Collections.Generic.List<CachedSettings>();

    internal System.Collections.Generic.List<string>    ModifiedProfiles=new System.Collections.Generic.List<string>();
    internal System.Collections.Generic.HashSet<string> UserProfiles=new System.Collections.Generic.HashSet<string>();

    public DrsScannerService(DrsSettingsMetaService metaService,DrsDecrypterService decrpterService)
        :base(metaService,decrpterService) {}


    bool CheckCommonSetting(System.IntPtr hSession,System.IntPtr hProfile,nspector.Native.NVAPI2.NVDRS_PROFILE profile,
        ref int checkedSettingsCount,uint checkSettingId,bool addToScanResult,
        ref System.Collections.Generic.List<uint> alreadyCheckedSettingIds)
    {
        if(checkedSettingsCount>=profile.numOfSettings)
        {
            return false;
        }

        var setting=new nspector.Native.NVAPI2.NVDRS_SETTING();
        setting.version=nvw.NVDRS_SETTING_VER;

        if(nvw.DRS_GetSetting(hSession,hProfile,checkSettingId,ref setting)
            !=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            return false;
        }

        if(setting.settingLocation!=nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION)
        {
            return false;
        }

        if(!addToScanResult&&setting.isCurrentPredefined==1)
        {
            checkedSettingsCount++;
        }
        else if(addToScanResult)
        {
            if(this.decrypter!=null)
            {
                this.decrypter.DecryptSettingIfNeeded(profile.profileName,ref setting);
            }

            checkedSettingsCount++;
            this.AddScannedSettingToCache(profile,setting);
            alreadyCheckedSettingIds.Add(setting.settingId);
            return setting.isCurrentPredefined!=1;
        }
        else if(setting.isCurrentPredefined!=1)
        {
            return true;
        }

        return false;
    }


    int CalcPercent(int current,int max)=>current>0?(int)System.Math.Round(current*100f/max):0;

    public async System.Threading.Tasks.Task ScanProfileSettingsAsync(bool justModified,System.IProgress<int> progress,
        System.Threading.CancellationToken token=default(System.Threading.CancellationToken))
    {
        await System.Threading.Tasks.Task.Run(()=>
        {
            this.ModifiedProfiles=new System.Collections.Generic.List<string>();
            this.UserProfiles    =new System.Collections.Generic.HashSet<string>();
            var knownPredefines=new System.Collections.Generic.List<uint>(this._commonSettingIds);

            this.DrsSession(hSession=>
            {
                var hBaseProfile  =this.GetProfileHandle(hSession,"");
                var profileHandles=this.EnumProfileHandles(hSession);

                var maxProfileCount=profileHandles.Count;
                var curProfilePos  =0;

                foreach(var hProfile in profileHandles)
                {
                    if(token.IsCancellationRequested)
                    {
                        break;
                    }

                    progress?.Report(this.CalcPercent(curProfilePos++,maxProfileCount));

                    var profile=this.GetProfileInfo(hSession,hProfile);

                    var checkedSettingsCount=0;
                    var alreadyChecked      =new System.Collections.Generic.List<uint>();

                    var foundModifiedProfile=false;
                    if(profile.isPredefined==0)
                    {
                        this.ModifiedProfiles.Add(profile.profileName);
                        this.UserProfiles.Add(profile.profileName);
                        foundModifiedProfile=true;
                        if(justModified)
                        {
                            continue;
                        }
                    }


                    foreach(var kpd in knownPredefines)
                    {
                        if(this.CheckCommonSetting(hSession,hProfile,profile,
                            ref checkedSettingsCount,kpd,!justModified,ref alreadyChecked))
                        {
                            if(!foundModifiedProfile)
                            {
                                foundModifiedProfile=true;
                                this.ModifiedProfiles.Add(profile.profileName);
                                if(justModified)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if(foundModifiedProfile&&justModified||checkedSettingsCount>=profile.numOfSettings)
                    {
                        continue;
                    }

                    var settings=this.GetProfileSettings(hSession,hProfile);
                    foreach(var setting in settings)
                    {
                        if(knownPredefines.IndexOf(setting.settingId)<0)
                        {
                            knownPredefines.Add(setting.settingId);
                        }

                        if(!justModified&&alreadyChecked.IndexOf(setting.settingId)<0)
                        {
                            this.AddScannedSettingToCache(profile,setting);
                        }

                        if(setting.isCurrentPredefined!=1)
                        {
                            if(!foundModifiedProfile)
                            {
                                foundModifiedProfile=true;
                                this.ModifiedProfiles.Add(profile.profileName);
                                if(justModified)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            });
        });
    }


    void AddScannedSettingToCache(nspector.Native.NVAPI2.NVDRS_PROFILE profile,
        nspector.Native.NVAPI2.NVDRS_SETTING                           setting)
    {
        // Skip 3D Vision string values here for improved scan performance
        var allowAddValue=
            setting.settingId    <0x70000000
            ||setting.settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;

        //bool allowAddValue = true;

        var cachedSetting=Enumerable.FirstOrDefault(this.CachedSettings,x=>x.SettingId.Equals(setting.settingId));

        var cacheEntryExists=true;
        if(cachedSetting==null)
        {
            cacheEntryExists=false;
            cachedSetting   =new CachedSettings(setting.settingId,setting.settingType);
        }

        if(setting.isPredefinedValid==1)
        {
            if(allowAddValue)
            {
                if(setting.settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                {
                    cachedSetting.AddStringValue(setting.predefinedValue.stringValue,profile.profileName);
                }
                else if(setting.settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
                {
                    cachedSetting.AddDwordValue(setting.predefinedValue.dwordValue,profile.profileName);
                }
                else if(setting.settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
                {
                    cachedSetting.AddBinaryValue(setting.predefinedValue.binaryValue,profile.profileName);
                }
            }
            else
            {
                cachedSetting.ProfileCount++;
            }

            if(!cacheEntryExists)
            {
                this.CachedSettings.Add(cachedSetting);
            }
        }
    }

    public string FindProfilesUsingApplication(string applicationName)
    {
        var lowerApplicationName=applicationName.ToLower();
        var tmpfile             =nspector.Common.Helper.TempFile.GetTempFileName();

        try
        {
            var matchingProfiles=new System.Collections.Generic.List<string>();

            this.DrsSession(hSession=>
            {
                this.SaveSettingsFileEx(hSession,tmpfile);
            });

            if(System.IO.File.Exists(tmpfile))
            {
                var content=System.IO.File.ReadAllText(tmpfile);
                var pattern="\\sProfile\\s\\\"(?<profile>.*?)\\\"(?<scope>.*?Executable.*?)EndProfile";
                foreach(System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(content,
                    pattern,System.Text.RegularExpressions.RegexOptions.Singleline))
                {
                    var scope=m.Result("${scope}");
                    foreach(System.Text.RegularExpressions.Match ms in System.Text.RegularExpressions.Regex.Matches(
                        scope,"Executable\\s\\\"(?<app>.*?)\\\"",
                        System.Text.RegularExpressions.RegexOptions.Singleline))
                    {
                        if(ms.Result("${app}").ToLower()==lowerApplicationName)
                        {
                            matchingProfiles.Add(m.Result("${profile}"));
                        }
                    }
                }
            }

            return string.Join(";",matchingProfiles);
        }
        finally
        {
            if(System.IO.File.Exists(tmpfile))
            {
                System.IO.File.Delete(tmpfile);
            }
        }
    }
}