using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using nspector.Common;
using nspector.Common.CustomSettings;

namespace nspector
{
    internal partial class frmBitEditor : Form
    {
        private uint _Settingid = 0;
        private frmDrvSettings _SettingsOwner = null;
        private uint _InitValue = 0;
        private uint _CurrentValue = 0;


        internal frmBitEditor()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.DoubleBuffered = true;
        }

        internal void ShowDialog(frmDrvSettings SettingsOwner, uint SettingId, uint InitValue, string SettingName)
        {
            _Settingid = SettingId;
            _SettingsOwner = SettingsOwner;
            _InitValue = InitValue;
            Text = string.Format("Bit Value Editor - {0}", SettingName);
            this.ShowDialog(SettingsOwner);
        }

        private void frmBitEditor_Load(object sender, EventArgs e)
        {
            SplitBitsFromUnknownSettings();
            clbBits.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            SetValue(_InitValue);
        }

        private void SplitBitsFromUnknownSettings()
        {
            uint lastValue = 0;
            lastValue = _CurrentValue;
            string[] filters = tbFilter.Text.Split(',');
            clbBits.Items.Clear();

            var referenceSettings = DrsServiceLocator.ReferenceSettings?.Settings.FirstOrDefault(s => s.SettingId == _Settingid);

            var settingsCache = DrsServiceLocator.ScannerService.CachedSettings.FirstOrDefault(x => x.SettingId == _Settingid);

            for (int bit = 0; bit < 32; bit++)
            {
                string profileNames = "";
                uint profileCount = 0;

                if (settingsCache != null)
                {

                    for (int i = 0; i < settingsCache.SettingValues.Count; i++)
                    {
                        if (((settingsCache.SettingValues[i].Value >> bit) & 0x1) == 0x1)
                        {
                            if (filters.Length == 0)
                            {
                                profileNames += settingsCache.SettingValues[i].ProfileNames + ",";
                            }
                            else
                            {
                                string[] settingProfileNames = settingsCache.SettingValues[i].ProfileNames.ToString().Split(',');
                                for (int p = 0; p < settingProfileNames.Length; p++)
                                {
                                    for (int f = 0; f < filters.Length; f++)
                                    {
                                        if (settingProfileNames[p].ToLowerInvariant().Contains(filters[f].ToLower()))
                                        {
                                            profileNames += settingProfileNames[p] + ",";
                                        }
                                    }
                                }
                            }
                            profileCount += settingsCache.SettingValues[i].ValueProfileCount;
                        }
                    }
                }

                uint mask = (uint)1 << bit;
                string maskStr = "";

                if (referenceSettings != null)
                {
                    var maskValue = referenceSettings.SettingValues.FirstOrDefault(v => v.SettingValue == mask);
                    if (maskValue != null)
                    {
                        maskStr = maskValue.UserfriendlyName;
                        if (maskStr.Contains("("))
                        {
                            maskStr = maskStr.Substring(0, maskStr.IndexOf("(") - 1);
                        }
                    }
                }

                clbBits.Items.Add(new ListViewItem(new string[] {
                        string.Format("#{0:00}",bit),
                        maskStr,
                        profileCount.ToString(),
                        profileNames,
                    }));


            }

            SetValue(lastValue);
        }

        private void updateValue(bool changeState, int changedIndex)
        {
            uint val = 0;
            for (int b = 0; b < clbBits.Items.Count; b++)
            {
                if (((clbBits.Items[b].Checked) && changedIndex != b) || (changeState && (changedIndex == b)))
                {
                    val = (uint)((uint)val | (uint)(1 << b));
                }
            }

            UpdateCurrent(val);
        }

        private void UpdateValue()
        {
            uint val = 0;
            for (int b = 0; b < clbBits.Items.Count; b++)
            {
                if (clbBits.Items[b].Checked)
                {
                    val = (uint)((uint)val | (uint)(1 << b));
                }
            }

            UpdateCurrent(val);
        }


        private void SetValue(uint val)
        {
            for (int b = 0; b < clbBits.Items.Count; b++)
            {
                if (((val >> b) & 0x1) == 0x1)
                    clbBits.Items[b].Checked = true;
                else
                    clbBits.Items[b].Checked = false;
            }

            UpdateValue();
        }

        private void UpdateCurrent(uint val)
        {
            _CurrentValue = val;
            textBox1.Text = "0x" + (val).ToString("X8");
        }

        private void UpdateCurrent(string text)
        {
            uint val = DrsUtil.ParseDwordByInputSafe(text);
            UpdateCurrent(val);
            SetValue(val);
        }

        private void clbBits_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            updateValue(e.NewValue == CheckState.Checked, e.Index);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _SettingsOwner.SetSelectedDwordValue(_CurrentValue);
            Close();
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            SplitBitsFromUnknownSettings();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SplitBitsFromUnknownSettings();
        }

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                UpdateCurrent(textBox1.Text);
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            UpdateCurrent(textBox1.Text);
        }


        private void ApplyValueToProfile(uint val)
        {
            DrsServiceLocator
               .SettingService
               .SetDwordValueToProfile(_SettingsOwner._CurrentProfile, _Settingid, val);
        }

        private async void btnDirectApply_Click(object sender, EventArgs e)
        {
            ApplyValueToProfile(_CurrentValue);

            await CheckIfSettingIsStored();

            if (File.Exists(tbGamePath.Text))
            {
                Process.Start(tbGamePath.Text);
            }
        }

        private async Task CheckIfSettingIsStored()
        {
            await Task.Run(async () =>
            {
                while (_CurrentValue != DrsServiceLocator.SettingService
                .GetDwordValueFromProfile(_SettingsOwner._CurrentProfile, _Settingid, false, true))
                {
                    await Task.Delay(50);
                }
            });
        }

        private void btnBrowseGame_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.exe";
            ofd.Filter = "Applications|*.exe";
            ofd.DereferenceLinks = false;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbGamePath.Text = ofd.FileName;
            }
        }

    }
}
