using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Lang;

namespace Scada.Comm.Drivers.DrvRodosBu.View
{
    public class DrvRodosBuView : DriverView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvRodosBuView()
        {
            CanCreateDevice = true;
            ProductCode = "DrvRodosBu";
        }

        /// <summary>
        /// Gets the driver name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ? "Драйвер реле Rodos." : "Rodos Relay Driver.";
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
                "Драйвер реле Rodos." :
                "Rodos Relay Driver.";
            }
        }

        /// <summary>
        /// Creates a new device user interface.
        /// </summary>
        public override DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
        {
            return new DevRodosBuView(this, lineConfig, deviceConfig);
        }

    }
}
