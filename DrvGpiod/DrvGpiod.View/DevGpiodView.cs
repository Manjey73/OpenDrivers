using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Data.Const;
using Scada.Lang;
using ScadaCommFunc;

namespace Scada.Comm.Drivers.DrvGpiod.View
{
    internal class DevGpiodView : DeviceView
    {
        private DevTemplate devTemplate = new DevTemplate();
        private Dictionary<string, DevTemplate.Gpiod> ActiveDevice = new Dictionary<string, DevTemplate.Gpiod>(); // int - номер pin

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevGpiodView(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig)
            : base(parentView, lineConfig, deviceConfig)
        {
            CanShowProperties = true;
        }

        /// <summary>
        /// Gets the default polling options for the device.
        /// </summary>
        public override PollingOptions GetPollingOptions()
        {
            PollingOptions pollingOptions = new PollingOptions(1000, 300);
            pollingOptions.Period = TimeSpan.FromMinutes(1);
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

            foreach (var dev in ActiveDevice)
            {
                CnlPrototype cannel = new CnlPrototype();

                int cnlType = CnlTypeID.Input;
                if (!string.IsNullOrEmpty(dev.Value.PinMode))
                {
                    if (dev.Value.PinMode.StartsWith("Output")) cnlType = CnlTypeID.InputOutput;
                }

                cannel.Active = dev.Value.Active;
                cannel.Name = dev.Value.Name;
                cannel.CnlTypeID = cnlType;
                cannel.TagCode = dev.Value.Code;
                cannel.FormatCode = FormatCode.OffOn;
                cannel.OutFormatCode = FormatCode.OffOn;

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
                GetDictDevice();
            }
        }

        private void GetDictDevice()
        {
            ActiveDevice.Clear();

            if (devTemplate.Gpios.Count > 0)
            {
                foreach (var dev in devTemplate.Gpios)
                {
                    if (dev.Active && !ActiveDevice.ContainsKey(dev.Code))
                    {
                        ActiveDevice.Add(dev.Code, dev);
                    }
                }
            }
        }
    }
}
