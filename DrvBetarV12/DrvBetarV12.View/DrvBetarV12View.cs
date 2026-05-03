using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Lang;

namespace Scada.Comm.Drivers.DrvBetarV12.View
{
    public class DrvBetarV12View : DriverView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvBetarV12View()
        {
            CanCreateDevice = true;

            RegistrationInfo = new RegistrationInfo
            {
                ProductCode = "DrvBetarV12",
            };

        }

        /// <summary>
        /// Gets the driver name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ? "Драйвер счетчиков воды Бетар." : "The Betar water meter driver.";
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
                "Драйвер счетчиков воды  Бетар - крыльчатые электронные СХВЭ, СГВЭ" :
                "Betar Water Meter Driver - Vane Electronic SHVE, SGVE";
            }
        }

        /// <summary>
        /// Creates a new device user interface.
        /// </summary>
        public override DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
        {
            return new DevBetarV12View(this, lineConfig, deviceConfig);
        }

    }
}
