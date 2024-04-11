using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Forms.Forms;
using Scada.Comm.Drivers.DrvDS18B20.Config;
using Scada.Data.Const;
using Scada.Data.Models;

namespace Scada.Comm.Drivers.DrvDS18B20.View
{
    internal class DevDS18B20View : DeviceView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevDS18B20View(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig)
            : base(parentView, lineConfig, deviceConfig)
        {
            CanShowProperties = true;
        }

        /// <summary>
        /// Shows a modal dialog box for editing device properties.
        /// </summary>
        public override bool ShowProperties()
        {
            new FrmModuleConfig(new DsConfigProvider(AppDirs.ConfigDir, DeviceNum)).ShowDialog();
            return false;
        }

        /// <summary>
        /// Gets the channel prototypes for the device.
        /// </summary>
        public override ICollection<CnlPrototype> GetCnlPrototypes()
        {
            // load device configuration
            Ds18b20DeviceConfig config = new();

            if (!config.Load(Path.Combine(AppDirs.ConfigDir, Ds18b20DeviceConfig.GetFileName(DeviceNum)),
                out string errMsg))
            {
                throw new ScadaException(errMsg);
            }

            // create channel prototypes
            List<CnlPrototype> cnlPrototypes = new();
            int eventMask = new EventMask { Enabled = true, StatusChange = true }.Value;

            foreach (VarGroupConfig varGroupConfig in config.VarGroups)
            {
                if(varGroupConfig.Active)
                {
                    foreach (VariableConfig variableConfig in varGroupConfig.Variables)
                    {
                        if (variableConfig.Active)
                        {
                            cnlPrototypes.Add(new CnlPrototype
                            {
                                Active = variableConfig.Active,
                                Name = variableConfig.Name,
                                CnlTypeID = CnlTypeID.Input,
                                TagCode = variableConfig.TagCode,
                                EventMask = eventMask,
                                FormatCode = FormatCode.N2,
                            });
                        }
                    }
                }
            }
            return cnlPrototypes;
        }

        /// <summary>
        /// Gets the default polling options for the device.
        /// </summary>
        public override PollingOptions GetPollingOptions()
        {
            PollingOptions pollingOptions = new PollingOptions(1000, 300);
            pollingOptions.Period = TimeSpan.FromMinutes(1);
            return pollingOptions;
        }
    }
}
