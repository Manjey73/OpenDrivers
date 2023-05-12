using Scada.Comm.Config;
using Scada.Forms;
using Scada.Lang;
using ScadaCommFunc;

namespace Scada.Comm.Drivers.DrvGpiod.View
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
        private BindingSource bsTemplate = new BindingSource();
        // Интерфейс

        private List<string> pinmode = new List<string> { "", "Input", "Output", "InputPullDown", "InputPullUp" }; // Выпадающий список для ячейки PinMode
        private List<string> stvalue = new List<string> { "", "Low", "High" }; // Выпадающий список для ячейки стартовго значения


        public FrmDeviceProps()
        {
            InitializeComponent();

            DevTemplate.Gpiod gpiod = new DevTemplate.Gpiod();
            devTemplate.Gpios.Add(gpiod);



            bsTemplate.DataSource = devTemplate.Gpios;
            dgvTemplate.DataSource = bsTemplate;

            dgvTemplate.AutoGenerateColumns = false;

            //События для DataGridView
            dgvTemplate.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvTemplate_CellMouseClick);
        }


        /// <summary>
        /// Validates the form controls.
        /// </summary>
        /// 
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
        /// 
        /// Gets the file path of the device template.
        /// </summary>
        private string GetTemplatePath()
        {
            return Path.Combine(appDirs.ConfigDir, txtTemplateFileName.Text);
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

            //-------------------------
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

        private void Load_Properties()
        {
            try
            {
                devTemplate = FileFunc.LoadXml(typeof(DevTemplate), filePath) as DevTemplate;
                bsTemplate.DataSource = devTemplate.Gpios;
                dgvTemplate.DataSource = bsTemplate;

                txtName.Text = devTemplate.Name;

                //Read_Param();
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

        private void switchCellMClick(DataGridViewCellMouseEventArgs e)
        {

            if (e.ColumnIndex == 4)
            {
                DataGridViewComboBoxCell pm = new DataGridViewComboBoxCell();
                pm.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
                pm.Style.BackColor = Color.White;
                pm.DataSource = pinmode;
                dgvTemplate[4, e.RowIndex] = pm;  // change the cell с индексом 4 (PinMode)
                //pm.ContextMenuStrip = DataGridViewEditMode.EditOnEnter;
            }
            else if (e.ColumnIndex == 5)
            {
                DataGridViewComboBoxCell vl = new DataGridViewComboBoxCell();
                vl.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
                vl.Style.BackColor = Color.White;
                vl.DataSource = stvalue;
                dgvTemplate[5, e.RowIndex] = vl;  // change the cell с индексом 5 (Start Value)
            }
        }

        private void dgvTemplate_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvTemplate.CurrentCell.RowIndex == -1) return;
            rowIndex = e.RowIndex;

            dgvTemplate.Columns[dgvTemplate.CurrentCell.ColumnIndex].Selected = true;
            switchCellMClick(e);
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            if (bsTemplate.Position == 0)
                return;

            var param = devTemplate.Gpios[bsTemplate.Position];
            devTemplate.Gpios.RemoveAt(bsTemplate.Position);

            bsTemplate.MovePrevious();
            dgvTemplate.ClearSelection();
            devTemplate.Gpios.Insert(bsTemplate.Position, param);
            dgvTemplate.Rows[bsTemplate.Position].Selected = true;
            rowIndex = bsTemplate.Position; // Корректируем параметр rowIndex

            dgvTemplate.Refresh();
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            if (bsTemplate.Position + 1 == bsTemplate.Count) return;

            var param = (DevTemplate.Gpiod)bsTemplate[bsTemplate.Position];
            devTemplate.Gpios.RemoveAt(bsTemplate.Position);

            dgvTemplate.ClearSelection();
            if (bsTemplate.Position + 1 == bsTemplate.Count)
            {
                devTemplate.Gpios.Add(param);
                bsTemplate.MoveNext();
            }
            else
            {
                bsTemplate.MoveNext();
                devTemplate.Gpios.Insert(bsTemplate.Position, param);
            }
            dgvTemplate.Rows[bsTemplate.Position].Selected = true;

            rowIndex = bsTemplate.Position; // Корректируем параметр rowIndex
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            devTemplate.Name = txtName.Text;
        }

    }
}
