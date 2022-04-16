using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace nspector;

internal delegate void DropFilesNativeHandler(string[] files);

internal class ListViewEx : ListView
{
    private const int LVM_FIRST = 0x1000;
    private const int LVM_GETCOLUMNORDERARRAY = LVM_FIRST + 59;

    private const int WM_PAINT = 0x000F;
    private const int WM_DROPFILES = 0x233;

    private readonly ArrayList _embeddedControls = new();

    public ListViewEx()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.EnableNotifyMessage, true);
    }

    [DefaultValue(View.LargeIcon)]
    internal new View View
    {
        get => base.View;
        set
        {
            foreach (EmbeddedControl ec in _embeddedControls)
                ec.Control.Visible = value == View.Details;

            base.View = value;
        }
    }

    public event DropFilesNativeHandler OnDropFilesNative;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wPar, IntPtr lPar);

    protected override void OnNotifyMessage(Message m)
    {
        if (m.Msg != 0x14)
            base.OnNotifyMessage(m);
    }

    protected int[] GetColumnOrder()
    {
        var lPar = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * Columns.Count);

        var res = SendMessage(Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(Columns.Count), lPar);
        if (res.ToInt32() == 0)
        {
            Marshal.FreeHGlobal(lPar);
            return null;
        }

        var order = new int[Columns.Count];
        Marshal.Copy(lPar, order, 0, Columns.Count);

        Marshal.FreeHGlobal(lPar);

        return order;
    }

    protected Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
    {
        var subItemRect = Rectangle.Empty;

        if (Item == null)
            throw new ArgumentNullException("Item");

        var order = GetColumnOrder();
        if (order == null) // No Columns
            return subItemRect;

        if (SubItem >= order.Length)
            throw new IndexOutOfRangeException("SubItem " + SubItem + " out of range");

        var lviBounds = Item.GetBounds(ItemBoundsPortion.Entire);
        var subItemX = lviBounds.Left;

        ColumnHeader col;
        int i;
        for (i = 0; i < order.Length; i++)
        {
            col = Columns[order[i]];
            if (col.Index == SubItem)
                break;
            subItemX += col.Width;
        }

        subItemRect = new Rectangle(subItemX, lviBounds.Top - 1, Columns[order[i]].Width, lviBounds.Height);

        return subItemRect;
    }

    internal void AddEmbeddedControl(Control c, int col, int row)
    {
        AddEmbeddedControl(c, col, row, DockStyle.Fill);
    }

    internal void AddEmbeddedControl(Control c, int col, int row, DockStyle dock)
    {
        if (c == null)
            throw new ArgumentNullException();
        if (col >= Columns.Count || row >= Items.Count)
            throw new ArgumentOutOfRangeException();

        EmbeddedControl ec;
        ec.Control = c;
        ec.Column = col;
        ec.Row = row;
        ec.Dock = dock;
        ec.Item = Items[row];

        _embeddedControls.Add(ec);

        c.Click += _embeddedControl_Click;

        Controls.Add(c);
    }

    internal void RemoveEmbeddedControl(Control c)
    {
        if (c == null)
            throw new ArgumentNullException();

        for (var i = 0; i < _embeddedControls.Count; i++)
        {
            var ec = (EmbeddedControl) _embeddedControls[i];
            if (ec.Control == c)
            {
                c.Click -= _embeddedControl_Click;
                Controls.Remove(c);
                _embeddedControls.RemoveAt(i);
                return;
            }
        }
    }

    internal Control GetEmbeddedControl(int col, int row)
    {
        foreach (EmbeddedControl ec in _embeddedControls)
            if (ec.Row == row && ec.Column == col)
                return ec.Control;

        return null;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern int DragQueryFile(IntPtr hDrop, uint iFile, [Out] StringBuilder lpszFile, int cch);

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_PAINT:
                if (View != View.Details)
                    break;

                foreach (EmbeddedControl ec in _embeddedControls)
                {
                    var rc = GetSubItemBounds(ec.Item, ec.Column);

                    if (HeaderStyle != ColumnHeaderStyle.None &&
                        rc.Top < Font.Height)
                    {
                        ec.Control.Visible = false;
                        continue;
                    }

                    ec.Control.Visible = true;

                    switch (ec.Dock)
                    {
                        case DockStyle.Fill:
                            break;
                        case DockStyle.Top:
                            rc.Height = ec.Control.Height;
                            break;
                        case DockStyle.Left:
                            rc.Width = ec.Control.Width;
                            break;
                        case DockStyle.Bottom:
                            rc.Offset(0, rc.Height - ec.Control.Height);
                            rc.Height = ec.Control.Height;
                            break;
                        case DockStyle.Right:
                            rc.Offset(rc.Width - ec.Control.Width, 0);
                            rc.Width = ec.Control.Width;
                            break;
                        case DockStyle.None:
                            rc.Size = ec.Control.Size;
                            break;
                    }


                    rc.X = rc.X + ec.Control.Margin.Left;
                    rc.Y = rc.Y + ec.Control.Margin.Top;
                    rc.Width = rc.Width - ec.Control.Margin.Right;
                    rc.Height = rc.Height - ec.Control.Margin.Bottom;

                    ec.Control.Bounds = rc;
                }

                break;

            case WM_DROPFILES:

                if (OnDropFilesNative != null)
                {
                    var dropped = DragQueryFile(m.WParam, 0xFFFFFFFF, null, 0);
                    if (dropped > 0)
                    {
                        var files = new List<string>();

                        for (uint i = 0; i < dropped; i++)
                        {
                            var size = DragQueryFile(m.WParam, i, null, 0);
                            if (size > 0)
                            {
                                var sb = new StringBuilder(size + 1);
                                var result = DragQueryFile(m.WParam, i, sb, size + 1);
                                files.Add(sb.ToString());
                            }
                        }

                        OnDropFilesNative(files.ToArray());
                    }
                }

                base.WndProc(ref m);
                break;
        }

        base.WndProc(ref m);
    }

    private void _embeddedControl_Click(object sender, EventArgs e)
    {
        foreach (EmbeddedControl ec in _embeddedControls)
            if (ec.Control == (Control) sender)
            {
                SelectedItems.Clear();
                ec.Item.Selected = true;
            }
    }

    private struct EmbeddedControl
    {
        internal Control Control;
        internal int Column;
        internal int Row;
        internal DockStyle Dock;
        internal ListViewItem Item;
    }
}