#region

using System.Linq;
using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector;

partial class frmDrvSettings:System.Windows.Forms.Form
{
    readonly nspector.Common.DrsSettingsService     _drs    =nspector.Common.DrsServiceLocator.SettingService;
    readonly nspector.Common.DrsImportService       _import =nspector.Common.DrsServiceLocator.ImportService;
    readonly nspector.Common.DrsSettingsMetaService _meta   =nspector.Common.DrsServiceLocator.MetaService;
    readonly nspector.Common.DrsScannerService      _scanner=nspector.Common.DrsServiceLocator.ScannerService;
    readonly bool                                   _skipScan;
    bool                                            _activated;
    bool                                            _alreadyScannedForPredefinedSettings;

    string _baseProfileName="";

    public string _CurrentProfile="";

    System.Collections.Generic.List<nspector.Common.SettingItem> _currentProfileSettingItems
        =new System.Collections.Generic.List<nspector.Common.SettingItem>();

    bool _isStartup=true;
    bool _isWin7TaskBar;
    int  _lastComboRowIndex=-1;

    System.Threading.CancellationTokenSource _scannerCancelationTokenSource;
    nspector.Native.WINAPI.ITaskbarList3     _taskbarList;
    System.IntPtr                            _taskbarParent=System.IntPtr.Zero;

    internal frmDrvSettings():this(false,false) {}

    internal frmDrvSettings(bool showCsnOnly,bool skipScan)
    {
        this._skipScan=skipScan;
        this.InitializeComponent();
        this.InitTaskbarList();
        this.SetupDropFilesNative();
        this.SetupToolbar();
        this.SetupDpiAdjustments();

        this.tscbShowCustomSettingNamesOnly.Checked=showCsnOnly;
        this.Icon=System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
    }

    protected override void WndProc(ref System.Windows.Forms.Message m)
    {
        switch(m.Msg)
        {
            case nspector.Native.WINAPI.MessageHelper.WM_COPYDATA:
                var copyDataStruct=new nspector.Native.WINAPI.MessageHelper.COPYDATASTRUCT();
                var copyDataType  =copyDataStruct.GetType();
                copyDataStruct=(nspector.Native.WINAPI.MessageHelper.COPYDATASTRUCT)m.GetLParam(copyDataType);
                if(copyDataStruct.lpData.Equals("ProfilesImported"))
                {
                    nspector.Common.DrsSessionScope.DestroyGlobalSession();
                    this.RefreshAll();
                }

                break;
        }

        base.WndProc(ref m);
    }

    System.Windows.Forms.ListViewGroup FindOrCreateGroup(string groupName)
    {
        if(string.IsNullOrEmpty(groupName))
        {
            groupName="Unknown";
        }

        foreach(System.Windows.Forms.ListViewGroup group in this.lvSettings.Groups)
        {
            if(group.Header==groupName)
            {
                return group;
            }
        }

        var newGroup=new System.Windows.Forms.ListViewGroup(groupName);
        this.lvSettings.Groups.Insert(0,newGroup);

        return newGroup;
    }

    System.Windows.Forms.ListViewItem CreateListViewItem(nspector.Common.SettingItem setting)
    {
        var group=this.FindOrCreateGroup(setting.GroupName);

        var item=new System.Windows.Forms.ListViewItem(setting.SettingText);
        item.Tag  =setting.SettingId;
        item.Group=group;
        item.SubItems.Add(setting.ValueText);
        item.SubItems.Add(setting.ValueRaw);

        switch(setting.State)
        {
            default:
                item.ImageIndex=1;
                item.ForeColor =System.Drawing.SystemColors.GrayText;
                break;

            case nspector.Common.SettingState.NvidiaSetting:
                item.ImageIndex=2;
                break;

            case nspector.Common.SettingState.GlobalSetting:
                item.ImageIndex=3;
                item.ForeColor =System.Drawing.SystemColors.GrayText;
                break;

            case nspector.Common.SettingState.UserdefinedSetting:
                item.ImageIndex=0;
                break;
        }

        return item;
    }

    void RefreshApplicationsCombosAndText(System.Collections.Generic.Dictionary<string,string> applications)
    {
        this.lblApplications.Text="";
        this.tssbRemoveApplication.DropDownItems.Clear();

        this.lblApplications.Text=" "+string.Join(", ",Enumerable.Select(applications,x=>x.Value));
        foreach(var app in applications)
        {
            var item=this.tssbRemoveApplication.DropDownItems.Add(app.Value,
                nspector.Properties.Resources.ieframe_1_18212);
            item.Tag=app.Key;
        }

        this.tssbRemoveApplication.Enabled=this.tssbRemoveApplication.DropDownItems.Count>0;
    }

    nspector.Common.SettingViewMode GetSettingViewMode()
    {
        if(this.tscbShowCustomSettingNamesOnly.Checked)
        {
            return nspector.Common.SettingViewMode.CustomSettingsOnly;
        }

        if(this.tscbShowScannedUnknownSettings.Checked)
        {
            return nspector.Common.SettingViewMode.IncludeScannedSetttings;
        }

        return nspector.Common.SettingViewMode.Normal;
    }

    void RefreshCurrentProfile()
    {
        var lvSelection="";
        if(this.lvSettings.SelectedItems.Count>0)
        {
            lvSelection=this.lvSettings.SelectedItems[0].Text;
        }

        this.lvSettings.BeginUpdate();
        try
        {
            this.lvSettings.Items.Clear();
            this.lvSettings.Groups.Clear();
            var applications=new System.Collections.Generic.Dictionary<string,string>();

            this._currentProfileSettingItems
                =this._drs.GetSettingsForProfile(this._CurrentProfile,this.GetSettingViewMode(),ref applications);
            this.RefreshApplicationsCombosAndText(applications);

            foreach(var settingItem in this._currentProfileSettingItems)
            {
                var itm=this.lvSettings.Items.Add(this.CreateListViewItem(settingItem));
                if(System.Diagnostics.Debugger.IsAttached&&!settingItem.IsApiExposed)
                {
                    itm.ForeColor=System.Drawing.Color.LightCoral;
                }
            }

            this.btnResetValue.Enabled=false;

            try
            {
                this.lvSettings.RemoveEmbeddedControl(this.cbValues);
                this.lvSettings.RemoveEmbeddedControl(this.btnResetValue);
            }
            catch {}
        }
        finally
        {
            this.lvSettings.EndUpdate();
            ((nspector.Common.Helper.ListViewGroupSorter)this.lvSettings).SortGroups(true);

            System.GC.Collect();
            for(var i=0;i<this.lvSettings.Items.Count;i++)
            {
                if(this.lvSettings.Items[i].Text==lvSelection)
                {
                    this.lvSettings.Items[i].Selected=true;
                    this.lvSettings.Items[i].EnsureVisible();

                    if(!this.cbProfiles.Focused)
                    {
                        this.lvSettings.Select();
                        this.cbValues.Text=this.lvSettings.Items[i].SubItems[1].Text;
                    }

                    break;
                }
            }
        }
    }

    void RefreshProfilesCombo()
    {
        this.cbProfiles.Items.Clear();

        var profileNames=this._drs.GetProfileNames(ref this._baseProfileName);
        this.cbProfiles.Items.AddRange(Enumerable.ToArray(Enumerable.Cast<object>(profileNames)));

        this.cbProfiles.Sorted=true;
    }

    void MoveComboToItemAndFill()
    {
        if(this.lvSettings.SelectedItems.Count>0)
        {
            if(!this.cbValues.ContainsFocus&&this._lastComboRowIndex!=this.lvSettings.SelectedItems[0].Index)
            {
                this.btnResetValue.Enabled=true;

                this.cbValues.BeginUpdate();

                this.cbValues.Items.Clear();
                this.cbValues.Tag=this.lvSettings.SelectedItems[0].Tag;
                var settingid=(uint)this.lvSettings.SelectedItems[0].Tag;

                var settingMeta=this._meta.GetSettingMeta(settingid,this.GetSettingViewMode());
                if(settingMeta!=null)
                {
                    if(settingMeta.SettingType ==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE&&
                        settingMeta.DwordValues!=null)
                    {
                        var valueNames=Enumerable.ToList(Enumerable.Select(settingMeta.DwordValues,x=>x.ValueName));
                        foreach(var v in valueNames)
                        {
                            var itm="";
                            if(v.Length>4000)
                            {
                                itm=v.Substring(0,4000)+" ...";
                            }
                            else
                            {
                                itm=v;
                            }

                            this.cbValues.Items.Add(itm);
                        }
                    }

                    if(settingMeta.SettingType  ==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE&&
                        settingMeta.StringValues!=null)
                    {
                        var valueNames=Enumerable.ToList(Enumerable.Select(settingMeta.StringValues,x=>x.ValueName));
                        foreach(var v in valueNames)
                        {
                            this.cbValues.Items.Add(v);
                        }
                    }

                    if(settingMeta.SettingType  ==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE&&
                        settingMeta.BinaryValues!=null)
                    {
                        var valueNames=Enumerable.ToList(Enumerable.Select(settingMeta.BinaryValues,x=>x.ValueName));
                        foreach(var v in valueNames)
                        {
                            this.cbValues.Items.Add(v);
                        }
                    }

                    var scannedCount=Enumerable.Count<nspector.Common.Meta.SettingValue<uint>>(settingMeta?.DwordValues?
                        .Where(x=>x.ValueSource==nspector.Common.Meta.SettingMetaSource.ScannedSettings));

                    this.tsbBitValueEditor.Enabled=scannedCount>0;
                }

                if(this.cbValues.Items.Count<1)
                {
                    this.cbValues.Items.Add("");
                    this.cbValues.Items.RemoveAt(0);
                }


                this.cbValues.Text=this.lvSettings.SelectedItems[0].SubItems[1].Text;
                this.cbValues.EndUpdate();

                this.lvSettings.AddEmbeddedControl(this.cbValues,1,this.lvSettings.SelectedItems[0].Index);

                if(this.lvSettings.SelectedItems[0].ImageIndex==0)
                {
                    this.lvSettings.AddEmbeddedControl(this.btnResetValue,2,this.lvSettings.SelectedItems[0].Index,
                        System.Windows.Forms.DockStyle.Right);
                }

                this._lastComboRowIndex=this.lvSettings.SelectedItems[0].Index;
                this.cbValues.Visible  =true;
            }
        }
        else
        {
            this._lastComboRowIndex=-1;

            if(!this.cbValues.ContainsFocus)
            {
                try
                {
                    this.lvSettings.RemoveEmbeddedControl(this.cbValues);
                    this.lvSettings.RemoveEmbeddedControl(this.btnResetValue);
                }
                catch {}

                this.btnResetValue.Enabled=false;
                this.cbValues.Visible     =false;

                this.tsbBitValueEditor.Enabled=false;
            }
        }
    }

    int GetListViewIndexOfSetting(uint settingId)
    {
        var idx=0;
        foreach(System.Windows.Forms.ListViewItem lvi in this.lvSettings.Items)
        {
            if(settingId==(uint)lvi.Tag)
            {
                return idx;
            }

            idx++;
        }

        return-1;
    }

    void UpdateItemByComboValue()
    {
        var settingId=(uint)this.cbValues.Tag;
        var activeImages=new[]
        {
            0,2,
        };

        var idx=this.GetListViewIndexOfSetting(settingId);
        if(idx!=-1)
        {
            var lvItem=this.lvSettings.Items[idx];

            var settingMeta=this._meta.GetSettingMeta(settingId,this.GetSettingViewMode());

            var currentProfileItem=Enumerable.First(this._currentProfileSettingItems,x=>x.SettingId.Equals(settingId));

            var cbValueText=this.cbValues.Text.Trim();

            var valueHasChanged=currentProfileItem.ValueText!=cbValueText;
            if(settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
            {
                var stringBehind=nspector.Common.DrsUtil.ParseStringSettingValue(settingMeta,cbValueText);
                valueHasChanged=currentProfileItem.ValueRaw!=stringBehind;
            }

            if(valueHasChanged||Enumerable.Contains(activeImages,lvItem.ImageIndex))
            {
                lvItem.ForeColor=System.Drawing.SystemColors.ControlText;
            }
            else
            {
                lvItem.ForeColor=System.Drawing.SystemColors.GrayText;
            }

            if(settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
            {
                lvItem.SubItems[2].Text=
                    nspector.Common.DrsUtil.GetDwordString(
                        nspector.Common.DrsUtil.ParseDwordSettingValue(settingMeta,cbValueText));
                lvItem.SubItems[1].Text=cbValueText;
            }
            else if(settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
            {
                lvItem.SubItems[2].Text=
                    nspector.Common.DrsUtil.ParseStringSettingValue(settingMeta,cbValueText);// DrsUtil.StringValueRaw;
                lvItem.SubItems[1].Text=cbValueText;
            }
            else if(settingMeta.SettingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
            {
                lvItem.SubItems[2].Text=
                    nspector.Common.DrsUtil.GetBinaryString(nspector.Common.DrsUtil.ParseBinarySettingValue(settingMeta,
                        cbValueText));// DrsUtil.StringValueRaw;
                lvItem.SubItems[1].Text=cbValueText;
            }
        }
    }

    void StoreChangesOfProfileToDriver()
    {
        var settingsToStore=new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<uint,string>>();

        foreach(System.Windows.Forms.ListViewItem lvi in this.lvSettings.Items)
        {
            var currentProfileItem
                =Enumerable.First(this._currentProfileSettingItems,x=>x.SettingId.Equals((uint)lvi.Tag));

            var listValueX=lvi.SubItems[1].Text;

            var itmEmpty=string.IsNullOrEmpty(listValueX);
            var curEmpty=string.IsNullOrEmpty(currentProfileItem.ValueText);

            if(currentProfileItem.ValueText!=listValueX&&!(itmEmpty&&curEmpty))
            {
                settingsToStore.Add(new System.Collections.Generic.KeyValuePair<uint,string>((uint)lvi.Tag,listValueX));
            }
        }

        if(settingsToStore.Count>0)
        {
            this._drs.StoreSettingsToProfile(this._CurrentProfile,settingsToStore);
            this.AddToModifiedProfiles(this._CurrentProfile);
        }

        this.RefreshCurrentProfile();
    }

    void ResetCurrentProfile()
    {
        var removeFromModified=false;
        this._drs.ResetProfile(this._CurrentProfile,out removeFromModified);

        if(removeFromModified)
        {
            this.RemoveFromModifiedProfiles(this._CurrentProfile);
        }

        this.RefreshCurrentProfile();
    }

    void ResetSelectedValue()
    {
        if(this.lvSettings.SelectedItems!=null&&this.lvSettings.SelectedItems.Count>0)
        {
            var settingId=(uint)this.lvSettings.SelectedItems[0].Tag;

            bool removeFromModified;
            this._drs.ResetValue(this._CurrentProfile,settingId,out removeFromModified);

            if(removeFromModified)
            {
                this.RemoveFromModifiedProfiles(this._CurrentProfile);
            }

            this.RefreshCurrentProfile();
        }
    }

    void InitTaskbarList()
    {
        if(System.Environment.OSVersion.Version.Major>=6&&System.Environment.OSVersion.Version.Minor>=1)
        {
            try
            {
                this._taskbarList=(nspector.Native.WINAPI.ITaskbarList3)new nspector.Native.WINAPI.TaskbarList();
                this._taskbarList.HrInit();
                this._taskbarParent=this.Handle;
                this._isWin7TaskBar=true;
            }
            catch
            {
                this._taskbarList  =null;
                this._taskbarParent=System.IntPtr.Zero;
                this._isWin7TaskBar=false;
            }
        }
    }

    void SetTaskbarIcon()
    {
        if(this._taskbarList!=null&&this._isWin7TaskBar&&nspector.Common.Helper.AdminHelper.IsAdmin)
        {
            try
            {
                this._taskbarList.SetOverlayIcon(this._taskbarParent,nspector.Properties.Resources.shield16.Handle,
                    "Elevated");
            }
            catch {}
        }
    }

    void SetTitleVersion()
    {
        var numberFormat=new System.Globalization.NumberFormatInfo
        {
            NumberDecimalSeparator=".",
        };
        var version=System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        var fileVersionInfo
            =System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly()
                .Location);
        this.Text=
            $"{System.Windows.Forms.Application.ProductName} {version} - Geforce {this._drs.DriverVersion.ToString("#.00",numberFormat)} - Profile Settings - {fileVersionInfo.LegalCopyright}";
    }

    static void InitMessageFilter(System.IntPtr handle)
    {
        if(System.Environment.OSVersion.Version.Major>=6&&System.Environment.OSVersion.Version.Minor>=1)
        {
            nspector.Native.WINAPI.DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle,
                nspector.Native.WINAPI.DragAcceptNativeHelper.WM_DROPFILES,
                nspector.Native.WINAPI.DragAcceptNativeHelper.MSGFLT_ALLOW,System.IntPtr.Zero);
            nspector.Native.WINAPI.DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle,
                nspector.Native.WINAPI.DragAcceptNativeHelper.WM_COPYDATA,
                nspector.Native.WINAPI.DragAcceptNativeHelper.MSGFLT_ALLOW,System.IntPtr.Zero);
            nspector.Native.WINAPI.DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle,
                nspector.Native.WINAPI.DragAcceptNativeHelper.WM_COPYGLOBALDATA,
                nspector.Native.WINAPI.DragAcceptNativeHelper.MSGFLT_ALLOW,System.IntPtr.Zero);
        }
        else if(System.Environment.OSVersion.Version.Major>=6&&System.Environment.OSVersion.Version.Minor>=0)
        {
            nspector.Native.WINAPI.DragAcceptNativeHelper.ChangeWindowMessageFilter(
                nspector.Native.WINAPI.DragAcceptNativeHelper.WM_DROPFILES,
                nspector.Native.WINAPI.DragAcceptNativeHelper.MSGFLT_ADD);
            nspector.Native.WINAPI.DragAcceptNativeHelper.ChangeWindowMessageFilter(
                nspector.Native.WINAPI.DragAcceptNativeHelper.WM_COPYDATA,
                nspector.Native.WINAPI.DragAcceptNativeHelper.MSGFLT_ADD);
            nspector.Native.WINAPI.DragAcceptNativeHelper.ChangeWindowMessageFilter(
                nspector.Native.WINAPI.DragAcceptNativeHelper.WM_COPYGLOBALDATA,
                nspector.Native.WINAPI.DragAcceptNativeHelper.MSGFLT_ADD);
        }
    }

    void SetupDpiAdjustments()
    {
        this.chSettingID.Width      =this.lblWidth330.Width;
        this.chSettingValueHex.Width=this.lblWidth96.Width;
    }

    void SetupToolbar()
    {
        this.tsMain.Renderer        =new nspector.Common.Helper.NoBorderRenderer();
        this.tsMain.ImageScalingSize=new System.Drawing.Size(this.lblWidth16.Width,this.lblWidth16.Width);
    }

    void SetupDropFilesNative()
    {
        this.lvSettings.OnDropFilesNative+=this.lvSettings_OnDropFilesNative;
        nspector.Native.WINAPI.DragAcceptNativeHelper.DragAcceptFiles(this.Handle,           true);
        nspector.Native.WINAPI.DragAcceptNativeHelper.DragAcceptFiles(this.lvSettings.Handle,true);
        frmDrvSettings.InitMessageFilter(this.lvSettings.Handle);
    }

    void SetupLayout()
    {
        if(System.Windows.Forms.Screen.GetWorkingArea(this).Height<this.Height+10)
        {
            this.Height=System.Windows.Forms.Screen.GetWorkingArea(this).Height-20;
        }
    }

    void RefreshModifiesProfilesDropDown()
    {
        this.tsbModifiedProfiles.DropDownItems.Clear();
        this._scanner.ModifiedProfiles.Sort();
        foreach(var modProfile in this._scanner.ModifiedProfiles)
        {
            if(modProfile!=this._baseProfileName)
            {
                var newItem=this.tsbModifiedProfiles.DropDownItems.Add(modProfile);
                if(!this._scanner.UserProfiles.Contains(modProfile))
                {
                    newItem.Image=this.tsbRestoreProfile.Image;
                }
            }
        }

        if(this.tsbModifiedProfiles.DropDownItems.Count>0)
        {
            this.tsbModifiedProfiles.Enabled=true;
        }
    }

    void frmDrvSettings_Load(object sender,System.EventArgs e)
    {
        this.SetupLayout();
        this.SetTitleVersion();
        this.LoadSettings();

        this.RefreshProfilesCombo();
        this.cbProfiles.Text=this.GetBaseProfileName();

        this.tsbBitValueEditor.Enabled    =false;
        this.tsbDeleteProfile.Enabled     =false;
        this.tsbAddApplication.Enabled    =false;
        this.tssbRemoveApplication.Enabled=false;

        this.InitResetValueTooltip();
    }

    void InitResetValueTooltip()
    {
        var toolTip=new System.Windows.Forms.ToolTip();
        toolTip.SetToolTip(this.btnResetValue,"Restore this value to NVIDIA defaults.");
    }

    void lvSettings_SelectedIndexChanged(object sender,System.EventArgs e)
    {
        this.MoveComboToItemAndFill();
    }

    void cbValues_SelectedValueChanged(object sender,System.EventArgs e)
    {
        this.UpdateItemByComboValue();
    }

    void cbValues_Leave(object sender,System.EventArgs e)
    {
        this.UpdateItemByComboValue();
    }

    void btnResetValue_Click(object sender,System.EventArgs e)
    {
        this.ResetSelectedValue();
    }

    void ChangeCurrentProfile(string profileName)
    {
        if(profileName==this.GetBaseProfileName()||profileName==this._baseProfileName)
        {
            this._CurrentProfile              =this._baseProfileName;
            this.cbProfiles.Text              =this.GetBaseProfileName();
            this.tsbDeleteProfile.Enabled     =false;
            this.tsbAddApplication.Enabled    =false;
            this.tssbRemoveApplication.Enabled=false;
        }
        else
        {
            this._CurrentProfile              =this.cbProfiles.Text;
            this.tsbDeleteProfile.Enabled     =true;
            this.tsbAddApplication.Enabled    =true;
            this.tssbRemoveApplication.Enabled=true;
        }


        this.RefreshCurrentProfile();
    }

    void cbProfiles_SelectedIndexChanged(object sender,System.EventArgs e)
    {
        if(this.cbProfiles.SelectedIndex>-1)
        {
            this.ChangeCurrentProfile(this.cbProfiles.Text);
        }
    }

    void SetTaskbarProgress(int progress)
    {
        if(this._isWin7TaskBar)
        {
            try
            {
                if(progress==0)
                {
                    this._taskbarList.SetProgressState(this._taskbarParent,
                        nspector.Native.WINAPI.TBPFLAG.TBPF_NOPROGRESS);
                }
                else
                {
                    this._taskbarList.SetProgressState(this._taskbarParent,nspector.Native.WINAPI.TBPFLAG.TBPF_NORMAL);
                    this._taskbarList.SetProgressValue(this._taskbarParent,(ulong)progress,100);
                }
            }
            catch {}
        }
    }

    void AddToModifiedProfiles(string profileName,bool userProfile=false)
    {
        if(!this._scanner.UserProfiles.Contains(profileName)&&profileName!=this._baseProfileName&&userProfile)
        {
            this._scanner.UserProfiles.Add(profileName);
        }

        if(!this._scanner.ModifiedProfiles.Contains(profileName)&&profileName!=this._baseProfileName)
        {
            this._scanner.ModifiedProfiles.Add(profileName);
            this.RefreshModifiesProfilesDropDown();
        }
    }

    void RemoveFromModifiedProfiles(string profileName)
    {
        if(this._scanner.UserProfiles.Contains(profileName))
        {
            this._scanner.UserProfiles.Remove(profileName);
        }

        if(this._scanner.ModifiedProfiles.Contains(profileName))
        {
            this._scanner.ModifiedProfiles.Remove(profileName);
            this.RefreshModifiesProfilesDropDown();
        }
    }

    void ShowExportProfiles()
    {
        if(this._scanner.ModifiedProfiles.Count>0)
        {
            var frmExport=new frmExportProfiles();
            frmExport.ShowDialog(this);
        }
        else
        {
            System.Windows.Forms.MessageBox.Show("No user modified profiles found! Nothing to export.",
                "Userprofile Search",
                System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Information);
        }
    }

    async System.Threading.Tasks.Task ScanProfilesSilentAsync(bool scanPredefined,bool showProfileDialog)
    {
        if(this._skipScan)
        {
            if(scanPredefined&&!this._alreadyScannedForPredefinedSettings)
            {
                this._alreadyScannedForPredefinedSettings=true;
                this._meta.ResetMetaCache();
                this.tsbModifiedProfiles.Enabled                       =true;
                this.exportUserdefinedProfilesToolStripMenuItem.Enabled=false;
                this.RefreshCurrentProfile();
            }

            return;
        }

        this.tsbModifiedProfiles.Enabled=false;
        this.tsbRefreshProfile.Enabled  =false;
        this.pbMain.Minimum             =0;
        this.pbMain.Maximum             =100;

        this._scannerCancelationTokenSource=new System.Threading.CancellationTokenSource();

        var progressHandler=new System.Progress<int>(value=>
        {
            this.pbMain.Value=value;
            this.SetTaskbarProgress(value);
        });

        if(scanPredefined&&!this._alreadyScannedForPredefinedSettings)
        {
            this._alreadyScannedForPredefinedSettings=true;
            await this._scanner.ScanProfileSettingsAsync(false,progressHandler,
                this._scannerCancelationTokenSource.Token);
            this._meta.ResetMetaCache();
            this.tscbShowScannedUnknownSettings.Enabled=true;
        }
        else
        {
            await this._scanner.ScanProfileSettingsAsync(true,progressHandler,
                this._scannerCancelationTokenSource.Token);
        }

        this.RefreshModifiesProfilesDropDown();
        this.tsbModifiedProfiles.Enabled=true;

        this.pbMain.Value  =0;
        this.pbMain.Enabled=false;
        this.SetTaskbarProgress(0);

        if(showProfileDialog)
        {
            this.ShowExportProfiles();
        }

        this.RefreshCurrentProfile();
        this.tsbRefreshProfile.Enabled=true;
    }

    void cbCustomSettingsOnly_CheckedChanged(object sender,System.EventArgs e)
    {
        this.RefreshCurrentProfile();
    }

    internal void SetSelectedDwordValue(uint dwordValue)
    {
        if(this.lvSettings.SelectedItems!=null&this.lvSettings.SelectedItems.Count>0)
        {
            this.cbValues.Text=nspector.Common.DrsUtil.GetDwordString(dwordValue);
            ;
            this.UpdateItemByComboValue();
        }
    }

    async void tsbRestoreProfile_Click(object sender,System.EventArgs e)
    {
        if(frmDrvSettings.ModifierKeys==System.Windows.Forms.Keys.Control)
        {
            if(System.Windows.Forms.MessageBox.Show(this,
                    "Restore all profiles to NVIDIA driver defaults?",
                    "Restore all profiles",
                    System.Windows.Forms.MessageBoxButtons.YesNo,System.Windows.Forms.MessageBoxIcon.Question)
                ==System.Windows.Forms.DialogResult.Yes)
            {
                this._drs.ResetAllProfilesInternal();

                this.RefreshProfilesCombo();
                this.RefreshCurrentProfile();
                await this.ScanProfilesSilentAsync(true,false);
                this.cbProfiles.Text=this.GetBaseProfileName();
            }
        }
        else
        {
            this.ResetCurrentProfile();
        }
    }

    void tsbRefreshProfile_Click(object sender,System.EventArgs e)
    {
        nspector.Common.DrsSessionScope.DestroyGlobalSession();
        this.RefreshAll();
    }

    void tsbApplyProfile_Click(object sender,System.EventArgs e)
    {
        try
        {
            this.UpdateItemByComboValue();
        }
        catch {}

        this.StoreChangesOfProfileToDriver();
    }

    void tsbBitValueEditor_Click(object sender,System.EventArgs e)
    {
        if(this.lvSettings.SelectedItems!=null&this.lvSettings.SelectedItems.Count>0)
        {
            var frmBits=new frmBitEditor();
            frmBits.ShowDialog(this,
                (uint)this.lvSettings.SelectedItems[0].Tag,
                uint.Parse(this.lvSettings.SelectedItems[0].SubItems[2].Text.Substring(2),
                    System.Globalization.NumberStyles.AllowHexSpecifier),this.lvSettings.SelectedItems[0].Text);
        }
    }

    void tscbShowScannedUnknownSettings_Click(object sender,System.EventArgs e)
    {
        this.RefreshCurrentProfile();
    }

    void lvSettings_Resize(object sender,System.EventArgs e)
    {
        this.ResizeColumn();
    }

    void ResizeColumn()
    {
        this.lvSettings.Columns[1].Width=this.lvSettings.Width-
            (this.lvSettings.Columns[0].Width+this.lvSettings.Columns[2].Width+this.lblWidth30.Width);
    }

    void lvSettings_ColumnWidthChanging(object sender,System.Windows.Forms.ColumnWidthChangingEventArgs e)
    {
        if(e.ColumnIndex!=1)
        {
            if(e.ColumnIndex==0&&e.NewWidth<260)
            {
                e.NewWidth=260;
                e.Cancel  =true;
            }
            else if(e.ColumnIndex==2&&e.NewWidth<96)
            {
                e.Cancel  =true;
                e.NewWidth=96;
            }

            this.ResizeColumn();
        }
    }

    async void frmDrvSettings_Shown(object sender,System.EventArgs e)
    {
        if(this._isStartup)
        {
            new System.Threading.Thread(this.SetTaskbarIcon).Start();
            await this.ScanProfilesSilentAsync(true,false);

            if(this._scannerCancelationTokenSource!=null&&
                !this._scannerCancelationTokenSource.Token.IsCancellationRequested
                &&this.WindowState!=System.Windows.Forms.FormWindowState.Maximized)
            {
                new nspector.Native.WINAPI.MessageHelper().bringAppToFront((int)this.Handle);
            }

            this._isStartup=false;
        }
    }

    void tsbDeleteProfile_Click(object sender,System.EventArgs e)
    {
        if(frmDrvSettings.ModifierKeys==System.Windows.Forms.Keys.Control)
        {
            if(System.Windows.Forms.MessageBox.Show(this,"Really delete all profiles?","Delete all profiles",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question)==System.Windows.Forms.DialogResult.Yes)
            {
                this._drs.DeleteAllProfilesHard();
                this.ChangeCurrentProfile(this._baseProfileName);
                nspector.Common.DrsSessionScope.DestroyGlobalSession();
                this.RefreshAll();
            }
        }
        else if(System.Windows.Forms.MessageBox.Show(this,
                "Really delete this profile?\r\n\r\nNote: NVIDIA predefined profiles can not be restored until next driver installation!",
                "Delete Profile",System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Question)==System.Windows.Forms.DialogResult.Yes)
        {
            if(this._drs.DriverVersion>280&&this._drs.DriverVersion<310)
                // hack for driverbug
            {
                this._drs.DeleteProfileHard(this._CurrentProfile);
            }
            else
            {
                this._drs.DeleteProfile(this._CurrentProfile);
            }

            this.RemoveFromModifiedProfiles(this._CurrentProfile);
            System.Windows.Forms.MessageBox.Show(this,
                string.Format("Profile '{0}' has been deleted.",this._CurrentProfile),"Info",
                System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Information);
            this.RefreshProfilesCombo();
            this.ChangeCurrentProfile(this._baseProfileName);
        }
    }

    void tsbAddApplication_Click(object sender,System.EventArgs e)
    {
        var openDialog=new System.Windows.Forms.OpenFileDialog();
        openDialog.DefaultExt="*.exe";
        openDialog.Filter    ="Application EXE Name|*.exe|Application Absolute Path|*.exe";

        if(openDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
        {
            var applicationName=new System.IO.FileInfo(openDialog.FileName).Name;
            if(openDialog.FilterIndex==2)
            {
                applicationName=openDialog.FileName;
            }

            try
            {
                this._drs.AddApplication(this._CurrentProfile,applicationName);
            }
            catch(nspector.Common.NvapiException ex)
            {
                if(ex.Status   ==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_EXECUTABLE_ALREADY_IN_USE
                    ||ex.Status==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_ERROR)
                {
                    if(this.lblApplications.Text.ToUpper().IndexOf(" "+applicationName.ToUpper()+",")!=-1)
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "This application executable is already assigned to this profile!",
                            "Error adding Application",System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        var profileNames=this._scanner.FindProfilesUsingApplication(applicationName);
                        if(profileNames=="")
                        {
                            System.Windows.Forms.MessageBox.Show(
                                "This application executable might already be assigned to another profile!",
                                "Error adding Application",System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(
                                "This application executable is already assigned to the following profiles: "+
                                profileNames,"Error adding Application",System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        this.RefreshCurrentProfile();
    }

    void tssbRemoveApplication_DropDownItemClicked(object sender,System.Windows.Forms.ToolStripItemClickedEventArgs e)
    {
        //if ((uint)e.ClickedItem.Tag == 0
        //    || (
        //        (uint)e.ClickedItem.Tag == 1
        //        &&
        //        MessageBox.Show(this,
        //            "Do you really want to delete this NVIDIA predefined application executeable?\r\n\r\nNote: This can not be restored until next driver installation!",
        //            "Delete Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        //    )
        //{
        //    drs.DeleteApplication(currentProfile, e.ClickedItem.Text);
        //}
        this._drs.RemoveApplication(this._CurrentProfile,e.ClickedItem.Tag.ToString());
        this.RefreshCurrentProfile();
    }

    void tsbCreateProfile_Click(object sender,System.EventArgs e)
    {
        this.ShowCreateProfileDialog("");
    }

    void ShowCreateProfileDialog(string nameProposal,string applicationName=null)
    {
        var ignoreList=Enumerable.ToList(Enumerable.Cast<string>(this.cbProfiles.Items));
        var result    =nameProposal;

        if(nspector.Common.Helper.InputBox.Show("Create Profile","Please enter profile name:",ref result,ignoreList,"",
                2048)==
            System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                this._drs.CreateProfile(result,applicationName);
                this.RefreshProfilesCombo();
                this.cbProfiles.SelectedIndex=this.cbProfiles.Items.IndexOf(result);
                this.AddToModifiedProfiles(result,true);
            }
            catch(nspector.Common.NvapiException ex)
            {
                //TODO: could not create profile
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }

    void tsbExportProfiles_Click(object sender,System.EventArgs e)
    {
        this.tsbExportProfiles.ShowDropDown();
    }

    void tsbImportProfiles_Click(object sender,System.EventArgs e)
    {
        this.tsbImportProfiles.ShowDropDown();
    }

    async void exportUserdefinedProfilesToolStripMenuItem_Click(object sender,System.EventArgs e)
    {
        await this.ScanProfilesSilentAsync(false,true);
    }

    void ExportCurrentProfile(bool includePredefined)
    {
        var saveDialog=new System.Windows.Forms.SaveFileDialog();
        saveDialog.DefaultExt="*.nip";
        saveDialog.Filter    =System.Windows.Forms.Application.ProductName+" Profiles|*.nip";
        saveDialog.FileName  =this._CurrentProfile                        +".nip";
        if(saveDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
        {
            var profiles=Enumerable.ToList(new[]
            {
                this._CurrentProfile,
            });
            this._import.ExportProfiles(profiles,saveDialog.FileName,includePredefined);
        }
    }

    void exportCurrentProfileOnlyToolStripMenuItem_Click(object sender,System.EventArgs e)
    {
        this.ExportCurrentProfile(false);
    }

    void exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem_Click(object sender,System.EventArgs e)
    {
        this.ExportCurrentProfile(true);
    }

    void tssbRemoveApplication_Click(object sender,System.EventArgs e)
    {
        if(this.tssbRemoveApplication.DropDown.Items.Count>0)
        {
            this.tssbRemoveApplication.ShowDropDown();
        }
    }

    void tsbModifiedProfiles_DropDownItemClicked(object sender,System.Windows.Forms.ToolStripItemClickedEventArgs e)
    {
        this.cbProfiles.SelectedIndex=this.cbProfiles.FindStringExact(e.ClickedItem.Text);
    }

    string GetBaseProfileName()=>string.Format("_GLOBAL_DRIVER_PROFILE ({0})",this._baseProfileName);

    void tsbModifiedProfiles_ButtonClick(object sender,System.EventArgs e)
    {
        this.ChangeCurrentProfile(this.GetBaseProfileName());
    }

    void frmDrvSettings_Activated(object sender,System.EventArgs e)
    {
        if(!this._activated)
        {
            this._activated=true;
        }
    }

    void exportAllProfilesNVIDIATextFormatToolStripMenuItem_Click(object sender,System.EventArgs e)
    {
        var saveDialog=new System.Windows.Forms.SaveFileDialog();
        saveDialog.DefaultExt="*.txt";
        saveDialog.Filter    ="Profiles (NVIDIA Text Format)|*.txt";
        if(saveDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
        {
            this._import.ExportAllProfilesToNvidiaTextFile(saveDialog.FileName);
        }
    }

    async void RefreshAll()
    {
        this.RefreshProfilesCombo();
        await this.ScanProfilesSilentAsync(true,false);

        var idx=this.cbProfiles.Items.IndexOf(this._CurrentProfile);
        if(idx==-1||this._CurrentProfile==this._baseProfileName)
        {
            this.cbProfiles.Text=this.GetBaseProfileName();
        }
        else
        {
            this.cbProfiles.SelectedIndex=idx;
        }

        this.RefreshCurrentProfile();
    }

    void importAllProfilesNVIDIATextFormatToolStripMenuItem_Click(object sender,System.EventArgs e)
    {
        var openDialog=new System.Windows.Forms.OpenFileDialog();
        openDialog.DefaultExt="*.txt";
        openDialog.Filter    ="Profiles (NVIDIA Text Format)|*.txt";
        if(openDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                this._import.ImportAllProfilesFromNvidiaTextFile(openDialog.FileName);
                System.Windows.Forms.MessageBox.Show("Profile(s) successfully imported!",
                    System.Windows.Forms.Application.ProductName,System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
                nspector.Common.DrsSessionScope.DestroyGlobalSession();
                this.RefreshAll();
            }
            catch(nspector.Common.NvapiException)
            {
                System.Windows.Forms.MessageBox.Show("Profile(s) could not imported!",
                    System.Windows.Forms.Application.ProductName,System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    void importProfilesToolStripMenuItem_Click(object sender,System.EventArgs e)
    {
        var openDialog=new System.Windows.Forms.OpenFileDialog();
        openDialog.DefaultExt="*.nip";
        openDialog.Filter    =System.Windows.Forms.Application.ProductName+" Profiles|*.nip";
        if(openDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
        {
            this.ImportProfiles(openDialog.FileName);
        }
    }

    void cbProfiles_TextChanged(object sender,System.EventArgs e)
    {
        if(this.cbProfiles.DroppedDown)
        {
            var txt=this.cbProfiles.Text;
            this.cbProfiles.DroppedDown=false;
            this.cbProfiles.Text       =txt;
            this.cbProfiles.Select(this.cbProfiles.Text.Length,0);
        }
    }


    public static void ShowImportDoneMessage(string importReport)
    {
        if(string.IsNullOrEmpty(importReport))
        {
            System.Windows.Forms.MessageBox.Show("Profile(s) successfully imported!",
                System.Windows.Forms.Application.ProductName,System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
        }
        else
        {
            System.Windows.Forms.MessageBox.Show("Some profile(s) could not imported!\r\n\r\n"+importReport,
                System.Windows.Forms.Application.ProductName,
                System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
        }
    }

    void ImportProfiles(string nipFileName)
    {
        var importReport=this._import.ImportProfiles(nipFileName);
        this.RefreshAll();
        frmDrvSettings.ShowImportDoneMessage(importReport);
    }

    void lvSettings_OnDropFilesNative(string[] files)
    {
        if(files.Length==1)
        {
            var fileInfo=new System.IO.FileInfo(files[0]);
            if(fileInfo.Extension.ToLower().Equals(".nip"))
            {
                this.ImportProfiles(fileInfo.FullName);
                return;
            }


            var profileName="";
            var exeFile    =nspector.Common.Helper.ShortcutResolver.ResolveExecuteable(files[0],out profileName);
            if(exeFile!="")
            {
                var profiles=this._scanner.FindProfilesUsingApplication(exeFile);
                if(profiles!="")
                {
                    var profile=profiles.Split(';')[0];
                    var idx    =this.cbProfiles.Items.IndexOf(profile);
                    if(idx>-1)
                    {
                        this.cbProfiles.SelectedIndex=idx;
                    }
                }
                else
                {
                    var dr=System.Windows.Forms.MessageBox.Show(
                        "Would you like to create a new profile for this application?",
                        "Profile not found!",System.Windows.Forms.MessageBoxButtons.YesNo);
                    if(dr==System.Windows.Forms.DialogResult.Yes)
                    {
                        this.ShowCreateProfileDialog(profileName,exeFile);
                    }
                }
            }
        }
    }

    void lvSettings_DoubleClick(object sender,System.EventArgs e)
    {
        if(System.Diagnostics.Debugger.IsAttached&&this.lvSettings.SelectedItems!=null
            &&this.lvSettings.SelectedItems.Count                               ==1)
        {
            var settingId  =(uint)this.lvSettings.SelectedItems[0].Tag;
            var settingName=this.lvSettings.SelectedItems[0].Text;
            System.Windows.Forms.Clipboard.SetText(string.Format($"0x{settingId:X8} {settingName}"));
        }
    }

    void HandleScreenConstraints()
    {
        var workingArea=System.Windows.Forms.Screen.GetWorkingArea(this);

        if(this.Left<workingArea.X)
        {
            this.Left=workingArea.X;
        }

        if(this.Top<workingArea.Y)
        {
            this.Top=workingArea.Y;
        }

        if(this.Left+this.Width>workingArea.X+workingArea.Width)
        {
            this.Left=workingArea.X+workingArea.Width-this.Width;
        }

        if(this.Top+this.Height>workingArea.Y+workingArea.Height)
        {
            this.Top=workingArea.Y+workingArea.Height-this.Height;
        }
    }

    void SaveSettings()
    {
        var settings=nspector.Common.Helper.UserSettings.LoadSettings();

        if(this.WindowState==System.Windows.Forms.FormWindowState.Normal)
        {
            settings.WindowTop   =this.Top;
            settings.WindowLeft  =this.Left;
            settings.WindowHeight=this.Height;
            settings.WindowWidth =this.Width;
        }
        else
        {
            settings.WindowTop   =this.RestoreBounds.Top;
            settings.WindowLeft  =this.RestoreBounds.Left;
            settings.WindowHeight=this.RestoreBounds.Height;
            settings.WindowWidth =this.RestoreBounds.Width;
        }

        settings.WindowState                   =this.WindowState;
        settings.ShowCustomizedSettingNamesOnly=this.tscbShowCustomSettingNamesOnly.Checked;
        settings.ShowScannedUnknownSettings    =this.tscbShowScannedUnknownSettings.Checked;
        settings.SaveSettings();
    }

    void LoadSettings()
    {
        var settings=nspector.Common.Helper.UserSettings.LoadSettings();
        this.SetBounds(settings.WindowLeft,settings.WindowTop,settings.WindowWidth,settings.WindowHeight);
        this.WindowState=settings.WindowState!=System.Windows.Forms.FormWindowState.Minimized?settings.WindowState
            :System.Windows.Forms.FormWindowState.Normal;
        this.HandleScreenConstraints();
        this.tscbShowCustomSettingNamesOnly.Checked=settings.ShowCustomizedSettingNamesOnly;
        this.tscbShowScannedUnknownSettings.Checked=!this._skipScan&&settings.ShowScannedUnknownSettings;
    }

    void frmDrvSettings_FormClosed(object sender,System.Windows.Forms.FormClosedEventArgs e)
    {
        this._scannerCancelationTokenSource?.Cancel();
        this.SaveSettings();
    }

    void lvSettings_KeyDown(object sender,System.Windows.Forms.KeyEventArgs e)
    {
        if(e.Control&&e.KeyCode==System.Windows.Forms.Keys.C)
        {
            this.CopyModifiedSettingsToClipBoard();
        }
    }

    void CopyModifiedSettingsToClipBoard()
    {
        var sbSettings=new System.Text.StringBuilder();
        sbSettings.AppendFormat("{0,-40} {1}\r\n","### NVIDIA Profile Inspector ###",this._CurrentProfile);

        foreach(System.Windows.Forms.ListViewGroup group in this.lvSettings.Groups)
        {
            var groupTitleAdded=false;
            foreach(System.Windows.Forms.ListViewItem item in group.Items)
            {
                if(item.ImageIndex!=0)
                {
                    continue;
                }

                if(!groupTitleAdded)
                {
                    sbSettings.AppendFormat("\r\n[{0}]\r\n",group.Header);
                    groupTitleAdded=true;
                }

                sbSettings.AppendFormat("{0,-40} {1}\r\n",item.Text,item.SubItems[1].Text);
            }
        }

        System.Windows.Forms.Clipboard.SetText(sbSettings.ToString());
    }
}