using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Lang;

namespace Scada.Comm.Drivers.DrvGpiod.View
{
    public class DrvGpiodView : DriverView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvGpiodView()
        {
            ProductCode = "DrvGpiod";
            CanCreateDevice = true;
        }

        /// <summary>
        /// Gets the driver name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ? "Драйвер GPIOD" : "GPIOD Driver";
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
                "Драйвер управления портами GPIO\n" :


                "GPIO Port Management Driver\n";
            }
        }


        /// <summary>
        /// Creates a new device user interface.
        /// </summary>
        public override DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
        {
            return new DevGpiodView(this, lineConfig, deviceConfig);
        }
    }
}