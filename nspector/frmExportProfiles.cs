#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using nspector.Common;

#endregion

namespace nspector;

internal partial class frmExportProfiles : Form
{
    private frmDrvSettings settingsOwner;

    internal frmExportProfiles()
    {
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        DoubleBuffered = true;
    }

    internal void ShowDialog(frmDrvSettings SettingsOwner)
    {
        settingsOwner = SettingsOwner;
        Text = "Profile Export";
        updateProfileList();
        ShowDialog();
    }


    private void updateProfileList()
    {
        lvProfiles.Items.Clear();

        if (settingsOwner != null)
            foreach (var mp in DrsServiceLocator.ScannerService.ModifiedProfiles)
                lvProfiles.Items.Add(mp);
    }


    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnSelAll_Click(object sender, EventArgs e)
    {
        for (var i = 0; i < lvProfiles.Items.Count; i++)
            lvProfiles.Items[i].Checked = true;
    }

    private void btnSelNone_Click(object sender, EventArgs e)
    {
        for (var i = 0; i < lvProfiles.Items.Count; i++)
            lvProfiles.Items[i].Checked = false;
    }

    private void btnInvertSelection_Click(object sender, EventArgs e)
    {
        for (var i = 0; i < lvProfiles.Items.Count; i++)
            lvProfiles.Items[i].Checked = !lvProfiles.Items[i].Checked;
    }

    private void btnExport_Click(object sender, EventArgs e)
    {
        var sfd = new SaveFileDialog();
        sfd.DefaultExt = "*.nip";
        sfd.Filter = Application.ProductName + " Profiles|*.nip";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            var profileNamesToExport = new List<string>();
            for (var i = 0; i < lvProfiles.Items.Count; i++)
                if (lvProfiles.Items[i].Checked)
                    profileNamesToExport.Add(lvProfiles.Items[i].Text);

            DrsServiceLocator.ImportService.ExportProfiles(profileNamesToExport, sfd.FileName, cbIncludePredefined.Checked);

            if (profileNamesToExport.Count > 0)
            {
                if (MessageBox.Show("Export succeeded.\r\n\r\nWould you like to continue exporting profiles?", "Profiles Export", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.No)
                    Close();
            }
            else
            {
                MessageBox.Show("Nothing to export");
            }
        }
    }

    private void lvProfiles_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        var cc = 0;
        for (var i = 0; i < lvProfiles.Items.Count; i++)
            if (lvProfiles.Items[i].Checked)
                cc++;

        if (cc > 0)
            btnExport.Enabled = true;
        else
            btnExport.Enabled = false;
    }
}