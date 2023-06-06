
using Scada.Comm.Config;
using Scada.Forms;
using Scada.Lang;
using ScadaCommFunc;

namespace Scada.Comm.Drivers.DrvPulsar.View
{
    public partial class FrmDeviceProps : Form
    {
        private readonly AppDirs appDirs;           // the application directories
        private readonly LineConfig lineConfig;     // the communication line configuration
        private DeviceConfig deviceConfig;          // the device configuration // readonly
        private string errMsg = "";
        private string shortFileName = "";
        private string fileName;
        private string filePath;

        // Интерфейс
        private DevTemplate devTemplate = new DevTemplate();
        private int rowIndex = 0;
        private int rowIndexVal = 0;

        private BindingSource bsSend = new BindingSource();
        private BindingSource bsValue = new BindingSource();
        private BindingSource bsCmd = new BindingSource();
        private List<string> Format = new List<string> { "float", "double", "uint16", "uint32", "DateTime" };

        // Интерфейс

        public FrmDeviceProps()
        {
            InitializeComponent();

            DevTemplate.SndGroup querySnd = new DevTemplate.SndGroup();
            DevTemplate.SndGroup.Val queryVal = new DevTemplate.SndGroup.Val();

            devTemplate.SndGroups.Add(querySnd);

            bsSend.DataSource = devTemplate.SndGroups;
            dgvSend.DataSource = bsSend;

            DevTemplate.CmdGroup queryCmd = new DevTemplate.CmdGroup();
            devTemplate.CmdGroups.Add(queryCmd);
            bsCmd.DataSource = devTemplate.CmdGroups;
            dgvComm.DataSource = bsCmd;

            // Настройки вида 
            dgvSend.AutoGenerateColumns = false;
            dgvSend.Columns["Counter"].Width = 70;
            dgvSend.Columns["Active"].Width = 80;
            dgvSend.Columns["Name"].Width = 200;
            dgvSend.Columns["GroupName"].Width = 200;
            dgvSend.Columns["Command"].Width = 86;
            dgvSend.Columns["userData"].Width = 86;

            dgvComm.Columns["Channel"].Width = 70;
            dgvComm.Columns["Active"].Width = 80;
            dgvComm.Columns["Name"].Width = 200;


            // ---------      События для DataGridView    -----------------------
            dgvSend.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvSend_CellMouseClick);
            dgvVal.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvVal_CellMouseClick);
            dgvComm.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvComm_CellMouseClick);
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmDeviceProps(AppDirs appDirs, LineConfig lineConfig, DeviceConfig deviceConfig)
            : this()
        {
            this.appDirs = appDirs ?? throw new ArgumentNullException(nameof(appDirs));
            this.lineConfig = lineConfig ?? throw new ArgumentNullException(nameof(lineConfig));
            this.deviceConfig = deviceConfig ?? throw new ArgumentNullException(nameof(deviceConfig));
        }

        /// <summary>
        /// Validates the form controls.
        /// </summary>
        /// 
        private bool ValidateControls()
        {
            if (!File.Exists(GetTemplatePath()))
            {
                ScadaUiUtils.ShowError(Locale.IsRussian ? "Файл шаблона устройства не существует." :
                 "The device template file does not exist.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// Gets the file path of the device template.
        /// </summary>
        private string GetTemplatePath()
        {
            return Path.Combine(appDirs.ConfigDir, txtTemplateFileName.Text);
        }

        private void FrmDeviceProps_Load(object sender, EventArgs e)
        {
            // настройка элементов управления
            fileName = deviceConfig.PollingOptions.CmdLine == null ? "" : deviceConfig.PollingOptions.CmdLine.Trim();
            filePath = appDirs.ConfigDir + fileName;
            //-----------------------
            openFileDialog.InitialDirectory = appDirs.ConfigDir;
            saveFileDialog.InitialDirectory = appDirs.ConfigDir;

            FormTranslator.Translate(this, GetType().FullName);
            openFileDialog.SetFilter(CommonPhrases.XmlFileFilter); // Файлы XML (*.xml)|*.xml|Все файлы (*.*)|*.*
            saveFileDialog.SetFilter(CommonPhrases.XmlFileFilter);

            Text = string.Format(Text, deviceConfig.DeviceNum); // deviceConfig.DeviceNum
            ConfigToControls();

            shortFileName = txtTemplateFileName == null ? "" : txtTemplateFileName.Text;
            if (txtTemplateFileName.Text != "")
            {
                Load_Properties();
            }
        }

        private void Load_Properties()
        {
            try
            {
                devTemplate = FileFunc.LoadXml(typeof(DevTemplate), filePath) as DevTemplate;

                bsSend.DataSource = devTemplate.SndGroups;
                dgvSend.DataSource = bsSend;

                bsCmd.DataSource = devTemplate.CmdGroups;
                dgvComm.DataSource = bsCmd;

                txtName.Text = devTemplate.Name;

                Read_Param();
            }
            catch (FileNotFoundException ex)
            {
                errMsg = ex.Message;
                ScadaUiUtils.ShowError(errMsg);
                txtTemplateFileName.Text = "";
            }
            catch (Exception)
            {
            }
        }

        private void Read_Param()
        {
            List<DevTemplate.SndGroup.Val> vals = new List<DevTemplate.SndGroup.Val>();

            try
            {
                vals = devTemplate.SndGroups[rowIndex].Vals;
            }
            catch
            {
                //richTextBox1.Text += Environment.NewLine + "Мы здесь";
            }
            finally
            {

                bsValue.DataSource = vals;
                dgvVal.DataSource = bsValue;

                dgvVal.AutoGenerateColumns = false;
                dgvVal.Columns["WritableSpecified"].Visible = false;
                dgvVal.Columns["CommandSpecified"].Visible = false;
                dgvVal.Columns["userDataSpecified"].Visible = false;
                dgvVal.Columns["Channel"].Width = 60;
                dgvVal.Columns["Active"].Width = 80;
                dgvVal.Columns["Code"].Width = 100;
                dgvVal.Columns["Name"].Width = 150;
                dgvVal.Columns["Format"].Width = 80;
                dgvVal.Columns["Multiplier"].Width = 80;
                dgvVal.Columns["Writable"].Width = 80;
                dgvVal.Columns["Command"].Width = 80;
                dgvVal.Columns["userData"].Width = 80;
            }
        }

        /// <summary>
        /// Sets the configuration according to the controls.
        /// </summary>
        private void ControlsToConfig()
        {
            deviceConfig.PollingOptions.CmdLine = txtTemplateFileName.Text;
        }

        /// <summary>
        /// Sets the controls according to the configuration.
        /// </summary>
        private void ConfigToControls()
        {
            txtTemplateFileName.Text = deviceConfig.PollingOptions.CmdLine;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            devTemplate.Name = txtName.Text;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtTemplateFileName.Text = Path.GetFileName(openFileDialog.FileName);
                filePath = Path.GetFullPath(openFileDialog.FileName);
                shortFileName = Path.GetFileName(openFileDialog.FileName);
            }
            txtTemplateFileName.Select();

            Load_Properties();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // сохранение шаблона устройства в файл
            SaveChanges(false);
            txtTemplateFileName.Text = shortFileName;
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog.InitialDirectory = appDirs.ConfigDir;
            // сохранение шаблона устройства в новый файл
            SaveChanges(sender == btnSaveAs);
            txtTemplateFileName.Text = shortFileName;
        }

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        private bool SaveChanges(bool saveAs)
        {
            // определение имени файла
            string newFileName = "";

            if (saveAs || shortFileName == "")
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    newFileName = saveFileDialog.FileName;
                }
            }
            else
            {
                newFileName = appDirs.ConfigDir + shortFileName;
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
                    shortFileName = file.Name; // file.Name
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


        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateControls())
            {
                ControlsToConfig();
                DialogResult = DialogResult.OK;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }


        // -------------------------------------- Интерфейс ------------------------------
        private void dgvSend_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvSend.CurrentCell.RowIndex == -1) return;
            rowIndex = e.RowIndex;

            var val = devTemplate.SndGroups[rowIndex].Vals;
            bsValue.DataSource = val;
            dgvVal.DataSource = bsValue;
        }


        private void btnMoveUp_Click(object sender, EventArgs e)
        {

            if (bsValue.Position == 0) return;

            var param = devTemplate.SndGroups[rowIndex].Vals[bsValue.Position];
            devTemplate.SndGroups[rowIndex].Vals.RemoveAt(bsValue.Position);

            bsValue.MovePrevious();
            dgvVal.ClearSelection();
            devTemplate.SndGroups[rowIndex].Vals.Insert(bsValue.Position, param);
            dgvVal.Rows[bsValue.Position].Selected = true;
            rowIndexVal = bsValue.Position; // Корректируем параметр rowIndexVal

            dgvVal.Refresh();
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            if (bsValue.Position + 1 == bsValue.Count) return;

            var param = (DevTemplate.SndGroup.Val)bsValue[bsValue.Position];
            devTemplate.SndGroups[rowIndex].Vals.RemoveAt(bsValue.Position);

            dgvVal.ClearSelection();
            if (bsValue.Position + 1 == bsValue.Count)
            {
                devTemplate.SndGroups[rowIndex].Vals.Add(param);
                bsValue.MoveNext();
            }
            else
            {
                bsValue.MoveNext();
                devTemplate.SndGroups[rowIndex].Vals.Insert(bsValue.Position, param);
            }
            dgvVal.Rows[bsValue.Position].Selected = true;

            rowIndexVal = bsValue.Position; // Корректируем параметр rowIndexVal
        }

        private void dgvVal_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvVal.CurrentCell.RowIndex == -1) return;
            switchCellValueFormat(e);
        }

        private void dgvComm_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvComm.CurrentCell.RowIndex == -1) return;
            switchCellCommFormat(e);
        }

        private void switchCellValueFormat(DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                DataGridViewComboBoxCell fm = new DataGridViewComboBoxCell();
                fm.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
                fm.Style.BackColor = Color.White;
                fm.DataSource = Format;
                dgvVal[e.ColumnIndex, e.RowIndex] = fm;  // change the cell с индексом 4 (Value Format)
            }
        }
        private void switchCellCommFormat(DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                DataGridViewComboBoxCell fm = new DataGridViewComboBoxCell();
                fm.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
                fm.Style.BackColor = Color.White;
                fm.DataSource = Format;
                dgvComm[e.ColumnIndex, e.RowIndex] = fm;  // change the cell с индексом 4 (Value Format)
            }
        }


    }
}
