using Scada.Comm.Config;
using Scada.Comm.Devices;


namespace Scada.Comm.Drivers.DrvMercury23x.Logic
{
    public class DrvMercury23xLogic : DriverLogic
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DrvMercury23xLogic(ICommContext commContext)
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
                return "DrvMercury23x";
            }
        }

        /// <summary>
        /// Creates a new device.
        /// </summary>
        public override DeviceLogic CreateDevice(ILineContext lineContext, DeviceConfig deviceConfig)
        {
            return new DevMercury23xLogic(CommContext, lineContext, deviceConfig);
        }
    }
}
