using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Lang;

namespace Scada.Comm.Drivers.DrvPulsar.View
{
    public class DrvPulsarView : DriverView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvPulsarView()
        {
            ProductCode = "DrvPulsar";
            CanCreateDevice = true;
        }

        /// <summary>
        /// Gets the driver name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ? "Драйвер протокола Пульсар." : "The Pulsar protocol driver.";
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
                "Драйвер протокола Пульсар.\n" :

                "The Pulsar protocol driver.\n";
            }
        }

        /// <summary>
        /// Creates a new device user interface.
        /// </summary>
        public override DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
        {
            return new DevPulsarView(this, lineConfig, deviceConfig);
        }
    }
}
