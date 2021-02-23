﻿using nspector.Common;
using nspector.Common.Helper;
using nspector.Native.NVAPI2;
using nspector.Native.WINAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nspector
{

    internal partial class frmDrvSettings : Form
    {
        private readonly DrsSettingsMetaService _meta = DrsServiceLocator.MetaService;
        private readonly DrsSettingsService _drs = DrsServiceLocator.SettingService;
        private readonly DrsScannerService _scanner = DrsServiceLocator.ScannerService;
        private readonly DrsImportService _import = DrsServiceLocator.ImportService;

        private List<SettingItem> _currentProfileSettingItems = new List<SettingItem>();
        private bool _alreadyScannedForPredefinedSettings = false;
        private IntPtr _taskbarParent = IntPtr.Zero;
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
                        DrsSessionScope.DestroyGlobalSession();
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
                    item.ForeColor = ColorHelper.GetInactiveFunctionText(); 
                    break;

                case SettingState.NvidiaSetting:
                    item.ImageIndex = 2;
                    item.ForeColor = ColorHelper.GetGlobalSettingState2Color();
                    break;

                case SettingState.GlobalSetting:
                    item.ImageIndex = 3;
                    item.ForeColor = ColorHelper.GetGlobalSettingState3Color();
                    break;

                case SettingState.UserdefinedSetting:
                    item.ImageIndex = 0;
                    item.ForeColor = ColorHelper.GetGlobalSettingState0Color();
                    break;


            }

            return item;
        }

        private void RefreshApplicationsCombosAndText(Dictionary<string,string> applications)
        {
            lblApplications.Text = "";
            tssbRemoveApplication.DropDownItems.Clear();

            lblApplications.Text = " " + string.Join(", ", applications.Select(x=>x.Value));
            foreach (var app in applications)
            {
                var item = tssbRemoveApplication.DropDownItems.Add(app.Value, Properties.Resources.ieframe_1_18212);
                item.Tag = app.Key;
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
                var applications = new Dictionary<string,string>();

                _currentProfileSettingItems = _drs.GetSettingsForProfile(_CurrentProfile, GetSettingViewMode(), ref applications);
                RefreshApplicationsCombosAndText(applications);

                foreach (var settingItem in _currentProfileSettingItems)
                {
                    var itm = lvSettings.Items.Add(CreateListViewItem(settingItem));
                    if (Debugger.IsAttached && !settingItem.IsApiExposed)
                    {
                        itm.ForeColor = ColorHelper.GetTextColor();
                    }
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

                    var settingMeta = _meta.GetSettingMeta(settingid, GetSettingViewMode());
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

                        if (settingMeta.SettingType == Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE && settingMeta.BinaryValues != null)
                        {
                            var valueNames = settingMeta.BinaryValues.Select(x => x.ValueName).ToList();
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

                var settingMeta = _meta.GetSettingMeta(settingId, GetSettingViewMode());

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
                    lvItem.ForeColor = ColorHelper.GetControlImageText();
                }
                else if(!ColorHelper.GetIgnoreChangeText())
                {
                    lvItem.ForeColor = ColorHelper.GetOnChangeText();
                }

                if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
                {
                    lvItem.SubItems[2].Text = DrsUtil.GetDwordString(DrsUtil.ParseDwordSettingValue(settingMeta, cbValueText));
                    lvItem.SubItems[1].Text = cbValueText;
                }
                else if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                {
                    lvItem.SubItems[2].Text = DrsUtil.ParseStringSettingValue(settingMeta, cbValueText); // DrsUtil.StringValueRaw;
                    lvItem.SubItems[1].Text = cbValueText;
                }
                else if (settingMeta.SettingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
                {
                    lvItem.SubItems[2].Text = DrsUtil.GetBinaryString(DrsUtil.ParseBinarySettingValue(settingMeta, cbValueText)); // DrsUtil.StringValueRaw;
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

                var itmEmpty = string.IsNullOrEmpty(listValueX);
                var curEmpty = string.IsNullOrEmpty(currentProfileItem.ValueText);

                if (currentProfileItem.ValueText != listValueX && !(itmEmpty && curEmpty))
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
            Text = $"{Application.ProductName} {version} - Geforce {_drs.DriverVersion.ToString("#.00", numberFormat)} - Profile Settings - {fileVersionInfo.LegalCopyright}";
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
            SetupDropFilesNative();
            SetupToolbar();
            SetupDpiAdjustments();

            tscbShowCustomSettingNamesOnly.Checked = showCsnOnly;
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
                {
                    var newItem = tsbModifiedProfiles.DropDownItems.Add(modProfile);
                    if (!_scanner.UserProfiles.Contains(modProfile))
                    {
                        newItem.Image = tsbRestoreProfile.Image;
                    }
                }

            if (tsbModifiedProfiles.DropDownItems.Count > 0)
                tsbModifiedProfiles.Enabled = true;
        }

        private void frmDrvSettings_Load(object sender, EventArgs e)
        {
            SetupLayout();
            SetTitleVersion();
            LoadSettings();

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

        private void AddToModifiedProfiles(string profileName, bool userProfile = false)
        {
            if (!_scanner.UserProfiles.Contains(profileName) && profileName != _baseProfileName && userProfile)
            {
                _scanner.UserProfiles.Add(profileName);
            }

            if (!_scanner.ModifiedProfiles.Contains(profileName) && profileName != _baseProfileName)
            {
                _scanner.ModifiedProfiles.Add(profileName);
                RefreshModifiesProfilesDropDown();
            }
        }

        private void RemoveFromModifiedProfiles(string profileName)
        {
            if (_scanner.UserProfiles.Contains(profileName))
            {
                _scanner.UserProfiles.Remove(profileName);
            }

            if (_scanner.ModifiedProfiles.Contains(profileName))
            {
                _scanner.ModifiedProfiles.Remove(profileName);
                RefreshModifiesProfilesDropDown();
            }
        }

        private void ShowExportProfiles()
        {
            if (_scanner.ModifiedProfiles.Count > 0)
            {
                var frmExport = new frmExportProfiles();
                frmExport.ShowDialog(this);
            }
            else
                MessageBox.Show("No user modified profiles found! Nothing to export.", "Userprofile Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private CancellationTokenSource _scannerCancelationTokenSource;

        private async Task ScanProfilesSilentAsync(bool scanPredefined, bool showProfileDialog)
        {
            if (_skipScan)
            {

                if (scanPredefined && !_alreadyScannedForPredefinedSettings)
                {
                    _alreadyScannedForPredefinedSettings = true;
                    _meta.ResetMetaCache();
                    tsbModifiedProfiles.Enabled = true;
                    exportUserdefinedProfilesToolStripMenuItem.Enabled = false;
                    RefreshCurrentProfile();
                }

                return;
            }

            tsbModifiedProfiles.Enabled = false;
            tsbRefreshProfile.Enabled = false;
            pbMain.Minimum = 0;
            pbMain.Maximum = 100;

            _scannerCancelationTokenSource = new CancellationTokenSource();

            var progressHandler = new Progress<int>(value =>
            {
                pbMain.Value = value;
                SetTaskbarProgress(value);
            });

            if (scanPredefined && !_alreadyScannedForPredefinedSettings)
            {
                _alreadyScannedForPredefinedSettings = true;
                await _scanner.ScanProfileSettingsAsync(false, progressHandler, _scannerCancelationTokenSource.Token);
                _meta.ResetMetaCache();
                tscbShowScannedUnknownSettings.Enabled = true;
            }
            else
            {
                await _scanner.ScanProfileSettingsAsync(true, progressHandler, _scannerCancelationTokenSource.Token);
            }
                        
            RefreshModifiesProfilesDropDown();
            tsbModifiedProfiles.Enabled = true;

            pbMain.Value = 0;
            pbMain.Enabled = false;
            SetTaskbarProgress(0);

            if (showProfileDialog)
            {
                ShowExportProfiles();
            }

            RefreshCurrentProfile();
            tsbRefreshProfile.Enabled = true;
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

        private async void tsbRestoreProfile_Click(object sender, EventArgs e)
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
                    await ScanProfilesSilentAsync(true, false);
                    cbProfiles.Text = GetBaseProfileName();
                }
            }
            else
                ResetCurrentProfile();
        }

        private void tsbRefreshProfile_Click(object sender, EventArgs e)
        {
            DrsSessionScope.DestroyGlobalSession();
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

        private async void frmDrvSettings_Shown(object sender, EventArgs e)
        {
            if (_isStartup)
            {
                new Thread(SetTaskbarIcon).Start();
                await ScanProfilesSilentAsync(true, false);

                if (_scannerCancelationTokenSource != null && !_scannerCancelationTokenSource.Token.IsCancellationRequested && WindowState != FormWindowState.Maximized)
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
                    ChangeCurrentProfile(_baseProfileName);
                    DrsSessionScope.DestroyGlobalSession();
                    RefreshAll();
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
                    if (ex.Status == Native.NVAPI2.NvAPI_Status.NVAPI_EXECUTABLE_ALREADY_IN_USE || ex.Status == Native.NVAPI2.NvAPI_Status.NVAPI_ERROR)
                    {
                        if (lblApplications.Text.ToUpper().IndexOf(" " + applicationName.ToUpper() + ",") != -1)
                            MessageBox.Show("This application executable is already assigned to this profile!",
                                "Error adding Application", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                        {
                            string profileNames = _scanner.FindProfilesUsingApplication(applicationName);
                            if (profileNames == "")
                                MessageBox.Show("This application executable might already be assigned to another profile!",
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
            _drs.RemoveApplication(_CurrentProfile, e.ClickedItem.Tag.ToString());
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
                    AddToModifiedProfiles(result, true);
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

        private async void exportUserdefinedProfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await ScanProfilesSilentAsync(false, true);
        }

        private void ExportCurrentProfile(bool includePredefined)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "*.nip";
            saveDialog.Filter = Application.ProductName + " Profiles|*.nip";
            saveDialog.FileName = _CurrentProfile + ".nip";
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var profiles = new[] { _CurrentProfile }.ToList();
                _import.ExportProfiles(profiles, saveDialog.FileName, includePredefined);
            }
        }

        private void exportCurrentProfileOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCurrentProfile(false);
        }

        private void exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCurrentProfile(true);
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

        private async void RefreshAll()
        {
            RefreshProfilesCombo();
            await ScanProfilesSilentAsync(true, false);

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
                    DrsSessionScope.DestroyGlobalSession();
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
                ImportProfiles(openDialog.FileName);
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


        public static void ShowImportDoneMessage(string importReport)
        {
            if (string.IsNullOrEmpty(importReport))
            {
                MessageBox.Show("Profile(s) successfully imported!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Some profile(s) could not imported!\r\n\r\n" + importReport, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ImportProfiles(string nipFileName)
        {
            var importReport = _import.ImportProfiles(nipFileName);
            RefreshAll();
            ShowImportDoneMessage(importReport);
        }

        private void lvSettings_OnDropFilesNative(string[] files)
        {
            if (files.Length == 1)
            {
                var fileInfo = new FileInfo(files[0]);
                if (fileInfo.Extension.ToLower().Equals(".nip"))
                {
                    ImportProfiles(fileInfo.FullName);
                    return;
                }


                var profileName = "";
                var exeFile = ShortcutResolver.ResolveExecuteable(files[0], out profileName);
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

        private void HandleScreenConstraints()
        {
            var workingArea = Screen.GetWorkingArea(this);

            if (Left < workingArea.X)
                Left = workingArea.X;

            if (Top < workingArea.Y)
                Top = workingArea.Y;

            if ((Left + Width) > workingArea.X + workingArea.Width)
                Left = (workingArea.X + workingArea.Width) - Width;

            if ((Top + Height) > workingArea.Y + workingArea.Height)
                Top = (workingArea.Y + workingArea.Height) - Height;
        }

        private void SaveSettings()
        {
            var settings = UserSettings.LoadSettings();

            if (WindowState == FormWindowState.Normal)
            {
                settings.WindowTop = Top;
                settings.WindowLeft = Left;
                settings.WindowHeight = Height;
                settings.WindowWidth = Width;
            }
            else
            {
                settings.WindowTop = RestoreBounds.Top;
                settings.WindowLeft = RestoreBounds.Left;
                settings.WindowHeight = RestoreBounds.Height;
                settings.WindowWidth = RestoreBounds.Width;
            }
            settings.WindowState = WindowState;
            settings.ShowCustomizedSettingNamesOnly = tscbShowCustomSettingNamesOnly.Checked;
            settings.ShowScannedUnknownSettings = tscbShowScannedUnknownSettings.Checked;
            settings.SaveSettings();
        }

        private void LoadSettings()
        {
            var settings = UserSettings.LoadSettings();
            SetBounds(settings.WindowLeft, settings.WindowTop, settings.WindowWidth, settings.WindowHeight);
            WindowState = settings.WindowState != FormWindowState.Minimized ? settings.WindowState : FormWindowState.Normal;
            HandleScreenConstraints();
            tscbShowCustomSettingNamesOnly.Checked = settings.ShowCustomizedSettingNamesOnly;
            tscbShowScannedUnknownSettings.Checked = !_skipScan && settings.ShowScannedUnknownSettings;
        }

        private void frmDrvSettings_FormClosed(object sender, FormClosedEventArgs e)
        {
            _scannerCancelationTokenSource?.Cancel();
            SaveSettings();
        }

        private void lvSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopyModifiedSettingsToClipBoard();
            }
        }

        private void CopyModifiedSettingsToClipBoard()
        {
            var sbSettings = new StringBuilder();
            sbSettings.AppendFormat("{0,-40} {1}\r\n", "### NVIDIA Profile Inspector ###", _CurrentProfile);

            foreach (ListViewGroup group in lvSettings.Groups)
            {
                bool groupTitleAdded = false;
                foreach (ListViewItem item in group.Items)
                {
                    if (item.ImageIndex != 0) continue;

                    if(!groupTitleAdded)
                    {
                        sbSettings.AppendFormat("\r\n[{0}]\r\n", group.Header);
                        groupTitleAdded = true;
                    }
                    sbSettings.AppendFormat("{0,-40} {1}\r\n", item.Text, item.SubItems[1].Text);
                }
            }
            
            Clipboard.SetText(sbSettings.ToString());

        }
    }
}





