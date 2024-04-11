using Scada.Lang;

namespace Scada.Comm.Drivers.DrvDS18B20.View
{
    /// <summary>
    /// The phrases used by the driver.
    /// <para>Фразы, используемые драйвером.</para>
    /// </summary>
    public static class DriverPhrases
    {
        // Scada.Comm.Drivers.DrvDS18B20.View.DsConfigProvider
        public static string FormTitle { get; private set; }
        public static string AddVarGroupButton { get; private set; }
        public static string AddVariableButton { get; private set; }
        public static string OptionsNode { get; private set; }
        public static string VarGroupsNode { get; private set; }
        public static string UnnamedGroup { get; private set; }
        public static string UnnamedVariable { get; private set; }

        public static void Init()
        {
            LocaleDict dict = Locale.GetDictionary("Scada.Comm.Drivers.DrvDS18B20.View.DsConfigProvider"); //"Scada.Comm.Drivers.DrvSnmp.View.SnmpConfigProvider"
            FormTitle = dict[nameof(FormTitle)];
            AddVarGroupButton = dict[nameof(AddVarGroupButton)];
            AddVariableButton = dict[nameof(AddVariableButton)];
            OptionsNode = dict[nameof(OptionsNode)];
            VarGroupsNode = dict[nameof(VarGroupsNode)];
            UnnamedGroup = dict[nameof(UnnamedGroup)];
            UnnamedVariable = dict[nameof(UnnamedVariable)];
        }
    }
}
