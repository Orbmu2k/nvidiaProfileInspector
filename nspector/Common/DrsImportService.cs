#region

using Enumerable=System.Linq.Enumerable;
using nvw=nspector.Native.NVAPI2.NvapiDrsWrapper;

#endregion

namespace nspector.Common;

class DrsImportService:DrsSettingsServiceBase
{
    readonly DrsDecrypterService _DecrypterService;
    readonly DrsScannerService   _ScannerService;

    readonly DrsSettingsService _SettingService;

    public DrsImportService(
        DrsSettingsMetaService metaService,
        DrsSettingsService     settingService,
        DrsScannerService      scannerService,
        DrsDecrypterService    decrypterService)
        :base(metaService)
    {
        this._SettingService  =settingService;
        this._ScannerService  =scannerService;
        this._DecrypterService=decrypterService;
    }

    internal void ExportAllProfilesToNvidiaTextFile(string filename)
    {
        this.DrsSession(hSession=>
        {
            this.SaveSettingsFileEx(hSession,filename);
        });
    }

    internal void ImportAllProfilesFromNvidiaTextFile(string filename)
    {
        this.DrsSession(hSession=>
        {
            this.LoadSettingsFileEx(hSession,filename);
            this.SaveSettings(hSession);
        },true,true);
    }

    internal void ExportProfiles(System.Collections.Generic.List<string> profileNames,string filename,
        bool                                                             includePredefined)
    {
        var exports=new nspector.Common.Import.Profiles();

        this.DrsSession(hSession=>
        {
            foreach(var profileName in profileNames)
            {
                var profile=this.CreateProfileForExport(hSession,profileName,includePredefined);
                exports.Add(profile);
            }
        });

        nspector.Common.Helper.XMLHelper<nspector.Common.Import.Profiles>.SerializeToXmlFile(exports,filename,
            System.Text.Encoding.Unicode,true);
    }

    nspector.Common.Import.Profile CreateProfileForExport(System.IntPtr hSession,string profileName,
        bool                                                            includePredefined)
    {
        var result=new nspector.Common.Import.Profile();

        var hProfile=this.GetProfileHandle(hSession,profileName);
        if(hProfile!=System.IntPtr.Zero)
        {
            result.ProfileName=profileName;

            var apps=this.GetProfileApplications(hSession,hProfile);
            foreach(var app in apps)
            {
                result.Executeables.Add(app.appName);
            }

            var settings=this.GetProfileSettings(hSession,hProfile);
            foreach(var setting in settings)
            {
                var isPredefined=setting.isCurrentPredefined==1;
                var isCurrentProfile=setting.settingLocation==
                    nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;

                if(isCurrentProfile&&(!isPredefined||includePredefined))
                {
                    var exportSetting=setting;
                    this._DecrypterService.DecryptSettingIfNeeded(profileName,ref exportSetting);

                    var profileSetting=nspector.Common.Import.ImportExportUitl
                        .ConvertDrsSettingToProfileSetting(exportSetting);

                    result.Settings.Add(profileSetting);
                }
            }
        }

        return result;
    }

    internal string ImportProfiles(string filename)
    {
        var sbFailedProfilesMessage=new System.Text.StringBuilder();
        var appInUseHint=false;
        var profiles=nspector.Common.Helper.XMLHelper<nspector.Common.Import.Profiles>.DeserializeFromXMLFile(filename);

        this.DrsSession(hSession=>
        {
            foreach(var profile in profiles)
            {
                var profileCreated=false;
                var hProfile      =this.GetProfileHandle(hSession,profile.ProfileName);
                if(hProfile==System.IntPtr.Zero)
                {
                    hProfile=this.CreateProfile(hSession,profile.ProfileName);
                    nvw.DRS_SaveSettings(hSession);
                    profileCreated=true;
                }

                if(hProfile!=System.IntPtr.Zero)
                {
                    var modified=false;
                    this._SettingService.ResetProfile(profile.ProfileName,out modified);
                    try
                    {
                        this.UpdateApplications(hSession,hProfile,profile);
                        this.UpdateSettings(hSession,hProfile,profile,profile.ProfileName);
                    }
                    catch(NvapiException nex)
                    {
                        if(profileCreated)
                        {
                            nvw.DRS_DeleteProfile(hSession,hProfile);
                        }

                        sbFailedProfilesMessage.AppendLine(string.Format("Failed to import profile '{0}'",
                            profile.ProfileName));
                        var appEx=nex as NvapiAddApplicationException;
                        if(appEx!=null)
                        {
                            var profilesWithThisApp
                                =this._ScannerService.FindProfilesUsingApplication(appEx.ApplicationName);
                            sbFailedProfilesMessage.AppendLine(string.Format(
                                "- application '{0}' is already in use by profile '{1}'",appEx.ApplicationName,
                                profilesWithThisApp));
                            appInUseHint=true;
                        }
                        else
                        {
                            sbFailedProfilesMessage.AppendLine(string.Format("- {0}",nex.Message));
                        }

                        sbFailedProfilesMessage.AppendLine("");
                    }

                    nvw.DRS_SaveSettings(hSession);
                }
            }
        });

        if(appInUseHint)
        {
            sbFailedProfilesMessage.AppendLine(
                "Hint: If just the profile name has been changed by nvidia, consider to manually modify the profile name inside the import file using a text editor.");
        }

        return sbFailedProfilesMessage.ToString();
    }

    bool ExistsImportApp(string appName,nspector.Common.Import.Profile importProfile)
    {
        return Enumerable.Any(importProfile.Executeables,x=>x.Equals(appName));
    }

    void UpdateApplications(System.IntPtr hSession,System.IntPtr hProfile,nspector.Common.Import.Profile importProfile)
    {
        var alreadySet=new System.Collections.Generic.HashSet<string>();

        var apps=this.GetProfileApplications(hSession,hProfile);
        foreach(var app in apps)
        {
            if(this.ExistsImportApp(app.appName,importProfile)&&!alreadySet.Contains(app.appName))
            {
                alreadySet.Add(app.appName);
            }
            else
            {
                nvw.DRS_DeleteApplication(hSession,hProfile,new System.Text.StringBuilder(app.appName));
            }
        }

        foreach(var appName in importProfile.Executeables)
        {
            if(!alreadySet.Contains(appName))
            {
                try
                {
                    this.AddApplication(hSession,hProfile,appName);
                }
                catch(NvapiException)
                {
                    throw new NvapiAddApplicationException(appName);
                }
            }
        }
    }

    uint GetImportValue(uint settingId,nspector.Common.Import.Profile importProfile)
    {
        var setting=Enumerable.FirstOrDefault(importProfile.Settings,x=>x.SettingId.Equals(settingId));

        if(setting!=null)
        {
            return uint.Parse(setting.SettingValue);
        }

        return 0;
    }

    nspector.Common.Import.ProfileSetting GetImportProfileSetting(uint settingId,
        nspector.Common.Import.Profile                                 importProfile)
    {
        return Enumerable.FirstOrDefault(importProfile.Settings,x=>x.SettingId.Equals(settingId));
    }

    bool ExistsImportValue(uint settingId,nspector.Common.Import.Profile importProfile)
    {
        return Enumerable.Any(importProfile.Settings,x=>x.SettingId.Equals(settingId));
    }

    void UpdateSettings(System.IntPtr hSession,System.IntPtr hProfile,nspector.Common.Import.Profile importProfile,
        string                        profileName)
    {
        var alreadySet=new System.Collections.Generic.HashSet<uint>();

        var settings=this.GetProfileSettings(hSession,hProfile);
        foreach(var setting in settings)
        {
            var isCurrentProfile=setting.settingLocation
                ==nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION;
            var isPredefined=setting.isCurrentPredefined==1;

            if(isCurrentProfile)
            {
                var exitsValueInImport=this.ExistsImportValue(setting.settingId,importProfile);
                var importSetting     =this.GetImportProfileSetting(setting.settingId,importProfile);

                var decryptedSetting=setting;
                this._DecrypterService.DecryptSettingIfNeeded(profileName,ref decryptedSetting);

                if(isPredefined&&exitsValueInImport&&
                    nspector.Common.Import.ImportExportUitl.AreDrsSettingEqualToProfileSetting(decryptedSetting,
                        importSetting))
                {
                    alreadySet.Add(setting.settingId);
                }
                else if(exitsValueInImport)
                {
                    var updatedSetting
                        =nspector.Common.Import.ImportExportUitl.ConvertProfileSettingToDrsSetting(importSetting);
                    this.StoreSetting(hSession,hProfile,updatedSetting);
                    alreadySet.Add(setting.settingId);
                }
                else if(!isPredefined)
                {
                    nvw.DRS_DeleteProfileSetting(hSession,hProfile,setting.settingId);
                }
            }
        }

        foreach(var setting in importProfile.Settings)
        {
            if(!alreadySet.Contains(setting.SettingId))
            {
                var newSetting=nspector.Common.Import.ImportExportUitl.ConvertProfileSettingToDrsSetting(setting);
                try
                {
                    this.StoreSetting(hSession,hProfile,newSetting);
                }
                catch(NvapiException ex)
                {
                    if(ex.Status!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_SETTING_NOT_FOUND)
                    {
                        throw;
                    }
                }
            }
        }
    }
}