namespace Scada.Comm.Drivers.DrvPulsar.View
{
    partial class FrmDeviceProps
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
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            toolStrip = new ToolStrip();
            txtTemplateFileName = new ToolStripTextBox();
            btnOpen = new ToolStripButton();
            btnSave = new ToolStripButton();
            btnSaveAs = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnMoveUp = new ToolStripButton();
            btnMoveDown = new ToolStripButton();
            btnValidate = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            tsLabel = new ToolStripLabel();
            txtName = new ToolStripTextBox();
            dgvComm = new DataGridView();
            btnCancel = new Button();
            btnOK = new Button();
            tabControl1 = new TabControl();
            tabSnd = new TabPage();
            dgvVal = new DataGridView();
            dgvSend = new DataGridView();
            tabCmd = new TabPage();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvComm).BeginInit();
            tabControl1.SuspendLayout();
            tabSnd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvSend).BeginInit();
            tabCmd.SuspendLayout();
            SuspendLayout();
            // 
            // openFileDialog
            // 
            openFileDialog.FileName = "openFileDialog1";
            // 
            // toolStrip
            // 
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { txtTemplateFileName, btnOpen, btnSave, btnSaveAs, toolStripSeparator1, btnMoveUp, btnMoveDown, btnValidate, toolStripSeparator2, tsLabel, txtName });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(884, 25);
            toolStrip.TabIndex = 2;
            toolStrip.Text = "toolStrip1";
            // 
            // txtTemplateFileName
            // 
            txtTemplateFileName.Name = "txtTemplateFileName";
            txtTemplateFileName.Size = new Size(176, 25);
            // 
            // btnOpen
            // 
            btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnOpen.Image = global::DrvPulsar.View.Properties.Resources.open;
            btnOpen.ImageScaling = ToolStripItemImageScaling.None;
            btnOpen.ImageTransparentColor = Color.Magenta;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(23, 22);
            btnOpen.ToolTipText = "Open Template";
            btnOpen.Click += btnOpen_Click;
            // 
            // btnSave
            // 
            btnSave.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnSave.Image = global::DrvPulsar.View.Properties.Resources.save;
            btnSave.ImageScaling = ToolStripItemImageScaling.None;
            btnSave.ImageTransparentColor = Color.Magenta;
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(23, 22);
            btnSave.ToolTipText = "Save Template";
            btnSave.Click += btnSave_Click;
            // 
            // btnSaveAs
            // 
            btnSaveAs.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnSaveAs.Image = global::DrvPulsar.View.Properties.Resources.save_as;
            btnSaveAs.ImageScaling = ToolStripItemImageScaling.None;
            btnSaveAs.ImageTransparentColor = Color.Magenta;
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Size = new Size(23, 22);
            btnSaveAs.ToolTipText = "Save Template As";
            btnSaveAs.Click += btnSaveAs_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // btnMoveUp
            // 
            btnMoveUp.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnMoveUp.Image = global::DrvPulsar.View.Properties.Resources.move_up;
            btnMoveUp.ImageScaling = ToolStripItemImageScaling.None;
            btnMoveUp.ImageTransparentColor = Color.Magenta;
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(23, 22);
            btnMoveUp.ToolTipText = "Move Up";
            btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnMoveDown
            // 
            btnMoveDown.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnMoveDown.Image = global::DrvPulsar.View.Properties.Resources.move_down;
            btnMoveDown.ImageScaling = ToolStripItemImageScaling.None;
            btnMoveDown.ImageTransparentColor = Color.Magenta;
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(23, 22);
            btnMoveDown.ToolTipText = "Move Down";
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // btnValidate
            // 
            btnValidate.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnValidate.Image = global::DrvPulsar.View.Properties.Resources.validate;
            btnValidate.ImageScaling = ToolStripItemImageScaling.None;
            btnValidate.ImageTransparentColor = Color.Magenta;
            btnValidate.Name = "btnValidate";
            btnValidate.Size = new Size(23, 22);
            btnValidate.ToolTipText = "Validate Template";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // tsLabel
            // 
            tsLabel.ImageScaling = ToolStripItemImageScaling.None;
            tsLabel.Name = "tsLabel";
            tsLabel.Size = new Size(77, 22);
            tsLabel.Text = "Device Name";
            // 
            // txtName
            // 
            txtName.Name = "txtName";
            txtName.Size = new Size(250, 25);
            txtName.TextChanged += txtName_TextChanged;
            // 
            // dgvComm
            // 
            dgvComm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvComm.BackgroundColor = SystemColors.Window;
            dgvComm.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComm.Location = new Point(0, 3);
            dgvComm.Name = "dgvComm";
            dgvComm.RowHeadersWidth = 51;
            dgvComm.RowTemplate.Height = 25;
            dgvComm.Size = new Size(873, 488);
            dgvComm.TabIndex = 3;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(93, 556);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOK.Location = new Point(12, 556);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 5;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabSnd);
            tabControl1.Controls.Add(tabCmd);
            tabControl1.Location = new Point(0, 28);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(884, 522);
            tabControl1.TabIndex = 7;
            // 
            // tabSnd
            // 
            tabSnd.Controls.Add(dgvVal);
            tabSnd.Controls.Add(dgvSend);
            tabSnd.Location = new Point(4, 24);
            tabSnd.Name = "tabSnd";
            tabSnd.Padding = new Padding(3);
            tabSnd.Size = new Size(876, 494);
            tabSnd.TabIndex = 1;
            tabSnd.Text = "Send Request";
            tabSnd.UseVisualStyleBackColor = true;
            // 
            // dgvVal
            // 
            dgvVal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvVal.BackgroundColor = SystemColors.Window;
            dgvVal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVal.Location = new Point(3, 219);
            dgvVal.Name = "dgvVal";
            dgvVal.RowTemplate.Height = 25;
            dgvVal.Size = new Size(870, 272);
            dgvVal.TabIndex = 5;
            // 
            // dgvSend
            // 
            dgvSend.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvSend.BackgroundColor = SystemColors.Window;
            dgvSend.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSend.Location = new Point(3, 3);
            dgvSend.Name = "dgvSend";
            dgvSend.RowHeadersWidth = 51;
            dgvSend.RowTemplate.Height = 25;
            dgvSend.Size = new Size(870, 210);
            dgvSend.TabIndex = 0;
            // 
            // tabCmd
            // 
            tabCmd.Controls.Add(dgvComm);
            tabCmd.Location = new Point(4, 24);
            tabCmd.Name = "tabCmd";
            tabCmd.Padding = new Padding(3);
            tabCmd.Size = new Size(876, 494);
            tabCmd.TabIndex = 0;
            tabCmd.Text = "Command Request";
            tabCmd.UseVisualStyleBackColor = true;
            // 
            // FrmDeviceProps
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 591);
            Controls.Add(tabControl1);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(toolStrip);
            Name = "FrmDeviceProps";
            Text = "FrmDeviceProps";
            Load += FrmDeviceProps_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvComm).EndInit();
            tabControl1.ResumeLayout(false);
            tabSnd.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvVal).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvSend).EndInit();
            tabCmd.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private ToolStrip toolStrip;
        private ToolStripTextBox txtTemplateFileName;
        private ToolStripButton btnOpen;
        private ToolStripButton btnSave;
        private ToolStripButton btnSaveAs;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnMoveUp;
        private ToolStripButton btnMoveDown;
        private ToolStripButton btnValidate;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripLabel tsLabel;
        private ToolStripTextBox txtName;
        private DataGridView dgvComm;
        private Button btnCancel;
        private Button btnOK;
        private TabControl tabControl1;
        private TabPage tabCmd;
        private TabPage tabSnd;
        private DataGridView dgvSend;
        private DataGridView dgvVal;
    }
}
