
using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Data.Const;

namespace Scada.Comm.Drivers.DrvBetarV12.View
{
    internal class DevBetarV12View : DeviceView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevBetarV12View(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig)
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
            // new MBusOptions().AddToOptionList(pollingOptions.CustomOptions);
            return pollingOptions;
        }

        /// <summary>
        /// Gets the grouped channel prototypes.
        /// </summary>
        public static List<CnlPrototypeGroup> GetGroups()
        {
            List<CnlPrototypeGroup> groups = new List<CnlPrototypeGroup>();

            CnlPrototypeGroup group = new CnlPrototypeGroup("Inputs");
            group.AddCnlPrototype("directflow", "Пок. прямого потока");
            group.AddCnlPrototype("reverseflow", "Пок. обратного потока");
            group.AddCnlPrototype("magnetictime", "Время магн. воздействия");
            group.AddCnlPrototype("servicebyte", "Служебный байт").SetFormat(FormatCode.N0);
            groups.Add(group);

            return groups;
        }

        /// <summary>
        /// Gets the channel prototypes for the device.
        /// </summary>
        public override ICollection<CnlPrototype> GetCnlPrototypes()
        {
            return GetGroups().GetCnlPrototypes();
        }

    }
}
