using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace nspector
{
    public class WatermarkTextBox : TextBox
    {
        private const int WM_PAINT = 0x000F;

        private string _watermarkText;
        [Category("Appearance")]
        public string WatermarkText
        {
            get => _watermarkText;
            set { _watermarkText = value; Invalidate(); }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_PAINT && string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(_watermarkText))
            {
                using (Graphics g = this.CreateGraphics())
                using (Brush brush = new SolidBrush(SystemColors.GrayText))
                {
                    TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                    TextRenderer.DrawText(g, _watermarkText, this.Font, this.ClientRectangle, SystemColors.GrayText, flags);
                }
            }
        }
    }
}
