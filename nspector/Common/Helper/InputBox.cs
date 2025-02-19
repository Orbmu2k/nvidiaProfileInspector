using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace nspector.Common.Helper
{
    internal class InputBox
    {

        internal static DialogResult Show(string title, string promptText, ref string value, List<string> invalidInputs, string mandatoryFormatRegExPattern, int maxLength, bool allowExeBrowse = false)
        {
            var form = new Form();
            var label = new Label();
            var textBox = new TextBox();
            var buttonOk = new Button();
            var buttonCancel = new Button();
            var buttonBrowse = new Button();
            var imageBox = new PictureBox();

            EventHandler textchanged = delegate (object sender, EventArgs e)
            {
                bool mandatory_success = Regex.IsMatch(textBox.Text, mandatoryFormatRegExPattern);

                if (textBox.Text == "" || textBox.Text.Length > maxLength || !mandatory_success)
                {
                    imageBox.Image = nspector.Properties.Resources.ieframe_1_18212;
                    buttonOk.Enabled = false;
                    return;
                }

                foreach (string invStr in invalidInputs)
                {
                    if (textBox.Text.ToUpper() == invStr.ToUpper())
                    {
                        imageBox.Image = Properties.Resources.ieframe_1_18212;
                        buttonOk.Enabled = false;
                        return;
                    }
                }

                imageBox.Image = Properties.Resources.ieframe_1_31073_002;
                buttonOk.Enabled = true;
            };

            EventHandler buttonBrowse_Click = delegate (object sender, EventArgs e)
            {
                var openDialog = new OpenFileDialog();
                openDialog.DefaultExt = "*.exe";
                openDialog.Filter = "Application EXE Name|*.exe|Application Absolute Path|*.exe";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    string applicationName = new FileInfo(openDialog.FileName).Name;
                    if (openDialog.FilterIndex == 2)
                        applicationName = openDialog.FileName;
                    textBox.Text = applicationName;
                }
            };

            textBox.TextChanged += textchanged;

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            textBox.MaxLength = maxLength;
            imageBox.Image = Properties.Resources.ieframe_1_18212;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonBrowse.Text = "Browse...";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            buttonOk.Enabled = false;

            label.SetBounds(Dpi(9), Dpi(20), Dpi(372), Dpi(13));
            textBox.SetBounds(Dpi(12), Dpi(44), Dpi(352), Dpi(20));
            buttonOk.SetBounds(Dpi(224), Dpi(72), Dpi(75), Dpi(23));
            buttonCancel.SetBounds(Dpi(305), Dpi(72), Dpi(75), Dpi(23));

            if (allowExeBrowse)
            {
                textBox.SetBounds(Dpi(12), Dpi(44), Dpi(286), Dpi(20));
                buttonBrowse.SetBounds(Dpi(305), Dpi(39), Dpi(75), Dpi(23));
                buttonBrowse.Click += buttonBrowse_Click;
            }

            imageBox.SetBounds(Dpi(368), Dpi(44), Dpi(16), Dpi(16));

            label.AutoSize = true;
            label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            imageBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonBrowse.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(Dpi(396), Dpi(107));
            form.ClientSize = new Size(Math.Max(Dpi(300), label.Right + Dpi(10)), form.ClientSize.Height);
            form.MinimumSize = form.Size;
            form.MaximumSize = new Size(form.MinimumSize.Width * 2, form.MinimumSize.Height);

            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            if (!allowExeBrowse)
                form.Controls.Add(imageBox);
            else
                form.Controls.Add(buttonBrowse);

            form.ShowIcon = false;
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            textchanged(form, new EventArgs());

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private static int Dpi(int input)
        {
            return (int)Math.Round(input * frmDrvSettings.ScaleFactor);
        }
    }
}
