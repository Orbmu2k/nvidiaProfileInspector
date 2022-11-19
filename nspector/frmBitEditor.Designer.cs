namespace nspector
{
    partial class frmBitEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnClose = new System.Windows.Forms.Button();
            this.lValue = new System.Windows.Forms.Label();
            this.lFilter = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnDirectApplyStart = new System.Windows.Forms.Button();
            this.gbDirectTest = new System.Windows.Forms.GroupBox();
            this.btnBrowseGame = new System.Windows.Forms.Button();
            this.tbGamePath = new System.Windows.Forms.TextBox();
            this.lblGamePath = new System.Windows.Forms.Label();
            this.clbBits = new nspector.ListViewEx();
            this.chBit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chProfileCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chProfileNames = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.gbDirectTest.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(731, 645);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(106, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Apply && Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lValue
            // 
            this.lValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lValue.AutoSize = true;
            this.lValue.Location = new System.Drawing.Point(17, 650);
            this.lValue.Name = "lValue";
            this.lValue.Size = new System.Drawing.Size(37, 13);
            this.lValue.TabIndex = 2;
            this.lValue.Text = "Value:";
            // 
            // lFilter
            // 
            this.lFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lFilter.AutoSize = true;
            this.lFilter.Location = new System.Drawing.Point(150, 650);
            this.lFilter.Name = "lFilter";
            this.lFilter.Size = new System.Drawing.Size(64, 13);
            this.lFilter.TabIndex = 23;
            this.lFilter.Text = "Profile Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(219, 647);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(506, 20);
            this.tbFilter.TabIndex = 24;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(59, 647);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(70, 20);
            this.textBox1.TabIndex = 31;
            this.textBox1.Text = "0x00FF00FF";
            this.textBox1.Leave += new System.EventHandler(this.textBox1_Leave);
            this.textBox1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.textBox1_PreviewKeyDown);
            // 
            // btnDirectApplyStart
            // 
            this.btnDirectApplyStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDirectApplyStart.Location = new System.Drawing.Point(5, 15);
            this.btnDirectApplyStart.Name = "btnDirectApplyStart";
            this.btnDirectApplyStart.Size = new System.Drawing.Size(84, 34);
            this.btnDirectApplyStart.TabIndex = 32;
            this.btnDirectApplyStart.Text = "GO!";
            this.btnDirectApplyStart.UseVisualStyleBackColor = true;
            this.btnDirectApplyStart.Click += new System.EventHandler(this.btnDirectApply_Click);
            // 
            // gbDirectTest
            // 
            this.gbDirectTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDirectTest.Controls.Add(this.btnBrowseGame);
            this.gbDirectTest.Controls.Add(this.tbGamePath);
            this.gbDirectTest.Controls.Add(this.lblGamePath);
            this.gbDirectTest.Controls.Add(this.btnDirectApplyStart);
            this.gbDirectTest.Location = new System.Drawing.Point(14, 586);
            this.gbDirectTest.Name = "gbDirectTest";
            this.gbDirectTest.Size = new System.Drawing.Size(823, 53);
            this.gbDirectTest.TabIndex = 33;
            this.gbDirectTest.TabStop = false;
            this.gbDirectTest.Text = "Quick Bit Value Tester (stores this setting value to the current profile and imme" +
    "diately starts the game when successful)";
            // 
            // btnBrowseGame
            // 
            this.btnBrowseGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseGame.Location = new System.Drawing.Point(777, 19);
            this.btnBrowseGame.Name = "btnBrowseGame";
            this.btnBrowseGame.Size = new System.Drawing.Size(33, 23);
            this.btnBrowseGame.TabIndex = 35;
            this.btnBrowseGame.Text = "...";
            this.btnBrowseGame.UseVisualStyleBackColor = true;
            this.btnBrowseGame.Click += new System.EventHandler(this.btnBrowseGame_Click);
            // 
            // tbGamePath
            // 
            this.tbGamePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbGamePath.Location = new System.Drawing.Point(174, 21);
            this.tbGamePath.Name = "tbGamePath";
            this.tbGamePath.Size = new System.Drawing.Size(597, 20);
            this.tbGamePath.TabIndex = 34;
            // 
            // lblGamePath
            // 
            this.lblGamePath.AutoSize = true;
            this.lblGamePath.Location = new System.Drawing.Point(95, 23);
            this.lblGamePath.Name = "lblGamePath";
            this.lblGamePath.Size = new System.Drawing.Size(73, 13);
            this.lblGamePath.TabIndex = 33;
            this.lblGamePath.Text = "Game to start:";
            // 
            // clbBits
            // 
            this.clbBits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbBits.CheckBoxes = true;
            this.clbBits.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chBit,
            this.chName,
            this.chProfileCount,
            this.chProfileNames});
            this.clbBits.FullRowSelect = true;
            this.clbBits.GridLines = true;
            this.clbBits.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.clbBits.HideSelection = false;
            this.clbBits.Location = new System.Drawing.Point(10, 10);
            this.clbBits.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.clbBits.MultiSelect = false;
            this.clbBits.Name = "clbBits";
            this.clbBits.ShowGroups = false;
            this.clbBits.Size = new System.Drawing.Size(829, 572);
            this.clbBits.TabIndex = 34;
            this.clbBits.UseCompatibleStateImageBehavior = false;
            this.clbBits.View = System.Windows.Forms.View.Details;
            this.clbBits.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbBits_ItemCheck);
            // 
            // chBit
            // 
            this.chBit.Text = "Bit";
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 200;
            // 
            // chProfileCount
            // 
            this.chProfileCount.Text = "Count";
            // 
            // chProfileNames
            // 
            this.chProfileNames.Text = "Profiles";
            this.chProfileNames.Width = 4000;
            // 
            // frmBitEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(847, 678);
            this.Controls.Add(this.clbBits);
            this.Controls.Add(this.gbDirectTest);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.lFilter);
            this.Controls.Add(this.lValue);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(686, 495);
            this.Name = "frmBitEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bit Value Editor";
            this.Load += new System.EventHandler(this.frmBitEditor_Load);
            this.gbDirectTest.ResumeLayout(false);
            this.gbDirectTest.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lValue;
        private System.Windows.Forms.Label lFilter;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnDirectApplyStart;
        private System.Windows.Forms.GroupBox gbDirectTest;
        private System.Windows.Forms.Button btnBrowseGame;
        private System.Windows.Forms.TextBox tbGamePath;
        private System.Windows.Forms.Label lblGamePath;
        private ListViewEx clbBits;
        private System.Windows.Forms.ColumnHeader chBit;
        private System.Windows.Forms.ColumnHeader chProfileCount;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chProfileNames;
    }
}