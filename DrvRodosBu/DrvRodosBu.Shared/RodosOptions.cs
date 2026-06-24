using Scada.Config;
using System.ComponentModel;

namespace Scada.Comm.Drivers.DrvRodosBu
{
    internal class RodosOptions
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public RodosOptions()
            : this(new OptionList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public RodosOptions(OptionList options)
        {
            Login = options.GetValueAsString("Login");
            Password = options.GetValueAsString("Password");

            Full_Log = options.GetValueAsString("Full_Log"); 

            try
            {
                OriginalPassword = ScadaUtils.Decrypt(Password);
            }
            catch { }
        }

        [Description("Введите Логин"), Category("Authorization")]
        public string Login { get; set; }

        [Description("Введите пароль"), Category("Authorization")]
        public string Password { get; set; }

        [Description("Выводить полный лог ответа - true/fasle or empty"), Category("Settings")]
        public string Full_Log { get; set; }

        private string OriginalPassword;

        /// <summary>
        /// Adds the options to the list.
        /// </summary>
        [Description("")]
        public void AddToOptionList(OptionList options)
        {
            options.Clear();
            options["Login"] = Login;
            if (ScadaUtils.Encrypt(OriginalPassword) != Password)
            {
                options["Password"] = ScadaUtils.Encrypt(Password); // TEST
            }
            else
            {
                options["Password"] = ScadaUtils.Encrypt(OriginalPassword); // TEST
            }
            options["Full_Log"] = Full_Log; 
        }
    }
}
