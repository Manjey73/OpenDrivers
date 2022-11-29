using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Lang;
using ScadaCommFunc;
using Scada.Data.Const;
using Scada.Comm.Drivers.DrvDanfossECL.View.Forms;

namespace Scada.Comm.Drivers.DrvDanfossECL.View
{
    internal class DevDanfossECLView : DeviceView
    {
        private DevTemplate devTemplate = new DevTemplate();
        Dictionary<string, CnlPrototypeFactory.ActiveChannel> channels = new Dictionary<string, CnlPrototypeFactory.ActiveChannel>();

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevDanfossECLView(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig)
            : base(parentView, lineConfig, deviceConfig)
        {
            CanShowProperties = true;
        }

        /// <summary>
        /// Gets the default polling options for the device.
        /// </summary>
        public override PollingOptions GetPollingOptions()
        {
            PollingOptions pollingOptions = new PollingOptions(1000, 400); // Wait 400 ms задержка из документации, но возможно работает быстрее
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

        public override ICollection<CnlPrototype> GetCnlPrototypes()
        {
            GetDictChannel();
            return CnlPrototypeFactory.GetCnlPrototypes(channels);
        }

        public void GetDictChannel()
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
                channels.Clear(); // Очищаем список, так как код срабатывает при выборе КП при Создании каналов каждый раз...

                if (devTemplate.Parameter.Count > 0) // Определить активные запросы объектов и записать в список индексы запросов для создания тегов
                {
                    for (int sg = 0; sg < devTemplate.Parameter.Count; sg++)
                    {
                        if (devTemplate.Parameter[sg].Active)
                        {
                            string format = string.IsNullOrEmpty(devTemplate.Parameter[sg].Format) ? "sbyte" : devTemplate.Parameter[sg].Format; // TEST List<string>
                            int datatype = DataTypeID.Double;
                            int cnltype;

                            if(devTemplate.Parameter[sg].Write && devTemplate.Parameter[sg].Address != "")
                            {
                                cnltype = CnlTypeID.InputOutput;
                            }
                            else
                            {
                                cnltype = CnlTypeID.Input;
                            }

                            channels.Add(devTemplate.Parameter[sg].Name,
                            new CnlPrototypeFactory.ActiveChannel()
                            {
                                Name = devTemplate.Parameter[sg].Name,
                                Code = devTemplate.Parameter[sg].Code,
                                CnlType = cnltype,
                                DataType = datatype,
                                format = format,
                            });
                        }
                    }
                }
            }
        }


    }
}
