namespace nspector;

delegate void DropFilesNativeHandler(string[] files);

class ListViewEx:System.Windows.Forms.ListView
{
    const int LVM_FIRST              =0x1000;
    const int LVM_GETCOLUMNORDERARRAY=ListViewEx.LVM_FIRST+59;

    const int WM_PAINT    =0x000F;
    const int WM_DROPFILES=0x233;

    readonly System.Collections.ArrayList _embeddedControls=new System.Collections.ArrayList();

    public ListViewEx()
    {
        this.SetStyle(
            System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer
            |System.Windows.Forms.ControlStyles.AllPaintingInWmPaint,true);
        this.SetStyle(System.Windows.Forms.ControlStyles.EnableNotifyMessage,true);
    }

    [System.ComponentModel.DefaultValueAttribute(System.Windows.Forms.View.LargeIcon)]
    internal new System.Windows.Forms.View View
    {
        get
        {
            return base.View;
        }
        set
        {
            foreach(EmbeddedControl ec in this._embeddedControls)
            {
                ec.Control.Visible=value==System.Windows.Forms.View.Details;
            }

            base.View=value;
        }
    }

    public event DropFilesNativeHandler OnDropFilesNative;

    [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
    static extern System.IntPtr SendMessage(System.IntPtr hWnd,int msg,System.IntPtr wPar,System.IntPtr lPar);

    protected override void OnNotifyMessage(System.Windows.Forms.Message m)
    {
        if(m.Msg!=0x14)
        {
            base.OnNotifyMessage(m);
        }
    }

    protected int[] GetColumnOrder()
    {
        var lPar=System.Runtime.InteropServices.Marshal.AllocHGlobal(
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(int))*this.Columns.Count);

        var res=ListViewEx.SendMessage(this.Handle,ListViewEx.LVM_GETCOLUMNORDERARRAY,
            new System.IntPtr(this.Columns.Count),lPar);
        if(res.ToInt32()==0)
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(lPar);
            return null;
        }

        var order=new int[this.Columns.Count];
        System.Runtime.InteropServices.Marshal.Copy(lPar,order,0,this.Columns.Count);

        System.Runtime.InteropServices.Marshal.FreeHGlobal(lPar);

        return order;
    }

    protected System.Drawing.Rectangle GetSubItemBounds(System.Windows.Forms.ListViewItem Item,int SubItem)
    {
        var subItemRect=System.Drawing.Rectangle.Empty;

        if(Item==null)
        {
            throw new System.ArgumentNullException("Item");
        }

        var order=this.GetColumnOrder();
        if(order==null)// No Columns
        {
            return subItemRect;
        }

        if(SubItem>=order.Length)
        {
            throw new System.IndexOutOfRangeException("SubItem "+SubItem+" out of range");
        }

        var lviBounds=Item.GetBounds(System.Windows.Forms.ItemBoundsPortion.Entire);
        var subItemX =lviBounds.Left;

        System.Windows.Forms.ColumnHeader col;
        int                               i;
        for(i=0;i<order.Length;i++)
        {
            col=this.Columns[order[i]];
            if(col.Index==SubItem)
            {
                break;
            }

            subItemX+=col.Width;
        }

        subItemRect=new System.Drawing.Rectangle(subItemX,lviBounds.Top-1,this.Columns[order[i]].Width,
            lviBounds.Height);

        return subItemRect;
    }

    internal void AddEmbeddedControl(System.Windows.Forms.Control c,int col,int row)
    {
        this.AddEmbeddedControl(c,col,row,System.Windows.Forms.DockStyle.Fill);
    }

    internal void AddEmbeddedControl(System.Windows.Forms.Control c,int col,int row,System.Windows.Forms.DockStyle dock)
    {
        if(c==null)
        {
            throw new System.ArgumentNullException();
        }

        if(col>=this.Columns.Count||row>=this.Items.Count)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        EmbeddedControl ec;
        ec.Control=c;
        ec.Column =col;
        ec.Row    =row;
        ec.Dock   =dock;
        ec.Item   =this.Items[row];

        this._embeddedControls.Add(ec);

        c.Click+=this._embeddedControl_Click;

        this.Controls.Add(c);
    }

    internal void RemoveEmbeddedControl(System.Windows.Forms.Control c)
    {
        if(c==null)
        {
            throw new System.ArgumentNullException();
        }

        for(var i=0;i<this._embeddedControls.Count;i++)
        {
            var ec=(EmbeddedControl)this._embeddedControls[i];
            if(ec.Control==c)
            {
                c.Click-=this._embeddedControl_Click;
                this.Controls.Remove(c);
                this._embeddedControls.RemoveAt(i);
                return;
            }
        }
    }

    internal System.Windows.Forms.Control GetEmbeddedControl(int col,int row)
    {
        foreach(EmbeddedControl ec in this._embeddedControls)
        {
            if(ec.Row==row&&ec.Column==col)
            {
                return ec.Control;
            }
        }

        return null;
    }

    [System.Runtime.InteropServices.DllImportAttribute("shell32.dll",
        CharSet=System.Runtime.InteropServices.CharSet.Auto)]
    public static extern int DragQueryFile(System.IntPtr                       hDrop,   uint iFile,
        [System.Runtime.InteropServices.OutAttribute]System.Text.StringBuilder lpszFile,int  cch);

    protected override void WndProc(ref System.Windows.Forms.Message m)
    {
        switch(m.Msg)
        {
            case ListViewEx.WM_PAINT:
                if(this.View!=System.Windows.Forms.View.Details)
                {
                    break;
                }

                foreach(EmbeddedControl ec in this._embeddedControls)
                {
                    var rc=this.GetSubItemBounds(ec.Item,ec.Column);

                    if(this.HeaderStyle!=System.Windows.Forms.ColumnHeaderStyle.None&&
                        rc.Top         <this.Font.Height)
                    {
                        ec.Control.Visible=false;
                        continue;
                    }

                    ec.Control.Visible=true;

                    switch(ec.Dock)
                    {
                        case System.Windows.Forms.DockStyle.Fill:
                            break;
                        case System.Windows.Forms.DockStyle.Top:
                            rc.Height=ec.Control.Height;
                            break;
                        case System.Windows.Forms.DockStyle.Left:
                            rc.Width=ec.Control.Width;
                            break;
                        case System.Windows.Forms.DockStyle.Bottom:
                            rc.Offset(0,rc.Height-ec.Control.Height);
                            rc.Height=ec.Control.Height;
                            break;
                        case System.Windows.Forms.DockStyle.Right:
                            rc.Offset(rc.Width-ec.Control.Width,0);
                            rc.Width=ec.Control.Width;
                            break;
                        case System.Windows.Forms.DockStyle.None:
                            rc.Size=ec.Control.Size;
                            break;
                    }


                    rc.X     =rc.X     +ec.Control.Margin.Left;
                    rc.Y     =rc.Y     +ec.Control.Margin.Top;
                    rc.Width =rc.Width -ec.Control.Margin.Right;
                    rc.Height=rc.Height-ec.Control.Margin.Bottom;

                    ec.Control.Bounds=rc;
                }

                break;

            case ListViewEx.WM_DROPFILES:

                if(this.OnDropFilesNative!=null)
                {
                    var dropped=ListViewEx.DragQueryFile(m.WParam,0xFFFFFFFF,null,0);
                    if(dropped>0)
                    {
                        var files=new System.Collections.Generic.List<string>();

                        for(uint i=0;i<dropped;i++)
                        {
                            var size=ListViewEx.DragQueryFile(m.WParam,i,null,0);
                            if(size>0)
                            {
                                var sb    =new System.Text.StringBuilder(size         +1);
                                var result=ListViewEx.DragQueryFile(m.WParam,i,sb,size+1);
                                files.Add(sb.ToString());
                            }
                        }

                        this.OnDropFilesNative(files.ToArray());
                    }
                }

                base.WndProc(ref m);
                break;
        }

        base.WndProc(ref m);
    }

    void _embeddedControl_Click(object sender,System.EventArgs e)
    {
        foreach(EmbeddedControl ec in this._embeddedControls)
        {
            if(ec.Control==(System.Windows.Forms.Control)sender)
            {
                this.SelectedItems.Clear();
                ec.Item.Selected=true;
            }
        }
    }

    struct EmbeddedControl
    {
        internal System.Windows.Forms.Control      Control;
        internal int                               Column;
        internal int                               Row;
        internal System.Windows.Forms.DockStyle    Dock;
        internal System.Windows.Forms.ListViewItem Item;
    }
}