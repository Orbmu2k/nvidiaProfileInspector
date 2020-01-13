using System;
using System.Windows.Forms;

namespace nspector.Common.Helper
{

    //by Bryce Wagner https://stackoverflow.com/questions/13139074/mouse-wheel-scrolling-toolstrip-menu-items

    public class DropDownMenuScrollWheelHandler : IMessageFilter
    {
        private static DropDownMenuScrollWheelHandler Instance;
        public static void Enable(bool enabled)
        {
            if (enabled)
            {
                if (Instance == null)
                {
                    Instance = new DropDownMenuScrollWheelHandler();
                    Application.AddMessageFilter(Instance);
                }
            }
            else
            {
                if (Instance != null)
                {
                    Application.RemoveMessageFilter(Instance);
                    Instance = null;
                }
            }
        }
        private IntPtr activeHwnd;
        private ToolStripDropDown activeMenu;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x200 && activeHwnd != m.HWnd) // WM_MOUSEMOVE
            {
                activeHwnd = m.HWnd;
                this.activeMenu = Control.FromHandle(m.HWnd) as ToolStripDropDown;
            }
            else if (m.Msg == 0x20A && this.activeMenu != null) // WM_MOUSEWHEEL
            {
                int delta = (short)(ushort)(((uint)(ulong)m.WParam) >> 16);
                HandleDelta(this.activeMenu, delta);
                return true;
            }
            return false;
        }

        private static readonly Action<ToolStrip, int> ScrollInternal
            = (Action<ToolStrip, int>)Delegate.CreateDelegate(typeof(Action<ToolStrip, int>),
                typeof(ToolStrip).GetMethod("ScrollInternal",
                    System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance));

        private void HandleDelta(ToolStripDropDown ts, int delta)
        {
            if (ts.Items.Count == 0)
                return;

            var firstItem = ts.Items[0];
            var lastItem = ts.Items[ts.Items.Count - 1];
            
            if (lastItem.Bounds.Bottom < ts.Height && firstItem.Bounds.Top > 0)
                return;
            
            delta = delta / -4;
            
            if (delta < 0 && firstItem.Bounds.Top - delta > 9)
            {
                delta = firstItem.Bounds.Top - 9;
            }
            else if (delta > 0 && delta > lastItem.Bounds.Bottom - ts.Height + 9)
            {
                delta = lastItem.Bounds.Bottom - ts.Height + 9;
            }
            
            if (delta != 0)
                ScrollInternal(ts, delta);
        }
    }
}
