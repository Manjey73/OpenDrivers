using Scada.Forms;
using Scada.Comm.Config;
using Scada.Lang;
using Scada.Forms.Forms;
using ScadaCommFunc;

namespace Scada.Comm.Drivers.DrvDanfossECL.View.Forms
{
    public partial class FrmDeviceProps : Form
    {
        private readonly AppDirs appDirs;           // the application directories
        private readonly LineConfig lineConfig;     // the communication line configuration
        private DeviceConfig deviceConfig; // the device configuration // readonly
        private string errMsg = "";
        string shortFileName;

        // Интерфейс
        private DevTemplate devTemplate = new DevTemplate();
        private int rowIndex;
        private int newRowIndex;
        private BindingSource bsAll = new BindingSource();
        private List<string> lFormat = new List<string> { "", "float", "byte", "syte", "int16", "temp" }; // fill the drop down items.. Выпадающий список для ячейки Format 
        // Интерфейс

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmDeviceProps()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.FixedSingle;  // Убираем возможность растяжения окна
            MaximizeBox = false;                            // Скрываем кнопки увеличения и уменьшения окна
            MinimizeBox = false;

            devTemplate.CmdGroups.Add(new DevTemplate.CmdGroup()); // Создаем пустой класс для первой строки

            bsAll.DataSource = devTemplate.CmdGroups; // 
            dgvCmd.DataSource = bsAll;

            // Настройки вида 
            dgvCmd.AutoGenerateColumns = false;
            dgvCmd.Columns["MulSpecified"].Visible = false; // Скрыть столбец со служебными переменными
            dgvCmd.Columns["DivSpecified"].Visible = false; // Скрыть столбец со служебными переменными
            dgvCmd.Columns["Name"].Width = 140;
            dgvCmd.Columns["Active"].Width = 56;
            dgvCmd.Columns["Read"].Width = 56;
            dgvCmd.Columns["Write"].Width = 56;
            dgvCmd.Columns["Parameter"].Width = 80;
            dgvCmd.Columns["Format"].Width = 80;
            dgvCmd.Columns["Mul"].Width = 80;
            dgvCmd.Columns["Div"].Width = 80;

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
                ScadaUiUtils.ShowError("Файл шаблона устройства не существует."); // ModbusDriverPhrases.TemplateNotExists
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
            openFileDialog.InitialDirectory = appDirs.ConfigDir;
            openFileDialog.FileName = "";

            if (openFileDialog.ShowDialog() == DialogResult.OK &&
                ValidateTemplatePath(openFileDialog.FileName, out string shortFileName))
            {
                txtTemplateFileName.Text = shortFileName;
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
                bsAll.DataSource = devTemplate.CmdGroups;
                dgvCmd.DataSource = bsAll;

                txtDevName.Text = devTemplate.Name;
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
                //richTextBox1.Text += $"Catch   {err}" + Environment.NewLine;
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
                ScadaUiUtils.ShowError("Файл шаблона устройства должен располагаться внутри {0}", appDirs.ConfigDir); // ModbusDriverPhrases.ConfigDirRequired
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
            FormTranslator.Translate(this, GetType().FullName);
            openFileDialog.SetFilter(CommonPhrases.XmlFileFilter);
            saveFileDialog.SetFilter(CommonPhrases.XmlFileFilter);


            Text = string.Format(Text, deviceConfig.DeviceNum); // deviceConfig.DeviceNum
            ConfigToControls();


            shortFileName = txtTemplateFileName == null ? "" : txtTemplateFileName.Text; // TEST
            if (txtTemplateFileName.Text != "")
            {
                Load_Properties();
            }
        }

        /// <summary>
        /// Sets the configuration according to the controls.
        /// </summary>
        private void ControlsToConfig()
        {
            //lineConfig.CustomOptions["TransMode"] = ((TransMode)cbTransMode.SelectedIndex).ToString();
            deviceConfig.PollingOptions.CmdLine = txtTemplateFileName.Text;
        }

        /// <summary>
        /// Sets the controls according to the configuration.
        /// </summary>
        private void ConfigToControls()
        {
            //cbTransMode.SelectedIndex = (int)lineConfig.CustomOptions.GetValueAsEnum("TransMode", TransMode.RTU);
            txtTemplateFileName.Text = deviceConfig.PollingOptions.CmdLine;
        }

        private void ButExt_Click(object sender, EventArgs e) // Тестовый пример вызова окна Custom Options
        {
            DanfossECLOptions options = new(deviceConfig.PollingOptions.CustomOptions);
            FrmOptions frmOptions = new() { Options = options };

            if (frmOptions.ShowDialog() == DialogResult.OK)
            {
                options.AddToOptionList(deviceConfig.PollingOptions.CustomOptions);
            }
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
            txtTemplateFileName.Text = shortFileName;

        }

        ///// <summary>
        ///// Сохранить изменения
        ///// </summary>
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

            var cmdGroup = devTemplate.CmdGroups[bsAll.Position];
            devTemplate.CmdGroups.RemoveAt(bsAll.Position);

            bsAll.MovePrevious();
            dgvCmd.ClearSelection();
            devTemplate.CmdGroups.Insert(bsAll.Position, cmdGroup);
            dgvCmd.Rows[bsAll.Position].Selected = true;
            rowIndex = bsAll.Position; // Корректируем параметр rowIndex

            dgvCmd.Refresh();
        }

        private void btnMoveDownItem_Click(object sender, EventArgs e)
        {
            if (bsAll.Position + 1 == bsAll.Count) return;

            var cmdGroup = devTemplate.CmdGroups[bsAll.Position];
            devTemplate.CmdGroups.RemoveAt(bsAll.Position);

            dgvCmd.ClearSelection();
            if (bsAll.Position + 1 == bsAll.Count)
            {
                devTemplate.CmdGroups.Add(cmdGroup);
                bsAll.MoveNext();
            }
            else
            {
                bsAll.MoveNext();
                devTemplate.CmdGroups.Insert(bsAll.Position, cmdGroup);
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

            dgvCmd[6, e.RowIndex] = c;  // change the cell с индексом 6 (Format)
        }
        // -------------------------------------- Интерфейс ------------------------------
    }
}
