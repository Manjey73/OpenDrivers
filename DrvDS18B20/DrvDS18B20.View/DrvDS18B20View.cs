using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Drivers.DrvDS18B20.Config;
using Scada.ComponentModel;
using Scada.Forms;
using Scada.Lang;

namespace Scada.Comm.Drivers.DrvDS18B20.View
{
    public class DrvDS18B20View : DriverView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvDS18B20View()
        {
            ProductCode = "DrvDS18B20";
            CanCreateDevice = true;
        }

        /// <summary>
        /// Gets the driver name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ? "Драйвер DS18B20" : "DS18B20 Driver";
            }
        }

        /// <summary>
        /// Gets the driver description.
        /// </summary>
        public override string Descr
        {
            get
            {
                // На русском и английском информация
                return Locale.IsRussian ?
                "Драйвер чтения датчиков температуры ds18b20\n" :


                "ds18b20 Temperature Sensor Reader Driver\n";
            }
        }


        /// <summary>
        /// Loads language dictionaries.
        /// </summary>
        public override void LoadDictionaries()
        {
            if (!Locale.LoadDictionaries(AppDirs.LangDir, DriverUtils.DriverCode, out string errMsg))
                ScadaUiUtils.ShowError(errMsg);

            DriverPhrases.Init();
            AttrTranslator.Translate(typeof(DeviceOptions));
            AttrTranslator.Translate(typeof(VarGroupConfig));
            AttrTranslator.Translate(typeof(VariableConfig));
        }


        /// <summary>
        /// Creates a new device user interface.
        /// </summary>
        public override DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
        {
            return new DevDS18B20View(this, lineConfig, deviceConfig);
        }
    }
}
