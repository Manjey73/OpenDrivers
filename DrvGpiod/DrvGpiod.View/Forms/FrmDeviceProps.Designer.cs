namespace Scada.Comm.Drivers.DrvGpiod.View
{
    partial class FrmDeviceProps : Form
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
            btnOK = new Button();
            btnCancel = new Button();
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            dgvTemplate = new DataGridView();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTemplate).BeginInit();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { txtTemplateFileName, btnOpen, btnSave, btnSaveAs, toolStripSeparator1, btnMoveUp, btnMoveDown, btnValidate, toolStripSeparator2, tsLabel, txtName });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(724, 25);
            toolStrip.TabIndex = 1;
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
            btnOpen.Image = global::DrvGpiod.View.Properties.Resources.open;
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
            btnSave.Image = global::DrvGpiod.View.Properties.Resources.save;
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
            btnSaveAs.Image = global::DrvGpiod.View.Properties.Resources.save_as;
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
            btnMoveUp.Image = global::DrvGpiod.View.Properties.Resources.move_up;
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
            btnMoveDown.Image = global::DrvGpiod.View.Properties.Resources.move_down;
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
            btnValidate.Image = global::DrvGpiod.View.Properties.Resources.validate;
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
            txtName.Size = new Size(176, 25);
            txtName.TextChanged += txtName_TextChanged;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOK.Location = new Point(10, 318);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 3;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(91, 318);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // openFileDialog
            // 
            openFileDialog.FileName = "openFileDialog1";
            // 
            // dgvTemplate
            // 
            dgvTemplate.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvTemplate.BackgroundColor = SystemColors.Window;
            dgvTemplate.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTemplate.Location = new Point(0, 28);
            dgvTemplate.Name = "dgvTemplate";
            dgvTemplate.RowTemplate.Height = 25;
            dgvTemplate.Size = new Size(724, 284);
            dgvTemplate.TabIndex = 5;
            // 
            // FrmDeviceProps
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(724, 352);
            Controls.Add(dgvTemplate);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(toolStrip);
            Margin = new Padding(3, 2, 3, 2);
            Name = "FrmDeviceProps";
            Text = "Device {0} Properties";
            Load += FrmDeviceProps_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTemplate).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripButton btnOpen;
        private ToolStripButton btnSave;
        private ToolStripButton btnSaveAs;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnMoveUp;
        private ToolStripButton btnMoveDown;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnValidate;
        private ToolStripTextBox txtTemplateFileName;
        private Button btnOK;
        private Button btnCancel;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private ToolStripTextBox txtName;
        private ToolStripLabel tsLabel;
        private DataGridView dgvTemplate;
    }
}