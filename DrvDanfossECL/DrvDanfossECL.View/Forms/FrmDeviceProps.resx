using Scada.Forms;
using Scada.Comm.Config;
using Scada.Lang;
using Scada.Forms.Forms;
using ScadaCommFunc;

namespace Scada.Comm.Drivers.DrvDanfossECL.View.Forms
{
    public partial class FrmDeviceProps : Form
    {
        /// <summary>
        /// Имя файла нового шаблона устройства
        /// </summary>
        private const string NewFileName = "DanfossECL_NewTemplate.xml";

        private readonly AppDirs appDirs;           // the application directories
        private readonly LineConfig lineConfig;     // the communication line configuration
        private DeviceConfig deviceConfig; // the device configuration // readonly
        private string errMsg = "";
        private string FileName;

        // Интерфейс
        private DevTemplate devTemplate = new DevTemplate();
        private int rowIndex;
        private int newRowIndex;
        private BindingSource bsAll = new BindingSource();
        private List<string> lFormat = new List<string> {"sbyte", "int16", "temp" }; // fill the drop down items.. Выпадающий список для ячейки Format 
        // Интерфейс

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmDeviceProps()
        {
            InitializeComponent();

            devTemplate.Parameter.Add(new DevTemplate.Parameters()); // Создаем пустой класс для первой строки

            bsAll.DataSource = devTemplate.Parameter; // 
            dgvCmd.DataSource = bsAll;

            // Настройки вида 
            dgvCmd.AutoGenerateColumns = false;
            dgvCmd.Columns["min_valSpecified"].Visible = false; // Скрыть столбец со служебными переменными
            dgvCmd.Columns["max_valSpecified"].Visible = false; // Скрыть столбец со служебными переменными
            dgvCmd.Columns["MultiplierSpecified"].Visible = false; // Скрыть столбец со служебными переменными
            dgvCmd.Columns["Code"].Width = 150;
            dgvCmd.Columns["Name"].Width = 220;
            dgvCmd.Columns["Active"].Width = 56;
            dgvCmd.Columns["Address"].Width = 60;
            dgvCmd.Columns["Write"].Width = 56;
            dgvCmd.Columns["min_val"].Width = 60;
            dgvCmd.Columns["max_val"].Width = 60;
            dgvCmd.Columns["Format"].Width = 70;
            dgvCmd.Columns["Multiplier"].Width = 70;

            //События для DataGridView
            dgvCmd.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvCmd_CellMouseClick);
            dgvCmd.SelectionChanged += new EventHandler(dgvCmd_SelectionChanged);
            dgvCmd.UserDeletedRow += new DataGridViewRowEventHandler(dgvCmd_UserDeletedgRow);
            dgvCmd.DataError += new DataGridViewDataErrorEventHandler(dgvCmd_DataError);
            dgvCmd.RowsAdded += new DataGridViewRowsAddedEventHandler(dgvCmd_RowsAdded);
        }


        /// <summary>
        /// Validates the form controls.
        /// </summary>
        private bool ValidateControls()
        {
            if (!File.Exists(GetTemplatePath()))
            {
                ScadaUiUtils.ShowError(Locale.IsRussian ? "Файл шаблона устройства не существует." : "The device template file does not exist.");
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

        private void btnBrowseTemplate_Click(object sender, EventArgs e)
        {
            // show dialog to select template file
            openFileDialog.InitialDirectory = appDirs.ConfigDir; // TEST
            openFileDialog.FileName = "";

            if (openFileDialog.ShowDialog() == DialogResult.OK &&
                ValidateTemplatePath(openFileDialog.FileName, out string shortFileName))
            {
                txtTemplateFileName.Text = shortFileName;
                ControlsToConfig();
                FileName = GetTemplatePath();
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
                devTemplate = FileFunc.LoadXml(typeof(DevTemplate), GetTemplatePath()) as DevTemplate;
                bsAll.DataSource = devTemplate.Parameter;
                dgvCmd.DataSource = bsAll;

                txtDevName.Text = devTemplate.Name;
            }
            catch (FileNotFoundException ex)
            {
                errMsg = ex.Message;
                ScadaUiUtils.ShowError(errMsg);
                txtTemplateFileName.Text = "";
            }
            catch // (Exception err)
            {
                //ScadaUiUtils.ShowError(err.ToString());
            }
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
                ScadaUiUtils.ShowError(Locale.IsRussian ? "Файл шаблона устройства должен располагаться внутри {0}" : "The device template file should be located inside {0}", appDirs.ConfigDir);
                shortFileName = "";
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

        private void FrmDeviceProps_Load(object sender, EventArgs e)
        {
            // настройка элементов управления
            openFileDialog.InitialDirectory = appDirs.ConfigDir;

            FormTranslator.Translate(this, GetType().FullName);
            openFileDialog.SetFilter(CommonPhrases.XmlFileFilter);
            saveFileDialog.SetFilter(CommonPhrases.XmlFileFilter);

            Text = string.Format(Text, deviceConfig.DeviceNum); // deviceConfig.DeviceNum
            ConfigToControls();

            if (!string.IsNullOrEmpty(txtTemplateFileName.Text))
            {
                FileName = GetTemplatePath();
                Load_Properties();
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            // сохранение шаблона устройства в файл
            SaveChanges(false);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog.InitialDirectory = appDirs.ConfigDir;
            // сохранение шаблона устройства в новый файл
            SaveChanges(sender == btnSaveAs);
        }

        ///// <summary>
        ///// Сохранить изменения
        ///// </summary>
        private bool SaveChanges(bool saveAs)
        {
            // определение имени файла
            string newFileName = txtTemplateFileName.Text;

            if (saveAs || string.IsNullOrEmpty(FileName))
            {
                saveFileDialog.FileName = string.IsNullOrEmpty(FileName) ? NewFileName : Path.GetFileName(FileName);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    newFileName = saveFileDialog.FileName;
                else
                    return false; // при отказе записи
            }
            else
            {
                newFileName = FileName;
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
                    FileName = newFileName;
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
        public bool Save(string FileName, out string errMsg)
        {

            try
            {
                bool save = FileFunc.SaveXml(devTemplate, FileName); // добавить сохранение в выбранный файл TEST
                errMsg = "";

                if (save && ValidateTemplatePath(FileName, out string shortFileName))
                    txtTemplateFileName.Text = shortFileName;

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }

        }

        // -------------------------------------- Интерфейс ------------------------------
        private void txtDevName_TextChanged(object sender, EventArgs e)
        {
            devTemplate.Name = txtDevName.Text;
        }

        private void dgvCmd_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvCmd.CurrentCell.RowIndex == -1) return;
            rowIndex = e.RowIndex;

            // Если ячейка ComboBoxCell разрешаем ее редактировать сразу (избавления от тройного щелчка мыши)
            // При изменении стиля ячейки в Nothing добавляется лишний клик (глюк)
            if (dgvCmd.CurrentCell.GetType() == typeof(DataGridViewComboBoxCell))
            {
                dgvCmd.BeginEdit(true);
            }
        }

        private void dgvCmd_UserDeletedgRow(object sender, DataGridViewRowEventArgs e)
        {
            rowIndex = dgvCmd.CurrentCell.RowIndex;
        }

        private void dgvCmd_SelectionChanged(object sender, EventArgs e)
        {
            newRowIndex = dgvCmd.NewRowIndex;
        }

        private void btnMoveUpItem_Click(object sender, EventArgs e)
        {
            if (bsAll.Position == 0)
                return;

            var cmdGroup = devTemplate.Parameter[bsAll.Position];
            devTemplate.Parameter.RemoveAt(bsAll.Position);

            bsAll.MovePrevious();
            dgvCmd.ClearSelection();
            devTemplate.Parameter.Insert(bsAll.Position, cmdGroup);
            dgvCmd.Rows[bsAll.Position].Selected = true;
            rowIndex = bsAll.Position; // Корректируем параметр rowIndex

            dgvCmd.Refresh();
        }

        private void btnMoveDownItem_Click(object sender, EventArgs e)
        {
            if (bsAll.Position + 1 == bsAll.Count) return;

            var cmdGroup = devTemplate.Parameter[bsAll.Position];
            devTemplate.Parameter.RemoveAt(bsAll.Position);

            dgvCmd.ClearSelection();
            if (bsAll.Position + 1 == bsAll.Count)
            {
                devTemplate.Parameter.Add(cmdGroup);
                bsAll.MoveNext();
            }
            else
            {
                bsAll.MoveNext();
                devTemplate.Parameter.Insert(bsAll.Position, cmdGroup);
            }
            dgvCmd.Rows[bsAll.Position].Selected = true;

            rowIndex = bsAll.Position; // Корректируем параметр rowIndex

            dgvCmd.Refresh();
        }

        private void dgvCmd_DataError(object sender, DataGridViewDataErrorEventArgs anErr) // Если ошиблись с вводом параметра то в случае отсутствия будет пустая строка
        {
            string cval = (dgvCmd[anErr.ColumnIndex, anErr.RowIndex].Value as string).ToLower();

            int ilist = lFormat.FindIndex(x => x == cval);
            if (ilist < 0)
                dgvCmd[anErr.ColumnIndex, anErr.RowIndex].Value = "";
            else
                dgvCmd[anErr.ColumnIndex, anErr.RowIndex].Value = cval;
        }

        // Изменение DataGridTextBox в нужный DataGridComboBoxCell
        // При необходимости делаем для других колонок.
        private void dgvCmd_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            switchCellFormat(e);
        }

        private void switchCellFormat(DataGridViewRowsAddedEventArgs e)
        {
            DataGridViewComboBoxCell c = new DataGridViewComboBoxCell();
            c.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
            c.Style.BackColor = Color.White;
            c.DataSource = lFormat;

            dgvCmd[7, e.RowIndex] = c;  // change the cell с индексом 7 (Format)
        }
        // -------------------------------------- Интерфейс ------------------------------
    }
}
