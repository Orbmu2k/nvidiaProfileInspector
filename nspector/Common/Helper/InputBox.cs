namespace nspector.Common.Helper;

class InputBox
{
    internal static System.Windows.Forms.DialogResult Show(string title,string promptText,ref string value,
        System.Collections.Generic.List<string>                   invalidInputs,
        string                                                    mandatoryFormatRegExPattern,int maxLength)
    {
        var form        =new System.Windows.Forms.Form();
        var label       =new System.Windows.Forms.Label();
        var textBox     =new System.Windows.Forms.TextBox();
        var buttonOk    =new System.Windows.Forms.Button();
        var buttonCancel=new System.Windows.Forms.Button();
        var imageBox    =new System.Windows.Forms.PictureBox();

        System.EventHandler textchanged=delegate
        {
            var mandatory_success
                =System.Text.RegularExpressions.Regex.IsMatch(textBox.Text,mandatoryFormatRegExPattern);

            if(textBox.Text==""||textBox.Text.Length>maxLength||!mandatory_success)
            {
                imageBox.Image  =nspector.Properties.Resources.ieframe_1_18212;
                buttonOk.Enabled=false;
                return;
            }

            foreach(var invStr in invalidInputs)
            {
                if(textBox.Text.ToUpper()==invStr.ToUpper())
                {
                    imageBox.Image  =nspector.Properties.Resources.ieframe_1_18212;
                    buttonOk.Enabled=false;
                    return;
                }
            }

            imageBox.Image  =nspector.Properties.Resources.ieframe_1_31073_002;
            buttonOk.Enabled=true;
        };


        textBox.TextChanged+=textchanged;


        form.Text        =title;
        label.Text       =promptText;
        textBox.Text     =value;
        textBox.MaxLength=maxLength;
        imageBox.Image   =nspector.Properties.Resources.ieframe_1_18212;

        buttonOk.Text            ="OK";
        buttonCancel.Text        ="Cancel";
        buttonOk.DialogResult    =System.Windows.Forms.DialogResult.OK;
        buttonCancel.DialogResult=System.Windows.Forms.DialogResult.Cancel;

        buttonOk.Enabled=false;

        label.SetBounds(9,20,372,13);
        textBox.SetBounds(12,36,352,20);
        buttonOk.SetBounds(228,72,75,23);
        buttonCancel.SetBounds(309,72,75,23);

        imageBox.SetBounds(368,36,16,16);

        label.AutoSize     =true;
        imageBox.Anchor    =System.Windows.Forms.AnchorStyles.Top   |System.Windows.Forms.AnchorStyles.Right;
        textBox.Anchor     =textBox.Anchor                          |System.Windows.Forms.AnchorStyles.Right;
        buttonOk.Anchor    =System.Windows.Forms.AnchorStyles.Bottom|System.Windows.Forms.AnchorStyles.Right;
        buttonCancel.Anchor=System.Windows.Forms.AnchorStyles.Bottom|System.Windows.Forms.AnchorStyles.Right;

        form.ClientSize=new System.Drawing.Size(396,107);
        form.Controls.AddRange(new System.Windows.Forms.Control[]
        {
            label,textBox,imageBox,buttonOk,buttonCancel,
        });
        form.ClientSize     =new System.Drawing.Size(System.Math.Max(300,label.Right+10),form.ClientSize.Height);
        form.FormBorderStyle=System.Windows.Forms.FormBorderStyle.FixedDialog;
        form.StartPosition  =System.Windows.Forms.FormStartPosition.CenterParent;
        form.MinimizeBox    =false;
        form.MaximizeBox    =false;
        form.AcceptButton   =buttonOk;
        form.CancelButton   =buttonCancel;

        textchanged(form,new System.EventArgs());

        var dialogResult=form.ShowDialog();
        value=textBox.Text;
        return dialogResult;
    }
}