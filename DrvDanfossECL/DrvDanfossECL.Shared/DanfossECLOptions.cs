using Scada.Config;
using System.ComponentModel;


namespace Scada.Comm.Drivers.DrvDanfossECL  // Это пример из драйвера Меркурий для дополнительных параметров, по идее можно убрать в том числе и из интерфейса или 
{                                           // применить для других целей 
    internal class DanfossECLOptions
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DanfossECLOptions()
            : this(new OptionList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DanfossECLOptions(OptionList options)
        {
            UserPwd = options.GetValueAsString("UserPassword");
            AdminPwd = options.GetValueAsString("AdminPassword");
            Level = options.GetValueAsString("Level");
        }

        [Description("Пароль 1-ого уровня"), Category("Доступ")]
        public string UserPwd { get; set; }

        [Description("Пароль 2-ого уровня"), Category("Доступ")]
        public string AdminPwd { get; set; }

        [Description("Уровень доступа"), Category("Доступ")]
        public string Level { get; set; }

        /// <summary>
        /// Adds the options to the list.
        /// </summary>
        [Description("")]
        public void AddToOptionList(OptionList options)
        {
            options.Clear();
            options["UserPassword"] = UserPwd;
            options["AdminPassword"] = AdminPwd;
            options["Level"] = Level;
        }
    }
}
