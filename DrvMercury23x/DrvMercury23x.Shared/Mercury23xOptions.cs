using System;
using System.Collections.Generic;
using System.Text;
using Scada.Config;
using System.ComponentModel;
using Scada.Lang;

namespace Scada.Comm.Drivers.DrvMercury23x
{
    internal class Mercury23xOptions
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public Mercury23xOptions()
            : this(new OptionList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public Mercury23xOptions(OptionList options)
        {
            UserPwd = options.GetValueAsString("UserPassword");
            AdminPwd = options.GetValueAsString("AdminPassword");
            Level = options.GetValueAsString("Level");
        }

        //var category = Locale.IsRussian ? "Доступ" : "Access";

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
