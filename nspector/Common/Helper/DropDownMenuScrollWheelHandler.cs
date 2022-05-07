namespace nspector.Common.Helper;

//by Bryce Wagner https://stackoverflow.com/questions/13139074/mouse-wheel-scrolling-toolstrip-menu-items
public class DropDownMenuScrollWheelHandler:System.Windows.Forms.IMessageFilter
{
    static DropDownMenuScrollWheelHandler Instance;

    static readonly System.Action<System.Windows.Forms.ToolStrip,int> ScrollInternal
        =(System.Action<System.Windows.Forms.ToolStrip,int>)System.Delegate.CreateDelegate(
            typeof(System.Action<System.Windows.Forms.ToolStrip,int>),
            typeof(System.Windows.Forms.ToolStrip).GetMethod("ScrollInternal",
                System.Reflection.BindingFlags.NonPublic
                |System.Reflection.BindingFlags.Instance));

    System.IntPtr                          activeHwnd;
    System.Windows.Forms.ToolStripDropDown activeMenu;

    public bool PreFilterMessage(ref System.Windows.Forms.Message m)
    {
        if(m.Msg==0x200&&this.activeHwnd!=m.HWnd)// WM_MOUSEMOVE
        {
            this.activeHwnd=m.HWnd;
            this.activeMenu=System.Windows.Forms.Control.FromHandle(m.HWnd) as System.Windows.Forms.ToolStripDropDown;
        }
        else if(m.Msg==0x20A&&this.activeMenu!=null)// WM_MOUSEWHEEL
        {
            int delta=(short)(ushort)((uint)(ulong)m.WParam>> 16);
            this.HandleDelta(this.activeMenu,delta);
            return true;
        }

        return false;
    }

    public static void Enable(bool enabled)
    {
        if(enabled)
        {
            if(DropDownMenuScrollWheelHandler.Instance==null)
            {
                DropDownMenuScrollWheelHandler.Instance=new DropDownMenuScrollWheelHandler();
                System.Windows.Forms.Application.AddMessageFilter(DropDownMenuScrollWheelHandler.Instance);
            }
        }
        else
        {
            if(DropDownMenuScrollWheelHandler.Instance!=null)
            {
                System.Windows.Forms.Application.RemoveMessageFilter(DropDownMenuScrollWheelHandler.Instance);
                DropDownMenuScrollWheelHandler.Instance=null;
            }
        }
    }

    void HandleDelta(System.Windows.Forms.ToolStripDropDown ts,int delta)
    {
        if(ts.Items.Count==0)
        {
            return;
        }

        var firstItem=ts.Items[0];
        var lastItem =ts.Items[ts.Items.Count-1];

        if(lastItem.Bounds.Bottom<ts.Height&&firstItem.Bounds.Top>0)
        {
            return;
        }

        delta=delta/-4;

        if(delta<0&&firstItem.Bounds.Top-delta>9)
        {
            delta=firstItem.Bounds.Top-9;
        }
        else if(delta>0&&delta>lastItem.Bounds.Bottom-ts.Height+9)
        {
            delta=lastItem.Bounds.Bottom-ts.Height+9;
        }

        if(delta!=0)
        {
            DropDownMenuScrollWheelHandler.ScrollInternal(ts,delta);
        }
    }
}