#region

using System.Windows.Forms;

#endregion

namespace nspector.Common.Helper;

internal class NoBorderRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
    }
}