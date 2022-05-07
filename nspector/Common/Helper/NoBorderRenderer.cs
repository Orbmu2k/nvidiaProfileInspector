namespace nspector.Common.Helper;

class NoBorderRenderer:System.Windows.Forms.ToolStripProfessionalRenderer
{
    protected override void OnRenderToolStripBackground(System.Windows.Forms.ToolStripRenderEventArgs e) {}

    protected override void OnRenderToolStripBorder(System.Windows.Forms.ToolStripRenderEventArgs e) {}
}