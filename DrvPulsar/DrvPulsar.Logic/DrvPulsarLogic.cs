using Scada.Comm.Config;
using Scada.Comm.Devices;

namespace Scada.Comm.Drivers.DrvPulsar.Logic
{
    public class DrvPulsarLogic : DriverLogic
    {

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvPulsarLogic(ICommContext commContext)
            : base(commContext)
        {
        }

        /// <summary>
        /// Gets the driver code.
        /// </summary>
        public override string Code
        {
            get
            {
                return "DrvPulsar";
            }
        }


        /// <summary>
        /// Creates a new device.
        /// </summary>
        public override DeviceLogic CreateDevice(ILineContext lineContext, DeviceConfig deviceConfig)
        {
            return new DevPulsarLogic(CommContext, lineContext, deviceConfig);
        }

    }
}
