namespace nspector;

partial class frmExportProfiles:System.Windows.Forms.Form
{
    frmDrvSettings settingsOwner;

    internal frmExportProfiles()
    {
        this.InitializeComponent();
        this.Icon          =System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
        this.DoubleBuffered=true;
    }

    internal void ShowDialog(frmDrvSettings SettingsOwner)
    {
        this.settingsOwner=SettingsOwner;
        this.Text         ="Profile Export";
        this.updateProfileList();
        this.ShowDialog();
    }


    void updateProfileList()
    {
        this.lvProfiles.Items.Clear();

        if(this.settingsOwner!=null)
        {
            foreach(var mp in nspector.Common.DrsServiceLocator.ScannerService.ModifiedProfiles)
            {
                this.lvProfiles.Items.Add(mp);
            }
        }
    }


    void btnCancel_Click(object sender,System.EventArgs e)
    {
        this.Close();
    }

    void btnSelAll_Click(object sender,System.EventArgs e)
    {
        for(var i=0;i<this.lvProfiles.Items.Count;i++)
        {
            this.lvProfiles.Items[i].Checked=true;
        }
    }

    void btnSelNone_Click(object sender,System.EventArgs e)
    {
        for(var i=0;i<this.lvProfiles.Items.Count;i++)
        {
            this.lvProfiles.Items[i].Checked=false;
        }
    }

    void btnInvertSelection_Click(object sender,System.EventArgs e)
    {
        for(var i=0;i<this.lvProfiles.Items.Count;i++)
        {
            this.lvProfiles.Items[i].Checked=!this.lvProfiles.Items[i].Checked;
        }
    }

    void btnExport_Click(object sender,System.EventArgs e)
    {
        var sfd=new System.Windows.Forms.SaveFileDialog();
        sfd.DefaultExt="*.nip";
        sfd.Filter    =System.Windows.Forms.Application.ProductName+" Profiles|*.nip";
        if(sfd.ShowDialog()==System.Windows.Forms.DialogResult.OK)
        {
            var profileNamesToExport=new System.Collections.Generic.List<string>();
            for(var i=0;i<this.lvProfiles.Items.Count;i++)
            {
                if(this.lvProfiles.Items[i].Checked)
                {
                    profileNamesToExport.Add(this.lvProfiles.Items[i].Text);
                }
            }

            nspector.Common.DrsServiceLocator.ImportService.ExportProfiles(profileNamesToExport,sfd.FileName,
                this.cbIncludePredefined.Checked);

            if(profileNamesToExport.Count>0)
            {
                if(System.Windows.Forms.MessageBox.Show(
                        "Export succeeded.\r\n\r\nWould you like to continue exporting profiles?",
                        "Profiles Export",System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Question)==System.Windows.Forms.DialogResult.No)
                {
                    this.Close();
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Nothing to export");
            }
        }
    }

    void lvProfiles_ItemChecked(object sender,System.Windows.Forms.ItemCheckedEventArgs e)
    {
        var cc=0;
        for(var i=0;i<this.lvProfiles.Items.Count;i++)
        {
            if(this.lvProfiles.Items[i].Checked)
            {
                cc++;
            }
        }

        if(cc>0)
        {
            this.btnExport.Enabled=true;
        }
        else
        {
            this.btnExport.Enabled=false;
        }
    }
}