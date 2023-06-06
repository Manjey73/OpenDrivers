using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Data.Const;
using Scada.Lang;
using ScadaCommFunc;

namespace Scada.Comm.Drivers.DrvPulsar.View
{
    internal class DevPulsarView : DeviceView
    {
        private DevTemplate devTemplate = new DevTemplate();

        //private int sigN = 1;
        private bool activeuse = false;
        private Dictionary<int, int> ActiveSnd = new Dictionary<int, int>();            // Ключ = Номер запроса SndCnt - Значение = Индекс Активного запроса SndCnt

        private Dictionary<string, DevTemplate.SndGroup.Val> ActiveValue = new Dictionary<string, DevTemplate.SndGroup.Val>();
        private Dictionary<string, DevTemplate.CmdGroup> ActiveCmd = new Dictionary<string, DevTemplate.CmdGroup>();

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevPulsarView(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig)
            : base(parentView, lineConfig, deviceConfig)
        {
            CanShowProperties = true;
        }

        /// <summary>
        /// Gets the default polling options for the device.
        /// </summary>
        public override PollingOptions GetPollingOptions()
        {
            PollingOptions pollingOptions = new PollingOptions(1000, 100);
            //pollingOptions.Period = TimeSpan.FromMinutes(1);
            return pollingOptions;
        }

        /// <summary>
        /// Shows a modal dialog box for editing device properties.
        /// </summary>
        public override bool ShowProperties()
        {
            if (new FrmDeviceProps(AppDirs, LineConfig, DeviceConfig).ShowDialog() == DialogResult.OK) // , customUi
            {
                LineConfigModified = true;
                DeviceConfigModified = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// Gets the channel prototypes for the device.
        /// </summary>
        public override ICollection<CnlPrototype> GetCnlPrototypes()
        {
            List<CnlPrototype> cnlPrototypes = new();
            int tagNum = 1;

            GetTemplateFile();

            string format = "";

            foreach (var dev in ActiveValue) // ActiveCnl
            {
                CnlPrototype cannel = new CnlPrototype();
                format = dev.Value.Format;

                int typeId = CnlTypeID.Input;
                if (dev.Value.Writable) typeId = CnlTypeID.InputOutput;

                cannel.Name = dev.Value.Name;
                cannel.CnlTypeID = typeId;
                cannel.TagCode = dev.Value.Code;

                if (format == "float" || format == "double")
                    cannel.FormatCode = FormatCode.N2;
                else if (format == "DateTime")
                    cannel.FormatCode = FormatCode.DateTime;
                else if (format == "uint16" || format == "uint32")
                    cannel.FormatCode = FormatCode.N0;

                if (cannel.FormatCode == FormatCode.String) cannel.DataTypeID = DataTypeID.Unicode;

                cnlPrototypes.Add(cannel);
                tagNum++;
            }

            foreach (var dev in ActiveCmd)
            {
                format = dev.Value.Format;
                CnlPrototype cannel = new CnlPrototype();

                cannel.Name = dev.Value.Name;
                cannel.CnlTypeID = CnlTypeID.Output;
                cannel.TagCode = dev.Value.Code;
                if (format == "float" || format == "double")
                    cannel.FormatCode = FormatCode.N2;
                else if (format == "DateTime")
                    cannel.FormatCode = FormatCode.DateTime;
                else if (format == "uint16" || format == "uint32") 
                    cannel.FormatCode = FormatCode.N0;

                if (cannel.FormatCode == FormatCode.String) cannel.DataTypeID = DataTypeID.Unicode;

                cnlPrototypes.Add(cannel);
                tagNum++;
            }

            return cnlPrototypes;

        }

        private void GetTemplateFile()
        {
            devTemplate = null;

            // загрузка шаблона устройства
            string fileName = DeviceConfig.PollingOptions.CmdLine == null ? "" : DeviceConfig.PollingOptions.CmdLine.Trim();

            string filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppDirs.ConfigDir, fileName);

            try
            {
                devTemplate = FileFunc.LoadXml(typeof(DevTemplate), filePath) as DevTemplate;
            }
            catch (Exception ex)
            {
                throw new ScadaException(string.Format(Locale.IsRussian ?
                "Ошибка при получении типа логики Устройства из библиотеки {0}" :
                "Error getting device logic type from the library {0}", ex.Message), ex);
            }

            // Проверка на наличие конфигурации XML
            if (devTemplate != null)
            {
                ActiveSnd.Clear(); // Очищаем список, так как код срабатывает при выборе КП при Создании каналов каждый раз...
                ActiveValue.Clear();
                // Определить Номера активных запросов.
                if (devTemplate.SndGroups.Count != 0) // Определить наличие списка запросов, найти активные запросы и записать в массив номера активных запросов для создания тегов по номерам 
                {
                    for (int snd = 0; snd < devTemplate.SndGroups.Count; snd++)
                    {
                        if (devTemplate.SndGroups[snd].Active) // Если запрос активен, заносим его номер SndCnt в массив
                        {
                            if (!ActiveSnd.ContainsKey(devTemplate.SndGroups[snd].Counter))                              // Ключ = SndCnt - Значение = Индекс Активного запроса SndCnt
                            {
                                ActiveSnd.Add(devTemplate.SndGroups[snd].Counter, devTemplate.SndGroups.FindIndex(x => x.Counter == devTemplate.SndGroups[snd].Counter));
                            }
                            activeuse = true; // Есть активные запросы
                        }
                    }
                }

                if (activeuse)
                {
                    for (int ac = 0; ac < ActiveSnd.Count; ac++)
                    {
                        for (int val = 0; val < devTemplate.SndGroups[ac].Vals.Count; val++)
                        {
                            if (devTemplate.SndGroups[ac].Vals[val].Active)            // Проверяем переменную на активность
                            {
                                if (devTemplate.SndGroups[ac].Vals[val].Code != null)
                                {
                                    if (!ActiveValue.ContainsKey(devTemplate.SndGroups[ac].Vals[val].Code))
                                    {
                                        ActiveValue.Add(devTemplate.SndGroups[ac].Vals[val].Code, devTemplate.SndGroups[ac].Vals[val]);
                                    }
                                }
                            }
                        }
                    }
                }

                if (devTemplate.CmdGroups.Count != 0) // Определяем наличие активных команд и заносим в словарь Индексы команд с нулевого значения
                {
                    ActiveCmd.Clear();
                    for (int cmd = 0; cmd < devTemplate.CmdGroups.Count; cmd++)
                    {
                        if (devTemplate.CmdGroups[cmd].Active)
                        {
                            if (!ActiveCmd.ContainsKey(devTemplate.CmdGroups[cmd].Code))
                            {
                                ActiveCmd.Add(devTemplate.CmdGroups[cmd].Code, devTemplate.CmdGroups[cmd]);
                            }
                        }
                    }
                }

            }
        }
    }
}
