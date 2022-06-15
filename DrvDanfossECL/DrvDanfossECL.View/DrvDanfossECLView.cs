using Scada.Lang;
using Scada.Comm.Config;
using Scada.Comm.Devices;

namespace Scada.Comm.Drivers.DrvDanfossECL.View
{
    public class DrvDanfossECLView : DriverView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvDanfossECLView()
        {
            CanCreateDevice = true;
        }

        /// <summary>
        /// Gets the driver name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ? "Драйвер Danfoss ECL 200/300" : "Danfoss ECL 200/300 Driver";
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
                "Тут что-то типа инфы\n\n" +
                "Продолжаем инфу если надо" :

                "There's something like info here\n\n" +
                "We continue the information if necessary";
            }
        }


        /// <summary>
        /// Creates a new device user interface.
        /// </summary>
        public override DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
        {
            return new DevDanfossECLView(this, lineConfig, deviceConfig);
        }


    }
}
