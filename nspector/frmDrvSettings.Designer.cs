namespace nspector
{
    partial class frmDrvSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDrvSettings));
            this.ilListView = new System.Windows.Forms.ImageList(this.components);
            this.pbMain = new System.Windows.Forms.ProgressBar();
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.tslProfiles = new System.Windows.Forms.ToolStripLabel();
            this.cbProfiles = new System.Windows.Forms.ToolStripComboBox();
            this.tsbModifiedProfiles = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbRefreshProfile = new System.Windows.Forms.ToolStripButton();
            this.tsbRestoreProfile = new System.Windows.Forms.ToolStripButton();
            this.tsbCreateProfile = new System.Windows.Forms.ToolStripButton();
            this.tsbDeleteProfile = new System.Windows.Forms.ToolStripButton();
            this.tsSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbAddApplication = new System.Windows.Forms.ToolStripButton();
            this.tssbRemoveApplication = new System.Windows.Forms.ToolStripSplitButton();
            this.tsSep3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbExportProfiles = new System.Windows.Forms.ToolStripSplitButton();
            this.exportCurrentProfileOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportUserdefinedProfilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllProfilesNVIDIATextFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbImportProfiles = new System.Windows.Forms.ToolStripSplitButton();
            this.importProfilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importAllProfilesNVIDIATextFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSep4 = new System.Windows.Forms.ToolStripSeparator();
            this.tscbShowCustomSettingNamesOnly = new System.Windows.Forms.ToolStripButton();
            this.tsSep5 = new System.Windows.Forms.ToolStripSeparator();
            this.tscbShowScannedUnknownSettings = new System.Windows.Forms.ToolStripButton();
            this.tsbBitValueEditor = new System.Windows.Forms.ToolStripButton();
            this.tsSep6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbApplyProfile = new System.Windows.Forms.ToolStripButton();
            this.btnResetValue = new System.Windows.Forms.Button();
            this.lblApplications = new System.Windows.Forms.Label();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.ilCombo = new System.Windows.Forms.ImageList(this.components);
            this.cbValues = new System.Windows.Forms.ComboBox();
            this.lblWidth96 = new System.Windows.Forms.Label();
            this.lblWidth330 = new System.Windows.Forms.Label();
            this.lblWidth16 = new System.Windows.Forms.Label();
            this.lblWidth30 = new System.Windows.Forms.Label();
            this.lvSettings = new nspector.ListViewEx();
            this.chSettingID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSettingValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSettingValueHex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tbSettingDescription = new System.Windows.Forms.TextBox();
            this.pnlListview = new System.Windows.Forms.Panel();
            this.txtFilter = new nspector.WatermarkTextBox();
            this.tsMain.SuspendLayout();
            this.pnlListview.SuspendLayout();
            this.SuspendLayout();
            // 
            // ilListView
            // 
            this.ilListView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilListView.ImageStream")));
            this.ilListView.TransparentColor = System.Drawing.Color.Transparent;
            this.ilListView.Images.SetKeyName(0, "0_gear2.png");
            this.ilListView.Images.SetKeyName(1, "1_gear2_2.png");
            this.ilListView.Images.SetKeyName(2, "4_gear_nv2.png");
            this.ilListView.Images.SetKeyName(3, "6_gear_inherit.png");
            // 
            // pbMain
            // 
            this.pbMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbMain.Location = new System.Drawing.Point(12, 475);
            this.pbMain.Margin = new System.Windows.Forms.Padding(4);
            this.pbMain.Name = "pbMain";
            this.pbMain.Size = new System.Drawing.Size(840, 9);
            this.pbMain.TabIndex = 19;
            // 
            // tsMain
            // 
            this.tsMain.AllowMerge = false;
            this.tsMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tsMain.AutoSize = false;
            this.tsMain.BackgroundImage = global::nspector.Properties.Resources.transparent16;
            this.tsMain.CanOverflow = false;
            this.tsMain.Dock = System.Windows.Forms.DockStyle.None;
            this.tsMain.GripMargin = new System.Windows.Forms.Padding(0);
            this.tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslProfiles,
            this.cbProfiles,
            this.tsbModifiedProfiles,
            this.toolStripSeparator1,
            this.tsbRefreshProfile,
            this.tsbRestoreProfile,
            this.tsbCreateProfile,
            this.tsbDeleteProfile,
            this.tsSep2,
            this.tsbAddApplication,
            this.tssbRemoveApplication,
            this.tsSep3,
            this.tsbExportProfiles,
            this.tsbImportProfiles,
            this.tsSep4,
            this.tscbShowCustomSettingNamesOnly,
            this.tsSep5,
            this.tscbShowScannedUnknownSettings,
            this.tsbBitValueEditor,
            this.tsSep6,
            this.tsbApplyProfile});
            this.tsMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.tsMain.Location = new System.Drawing.Point(12, 4);
            this.tsMain.Name = "tsMain";
            this.tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.tsMain.Size = new System.Drawing.Size(840, 25);
            this.tsMain.TabIndex = 24;
            this.tsMain.Text = "toolStrip1";
            // 
            // tslProfiles
            // 
            this.tslProfiles.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tslProfiles.Margin = new System.Windows.Forms.Padding(0, 5, 10, 2);
            this.tslProfiles.Name = "tslProfiles";
            this.tslProfiles.Size = new System.Drawing.Size(49, 18);
            this.tslProfiles.Text = "Profiles:";
            // 
            // cbProfiles
            // 
            this.cbProfiles.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbProfiles.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbProfiles.AutoSize = false;
            this.cbProfiles.DropDownWidth = 290;
            this.cbProfiles.Margin = new System.Windows.Forms.Padding(1);
            this.cbProfiles.MaxDropDownItems = 50;
            this.cbProfiles.Name = "cbProfiles";
            this.cbProfiles.Size = new System.Drawing.Size(290, 23);
            this.cbProfiles.SelectedIndexChanged += new System.EventHandler(this.cbProfiles_SelectedIndexChanged);
            this.cbProfiles.TextChanged += new System.EventHandler(this.cbProfiles_TextChanged);
            // 
            // tsbModifiedProfiles
            // 
            this.tsbModifiedProfiles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbModifiedProfiles.Enabled = false;
            this.tsbModifiedProfiles.Image = global::nspector.Properties.Resources.home_sm;
            this.tsbModifiedProfiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbModifiedProfiles.Name = "tsbModifiedProfiles";
            this.tsbModifiedProfiles.Size = new System.Drawing.Size(36, 22);
            this.tsbModifiedProfiles.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.tsbModifiedProfiles.ToolTipText = "Back to global profile (Home) / User modified profiles";
            this.tsbModifiedProfiles.ButtonClick += new System.EventHandler(this.tsbModifiedProfiles_ButtonClick);
            this.tsbModifiedProfiles.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tsbModifiedProfiles_DropDownItemClicked);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbRefreshProfile
            // 
            this.tsbRefreshProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRefreshProfile.Image = ((System.Drawing.Image)(resources.GetObject("tsbRefreshProfile.Image")));
            this.tsbRefreshProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefreshProfile.Name = "tsbRefreshProfile";
            this.tsbRefreshProfile.Size = new System.Drawing.Size(24, 22);
            this.tsbRefreshProfile.Text = "Refresh current profile.";
            this.tsbRefreshProfile.Click += new System.EventHandler(this.tsbRefreshProfile_Click);
            // 
            // tsbRestoreProfile
            // 
            this.tsbRestoreProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRestoreProfile.Image = ((System.Drawing.Image)(resources.GetObject("tsbRestoreProfile.Image")));
            this.tsbRestoreProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRestoreProfile.Name = "tsbRestoreProfile";
            this.tsbRestoreProfile.Size = new System.Drawing.Size(24, 22);
            this.tsbRestoreProfile.Text = "Restore current profile to NVIDIA defaults.";
            this.tsbRestoreProfile.Click += new System.EventHandler(this.tsbRestoreProfile_Click);
            // 
            // tsbCreateProfile
            // 
            this.tsbCreateProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCreateProfile.Image = ((System.Drawing.Image)(resources.GetObject("tsbCreateProfile.Image")));
            this.tsbCreateProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCreateProfile.Name = "tsbCreateProfile";
            this.tsbCreateProfile.Size = new System.Drawing.Size(24, 22);
            this.tsbCreateProfile.Text = "Create new profile";
            this.tsbCreateProfile.Click += new System.EventHandler(this.tsbCreateProfile_Click);
            // 
            // tsbDeleteProfile
            // 
            this.tsbDeleteProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbDeleteProfile.Image = global::nspector.Properties.Resources.ieframe_1_18212;
            this.tsbDeleteProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDeleteProfile.Name = "tsbDeleteProfile";
            this.tsbDeleteProfile.Size = new System.Drawing.Size(24, 22);
            this.tsbDeleteProfile.Text = "Delete current Profile";
            this.tsbDeleteProfile.Click += new System.EventHandler(this.tsbDeleteProfile_Click);
            // 
            // tsSep2
            // 
            this.tsSep2.Name = "tsSep2";
            this.tsSep2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbAddApplication
            // 
            this.tsbAddApplication.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddApplication.Image = global::nspector.Properties.Resources.window_application_add;
            this.tsbAddApplication.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddApplication.Name = "tsbAddApplication";
            this.tsbAddApplication.Size = new System.Drawing.Size(24, 22);
            this.tsbAddApplication.Text = "Add application to current profile.";
            this.tsbAddApplication.Click += new System.EventHandler(this.tsbAddApplication_Click);
            // 
            // tssbRemoveApplication
            // 
            this.tssbRemoveApplication.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tssbRemoveApplication.Image = global::nspector.Properties.Resources.window_application_delete;
            this.tssbRemoveApplication.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tssbRemoveApplication.Name = "tssbRemoveApplication";
            this.tssbRemoveApplication.Size = new System.Drawing.Size(36, 22);
            this.tssbRemoveApplication.Text = "Remove application from current profile";
            this.tssbRemoveApplication.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tssbRemoveApplication_DropDownItemClicked);
            this.tssbRemoveApplication.Click += new System.EventHandler(this.tssbRemoveApplication_Click);
            // 
            // tsSep3
            // 
            this.tsSep3.Name = "tsSep3";
            this.tsSep3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbExportProfiles
            // 
            this.tsbExportProfiles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbExportProfiles.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportCurrentProfileOnlyToolStripMenuItem,
            this.exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem,
            this.exportUserdefinedProfilesToolStripMenuItem,
            this.exportAllProfilesNVIDIATextFormatToolStripMenuItem});
            this.tsbExportProfiles.Image = global::nspector.Properties.Resources.export1;
            this.tsbExportProfiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExportProfiles.Name = "tsbExportProfiles";
            this.tsbExportProfiles.Size = new System.Drawing.Size(36, 22);
            this.tsbExportProfiles.Text = "Export user defined profiles";
            this.tsbExportProfiles.Click += new System.EventHandler(this.tsbExportProfiles_Click);
            // 
            // exportCurrentProfileOnlyToolStripMenuItem
            // 
            this.exportCurrentProfileOnlyToolStripMenuItem.Name = "exportCurrentProfileOnlyToolStripMenuItem";
            this.exportCurrentProfileOnlyToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.exportCurrentProfileOnlyToolStripMenuItem.Text = "Export current profile only";
            this.exportCurrentProfileOnlyToolStripMenuItem.Click += new System.EventHandler(this.exportCurrentProfileOnlyToolStripMenuItem_Click);
            // 
            // exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem
            // 
            this.exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem.Name = "exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem";
            this.exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem.Text = "Export current profile including predefined settings";
            this.exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem.Click += new System.EventHandler(this.exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem_Click);
            // 
            // exportUserdefinedProfilesToolStripMenuItem
            // 
            this.exportUserdefinedProfilesToolStripMenuItem.Name = "exportUserdefinedProfilesToolStripMenuItem";
            this.exportUserdefinedProfilesToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.exportUserdefinedProfilesToolStripMenuItem.Text = "Export all customized profiles";
            this.exportUserdefinedProfilesToolStripMenuItem.Click += new System.EventHandler(this.exportUserdefinedProfilesToolStripMenuItem_Click);
            // 
            // exportAllProfilesNVIDIATextFormatToolStripMenuItem
            // 
            this.exportAllProfilesNVIDIATextFormatToolStripMenuItem.Name = "exportAllProfilesNVIDIATextFormatToolStripMenuItem";
            this.exportAllProfilesNVIDIATextFormatToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
            this.exportAllProfilesNVIDIATextFormatToolStripMenuItem.Text = "Export all driver profiles (NVIDIA Text Format)";
            this.exportAllProfilesNVIDIATextFormatToolStripMenuItem.Click += new System.EventHandler(this.exportAllProfilesNVIDIATextFormatToolStripMenuItem_Click);
            // 
            // tsbImportProfiles
            // 
            this.tsbImportProfiles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbImportProfiles.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importProfilesToolStripMenuItem,
            this.importAllProfilesNVIDIATextFormatToolStripMenuItem});
            this.tsbImportProfiles.Image = global::nspector.Properties.Resources.import1;
            this.tsbImportProfiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbImportProfiles.Name = "tsbImportProfiles";
            this.tsbImportProfiles.Size = new System.Drawing.Size(36, 22);
            this.tsbImportProfiles.Text = "Import user defined profiles";
            this.tsbImportProfiles.Click += new System.EventHandler(this.tsbImportProfiles_Click);
            // 
            // importProfilesToolStripMenuItem
            // 
            this.importProfilesToolStripMenuItem.Name = "importProfilesToolStripMenuItem";
            this.importProfilesToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.importProfilesToolStripMenuItem.Text = "Import profile(s)";
            this.importProfilesToolStripMenuItem.Click += new System.EventHandler(this.importProfilesToolStripMenuItem_Click);
            // 
            // importAllProfilesNVIDIATextFormatToolStripMenuItem
            // 
            this.importAllProfilesNVIDIATextFormatToolStripMenuItem.Name = "importAllProfilesNVIDIATextFormatToolStripMenuItem";
            this.importAllProfilesNVIDIATextFormatToolStripMenuItem.Size = new System.Drawing.Size(363, 22);
            this.importAllProfilesNVIDIATextFormatToolStripMenuItem.Text = "Import (replace) all driver profiles (NVIDIA Text Format)";
            this.importAllProfilesNVIDIATextFormatToolStripMenuItem.Click += new System.EventHandler(this.importAllProfilesNVIDIATextFormatToolStripMenuItem_Click);
            // 
            // tsSep4
            // 
            this.tsSep4.Name = "tsSep4";
            this.tsSep4.Size = new System.Drawing.Size(6, 25);
            // 
            // tscbShowCustomSettingNamesOnly
            // 
            this.tscbShowCustomSettingNamesOnly.CheckOnClick = true;
            this.tscbShowCustomSettingNamesOnly.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tscbShowCustomSettingNamesOnly.Image = global::nspector.Properties.Resources.filter_user;
            this.tscbShowCustomSettingNamesOnly.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tscbShowCustomSettingNamesOnly.Name = "tscbShowCustomSettingNamesOnly";
            this.tscbShowCustomSettingNamesOnly.Size = new System.Drawing.Size(24, 22);
            this.tscbShowCustomSettingNamesOnly.Text = "Show the settings and values from CustomSettingNames file only.";
            this.tscbShowCustomSettingNamesOnly.CheckedChanged += new System.EventHandler(this.cbCustomSettingsOnly_CheckedChanged);
            // 
            // tsSep5
            // 
            this.tsSep5.Name = "tsSep5";
            this.tsSep5.Size = new System.Drawing.Size(6, 25);
            // 
            // tscbShowScannedUnknownSettings
            // 
            this.tscbShowScannedUnknownSettings.CheckOnClick = true;
            this.tscbShowScannedUnknownSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tscbShowScannedUnknownSettings.Enabled = false;
            this.tscbShowScannedUnknownSettings.Image = global::nspector.Properties.Resources.find_set2;
            this.tscbShowScannedUnknownSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tscbShowScannedUnknownSettings.Name = "tscbShowScannedUnknownSettings";
            this.tscbShowScannedUnknownSettings.Size = new System.Drawing.Size(24, 22);
            this.tscbShowScannedUnknownSettings.Text = "Show unknown settings from NVIDIA predefined profiles";
            this.tscbShowScannedUnknownSettings.Click += new System.EventHandler(this.tscbShowScannedUnknownSettings_Click);
            // 
            // tsbBitValueEditor
            // 
            this.tsbBitValueEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbBitValueEditor.Image = global::nspector.Properties.Resources.text_binary;
            this.tsbBitValueEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBitValueEditor.Name = "tsbBitValueEditor";
            this.tsbBitValueEditor.Size = new System.Drawing.Size(24, 22);
            this.tsbBitValueEditor.Text = "Show bit value editor.";
            this.tsbBitValueEditor.Click += new System.EventHandler(this.tsbBitValueEditor_Click);
            // 
            // tsSep6
            // 
            this.tsSep6.Name = "tsSep6";
            this.tsSep6.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbApplyProfile
            // 
            this.tsbApplyProfile.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbApplyProfile.Image = global::nspector.Properties.Resources.apply;
            this.tsbApplyProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbApplyProfile.Name = "tsbApplyProfile";
            this.tsbApplyProfile.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.tsbApplyProfile.Size = new System.Drawing.Size(109, 22);
            this.tsbApplyProfile.Text = "Apply changes";
            this.tsbApplyProfile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.tsbApplyProfile.Click += new System.EventHandler(this.tsbApplyProfile_Click);
            // 
            // btnResetValue
            // 
            this.btnResetValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetValue.Enabled = false;
            this.btnResetValue.Image = global::nspector.Properties.Resources.nv_btn;
            this.btnResetValue.Location = new System.Drawing.Point(732, 175);
            this.btnResetValue.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.btnResetValue.Name = "btnResetValue";
            this.btnResetValue.Size = new System.Drawing.Size(25, 19);
            this.btnResetValue.TabIndex = 7;
            this.btnResetValue.UseVisualStyleBackColor = true;
            this.btnResetValue.Click += new System.EventHandler(this.btnResetValue_Click);
            // 
            // lblApplications
            // 
            this.lblApplications.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblApplications.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(185)))), ((int)(((byte)(0)))));
            this.lblApplications.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblApplications.ForeColor = System.Drawing.Color.White;
            this.lblApplications.Location = new System.Drawing.Point(12, 32);
            this.lblApplications.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblApplications.Name = "lblApplications";
            this.lblApplications.Size = new System.Drawing.Size(840, 17);
            this.lblApplications.TabIndex = 25;
            this.lblApplications.Text = "fsagame.exe, bond.exe, herozero.exe";
            this.lblApplications.DoubleClick += new System.EventHandler(this.tsbAddApplication_Click);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.Text = "toolStripButton5";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(86, 22);
            this.toolStripLabel2.Text = "toolStripLabel2";
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton6.Text = "toolStripButton6";
            // 
            // ilCombo
            // 
            this.ilCombo.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilCombo.ImageSize = new System.Drawing.Size(16, 16);
            this.ilCombo.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // cbValues
            // 
            this.cbValues.BackColor = System.Drawing.SystemColors.Window;
            this.cbValues.FormattingEnabled = true;
            this.cbValues.Location = new System.Drawing.Point(524, 175);
            this.cbValues.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.cbValues.Name = "cbValues";
            this.cbValues.Size = new System.Drawing.Size(72, 21);
            this.cbValues.TabIndex = 5;
            this.cbValues.Visible = false;
            this.cbValues.SelectedValueChanged += new System.EventHandler(this.cbValues_SelectedValueChanged);
            this.cbValues.Leave += new System.EventHandler(this.cbValues_Leave);
            // 
            // lblWidth96
            // 
            this.lblWidth96.Location = new System.Drawing.Point(77, 233);
            this.lblWidth96.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWidth96.Name = "lblWidth96";
            this.lblWidth96.Size = new System.Drawing.Size(96, 18);
            this.lblWidth96.TabIndex = 77;
            this.lblWidth96.Text = "96";
            this.lblWidth96.Visible = false;
            // 
            // lblWidth330
            // 
            this.lblWidth330.Location = new System.Drawing.Point(77, 210);
            this.lblWidth330.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWidth330.Name = "lblWidth330";
            this.lblWidth330.Size = new System.Drawing.Size(330, 22);
            this.lblWidth330.TabIndex = 78;
            this.lblWidth330.Text = "330 (Helper Labels for DPI Scaling)";
            this.lblWidth330.Visible = false;
            // 
            // lblWidth16
            // 
            this.lblWidth16.Location = new System.Drawing.Point(77, 269);
            this.lblWidth16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWidth16.Name = "lblWidth16";
            this.lblWidth16.Size = new System.Drawing.Size(16, 18);
            this.lblWidth16.TabIndex = 79;
            this.lblWidth16.Text = "16";
            this.lblWidth16.Visible = false;
            // 
            // lblWidth30
            // 
            this.lblWidth30.Location = new System.Drawing.Point(77, 251);
            this.lblWidth30.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWidth30.Name = "lblWidth30";
            this.lblWidth30.Size = new System.Drawing.Size(30, 18);
            this.lblWidth30.TabIndex = 80;
            this.lblWidth30.Text = "30";
            this.lblWidth30.Visible = false;
            // 
            // lvSettings
            // 
            this.lvSettings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chSettingID,
            this.chSettingValue,
            this.chSettingValueHex});
            this.lvSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSettings.FullRowSelect = true;
            this.lvSettings.GridLines = true;
            this.lvSettings.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvSettings.HideSelection = false;
            this.lvSettings.Location = new System.Drawing.Point(0, 0);
            this.lvSettings.Margin = new System.Windows.Forms.Padding(4);
            this.lvSettings.MultiSelect = false;
            this.lvSettings.Name = "lvSettings";
            this.lvSettings.ShowItemToolTips = true;
            this.lvSettings.Size = new System.Drawing.Size(840, 372);
            this.lvSettings.SmallImageList = this.ilListView;
            this.lvSettings.TabIndex = 2;
            this.lvSettings.UseCompatibleStateImageBehavior = false;
            this.lvSettings.View = System.Windows.Forms.View.Details;
            this.lvSettings.GroupStateChanged += new System.EventHandler<nspector.GroupStateChangedEventArgs>(this.lvSettings_GroupStateChanged);
            this.lvSettings.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.lvSettings_ColumnWidthChanging);
            this.lvSettings.SelectedIndexChanged += new System.EventHandler(this.lvSettings_SelectedIndexChanged);
            this.lvSettings.DoubleClick += new System.EventHandler(this.lvSettings_DoubleClick);
            this.lvSettings.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvSettings_KeyDown);
            this.lvSettings.Resize += new System.EventHandler(this.lvSettings_Resize);
            // 
            // chSettingID
            // 
            this.chSettingID.Text = "SettingID";
            this.chSettingID.Width = 330;
            // 
            // chSettingValue
            // 
            this.chSettingValue.Text = "SettingValue";
            this.chSettingValue.Width = 340;
            // 
            // chSettingValueHex
            // 
            this.chSettingValueHex.Text = "SettingValueHex";
            this.chSettingValueHex.Width = 96;
            // 
            // tbSettingDescription
            // 
            this.tbSettingDescription.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbSettingDescription.Location = new System.Drawing.Point(0, 372);
            this.tbSettingDescription.Multiline = true;
            this.tbSettingDescription.Name = "tbSettingDescription";
            this.tbSettingDescription.ReadOnly = true;
            this.tbSettingDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbSettingDescription.Size = new System.Drawing.Size(840, 44);
            this.tbSettingDescription.TabIndex = 81;
            this.tbSettingDescription.Visible = false;
            // 
            // pnlListview
            // 
            this.pnlListview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlListview.Controls.Add(this.lvSettings);
            this.pnlListview.Controls.Add(this.txtFilter);
            this.pnlListview.Controls.Add(this.tbSettingDescription);
            this.pnlListview.Location = new System.Drawing.Point(12, 52);
            this.pnlListview.Name = "pnlListview";
            this.pnlListview.Size = new System.Drawing.Size(840, 416);
            this.pnlListview.TabIndex = 82;
            // 
            // txtFilter
            // 
            this.txtFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFilter.Location = new System.Drawing.Point(0, 0);
            this.txtFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txtFilter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txtFilter.Size = new System.Drawing.Size(2118, 35);
            this.txtFilter.TabIndex = 82;
            this.txtFilter.WatermarkText = "Search for setting...";
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            this.txtFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtFilter_KeyUp);
            // 
            // frmDrvSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 492);
            this.Controls.Add(this.pnlListview);
            this.Controls.Add(this.lblWidth30);
            this.Controls.Add(this.lblWidth16);
            this.Controls.Add(this.lblWidth330);
            this.Controls.Add(this.lblWidth96);
            this.Controls.Add(this.lblApplications);
            this.Controls.Add(this.tsMain);
            this.Controls.Add(this.pbMain);
            this.Controls.Add(this.btnResetValue);
            this.Controls.Add(this.cbValues);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(879, 346);
            this.Name = "frmDrvSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "nSpector - Driver Profile Settings";
            this.Activated += new System.EventHandler(this.frmDrvSettings_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmDrvSettings_FormClosed);
            this.Load += new System.EventHandler(this.frmDrvSettings_Load);
            this.Shown += new System.EventHandler(this.frmDrvSettings_Shown);
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.pnlListview.ResumeLayout(false);
            this.pnlListview.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ListViewEx lvSettings;
        private System.Windows.Forms.ColumnHeader chSettingID;
        private System.Windows.Forms.ColumnHeader chSettingValue;
        private System.Windows.Forms.ColumnHeader chSettingValueHex;
        private System.Windows.Forms.ImageList ilListView;
        private System.Windows.Forms.ComboBox cbValues;
        private System.Windows.Forms.Button btnResetValue;
        private System.Windows.Forms.ProgressBar pbMain;
        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ToolStripButton tsbRestoreProfile;
        private System.Windows.Forms.ToolStripButton tsbApplyProfile;
        private System.Windows.Forms.ToolStripButton tsbRefreshProfile;
        private System.Windows.Forms.ToolStripSeparator tsSep3;
        private System.Windows.Forms.ToolStripButton tsbBitValueEditor;
        private System.Windows.Forms.ToolStripSeparator tsSep6;
        private System.Windows.Forms.ToolStripButton tscbShowCustomSettingNamesOnly;
        private System.Windows.Forms.ToolStripSeparator tsSep5;
        private System.Windows.Forms.ToolStripButton tscbShowScannedUnknownSettings;
        private System.Windows.Forms.ToolStripLabel tslProfiles;
        private System.Windows.Forms.Label lblApplications;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripButton tsbDeleteProfile;
        private System.Windows.Forms.ToolStripButton tsbCreateProfile;
        private System.Windows.Forms.ToolStripButton tsbAddApplication;
        private System.Windows.Forms.ToolStripSplitButton tssbRemoveApplication;
        private System.Windows.Forms.ToolStripSeparator tsSep4;
        private System.Windows.Forms.ToolStripSplitButton tsbExportProfiles;
        private System.Windows.Forms.ToolStripMenuItem exportCurrentProfileOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportUserdefinedProfilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        public System.Windows.Forms.ToolStripComboBox cbProfiles;
        private System.Windows.Forms.ToolStripSplitButton tsbModifiedProfiles;
        private System.Windows.Forms.ImageList ilCombo;
        private System.Windows.Forms.ToolStripMenuItem exportAllProfilesNVIDIATextFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton tsbImportProfiles;
        private System.Windows.Forms.ToolStripMenuItem importProfilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importAllProfilesNVIDIATextFormatToolStripMenuItem;
        private System.Windows.Forms.Label lblWidth96;
        private System.Windows.Forms.Label lblWidth330;
        private System.Windows.Forms.Label lblWidth16;
        private System.Windows.Forms.Label lblWidth30;
        private System.Windows.Forms.ToolStripMenuItem exportCurrentProfileIncludingPredefinedSettingsToolStripMenuItem;
        private System.Windows.Forms.TextBox tbSettingDescription;
        private System.Windows.Forms.Panel pnlListview;
        private WatermarkTextBox txtFilter;
    }
}