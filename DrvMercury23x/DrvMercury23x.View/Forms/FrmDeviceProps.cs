using Scada.Comm.Config;
using Scada.Forms;
using Scada.Forms.Forms;
using Scada.Lang;
using ScadaCommFunc;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Scada.Comm.Drivers.DrvMercury23x.View
{
    public partial class FrmDeviceProps : Form
    {

        private readonly AppDirs appDirs;           // the application directories
        private readonly LineConfig lineConfig;     // the communication line configuration
        private DeviceConfig deviceConfig; // the device configuration // readonly
        private string errMsg = "";
        string FileName;

        // Интерфейс
        private DevTemplate devTemplate = new DevTemplate();
        private int rowIndex;
        //private int newRowIndex;
        private BindingSource bsReq = new BindingSource();
        private BindingSource bsComm = new BindingSource();
        private BindingSource bsVals = new BindingSource();
        private BindingSource bsProf = new BindingSource();
        private BindingSource bsPVals = new BindingSource();

        public FrmDeviceProps()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;  // Убираем возможность растяжения окна
            MaximizeBox = false;                            // Скрываем кнопки увеличения и уменьшения окна
            MinimizeBox = false;

            devTemplate.SndGroups.Add(new DevTemplate.SndRequest()); // Создаем пустой класс для первой строки

            bsReq.DataSource = devTemplate.SndGroups; // 
            dgvReq.DataSource = bsReq;

            dgvReq.AllowUserToAddRows = false;
            dgvReq.AllowUserToDeleteRows = false;
            dgvReq.RowHeadersVisible = false;
            dgvReq.BorderStyle = BorderStyle.None;
            dgvReq.Columns["valueSpecified"].Visible = false; // Скрыть столбец со служебными переменными
            dgvReq.Columns["Bit"].ReadOnly = true;
            dgvReq.Columns["Bit"].Visible = false;
            dgvReq.Columns["Name"].Width = 160;
            dgvReq.Columns["Active"].Width = 60;

            var vals = new DevTemplate.SndRequest.Vals();
            bsVals.DataSource = vals;
            dgvVals.DataSource = bsVals;

            dgvVals.RowHeadersVisible = false;
            dgvVals.BorderStyle = BorderStyle.None;
            dgvVals.AllowUserToAddRows = false;
            dgvVals.AllowUserToDeleteRows = false;
            dgvVals.Columns["active"].Width = 60;
            dgvVals.Columns["range"].Width = 60;


            devTemplate.CmdGroups.Add(new DevTemplate.CmdGroup()); // Создаем пустой класс для первой строки
            bsComm.DataSource = devTemplate.CmdGroups;
            dgvComm.DataSource = bsComm;
            dgvComm.BorderStyle = BorderStyle.None;

            dgvComm.Columns["Name"].Width = 130;
            dgvComm.Columns["Active"].Width = 56;
            dgvComm.Columns["Code"].Width = 120;
            dgvComm.Columns["Mode"].Width = 56;
            dgvComm.Columns["Cmd"].Width = 56;
            dgvComm.Columns["Par"].Width = 56;
            dgvComm.Columns["Data"].Width = 130;
            dgvComm.Columns["inCnt"].Width = 56;

            devTemplate.ProfileGroups.Add(new DevTemplate.PowerProfile()); // { value =  pVals}

            bsProf.DataSource = devTemplate.ProfileGroups;
            dgvProf.DataSource = bsProf;

            dgvProf.BorderStyle = BorderStyle.None;
            dgvProf.Columns["valueSpecified"].Visible = false; // Скрыть столбец со служебными переменными
            dgvProf.Columns["Name"].Width = 180;
            dgvProf.Columns["Active"].Width = 60;
            dgvProf.Columns["Energy"].Width = 60;

            var pVals = new DevTemplate.PowerProfile.Vals1();
            bsPVals.DataSource = pVals;
            dgvPVals.DataSource = bsPVals;


            // ---------      События для DataGridView    -----------------------
            dgvReq.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvReq_CellMouseClick);
            dgvProf.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvProf_CellMouseClick);

            //dgvCmd.UserDeletingRow += new DataGridViewRowCancelEventHandler(dgvCmd_UserDeletingRow);
            //dgvCmd.SelectionChanged += new EventHandler(dgvCmd_SelectionChanged);
            //dgvCmd.UserDeletedRow += new DataGridViewRowEventHandler(dgvCmd_UserDeletedgRow);
            //dgvCmd.CellBeginEdit += new DataGridViewCellCancelEventHandler(dgvCmd_CellBeginEdit);
            //dgvCmd.DataError += new DataGridViewDataErrorEventHandler(dgvCmd_DataError);
            //dgvCmd.RowsAdded += new DataGridViewRowsAddedEventHandler(dgvCmd_RowsAdded);


        }

        /// <summary>
        /// Validates the form controls.
        /// </summary>
        private bool ValidateControls()
        {
            if (!File.Exists(GetTemplatePath()))
            {
                ScadaUiUtils.ShowError("Файл шаблона устройства не существует.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmDeviceProps(AppDirs appDirs, LineConfig lineConfig, DeviceConfig deviceConfig) // , CustomUi customUi
            : this()
        {
            this.appDirs = appDirs ?? throw new ArgumentNullException(nameof(appDirs));
            this.lineConfig = lineConfig ?? throw new ArgumentNullException(nameof(lineConfig));
            this.deviceConfig = deviceConfig ?? throw new ArgumentNullException(nameof(deviceConfig));
        }

        /// <summary>
        /// Gets the file path of the device template.
        /// </summary>
        private string GetTemplatePath()
        {
            return Path.Combine(appDirs.ConfigDir, txtTemplateFileName.Text);
        }

        private void FrmDeviceProps_Load(object sender, EventArgs e)
        {
            FormTranslator.Translate(this, GetType().FullName);
            openFileDialog.SetFilter(CommonPhrases.XmlFileFilter);
            saveFileDialog.SetFilter(CommonPhrases.XmlFileFilter);

            Text = string.Format(Text, deviceConfig.DeviceNum);
            txtTemplateFileName.Text = deviceConfig.PollingOptions.CmdLine;


            FileName = txtTemplateFileName == null ? "" : txtTemplateFileName.Text;
            if (txtTemplateFileName.Text != "")
            {
                Load_Properties();
            }
            else
            {
                var xdoc = XDocument.Parse(Properties.Resources.Mercury23x);

                using (MemoryStream w = new MemoryStream())
                {
                    xdoc.Save(w);
                    w.Position = 0;
                    XmlSerializer serializer = new XmlSerializer(typeof(DevTemplate));
                    devTemplate = serializer.Deserialize(w) as DevTemplate;
                    w.Close();
                }
                Load_Data();
            }
        }

        private void btnBrowseTemplate_Click(object sender, EventArgs e)
        {
            // show dialog to select template file
            openFileDialog.InitialDirectory = appDirs.ConfigDir;
            openFileDialog.FileName = "";

            if (openFileDialog.ShowDialog() == DialogResult.OK &&
            ValidateTemplatePath(openFileDialog.FileName, out string shortFileName))
            {
                txtTemplateFileName.Text = shortFileName;
                FileName = shortFileName;
            }
            Load_Properties();
        }

        /// <summary>
        /// Загрузка шаблона при наличии
        /// </summary>
        private void Load_Properties()
        {
            try
            {
                devTemplate = FileFunc.LoadXml(typeof(DevTemplate), GetTemplatePath()) as DevTemplate; // При новой загрузке plcprojectinfo очищается
                Load_Data();
            }
            catch (FileNotFoundException ex)
            {
                errMsg = ex.Message;
                ScadaUiUtils.ShowError(errMsg);
                txtTemplateFileName.Text = "";
            }
            catch (Exception err)
            {
                ScadaUiUtils.ShowError(err.ToString());
            }
        }

        private void Load_Data()
        {
            txtDevName.Text = devTemplate.Name;
            cbReadParam.Text = devTemplate.readparam;
            cbMulti.Checked = devTemplate.multicast;
            cbInfo.Checked = devTemplate.info;
            cbSyncTime.Checked = devTemplate.SyncTime;
            cbReadStatus.Checked = devTemplate.readStatus;
            txtHalfStatus.Text = devTemplate.halfArchStat.ToString();
            txtMask.Text = devTemplate.ArchMask;

            bsReq.DataSource = devTemplate.SndGroups;
            dgvReq.DataSource = bsReq;

            bsComm.DataSource = devTemplate.CmdGroups;
            dgvComm.DataSource = bsComm;

            bsProf.DataSource = devTemplate.ProfileGroups;
            dgvProf.DataSource = bsProf;
        }

        /// <summary>
        /// Validates the path of the device template file.
        /// </summary>
        private bool ValidateTemplatePath(string fileName, out string shortFileName)
        {
            if (fileName.StartsWith(appDirs.ConfigDir))
            {
                shortFileName = fileName[appDirs.ConfigDir.Length..];
                return true;
            }
            else
            {
                ScadaUiUtils.ShowError("Файл шаблона устройства должен располагаться внутри {0}", appDirs.ConfigDir); // ModbusDriverPhrases.ConfigDirRequired
                shortFileName = "";
                return false;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateControls())
            {
                deviceConfig.PollingOptions.CmdLine = txtTemplateFileName.Text;
                DialogResult = DialogResult.OK;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // сохранение шаблона устройства в файл
            SaveChanges(false);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            // сохранение шаблона устройства в новый файл
            SaveChanges(sender == btnSaveAs);
            txtTemplateFileName.Text = FileName;
        }

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        private bool SaveChanges(bool saveAs)
        {
            // определение имени файла
            string newFileName = "";

            if (saveAs || FileName == "")
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    newFileName = saveFileDialog.FileName;
                }
            }
            else
            {
                newFileName = appDirs.ConfigDir + FileName;
            }

            if (newFileName == "")
            {
                return false;
            }
            else
            {
                //сохранение шаблона устройства
                if (Save(newFileName, out errMsg))
                {
                    var file = new FileInfo(newFileName);
                    FileName = file.Name; // file.Name
                    txtTemplateFileName.Text = FileName;
                    return true;
                }
                else
                {
                    ScadaUiUtils.ShowError(errMsg);
                    return false;
                }
            }
        }

        /// <summary>
        /// Сохранить шаблон устройства
        /// </summary>
        public bool Save(string filepath, out string errMsg)
        {
            try
            {
                FileFunc.SaveXml(devTemplate, filepath); // добавить сохранение в выбранный файл TEST
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        private void ButExt_Click(object sender, EventArgs e)
        {
            Mercury23xOptions options = new(deviceConfig.PollingOptions.CustomOptions);
            FrmOptions frmOptions = new() { Options = options };

            if (frmOptions.ShowDialog() == DialogResult.OK)
            {
                options.AddToOptionList(deviceConfig.PollingOptions.CustomOptions);
            }
        }


        // -------------------------------------- Интерфейс ------------------------------
        private void dgvReq_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvReq.CurrentCell.RowIndex == -1) return;
            rowIndex = e.RowIndex;

            var snd = devTemplate.SndGroups[rowIndex].value;
            bsVals.DataSource = snd;
            dgvVals.DataSource = bsVals;
        }

        private void dgvProf_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvProf.CurrentCell.RowIndex == -1) return;
            rowIndex = e.RowIndex;

            var pval = devTemplate.ProfileGroups[rowIndex].value;
            bsPVals.DataSource = pval;
            dgvPVals.DataSource = bsPVals;
        }

        private void txtDevName_TextChanged_1(object sender, EventArgs e)
        {
            devTemplate.Name = txtDevName.Text;
        }

        private void cbReadParam_SelectedIndexChanged(object sender, EventArgs e)
        {
            devTemplate.readparam = cbReadParam.SelectedItem.ToString();
        }

        private void cbMulti_CheckedChanged(object sender, EventArgs e)
        {
            devTemplate.multicast = cbMulti.Checked;
        }

        private void cbInfo_CheckedChanged(object sender, EventArgs e)
        {
            devTemplate.info = cbInfo.Checked;
        }

        private void cbSyncTime_CheckedChanged(object sender, EventArgs e)
        {
            devTemplate.SyncTime = cbSyncTime.Checked;
        }

        private void cbReadStatus_CheckedChanged(object sender, EventArgs e)
        {
            devTemplate.readStatus = cbReadStatus.Checked;
        }

        private void txtHalfStatus_TextChanged(object sender, EventArgs e)
        {
            devTemplate.halfArchStat = string.IsNullOrEmpty(txtHalfStatus.Text) ? 1 : int.Parse(txtHalfStatus.Text);
        }
        private void txtMask_TextChanged(object sender, EventArgs e)
        {
            devTemplate.ArchMask = string.IsNullOrEmpty(txtMask.Text) ? "" : txtMask.Text;
        }

        private void btnMoveUpItem_Click(object sender, EventArgs e)
        {
            if(tabContr.SelectedIndex == 0)
            {
                if (bsReq.Position == 0) return;

                var req = devTemplate.SndGroups[bsReq.Position];
                devTemplate.SndGroups.RemoveAt(bsReq.Position);
                bsReq.MovePrevious();
                dgvReq.ClearSelection();
                devTemplate.SndGroups.Insert(bsReq.Position, req);
                dgvReq.Rows[bsReq.Position].Selected = true;
                rowIndex = bsReq.Position; // Корректируем параметр rowIndex
                dgvReq.Refresh();
            }
        }

        private void btnMoveDownItem_Click(object sender, EventArgs e)
        {
            if (tabContr.SelectedIndex == 0)
            {
                if (bsReq.Position + 1 == bsReq.Count) return;

                var req = devTemplate.SndGroups[bsReq.Position];
                devTemplate.SndGroups.RemoveAt(bsReq.Position);

                dgvReq.ClearSelection();
                if (bsReq.Position + 1 == bsReq.Count)
                {
                    devTemplate.SndGroups.Add(req);
                    bsReq.MoveNext();
                }
                else
                {
                    bsReq.MoveNext();
                    devTemplate.SndGroups.Insert(bsReq.Position, req);
                }
                dgvReq.Rows[bsReq.Position].Selected = true;

                rowIndex = bsReq.Position; // Корректируем параметр rowIndex

                dgvReq.Refresh();
            }
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            devTemplate.SndGroups.RemoveAt(bsReq.Position);
            dgvReq.Refresh();
        }
        // -------------------------------------- Интерфейс ------------------------------
    }
}
