using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Data.Const;
using Scada.Forms.Forms;
using Scada.Lang;
using ScadaCommFunc;
using System.Diagnostics.Metrics;

namespace Scada.Comm.Drivers.DrvRodosBu.View
{
    internal class DevRodosBuView : DeviceView
    {
        #region Value
        private DevTemplate devTemplate = new DevTemplate();
        private Dictionary<string, string> ActiveValue = new Dictionary<string, string>(); // Словарь переменных


        #endregion Value

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevRodosBuView(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig)
            : base(parentView, lineConfig, deviceConfig)
        {
            CanShowProperties = true;
        }

        /// <summary>
        /// Shows a modal dialog box for editing device properties.
        /// </summary>
        public override bool ShowProperties()
        {
            RodosOptions options = new(DeviceConfig.PollingOptions.CustomOptions);
            FrmOptions frmOptions = new() { Options = options };

            if (frmOptions.ShowDialog() == DialogResult.OK)
            {
                options.AddToOptionList(DeviceConfig.PollingOptions.CustomOptions);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the default polling options for the device.
        /// </summary>
        public override PollingOptions GetPollingOptions()
        {
            PollingOptions pollingOptions = new PollingOptions(1000, 300);
            pollingOptions.Period = TimeSpan.FromMinutes(1);
            new RodosOptions().AddToOptionList(pollingOptions.CustomOptions);
            return pollingOptions;
        }

        /// Gets the channel prototypes for the device.
        /// </summary>
        public override ICollection<CnlPrototype> GetCnlPrototypes()
        {
            List<CnlPrototype> cnlPrototypes = new();
            int tagNum = 1;

            GetTemplateFile();

            foreach (var val in ActiveValue)
            {
                        CnlPrototype channel = new CnlPrototype();

                        channel.Active = true;
                        channel.Name = val.Value;
                        channel.CnlTypeID = CnlTypeID.InputOutput;
                        channel.TagCode = val.Key; 
                        channel.FormatCode = FormatCode.OffOn;
                        channel.QuantityCode = QuantityCode.State; 
                        cnlPrototypes.Add(channel);
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
                GetValue();
            }
        }

        private void GetValue()
        {
            ActiveValue.Clear();

            if (devTemplate.Value.Count > 0)
            {
                foreach (var dev in devTemplate.Value)
                {
                    if (!ActiveValue.ContainsKey(dev.code))
                    {
                        ActiveValue.Add(dev.code, dev.name);
                    }
                }
            }
        }
    }
}
