using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using nspector.Common;
using nspector.Common.Helper;
using nspector.Native.NVAPI2;
using nspector.Native.WINAPI;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace nspector
{

    internal partial class frmDrvSettings : Form
    {
        readonly DrsSettingsMetaService _meta = DrsServiceLocator.MetaService;
        readonly DrsSettingsService _drs = DrsServiceLocator.SettingService;
        readonly DrsScannerService _scanner = DrsServiceLocator.ScannerService;
        readonly DrsImportService _import = DrsServiceLocator.ImportService;

        private List<SettingItem> _currentProfileSettingItems = new List<SettingItem>();
        private bool _alreadyScannedForPredefinedSettings = false;
        private IntPtr _taskbarParent = IntPtr.Zero;
        private bool _showCustomSettingsOnly = false;
        private bool _activated = false;
        private bool _isStartup = true;
        private bool _skipScan = false;
        
        private string _baseProfileName = "";
        private bool _isWin7TaskBar = false;
        private int _lastComboRowIndex = -1;
        private ITaskbarList3 _taskbarList;

        public string _CurrentProfile = "";

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case MessageHelper.WM_COPYDATA:
                    MessageHelper.COPYDATASTRUCT copyDataStruct = new MessageHelper.COPYDATASTRUCT();
                    Type copyDataType = copyDataStruct.GetType();
                    copyDataStruct = (MessageHelper.COPYDATASTRUCT)m.GetLParam(copyDataType);
                    if (copyDataStruct.lpData.Equals("ProfilesImported"))
                    {
                        RefreshAll();
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private ListViewGroup FindOrCreateGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                groupName = "Unknown";

            foreach (ListViewGroup group in lvSettings.Groups)
                if (group.Header == groupName)
                    return group;

            var newGroup = new ListViewGroup(groupName);
            lvSettings.Groups.Insert(0, newGroup);

            return newGroup;
        }

        private ListViewItem CreateListViewItem(SettingItem setting)
        {
            var group = FindOrCreateGroup(setting.GroupName);

            var item = new ListViewItem(setting.SettingText);
            item.Tag = setting.SettingId;
            item.Group = group;
            item.SubItems.Add(setting.ValueText);
            item.SubItems.Add(setting.ValueRaw);

            switch (setting.State)
            {
                default:
                    item.ImageIndex = 1;
                    item.ForeColor = SystemColors.GrayText;
                    break;

                case SettingState.NvidiaSetting:
                    item.ImageIndex = 2;
                    break;

                case SettingState.GlobalSetting:
                    item.ImageIndex = 3;
                    item.ForeColor = SystemColors.GrayText;
                    break;

                case SettingState.UserdefinedSetting:
                    item.ImageIndex = 0;
                    break;


            }

            return item;
        }

        private void RefreshApplicationsCombosAndText(List<string> applications)
        {
            lblApplications.Text = "";
            tssbRemoveApplication.DropDownItems.Clear();

            lblApplications.Text = " " + string.Join(", ", applications);
            foreach (var app in applications)
            {
                tssbRemoveApplication.DropDownItems.Add(app, Properties.Resources.ieframe_1_18212);
            }
            tssbRemoveApplication.Enabled = (tssbRemoveApplication.DropDownItems.Count > 0);
        }

        private SettingViewMode GetSettingViewMode()
        {
            if (tscbShowCustomSettingNamesOnly.Checked)
                return SettingViewMode.CustomSettingsOnly;
            else if (tscbShowScannedUnknownSettings.Checked)
                return SettingViewMode.IncludeScannedSetttings;
            else
                return SettingViewMode.Normal;
        }

        private void RefreshCurrentProfile()
        {
            string lvSelection = "";
            if (lvSettings.SelectedItems.Count > 0)
                lvSelection = lvSettings.SelectedItems[0].Text;

            lvSettings.BeginUpdate();
            try
            {
                lvSettings.Items.Clear();
                lvSettings.Groups.Clear();
                var applications = new List<string>();

                _currentProfileSettingItems = _drs.GetSettingsForProfile(_CurrentProfile, GetSettingViewMode(), ref applications);
                RefreshApplicationsCombosAndText(applications);

                foreach (var settingItem in _currentProfileSettingItems)
                {
                    lvSettings.Items.Add(CreateListViewItem(settingItem));
                }

                btnResetValue.Enabled = false;

                try
                {
                    lvSettings.RemoveEmbeddedControl(cbValues);
                    lvSettings.RemoveEmbeddedControl(btnResetValue);
                }
                catch { }
            }
            finally
            {
                lvSettings.EndUpdate();
                ((ListViewGroupSorter)lvSettings).SortGroups(true);

                GC.Collect();
                for (int i = 0; i < lvSettings.Items.Count; i++)
                {
                    if (lvSettings.Items[i].Text == lvSelection)
                    {
                        lvSettings.Items[i].Selected = true;
                        lvSettings.Items[i].EnsureVisible();

                        if (!cbProfiles.Focused)
                        {
                            lvSettings.Select();
                            cbValues.Text = lvSettings.Items[i].SubItems[1].Text;
                        }
                        break;
                    }
                }
            }
        }

        private void RefreshProfilesCombo()
        {
            cbProfiles.Items.Clear();

            var profileNames = _drs.GetProfileNames(ref _baseProfileName);
            cbProfiles.Items.AddRange(profileNames.Cast<object>().ToArray());

            cbProfiles.Sorted = true;
        }

        private void MoveComboToItemAndFill()
        {
            if (lvSettings.SelectedItems.Count > 0)
            {
                if (!cbValues.ContainsFocus && (_lastComboRowIndex != lvSettings.SelectedItems[0].Index))
                {
                    btnResetValue.Enabled = true;

                    cbValues.BeginUpdate();

                    cbValues.Items.Clear();
                    cbValues.Tag = lvSettings.SelectedItems[0].Tag;
                    uint settingid = (uint)lvSettings.SelectedItems[0].Tag;

                    var settingMeta = _meta.GetSettingMeta(settingid);
                    if (settingMeta != null)
                    {
                        if (settingMeta.SettingType == Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE && settingMeta.DwordValues != null)
                        {
                            var valueNames = settingMeta.DwordValues.Select(x => x.ValueName).ToList();
                            foreach (string v in valueNames)
                            {
                                var itm = "";
                                if (v.Length > 4000)
                                    itm = v.Substring(0, 4000) + " ...";
                                else
                                    itm = v;

                                cbValues.Items.Add(itm);
                                
                            }
                        }

                        if (settingMeta.SettingType == Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE && settingMeta.StringValues != null)
                        {
                            var valueNames = settingMeta.StringValues.Select(x => x.ValueName).ToList();
                            foreach (string v in valueNames)
                                cbValues.Items.Add(v);
                        }

                        var scannedCount = settingMeta?.DwordValues?
                            .Where(x => x.ValueSource == Common.Meta.SettingMetaSource.ScannedSettings)
                            .Count();

                        tsbBitValueEditor.Enabled = scannedCount > 0;

                    }

                    if (cbValues.Items.Count < 1)
                    {
                        cbValues.Items.Add("");
                        cbValues.Items.RemoveAt(0);
                    }


                    cbValues.Text = lvSettings.SelectedItems[0].SubItems[1].Text;
                    cbValues.EndUpdate();

                    lvSettings.AddEmbeddedControl(cbValues, 1, lvSettings.SelectedItems[0].Index);

                    if (lvSettings.SelectedItems[0].ImageIndex == 0)
                        lvSettings.AddEmbeddedControl(btnResetValue, 2, lvSettings.SelectedItems[0].Index, DockStyle.Right);
                    _lastComboRowIndex = lvSettings.SelectedItems[0].Index;
                    cbValues.Visible = true;


                    
                }
            }
            else
            {
                _lastComboRowIndex = -1;

                if (!cbValues.ContainsFocus)
                {
                    try
                    {
                        lvSettings.RemoveEmbeddedControl(cbValues);
                        lvSettings.RemoveEmbeddedControl(btnResetValue);
                    }
                    catch { }

                    btnResetValue.Enabled = false;
                    cbValues.Visible = false;

                    tsbBitValueEditor.Enabled = false;
                }
            }
        }

        private int GetListViewIndexOfSetting(uint settingId)
        {
            int idx = 0;
            foreach (ListViewItem lvi in lvSettings.Items)
            {
                if (settingId == (uint)lvi.Tag)
                {
                    return idx;
                }
                idx++;
            }
            return -1;
        }

        private void UpdateItemByComboValue()
        {
            var settingId = (uint)cbValues.Tag;
            var activeImages = new[] { 0, 2 };
            
            int idx = GetListViewIndexOfSetting(settingId);
            if (idx != -1)
            {
                var lvItem = lvSettings.Items[idx];

                var settingMeta = _meta.GetSettingMeta(settingId);

                var currentProfileItem = _currentProfileSettingItems
                    .First(x => x.SettingId.Equals(settingId));

                var cbValueText = cbValues.Text.Trim();

                var valueHasChanged = currentProfileItem.ValueText != cbValueText;
                if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                {
                    var stringBehind = DrsUtil.ParseStringSettingValue(settingMeta, cbValueText);
                    valueHasChanged = currentProfileItem.ValueRaw != stringBehind;
                }

                if (valueHasChanged || activeImages.Contains(lvItem.ImageIndex))
                {
                    lvItem.ForeColor = SystemColors.ControlText;
                }
                else
                {
                    lvItem.ForeColor = SystemColors.GrayText;
                }

                if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
                {
                    lvItem.SubItems[2].Text = DrsUtil.GetDwordString(DrsUtil.ParseDwordSettingValue(settingMeta, cbValueText));
                    lvItem.SubItems[1].Text = cbValueText;
                }
                else
                {
                    lvItem.SubItems[2].Text = DrsUtil.ParseStringSettingValue(settingMeta, cbValueText); // DrsUtil.StringValueRaw;
                    lvItem.SubItems[1].Text = cbValueText;
                }

            }
        }

        private void StoreChangesOfProfileToDriver()
        {

            var settingsToStore = new List<KeyValuePair<uint, string>>();

            foreach (ListViewItem lvi in lvSettings.Items)
            {
                var currentProfileItem = _currentProfileSettingItems
                    .First(x => x.SettingId.Equals((uint)lvi.Tag));

                var listValueX = lvi.SubItems[1].Text;

                if (currentProfileItem.ValueText != listValueX)
                {
                    settingsToStore.Add(new KeyValuePair<uint, string>((uint)lvi.Tag, listValueX));
                }

            }

            if (settingsToStore.Count > 0)
            {
                _drs.StoreSettingsToProfile(_CurrentProfile, settingsToStore);
                AddToModifiedProfiles(_CurrentProfile);
            }

            RefreshCurrentProfile();
        }

        private void ResetCurrentProfile()
        {
            bool removeFromModified = false;
            _drs.ResetProfile(_CurrentProfile, out removeFromModified);

            if (removeFromModified)
            {
                RemoveFromModifiedProfiles(_CurrentProfile);
            }
            RefreshCurrentProfile();
        }

        private void ResetSelectedValue()
        {
            if (lvSettings.SelectedItems != null && lvSettings.SelectedItems.Count > 0)
            {
                var settingId = (uint)lvSettings.SelectedItems[0].Tag;

                bool removeFromModified;
                _drs.ResetValue(_CurrentProfile, settingId, out removeFromModified);

                if (removeFromModified)
                    RemoveFromModifiedProfiles(_CurrentProfile);

                RefreshCurrentProfile();
            }
        }

        private void InitTaskbarList()
        {
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                try
                {
                    _taskbarList = (ITaskbarList3)new TaskbarList();
                    _taskbarList.HrInit();
                    _taskbarParent = this.Handle;
                    _isWin7TaskBar = true;
                }
                catch
                {
                    _taskbarList = null;
                    _taskbarParent = IntPtr.Zero;
                    _isWin7TaskBar = false;
                }
            }
        }

        private void SetTaskbarIcon()
        {
            if (_taskbarList != null && _isWin7TaskBar && AdminHelper.IsAdmin)
            {
                try
                {
                    _taskbarList.SetOverlayIcon(_taskbarParent, Properties.Resources.shield16.Handle, "Elevated");
                }
                catch { }
            }
        }

        private void SetTitleVersion()
        {
            var numberFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var titleText = string.Format("{8} {0}.{1}{5}{2}{5}{4} - {6} Profile Settings {3}- {7}",
                version.Major, version.Minor, version.Build, AdminHelper.IsAdmin ? "(Elevated) " : "",
                (version.Revision > 0 ? version.Revision.ToString() : ""),
                (version.Revision > 0 ? "." : ""),
                (_drs.DriverVersion > 0) ? "GeForce " + _drs.DriverVersion.ToString("#.00", numberFormat) + " -" : "Driver",
                fileVersionInfo.LegalCopyright,
                Application.ProductName
                );

            Text = titleText;
        }

        private static void InitMessageFilter(IntPtr handle)
        {
            if ((Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1))
            {
                DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle, DragAcceptNativeHelper.WM_DROPFILES, DragAcceptNativeHelper.MSGFLT_ALLOW, IntPtr.Zero);
                DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle, DragAcceptNativeHelper.WM_COPYDATA, DragAcceptNativeHelper.MSGFLT_ALLOW, IntPtr.Zero);
                DragAcceptNativeHelper.ChangeWindowMessageFilterEx(handle, DragAcceptNativeHelper.WM_COPYGLOBALDATA, DragAcceptNativeHelper.MSGFLT_ALLOW, IntPtr.Zero);
            }
            else if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 0)
            {
                DragAcceptNativeHelper.ChangeWindowMessageFilter(DragAcceptNativeHelper.WM_DROPFILES, DragAcceptNativeHelper.MSGFLT_ADD);
                DragAcceptNativeHelper.ChangeWindowMessageFilter(DragAcceptNativeHelper.WM_COPYDATA, DragAcceptNativeHelper.MSGFLT_ADD);
                DragAcceptNativeHelper.ChangeWindowMessageFilter(DragAcceptNativeHelper.WM_COPYGLOBALDATA, DragAcceptNativeHelper.MSGFLT_ADD);
            }
        }
        
        internal frmDrvSettings() : this(false, false) { }
        internal frmDrvSettings(bool showCsnOnly, bool skipScan)
        {
            _skipScan = skipScan;
            InitializeComponent();
            InitTaskbarList();
            InitScannerEvents();
            SetupDropFilesNative();
            SetupToolbar();
            SetupDpiAdjustments();

            _showCustomSettingsOnly = showCsnOnly;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void SetupDpiAdjustments()
        {
            chSettingID.Width = lblWidth330.Width;
            chSettingValueHex.Width = lblWidth96.Width;
        }

        private void SetupToolbar()
        {
            tsMain.Renderer = new NoBorderRenderer();
            tsMain.ImageScalingSize = new Size(lblWidth16.Width, lblWidth16.Width);
        }

        private void SetupDropFilesNative()
        {
            lvSettings.OnDropFilesNative += new DropFilesNativeHandler(lvSettings_OnDropFilesNative);
            DragAcceptNativeHelper.DragAcceptFiles(this.Handle, true);
            DragAcceptNativeHelper.DragAcceptFiles(lvSettings.Handle, true);
            InitMessageFilter(lvSettings.Handle);
        }

        private void SetupLayout()
        {
            if (Screen.GetWorkingArea(this).Height < Height + 10)
            {
                Height = Screen.GetWorkingArea(this).Height - 20;
            }
        }
        
        private void RefreshModifiesProfilesDropDown()
        {
            tsbModifiedProfiles.DropDownItems.Clear();
            _scanner.ModifiedProfiles.Sort();
            foreach (string modProfile in _scanner.ModifiedProfiles)
                if (modProfile != _baseProfileName)
                    tsbModifiedProfiles.DropDownItems.Add(modProfile);

            if (tsbModifiedProfiles.DropDownItems.Count > 0)
                tsbModifiedProfiles.Enabled = true;
        }

        private void frmDrvSettings_Load(object sender, EventArgs e)
        {
            SetupLayout();
            SetTitleVersion();

            RefreshProfilesCombo();
            cbProfiles.Text = GetBaseProfileName();

            tsbBitValueEditor.Enabled = false;
            tsbDeleteProfile.Enabled = false;
            tsbAddApplication.Enabled = false;
            tssbRemoveApplication.Enabled = false;

            InitResetValueTooltip();
        }

        private void InitResetValueTooltip()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnResetValue, "Restore this value to NVIDIA defaults.");
        }

        private void lvSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            MoveComboToItemAndFill();
        }

        private void cbValues_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateItemByComboValue();
        }

        private void cbValues_Leave(object sender, EventArgs e)
        {
            UpdateItemByComboValue();
        }

        private void btnResetValue_Click(object sender, EventArgs e)
        {
            ResetSelectedValue();
        }

        private void ChangeCurrentProfile(string profileName)
        {
            if (profileName == GetBaseProfileName() || profileName == _baseProfileName)
            {
                _CurrentProfile = _baseProfileName;
                cbProfiles.Text = GetBaseProfileName();
                tsbDeleteProfile.Enabled = false;
                tsbAddApplication.Enabled = false;
                tssbRemoveApplication.Enabled = false;
            }
            else
            {
                _CurrentProfile = cbProfiles.Text;
                tsbDeleteProfile.Enabled = true;
                tsbAddApplication.Enabled = true;
                tssbRemoveApplication.Enabled = true;
            }


            RefreshCurrentProfile();
        }

        private void cbProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProfiles.SelectedIndex > -1)
            {
                ChangeCurrentProfile(cbProfiles.Text);
            }
        }

        private void SetTaskbarProgress(int progress)
        {
            if (_isWin7TaskBar)
            {
                try
                {
                    if (progress == 0)
                        _taskbarList.SetProgressState(_taskbarParent, TBPFLAG.TBPF_NOPROGRESS);
                    else
                    {
                        _taskbarList.SetProgressState(_taskbarParent, TBPFLAG.TBPF_NORMAL);
                        _taskbarList.SetProgressValue(_taskbarParent, (ulong)progress, 100);
                    }
                }
                catch { }
            }
        }
        
        private void AddToModifiedProfiles(string profileName)
        {
            if (!_scanner.ModifiedProfiles.Contains(profileName) && profileName != _baseProfileName)
            {
                _scanner.ModifiedProfiles.Add(profileName);
                RefreshModifiesProfilesDropDown();
            }
        }

        private void RemoveFromModifiedProfiles(string profileName)
        {
            if (_scanner.ModifiedProfiles.Contains(profileName))
            {
                _scanner.ModifiedProfiles.Remove(profileName);
                RefreshModifiesProfilesDropDown();
            }
        }

        private void InvokeUi(Control invokeControl, Action action)
        {
            MethodInvoker mi = () => action();

            if (invokeControl.InvokeRequired)
                invokeControl.BeginInvoke(mi);
            else
                mi.Invoke();
        }
        
        private void frmDrvSettings_OnModifiedScanDoneAndShowExport()
        {
            InvokeUi(this, () =>
            {

                pbMain.Value = 0;
                pbMain.Enabled = false;
                SetTaskbarProgress(0);

                if (_scanner.ModifiedProfiles.Count > 0)
                {
                    var frmExport = new frmExportProfiles();
                    frmExport.ShowDialog(this);
                }
                else
                    MessageBox.Show("No user modified profiles found! Nothing to export.", "Userprofile Search", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshModifiesProfilesDropDown();

            });
        }

        private void frmDrvSettings_OnPredefinedScanDoneAndStartModifiedProfileScan()
        {

            InvokeUi(this, () =>
            {
                pbMain.Value = 0;
                pbMain.Enabled = false;
                SetTaskbarProgress(0);
                
                tscbShowScannedUnknownSettings.Enabled = true;
            });

            StartModifiedProfilesScan(false);
        }
        
        private void frmDrvSettings_OnScanDoneDoNothing()
        {
            _meta.ResetMetaCache();

            InvokeUi(this, () =>
            {
                pbMain.Value = 0;
                pbMain.Enabled = false;
                SetTaskbarProgress(0);
                RefreshCurrentProfile();
                RefreshModifiesProfilesDropDown();
            });
        }

        private void frmDrvSettings_OnSettingScanProgress(int percent)
        {
            InvokeUi(this, () =>
            {
                pbMain.Value = percent;
                SetTaskbarProgress(percent);
            });

        }
        
        private void InitScannerEvents()
        {
            _scanner.OnSettingScanProgress += new Common.SettingScanProgressEvent(frmDrvSettings_OnSettingScanProgress);
            _scanner.OnPredefinedSettingsScanDone += new Common.SettingScanDoneEvent(frmDrvSettings_OnScanDoneDoNothing);
            _scanner.OnModifiedProfilesScanDone += new Common.SettingScanDoneEvent(frmDrvSettings_OnScanDoneDoNothing);
        }

        private void StartModifiedProfilesScan(bool showProfilesDialog)
        {
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;
            
            _scanner.OnModifiedProfilesScanDone -= new Common.SettingScanDoneEvent(frmDrvSettings_OnScanDoneDoNothing);
            _scanner.OnModifiedProfilesScanDone -= new Common.SettingScanDoneEvent(frmDrvSettings_OnModifiedScanDoneAndShowExport);

            if (showProfilesDialog)
                _scanner.OnModifiedProfilesScanDone += new Common.SettingScanDoneEvent(frmDrvSettings_OnModifiedScanDoneAndShowExport);
            else
                _scanner.OnModifiedProfilesScanDone += new Common.SettingScanDoneEvent(frmDrvSettings_OnScanDoneDoNothing);

            _scanner.StartScanForModifiedProfilesAsync();
        }

        private void StartPredefinedSettingsScan(bool startModifiedProfileScan)
        {
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;

            _scanner.OnPredefinedSettingsScanDone -= new Common.SettingScanDoneEvent(frmDrvSettings_OnScanDoneDoNothing);
            _scanner.OnPredefinedSettingsScanDone -= new Common.SettingScanDoneEvent(frmDrvSettings_OnPredefinedScanDoneAndStartModifiedProfileScan);

            if (startModifiedProfileScan)
                _scanner.OnPredefinedSettingsScanDone += new Common.SettingScanDoneEvent(frmDrvSettings_OnPredefinedScanDoneAndStartModifiedProfileScan);
            else
                _scanner.OnPredefinedSettingsScanDone += new Common.SettingScanDoneEvent(frmDrvSettings_OnScanDoneDoNothing);

            _alreadyScannedForPredefinedSettings = true;

            _scanner.StartScanForPredefinedSettingsAsync();
        }
      
        private void ScanProfilesSilent(bool scanPredefined, bool showProfileDialog)
        {
            tsbModifiedProfiles.Enabled = false;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;

            if (_skipScan)
                return;

            if (scanPredefined && !_alreadyScannedForPredefinedSettings)
                StartPredefinedSettingsScan(true);
            else
                StartModifiedProfilesScan(showProfileDialog);
        }

        private void cbCustomSettingsOnly_CheckedChanged(object sender, EventArgs e)
        {
            RefreshCurrentProfile();
        }

        internal void SetSelectedDwordValue(uint dwordValue)
        {
            if (lvSettings.SelectedItems != null & lvSettings.SelectedItems.Count > 0)
            {
                cbValues.Text = DrsUtil.GetDwordString(dwordValue); ;
                UpdateItemByComboValue();
            }
        }

        private void tsbRestoreProfile_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                if (MessageBox.Show(this,
                    "Restore all profiles to NVIDIA driver defaults?",
                    "Restore all profiles",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _drs.ResetAllProfilesInternal();

                    RefreshProfilesCombo();
                    RefreshCurrentProfile();
                    ScanProfilesSilent(true, false);
                    cbProfiles.Text = GetBaseProfileName();
                }
            }
            else
                ResetCurrentProfile();
        }

        private void tsbRefreshProfile_Click(object sender, EventArgs e)
        {
            DrsServiceLocator.ReCreateSession();
            RefreshAll();
        }

        private void tsbApplyProfile_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateItemByComboValue();
            }
            catch { }

            StoreChangesOfProfileToDriver();
        }

        private void tsbBitValueEditor_Click(object sender, EventArgs e)
        {
            if (lvSettings.SelectedItems != null & lvSettings.SelectedItems.Count > 0)
            {
                var frmBits = new frmBitEditor();
                frmBits.ShowDialog(this,
                    (uint)lvSettings.SelectedItems[0].Tag,
                    uint.Parse(lvSettings.SelectedItems[0].SubItems[2].Text.Substring(2), NumberStyles.AllowHexSpecifier),
                    lvSettings.SelectedItems[0].Text);
            }
        }

        private void tscbShowScannedUnknownSettings_Click(object sender, EventArgs e)
        {
            RefreshCurrentProfile();
        }

        private void lvSettings_Resize(object sender, EventArgs e)
        {
            ResizeColumn();
        }

        private void ResizeColumn()
        {
            lvSettings.Columns[1].Width = lvSettings.Width - (lvSettings.Columns[0].Width + lvSettings.Columns[2].Width + lblWidth30.Width);
        }

        private void lvSettings_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex != 1)
            {

                if (e.ColumnIndex == 0 && e.NewWidth < 260)
                {
                    e.NewWidth = 260;
                    e.Cancel = true;
                }
                else if (e.ColumnIndex == 2 && e.NewWidth < 96)
                {
                    e.Cancel = true;
                    e.NewWidth = 96;
                }

                ResizeColumn();
            }
        }

        private void frmDrvSettings_Shown(object sender, EventArgs e)
        {
            if (_isStartup)
            {
                new Thread(SetTaskbarIcon).Start();
                ScanProfilesSilent(true, false);

                if (WindowState != FormWindowState.Maximized)
                {
                    new MessageHelper().bringAppToFront((int)this.Handle);
                }
                _isStartup = false;
            }
        }

        private void tsbDeleteProfile_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                if (MessageBox.Show(this, "Really delete all profiles?", "Delete all profiles", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    _drs.DeleteAllProfilesHard();
                }
            }
            else if (MessageBox.Show(this, "Really delete this profile?\r\n\r\nNote: NVIDIA predefined profiles can not be restored until next driver installation!", "Delete Profile", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                if (_drs.DriverVersion > 280 && _drs.DriverVersion < 310)
                    // hack for driverbug
                    _drs.DeleteProfileHard(_CurrentProfile);
                else
                    _drs.DeleteProfile(_CurrentProfile);

                RemoveFromModifiedProfiles(_CurrentProfile);
                MessageBox.Show(this, string.Format("Profile '{0}' has been deleted.", _CurrentProfile), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshProfilesCombo();
                ChangeCurrentProfile(_baseProfileName);
            }
        }

        private void tsbAddApplication_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.DefaultExt = "*.exe";
            openDialog.Filter = "Application EXE Name|*.exe|Application Absolute Path|*.exe";

            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string applicationName = new FileInfo(openDialog.FileName).Name;
                if (openDialog.FilterIndex == 2)
                    applicationName = openDialog.FileName;

                try
                {
                    _drs.AddApplication(_CurrentProfile, applicationName);
                }
                catch (NvapiException ex)
                {
                    if (ex.Status == Native.NVAPI2.NvAPI_Status.NVAPI_EXECUTABLE_ALREADY_IN_USE)
                    {
                        if (lblApplications.Text.ToUpper().IndexOf(" " + applicationName.ToUpper() + ",") != -1)
                            MessageBox.Show("This application executable is already assigned to this profile!",
                                "Error adding Application", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                        {
                            string profileNames = _scanner.FindProfilesUsingApplication(applicationName);
                            if (profileNames == "")
                                MessageBox.Show("This application executable is already assigned to another profile!",
                                    "Error adding Application", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                                MessageBox.Show(
                                    "This application executable is already assigned to the following profiles: " +
                                    profileNames, "Error adding Application", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                        throw;
                }
            }
            RefreshCurrentProfile();
        }

        private void tssbRemoveApplication_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
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
            _drs.DeleteApplication(_CurrentProfile, e.ClickedItem.Text);
            RefreshCurrentProfile();
        }

        private void tsbCreateProfile_Click(object sender, EventArgs e)
        {
            ShowCreateProfileDialog("");
        }

        private void ShowCreateProfileDialog(string nameProposal, string applicationName = null)
        {
            var ignoreList = cbProfiles.Items.Cast<string>().ToList();
            string result = nameProposal;

            if (InputBox.Show("Create Profile", "Please enter profile name:", ref result, ignoreList, "", 2048) == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    _drs.CreateProfile(result, applicationName);
                    RefreshProfilesCombo();
                    cbProfiles.SelectedIndex = cbProfiles.Items.IndexOf(result);
                    AddToModifiedProfiles(result);
                }
                catch (NvapiException ex)
                {
                    //TODO: could not create profile
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void tsbExportProfiles_Click(object sender, EventArgs e)
        {
            tsbExportProfiles.ShowDropDown();
        }

        private void tsbImportProfiles_Click(object sender, EventArgs e)
        {
            tsbImportProfiles.ShowDropDown();
        }

        private void exportUserdefinedProfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScanProfilesSilent(false, true);
        }

        private void exportCurrentProfileOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "*.nip";
            saveDialog.Filter = Application.ProductName + " Profiles|*.nip";
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var profiles = new[] { _CurrentProfile }.ToList();
                _import.ExportProfiles(profiles, saveDialog.FileName, false);
            }
        }

        private void tssbRemoveApplication_Click(object sender, EventArgs e)
        {
            if (tssbRemoveApplication.DropDown.Items.Count > 0)
                tssbRemoveApplication.ShowDropDown();
        }

        private void tsbModifiedProfiles_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            cbProfiles.SelectedIndex = cbProfiles.FindStringExact(e.ClickedItem.Text);
        }

        private string GetBaseProfileName()
        {
            return string.Format("_GLOBAL_DRIVER_PROFILE ({0})", _baseProfileName);
        }

        private void tsbModifiedProfiles_ButtonClick(object sender, EventArgs e)
        {
            ChangeCurrentProfile(GetBaseProfileName());
        }

        private void frmDrvSettings_Activated(object sender, EventArgs e)
        {
            if (!_activated)
                _activated = true;
        }

        private void exportAllProfilesNVIDIATextFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "*.txt";
            saveDialog.Filter = "Profiles (NVIDIA Text Format)|*.txt";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                _import.ExportAllProfilesToNvidiaTextFile(saveDialog.FileName);
            }
        }

        private void RefreshAll()
        {
            RefreshProfilesCombo();
            ScanProfilesSilent(true, false);

            int idx = cbProfiles.Items.IndexOf(_CurrentProfile);
            if (idx == -1 || _CurrentProfile == _baseProfileName)
                cbProfiles.Text = GetBaseProfileName();
            else
                cbProfiles.SelectedIndex = idx;

            RefreshCurrentProfile();
        }

        private void importAllProfilesNVIDIATextFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.DefaultExt = "*.txt";
            openDialog.Filter = "Profiles (NVIDIA Text Format)|*.txt";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _import.ImportAllProfilesFromNvidiaTextFile(openDialog.FileName);
                    MessageBox.Show("Profile(s) successfully imported!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshAll();
                }
                catch (NvapiException)
                {
                    MessageBox.Show("Profile(s) could not imported!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void importProfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.DefaultExt = "*.nip";
            openDialog.Filter = Application.ProductName + " Profiles|*.nip";
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _import.ImportProfiles(openDialog.FileName);
                RefreshAll();
                MessageBox.Show("Profile(s) successfully imported!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cbProfiles_TextChanged(object sender, EventArgs e)
        {
            if (cbProfiles.DroppedDown)
            {
                string txt = cbProfiles.Text;
                cbProfiles.DroppedDown = false;
                cbProfiles.Text = txt;
                cbProfiles.Select(cbProfiles.Text.Length, 0);
            }
        }

        private string ResolveExecuteable(string filename, out string profileName)
        {
            var fileInfo = new FileInfo(filename);
            profileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            if (fileInfo.Extension.ToLower().Equals(".lnk"))
            {
                try
                {
                    var shellLink = new ShellLink(filename);
                    var targetInfo = new FileInfo(shellLink.Target);
                    if (targetInfo.Extension.ToLower().Equals(".exe"))
                        return targetInfo.Name;

                    return "";
                }
                catch
                {
                    return "";
                }
            }

            if (fileInfo.Extension.ToLower().Equals(".exe"))
                return fileInfo.Name;

            return "";
        }

        private void lvSettings_OnDropFilesNative(string[] files)
        {
            if (files.Length == 1)
            {
                var fileInfo = new FileInfo(files[0]);
                if (fileInfo.Extension.ToLower().Equals(".nip"))
                {
                    _import.ImportProfiles(fileInfo.FullName);
                    RefreshAll();
                    MessageBox.Show("Profile(s) successfully imported!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


                var profileName = "";
                var exeFile = ResolveExecuteable(files[0], out profileName);
                if (exeFile != "")
                {
                    var profiles = _scanner.FindProfilesUsingApplication(exeFile);
                    if (profiles != "")
                    {
                        var profile = profiles.Split(';')[0];
                        var idx = cbProfiles.Items.IndexOf(profile);
                        if (idx > -1)
                        {
                            cbProfiles.SelectedIndex = idx;
                        }
                    }
                    else
                    {
                        var dr = MessageBox.Show("Would you like to create a new profile for this application?", "Profile not found!", MessageBoxButtons.YesNo);
                        if (dr == DialogResult.Yes)
                        {
                            ShowCreateProfileDialog(profileName, exeFile);
                        }

                    }
                }
            }
        }

        private void lvSettings_DoubleClick(object sender, EventArgs e)
        {
            if (Debugger.IsAttached && lvSettings.SelectedItems != null && lvSettings.SelectedItems.Count == 1)
            {
                var settingId = ((uint)lvSettings.SelectedItems[0].Tag);
                var settingName = lvSettings.SelectedItems[0].Text;
                Clipboard.SetText(string.Format($"0x{settingId:X8} {settingName}"));
            }
        }
    }
}





