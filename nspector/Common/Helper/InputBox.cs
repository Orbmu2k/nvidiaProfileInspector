using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace nspector.Common.Helper;

internal class InputBox
{
    internal static DialogResult Show(string title, string promptText, ref string value, List<string> invalidInputs, string mandatoryFormatRegExPattern, int maxLength)
    {
        var form = new Form();
        var label = new Label();
        var textBox = new TextBox();
        var buttonOk = new Button();
        var buttonCancel = new Button();
        var imageBox = new PictureBox();

        EventHandler textchanged = delegate
        {
            var mandatory_success = Regex.IsMatch(textBox.Text, mandatoryFormatRegExPattern);

            if (textBox.Text == "" || textBox.Text.Length > maxLength || !mandatory_success)
            {
                imageBox.Image = Properties.Resources.ieframe_1_18212;
                buttonOk.Enabled = false;
                return;
            }

            foreach (var invStr in invalidInputs)
                if (textBox.Text.ToUpper() == invStr.ToUpper())
                {
                    imageBox.Image = Properties.Resources.ieframe_1_18212;
                    buttonOk.Enabled = false;
                    return;
                }

            imageBox.Image = Properties.Resources.ieframe_1_31073_002;
            buttonOk.Enabled = true;
        };


        textBox.TextChanged += textchanged;


        form.Text = title;
        label.Text = promptText;
        textBox.Text = value;
        textBox.MaxLength = maxLength;
        imageBox.Image = Properties.Resources.ieframe_1_18212;

        buttonOk.Text = "OK";
        buttonCancel.Text = "Cancel";
        buttonOk.DialogResult = DialogResult.OK;
        buttonCancel.DialogResult = DialogResult.Cancel;

        buttonOk.Enabled = false;

        label.SetBounds(9, 20, 372, 13);
        textBox.SetBounds(12, 36, 352, 20);
        buttonOk.SetBounds(228, 72, 75, 23);
        buttonCancel.SetBounds(309, 72, 75, 23);

        imageBox.SetBounds(368, 36, 16, 16);

        label.AutoSize = true;
        imageBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        form.ClientSize = new Size(396, 107);
        form.Controls.AddRange(new Control[]
        {
            label, textBox, imageBox, buttonOk, buttonCancel
        });
        form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterParent;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;

        textchanged(form, new EventArgs());

        var dialogResult = form.ShowDialog();
        value = textBox.Text;
        return dialogResult;
    }
}
