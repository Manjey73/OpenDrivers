namespace Scada.Comm.Drivers.DrvMercury23x.View
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
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ButExt = new System.Windows.Forms.Button();
            this.txtTemplateFileName = new System.Windows.Forms.TextBox();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnMoveDownItem = new System.Windows.Forms.Button();
            this.btnMoveUpItem = new System.Windows.Forms.Button();
            this.btnBrowseTemplate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabContr = new System.Windows.Forms.TabControl();
            this.tabReq = new System.Windows.Forms.TabPage();
            this.lbHalfStatus = new System.Windows.Forms.Label();
            this.txtHalfStatus = new System.Windows.Forms.TextBox();
            this.cbReadStatus = new System.Windows.Forms.CheckBox();
            this.cbSyncTime = new System.Windows.Forms.CheckBox();
            this.cbInfo = new System.Windows.Forms.CheckBox();
            this.cbMulti = new System.Windows.Forms.CheckBox();
            this.labDevName = new System.Windows.Forms.Label();
            this.txtDevName = new System.Windows.Forms.TextBox();
            this.ReadParam = new System.Windows.Forms.Label();
            this.cbReadParam = new System.Windows.Forms.ComboBox();
            this.dgvVals = new System.Windows.Forms.DataGridView();
            this.dgvReq = new System.Windows.Forms.DataGridView();
            this.tabComm = new System.Windows.Forms.TabPage();
            this.dgvComm = new System.Windows.Forms.DataGridView();
            this.tabProf = new System.Windows.Forms.TabPage();
            this.dgvPVals = new System.Windows.Forms.DataGridView();
            this.dgvProf = new System.Windows.Forms.DataGridView();
            this.btDel = new System.Windows.Forms.Button();
            this.txtMask = new System.Windows.Forms.TextBox();
            this.lbArchMask = new System.Windows.Forms.Label();
            this.tabContr.SuspendLayout();
            this.tabReq.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVals)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReq)).BeginInit();
            this.tabComm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComm)).BeginInit();
            this.tabProf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPVals)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProf)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // ButExt
            // 
            this.ButExt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ButExt.Image = global::Scada.Comm.Drivers.DrvMercury23x.View.Properties.Resources.options_extended;
            this.ButExt.Location = new System.Drawing.Point(307, 13);
            this.ButExt.Name = "ButExt";
            this.ButExt.Size = new System.Drawing.Size(23, 22);
            this.ButExt.TabIndex = 21;
            this.ButExt.UseVisualStyleBackColor = true;
            this.ButExt.Click += new System.EventHandler(this.ButExt_Click);
            // 
            // txtTemplateFileName
            // 
            this.txtTemplateFileName.Location = new System.Drawing.Point(12, 12);
            this.txtTemplateFileName.Name = "txtTemplateFileName";
            this.txtTemplateFileName.Size = new System.Drawing.Size(202, 23);
            this.txtTemplateFileName.TabIndex = 20;
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.Image = global::Scada.Comm.Drivers.DrvMercury23x.View.Properties.Resources.save_as;
            this.btnSaveAs.Location = new System.Drawing.Point(278, 13);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(23, 22);
            this.btnSaveAs.TabIndex = 19;
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // btnSave
            // 
            this.btnSave.Image = global::Scada.Comm.Drivers.DrvMercury23x.View.Properties.Resources.save;
            this.btnSave.Location = new System.Drawing.Point(249, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(23, 22);
            this.btnSave.TabIndex = 18;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnMoveDownItem
            // 
            this.btnMoveDownItem.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveDownItem.Image = global::Scada.Comm.Drivers.DrvMercury23x.View.Properties.Resources.down;
            this.btnMoveDownItem.Location = new System.Drawing.Point(381, 13);
            this.btnMoveDownItem.Name = "btnMoveDownItem";
            this.btnMoveDownItem.Size = new System.Drawing.Size(23, 22);
            this.btnMoveDownItem.TabIndex = 17;
            this.btnMoveDownItem.UseVisualStyleBackColor = true;
            this.btnMoveDownItem.Click += new System.EventHandler(this.btnMoveDownItem_Click);
            // 
            // btnMoveUpItem
            // 
            this.btnMoveUpItem.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveUpItem.Image = global::Scada.Comm.Drivers.DrvMercury23x.View.Properties.Resources.up;
            this.btnMoveUpItem.Location = new System.Drawing.Point(352, 13);
            this.btnMoveUpItem.Name = "btnMoveUpItem";
            this.btnMoveUpItem.Size = new System.Drawing.Size(23, 22);
            this.btnMoveUpItem.TabIndex = 16;
            this.btnMoveUpItem.UseVisualStyleBackColor = true;
            this.btnMoveUpItem.Click += new System.EventHandler(this.btnMoveUpItem_Click);
            // 
            // btnBrowseTemplate
            // 
            this.btnBrowseTemplate.Image = global::Scada.Comm.Drivers.DrvMercury23x.View.Properties.Resources.open;
            this.btnBrowseTemplate.Location = new System.Drawing.Point(220, 13);
            this.btnBrowseTemplate.Name = "btnBrowseTemplate";
            this.btnBrowseTemplate.Size = new System.Drawing.Size(23, 22);
            this.btnBrowseTemplate.TabIndex = 15;
            this.btnBrowseTemplate.UseVisualStyleBackColor = true;
            this.btnBrowseTemplate.Click += new System.EventHandler(this.btnBrowseTemplate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(93, 486);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 486);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tabContr
            // 
            this.tabContr.Controls.Add(this.tabReq);
            this.tabContr.Controls.Add(this.tabComm);
            this.tabContr.Controls.Add(this.tabProf);
            this.tabContr.Location = new System.Drawing.Point(12, 41);
            this.tabContr.Name = "tabContr";
            this.tabContr.SelectedIndex = 0;
            this.tabContr.Size = new System.Drawing.Size(706, 439);
            this.tabContr.TabIndex = 25;
            // 
            // tabReq
            // 
            this.tabReq.Controls.Add(this.lbArchMask);
            this.tabReq.Controls.Add(this.txtMask);
            this.tabReq.Controls.Add(this.lbHalfStatus);
            this.tabReq.Controls.Add(this.txtHalfStatus);
            this.tabReq.Controls.Add(this.cbReadStatus);
            this.tabReq.Controls.Add(this.cbSyncTime);
            this.tabReq.Controls.Add(this.cbInfo);
            this.tabReq.Controls.Add(this.cbMulti);
            this.tabReq.Controls.Add(this.labDevName);
            this.tabReq.Controls.Add(this.txtDevName);
            this.tabReq.Controls.Add(this.ReadParam);
            this.tabReq.Controls.Add(this.cbReadParam);
            this.tabReq.Controls.Add(this.dgvVals);
            this.tabReq.Controls.Add(this.dgvReq);
            this.tabReq.Location = new System.Drawing.Point(4, 24);
            this.tabReq.Name = "tabReq";
            this.tabReq.Padding = new System.Windows.Forms.Padding(3);
            this.tabReq.Size = new System.Drawing.Size(698, 411);
            this.tabReq.TabIndex = 0;
            this.tabReq.Text = "Requests";
            this.tabReq.UseVisualStyleBackColor = true;
            // 
            // lbHalfStatus
            // 
            this.lbHalfStatus.AutoSize = true;
            this.lbHalfStatus.Location = new System.Drawing.Point(495, 381);
            this.lbHalfStatus.Name = "lbHalfStatus";
            this.lbHalfStatus.Size = new System.Drawing.Size(122, 15);
            this.lbHalfStatus.TabIndex = 12;
            this.lbHalfStatus.Text = "Status incorect Profile";
            // 
            // txtHalfStatus
            // 
            this.txtHalfStatus.Location = new System.Drawing.Point(623, 378);
            this.txtHalfStatus.Name = "txtHalfStatus";
            this.txtHalfStatus.Size = new System.Drawing.Size(69, 23);
            this.txtHalfStatus.TabIndex = 11;
            this.txtHalfStatus.TextChanged += new System.EventHandler(this.txtHalfStatus_TextChanged);
            // 
            // cbReadStatus
            // 
            this.cbReadStatus.AutoSize = true;
            this.cbReadStatus.Location = new System.Drawing.Point(250, 380);
            this.cbReadStatus.Name = "cbReadStatus";
            this.cbReadStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbReadStatus.Size = new System.Drawing.Size(87, 19);
            this.cbReadStatus.TabIndex = 10;
            this.cbReadStatus.Text = "Read Status";
            this.cbReadStatus.UseVisualStyleBackColor = true;
            this.cbReadStatus.CheckedChanged += new System.EventHandler(this.cbReadStatus_CheckedChanged);
            // 
            // cbSyncTime
            // 
            this.cbSyncTime.AutoSize = true;
            this.cbSyncTime.Location = new System.Drawing.Point(250, 355);
            this.cbSyncTime.Name = "cbSyncTime";
            this.cbSyncTime.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbSyncTime.Size = new System.Drawing.Size(120, 19);
            this.cbSyncTime.TabIndex = 9;
            this.cbSyncTime.Text = "Synchronize Time";
            this.cbSyncTime.UseVisualStyleBackColor = true;
            this.cbSyncTime.CheckedChanged += new System.EventHandler(this.cbSyncTime_CheckedChanged);
            // 
            // cbInfo
            // 
            this.cbInfo.AutoSize = true;
            this.cbInfo.Location = new System.Drawing.Point(250, 330);
            this.cbInfo.Name = "cbInfo";
            this.cbInfo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbInfo.Size = new System.Drawing.Size(89, 19);
            this.cbInfo.TabIndex = 8;
            this.cbInfo.Text = "Information";
            this.cbInfo.UseVisualStyleBackColor = true;
            this.cbInfo.CheckedChanged += new System.EventHandler(this.cbInfo_CheckedChanged);
            // 
            // cbMulti
            // 
            this.cbMulti.AutoSize = true;
            this.cbMulti.Location = new System.Drawing.Point(250, 305);
            this.cbMulti.Name = "cbMulti";
            this.cbMulti.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbMulti.Size = new System.Drawing.Size(77, 19);
            this.cbMulti.TabIndex = 7;
            this.cbMulti.Text = "MultiCast";
            this.cbMulti.UseVisualStyleBackColor = true;
            this.cbMulti.CheckedChanged += new System.EventHandler(this.cbMulti_CheckedChanged);
            // 
            // labDevName
            // 
            this.labDevName.AutoSize = true;
            this.labDevName.Location = new System.Drawing.Point(250, 250);
            this.labDevName.Name = "labDevName";
            this.labDevName.Size = new System.Drawing.Size(77, 15);
            this.labDevName.TabIndex = 5;
            this.labDevName.Text = "Device Name";
            // 
            // txtDevName
            // 
            this.txtDevName.Location = new System.Drawing.Point(333, 247);
            this.txtDevName.Name = "txtDevName";
            this.txtDevName.Size = new System.Drawing.Size(133, 23);
            this.txtDevName.TabIndex = 4;
            this.txtDevName.TextChanged += new System.EventHandler(this.txtDevName_TextChanged_1);
            // 
            // ReadParam
            // 
            this.ReadParam.AutoSize = true;
            this.ReadParam.Location = new System.Drawing.Point(250, 279);
            this.ReadParam.Name = "ReadParam";
            this.ReadParam.Size = new System.Drawing.Size(67, 15);
            this.ReadParam.TabIndex = 3;
            this.ReadParam.Text = "ReadParam";
            // 
            // cbReadParam
            // 
            this.cbReadParam.FormattingEnabled = true;
            this.cbReadParam.Items.AddRange(new object[] {
            "14h",
            "16h"});
            this.cbReadParam.Location = new System.Drawing.Point(333, 276);
            this.cbReadParam.Name = "cbReadParam";
            this.cbReadParam.Size = new System.Drawing.Size(79, 23);
            this.cbReadParam.TabIndex = 2;
            this.cbReadParam.SelectedIndexChanged += new System.EventHandler(this.cbReadParam_SelectedIndexChanged);
            // 
            // dgvVals
            // 
            this.dgvVals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVals.Location = new System.Drawing.Point(250, 0);
            this.dgvVals.Name = "dgvVals";
            this.dgvVals.RowTemplate.Height = 25;
            this.dgvVals.Size = new System.Drawing.Size(452, 241);
            this.dgvVals.TabIndex = 1;
            // 
            // dgvReq
            // 
            this.dgvReq.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReq.Location = new System.Drawing.Point(0, 0);
            this.dgvReq.Name = "dgvReq";
            this.dgvReq.RowTemplate.Height = 25;
            this.dgvReq.Size = new System.Drawing.Size(244, 411);
            this.dgvReq.TabIndex = 0;
            // 
            // tabComm
            // 
            this.tabComm.Controls.Add(this.dgvComm);
            this.tabComm.Location = new System.Drawing.Point(4, 24);
            this.tabComm.Name = "tabComm";
            this.tabComm.Padding = new System.Windows.Forms.Padding(3);
            this.tabComm.Size = new System.Drawing.Size(698, 411);
            this.tabComm.TabIndex = 1;
            this.tabComm.Text = "Commands";
            this.tabComm.UseVisualStyleBackColor = true;
            // 
            // dgvComm
            // 
            this.dgvComm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvComm.Location = new System.Drawing.Point(-4, 0);
            this.dgvComm.Name = "dgvComm";
            this.dgvComm.RowTemplate.Height = 25;
            this.dgvComm.Size = new System.Drawing.Size(706, 415);
            this.dgvComm.TabIndex = 0;
            // 
            // tabProf
            // 
            this.tabProf.Controls.Add(this.dgvPVals);
            this.tabProf.Controls.Add(this.dgvProf);
            this.tabProf.Location = new System.Drawing.Point(4, 24);
            this.tabProf.Name = "tabProf";
            this.tabProf.Size = new System.Drawing.Size(698, 411);
            this.tabProf.TabIndex = 2;
            this.tabProf.Text = "Profile";
            this.tabProf.UseVisualStyleBackColor = true;
            // 
            // dgvPVals
            // 
            this.dgvPVals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPVals.Location = new System.Drawing.Point(-4, 215);
            this.dgvPVals.Name = "dgvPVals";
            this.dgvPVals.RowTemplate.Height = 25;
            this.dgvPVals.Size = new System.Drawing.Size(706, 200);
            this.dgvPVals.TabIndex = 1;
            // 
            // dgvProf
            // 
            this.dgvProf.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProf.Location = new System.Drawing.Point(-4, 0);
            this.dgvProf.Name = "dgvProf";
            this.dgvProf.RowTemplate.Height = 25;
            this.dgvProf.Size = new System.Drawing.Size(706, 209);
            this.dgvProf.TabIndex = 0;
            // 
            // btDel
            // 
            this.btDel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btDel.Image = global::Scada.Comm.Drivers.DrvMercury23x.View.Properties.Resources.delete;
            this.btDel.Location = new System.Drawing.Point(410, 13);
            this.btDel.Name = "btDel";
            this.btDel.Size = new System.Drawing.Size(23, 22);
            this.btDel.TabIndex = 26;
            this.btDel.UseVisualStyleBackColor = true;
            this.btDel.Click += new System.EventHandler(this.btDel_Click);
            // 
            // txtMask
            // 
            this.txtMask.Location = new System.Drawing.Point(623, 351);
            this.txtMask.Name = "txtMask";
            this.txtMask.Size = new System.Drawing.Size(69, 23);
            this.txtMask.TabIndex = 13;
            this.txtMask.TextChanged += new System.EventHandler(this.txtMask_TextChanged);
            // 
            // lbArchMask
            // 
            this.lbArchMask.AutoSize = true;
            this.lbArchMask.Location = new System.Drawing.Point(495, 356);
            this.lbArchMask.Name = "lbArchMask";
            this.lbArchMask.Size = new System.Drawing.Size(115, 15);
            this.lbArchMask.TabIndex = 14;
            this.lbArchMask.Text = "Profile Archive Mask";
            // 
            // FrmDeviceProps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 521);
            this.Controls.Add(this.btDel);
            this.Controls.Add(this.tabContr);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.ButExt);
            this.Controls.Add(this.txtTemplateFileName);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnMoveDownItem);
            this.Controls.Add(this.btnMoveUpItem);
            this.Controls.Add(this.btnBrowseTemplate);
            this.Name = "FrmDeviceProps";
            this.Text = "Device {0} Properties";
            this.Load += new System.EventHandler(this.FrmDeviceProps_Load);
            this.tabContr.ResumeLayout(false);
            this.tabReq.ResumeLayout(false);
            this.tabReq.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVals)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReq)).EndInit();
            this.tabComm.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvComm)).EndInit();
            this.tabProf.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPVals)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProf)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private Button ButExt;
        private TextBox txtTemplateFileName;
        private Button btnSaveAs;
        private Button btnSave;
        private Button btnMoveDownItem;
        private Button btnMoveUpItem;
        private Button btnBrowseTemplate;
        private Button btnCancel;
        private Button btnOK;
        private TabControl tabContr;
        private TabPage tabReq;
        private DataGridView dgvVals;
        private DataGridView dgvReq;
        private TabPage tabComm;
        private DataGridView dgvComm;
        private TabPage tabProf;
        private DataGridView dgvProf;
        private DataGridView dgvPVals;
        private Label labDevName;
        private TextBox txtDevName;
        private Label ReadParam;
        private ComboBox cbReadParam;
        private CheckBox cbMulti;
        private CheckBox cbInfo;
        private Label lbHalfStatus;
        private TextBox txtHalfStatus;
        private CheckBox cbReadStatus;
        private CheckBox cbSyncTime;
        private Button btDel;
        private Label lbArchMask;
        private TextBox txtMask;
    }
}
