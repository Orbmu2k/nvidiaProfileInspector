#region

using Enumerable=System.Linq.Enumerable;

#endregion

namespace nspector;

partial class frmBitEditor:System.Windows.Forms.Form
{
    uint           _CurrentValue;
    uint           _InitValue;
    uint           _Settingid;
    frmDrvSettings _SettingsOwner;


    internal frmBitEditor()
    {
        this.InitializeComponent();
        this.Icon          =System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
        this.DoubleBuffered=true;
    }

    internal void ShowDialog(frmDrvSettings SettingsOwner,uint SettingId,uint InitValue,string SettingName)
    {
        this._Settingid    =SettingId;
        this._SettingsOwner=SettingsOwner;
        this._InitValue    =InitValue;
        this.Text          =string.Format("Bit Value Editor - {0}",SettingName);
        this.ShowDialog(SettingsOwner);
    }

    void frmBitEditor_Load(object sender,System.EventArgs e)
    {
        this.SplitBitsFromUnknownSettings();
        this.clbBits.AutoResizeColumns(System.Windows.Forms.ColumnHeaderAutoResizeStyle.ColumnContent);
        this.SetValue(this._InitValue);
    }

    void SplitBitsFromUnknownSettings()
    {
        uint lastValue=0;
        lastValue=this._CurrentValue;
        var filters=this.tbFilter.Text.Split(',');
        this.clbBits.Items.Clear();

        var referenceSettings=
            Enumerable.FirstOrDefault(nspector.Common.DrsServiceLocator.ReferenceSettings?.Settings,
                s=>s.SettingId==this._Settingid);

        var settingsCache=
            Enumerable.FirstOrDefault(nspector.Common.DrsServiceLocator.ScannerService.CachedSettings,
                x=>x.SettingId==this._Settingid);
        if(settingsCache!=null)
        {
            for(var bit=0;bit<32;bit++)
            {
                var  profileNames="";
                uint profileCount=0;

                for(var i=0;i<settingsCache.SettingValues.Count;i++)
                {
                    if((settingsCache.SettingValues[i].Value>> bit&0x1)==0x1)
                    {
                        if(filters.Length==0)
                        {
                            profileNames+=settingsCache.SettingValues[i].ProfileNames+",";
                        }
                        else
                        {
                            var settingProfileNames=settingsCache.SettingValues[i].ProfileNames.ToString().Split(',');
                            for(var p=0;p<settingProfileNames.Length;p++)
                            for(var f=0;f<filters.Length;f++)
                            {
                                if(settingProfileNames[p].ToLower().Contains(filters[f].ToLower()))
                                {
                                    profileNames+=settingProfileNames[p]+",";
                                }
                            }
                        }

                        profileCount+=settingsCache.SettingValues[i].ValueProfileCount;
                    }
                }

                var mask   =(uint)1<<bit;
                var maskStr="";

                if(referenceSettings!=null)
                {
                    var maskValue=Enumerable.FirstOrDefault(referenceSettings.SettingValues,v=>v.SettingValue==mask);
                    if(maskValue!=null)
                    {
                        maskStr=maskValue.UserfriendlyName;
                        if(maskStr.Contains("("))
                        {
                            maskStr=maskStr.Substring(0,maskStr.IndexOf("(")-1);
                        }
                    }
                }

                this.clbBits.Items.Add(new System.Windows.Forms.ListViewItem(new[]
                {
                    string.Format("#{0:00}",bit),maskStr,profileCount.ToString(),profileNames,
                }));
            }
        }

        this.SetValue(lastValue);
    }

    void updateValue(bool changeState,int changedIndex)
    {
        uint val=0;
        for(var b=0;b<this.clbBits.Items.Count;b++)
        {
            if(this.clbBits.Items[b].Checked&&changedIndex!=b||changeState&&changedIndex==b)
            {
                val=val|(uint)(1<<b);
            }
        }

        this.UpdateCurrent(val);
    }

    void UpdateValue()
    {
        uint val=0;
        for(var b=0;b<this.clbBits.Items.Count;b++)
        {
            if(this.clbBits.Items[b].Checked)
            {
                val=val|(uint)(1<<b);
            }
        }

        this.UpdateCurrent(val);
    }


    void SetValue(uint val)
    {
        for(var b=0;b<this.clbBits.Items.Count;b++)
        {
            if((val>> b&0x1)==0x1)
            {
                this.clbBits.Items[b].Checked=true;
            }
            else
            {
                this.clbBits.Items[b].Checked=false;
            }
        }

        this.UpdateValue();
    }

    void UpdateCurrent(uint val)
    {
        this._CurrentValue=val;
        this.textBox1.Text="0x"+val.ToString("X8");
    }

    void UpdateCurrent(string text)
    {
        var val=nspector.Common.DrsUtil.ParseDwordByInputSafe(text);
        this.UpdateCurrent(val);
        this.SetValue(val);
    }

    void clbBits_ItemCheck(object sender,System.Windows.Forms.ItemCheckEventArgs e)
    {
        this.updateValue(e.NewValue==System.Windows.Forms.CheckState.Checked,e.Index);
    }

    void btnClose_Click(object sender,System.EventArgs e)
    {
        this._SettingsOwner.SetSelectedDwordValue(this._CurrentValue);
        this.Close();
    }

    void tbFilter_TextChanged(object sender,System.EventArgs e)
    {
        this.SplitBitsFromUnknownSettings();
    }

    void numericUpDown1_ValueChanged(object sender,System.EventArgs e)
    {
        this.SplitBitsFromUnknownSettings();
    }

    void textBox1_PreviewKeyDown(object sender,System.Windows.Forms.PreviewKeyDownEventArgs e)
    {
        if(e.KeyValue==13)
        {
            this.UpdateCurrent(this.textBox1.Text);
        }
    }

    void textBox1_Leave(object sender,System.EventArgs e)
    {
        this.UpdateCurrent(this.textBox1.Text);
    }


    void ApplyValueToProfile(uint val)
    {
        nspector.Common.DrsServiceLocator
            .SettingService
            .SetDwordValueToProfile(this._SettingsOwner._CurrentProfile,this._Settingid,val);
    }

    async void btnDirectApply_Click(object sender,System.EventArgs e)
    {
        this.ApplyValueToProfile(this._CurrentValue);

        await this.CheckIfSettingIsStored();

        if(System.IO.File.Exists(this.tbGamePath.Text))
        {
            System.Diagnostics.Process.Start(this.tbGamePath.Text);
        }
    }

    async System.Threading.Tasks.Task CheckIfSettingIsStored()
    {
        await System.Threading.Tasks.Task.Run(async ()=>
        {
            while(this._CurrentValue!=nspector.Common.DrsServiceLocator.SettingService
                .GetDwordValueFromProfile(this._SettingsOwner._CurrentProfile,this._Settingid,false,true))
            {
                await System.Threading.Tasks.Task.Delay(50);
            }
        });
    }

    void btnBrowseGame_Click(object sender,System.EventArgs e)
    {
        var ofd=new System.Windows.Forms.OpenFileDialog();
        ofd.DefaultExt      ="*.exe";
        ofd.Filter          ="Applications|*.exe";
        ofd.DereferenceLinks=false;

        if(ofd.ShowDialog()==System.Windows.Forms.DialogResult.OK)
        {
            this.tbGamePath.Text=ofd.FileName;
        }
    }
}