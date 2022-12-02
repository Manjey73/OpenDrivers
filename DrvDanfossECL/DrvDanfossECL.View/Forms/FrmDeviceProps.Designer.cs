namespace Scada.Comm.Drivers.DrvDanfossECL.View.Forms
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
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnBrowseTemplate = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.btnMoveUpItem = new System.Windows.Forms.Button();
            this.btnMoveDownItem = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.txtTemplateFileName = new System.Windows.Forms.TextBox();
            this.dgvCmd = new System.Windows.Forms.DataGridView();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtDevName = new System.Windows.Forms.TextBox();
            this.labDevName = new System.Windows.Forms.Label();
            this.cbWriteMult = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCmd)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // btnBrowseTemplate
            // 
            this.btnBrowseTemplate.Image = global::DrvDanfossECL.View.Properties.Resources.open;
            this.btnBrowseTemplate.Location = new System.Drawing.Point(220, 14);
            this.btnBrowseTemplate.Name = "btnBrowseTemplate";
            this.btnBrowseTemplate.Size = new System.Drawing.Size(23, 22);
            this.btnBrowseTemplate.TabIndex = 3;
            this.btnBrowseTemplate.UseVisualStyleBackColor = true;
            this.btnBrowseTemplate.Click += new System.EventHandler(this.btnBrowseTemplate_Click);
            // 
            // btnMoveUpItem
            // 
            this.btnMoveUpItem.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveUpItem.Image = global::DrvDanfossECL.View.Properties.Resources.up;
            this.btnMoveUpItem.Location = new System.Drawing.Point(332, 14);
            this.btnMoveUpItem.Name = "btnMoveUpItem";
            this.btnMoveUpItem.Size = new System.Drawing.Size(23, 22);
            this.btnMoveUpItem.TabIndex = 5;
            this.btnMoveUpItem.UseVisualStyleBackColor = true;
            this.btnMoveUpItem.Click += new System.EventHandler(this.btnMoveUpItem_Click);
            // 
            // btnMoveDownItem
            // 
            this.btnMoveDownItem.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveDownItem.Image = global::DrvDanfossECL.View.Properties.Resources.down;
            this.btnMoveDownItem.Location = new System.Drawing.Point(361, 14);
            this.btnMoveDownItem.Name = "btnMoveDownItem";
            this.btnMoveDownItem.Size = new System.Drawing.Size(23, 22);
            this.btnMoveDownItem.TabIndex = 6;
            this.btnMoveDownItem.UseVisualStyleBackColor = true;
            this.btnMoveDownItem.Click += new System.EventHandler(this.btnMoveDownItem_Click);
            // 
            // btnSave
            // 
            this.btnSave.Image = global::DrvDanfossECL.View.Properties.Resources.save;
            this.btnSave.Location = new System.Drawing.Point(249, 14);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(23, 22);
            this.btnSave.TabIndex = 8;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.Image = global::DrvDanfossECL.View.Properties.Resources.save_as;
            this.btnSaveAs.Location = new System.Drawing.Point(278, 14);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(23, 22);
            this.btnSaveAs.TabIndex = 9;
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // txtTemplateFileName
            // 
            this.txtTemplateFileName.Location = new System.Drawing.Point(12, 13);
            this.txtTemplateFileName.Name = "txtTemplateFileName";
            this.txtTemplateFileName.Size = new System.Drawing.Size(202, 23);
            this.txtTemplateFileName.TabIndex = 10;
            // 
            // dgvCmd
            // 
            this.dgvCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCmd.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvCmd.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCmd.Location = new System.Drawing.Point(12, 44);
            this.dgvCmd.Name = "dgvCmd";
            this.dgvCmd.RowHeadersWidth = 51;
            this.dgvCmd.RowTemplate.Height = 25;
            this.dgvCmd.Size = new System.Drawing.Size(879, 379);
            this.dgvCmd.TabIndex = 11;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Location = new System.Drawing.Point(12, 428);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 12;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(93, 428);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtDevName
            // 
            this.txtDevName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDevName.Location = new System.Drawing.Point(601, 13);
            this.txtDevName.Name = "txtDevName";
            this.txtDevName.Size = new System.Drawing.Size(172, 23);
            this.txtDevName.TabIndex = 15;
            this.txtDevName.TextChanged += new System.EventHandler(this.txtDevName_TextChanged);
            // 
            // labDevName
            // 
            this.labDevName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labDevName.AutoSize = true;
            this.labDevName.Location = new System.Drawing.Point(511, 18);
            this.labDevName.Name = "labDevName";
            this.labDevName.Size = new System.Drawing.Size(77, 15);
            this.labDevName.TabIndex = 16;
            this.labDevName.Text = "Device Name";
            // 
            // cbWriteMult
            // 
            this.cbWriteMult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbWriteMult.AutoSize = true;
            this.cbWriteMult.Location = new System.Drawing.Point(786, 16);
            this.cbWriteMult.Name = "cbWriteMult";
            this.cbWriteMult.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbWriteMult.Size = new System.Drawing.Size(105, 19);
            this.cbWriteMult.TabIndex = 17;
            this.cbWriteMult.Text = "WriteMultiplier";
            this.cbWriteMult.UseVisualStyleBackColor = true;
            this.cbWriteMult.CheckedChanged += new System.EventHandler(this.cbWriteMult_CheckedChanged);
            // 
            // FrmDeviceProps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 461);
            this.Controls.Add(this.cbWriteMult);
            this.Controls.Add(this.labDevName);
            this.Controls.Add(this.txtDevName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.dgvCmd);
            this.Controls.Add(this.txtTemplateFileName);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnMoveDownItem);
            this.Controls.Add(this.btnMoveUpItem);
            this.Controls.Add(this.btnBrowseTemplate);
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "FrmDeviceProps";
            this.Text = "Device {0} Properties";
            this.Load += new System.EventHandler(this.FrmDeviceProps_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCmd)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenFileDialog openFileDialog;
        private Button btnBrowseTemplate;
        private SaveFileDialog saveFileDialog;
        private Button btnMoveUpItem;
        private Button btnMoveDownItem;
        private Button btnSave;
        private Button btnSaveAs;
        private TextBox txtTemplateFileName;
        private DataGridView dgvCmd;
        private Button btnOK;
        private Button btnCancel;
        private TextBox txtDevName;
        private Label labDevName;
        private CheckBox cbWriteMult;
    }
}
