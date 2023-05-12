using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Data.Models;
using Scada.Lang;
using ScadaCommFunc;
using System.Device.Gpio;
using System.Diagnostics;

namespace Scada.Comm.Drivers.DrvGpiod.Logic
{
    internal class DevGpiodLogic : DeviceLogic
    {
        private GpioController controller = new GpioController();
        private Dictionary<int, pinA> AvailablePin = new Dictionary<int, pinA>();
        private DevTemplate devTemplate = new DevTemplate();
        private string fileName = "";
        private string filePath = "";
        private bool fileyes = false;   // При отсутствии файла не выполнять опрос

        private class pinA
        {
            public string code { get; set; }
            public string name { get; set; }
            public string mode { get; set; }
            public string stval { get; set; }
        }

        static DevGpiodLogic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevGpiodLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            CanSendCommands = true;
            ConnectionRequired = false;
        }

        public override void InitDeviceTags()
        {
            TagGroup tagGroup = new TagGroup("Pin");

            DeviceTag deviceTag;
            foreach (var APin in AvailablePin)
            {
                deviceTag = tagGroup.AddTag(APin.Value.code, APin.Value.name);
            }

            DeviceTags.AddGroup(tagGroup);
        }

        /// <summary>
        /// Выполнить действия при запуске линии связи
        /// </summary>
        public override void OnCommLineStart()
        {
            devTemplate = null;

            fileName = DeviceConfig.PollingOptions.CmdLine == null ? "" : DeviceConfig.PollingOptions.CmdLine.Trim();
            filePath = AppDirs.ConfigDir + fileName;

            if (fileName == "") // Чтение файла шаблона
            {
                Log.WriteLine(string.Format(Locale.IsRussian ?
                    "Ошибка: Не задан шаблон устройства для {0}" :
                    "Error: Template is undefined for the {0}", Title));
            } // Чтение файла шаблона
            else
            {
                try
                {
                    devTemplate = FileFunc.LoadXml(typeof(DevTemplate), filePath) as DevTemplate;
                    // Добавить сюда создание и открытие pin и их конфигурирование

                    foreach (var APin in devTemplate.Gpios)
                    {

                        if (APin.Active)
                        {
                            int inpin;
                            bool pinin = int.TryParse(APin.Pin, out inpin);
                            pinA Apin = new pinA
                            {
                                code = APin.Code,
                                name = APin.Name,
                                mode = APin.PinMode,
                                stval = APin.startValue
                            };

                            if (controller.IsPinOpen(inpin))
                            {
                                if (!AvailablePin.ContainsKey(inpin))
                                {
                                    AvailablePin.Add(inpin, Apin);
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(Apin.mode))
                                    {
                                        controller.OpenPin(inpin);
                                    }
                                    else if (!string.IsNullOrEmpty(Apin.mode) && string.IsNullOrEmpty(Apin.stval))
                                    {
                                        if (Apin.mode == "Input")
                                        {
                                            controller.OpenPin(inpin, PinMode.Input);
                                        }
                                        else if (Apin.mode == "Output")
                                        {
                                            controller.OpenPin(inpin, PinMode.Output);
                                        }
                                        else if (Apin.mode == "InputPullUp")
                                        {
                                            controller.OpenPin(inpin, PinMode.InputPullUp);
                                        }
                                        else if (Apin.mode == "InputPullDown")
                                        {
                                            controller.OpenPin(inpin, PinMode.InputPullDown);
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(Apin.mode) && !string.IsNullOrEmpty(Apin.stval))
                                    {
                                        PinValue pValue = new PinValue();
                                        if (Apin.stval == "Low") pValue = PinValue.Low;
                                        else if (Apin.stval == "High") pValue = PinValue.High;

                                        if (Apin.mode == "Input")
                                        {
                                            controller.OpenPin(inpin, PinMode.Input, pValue);
                                        }
                                        else if (Apin.mode == "Output")
                                        {
                                            controller.OpenPin(inpin, PinMode.Output, pValue);
                                        }
                                        else if (Apin.mode == "InputPullUp")
                                        {
                                            controller.OpenPin(inpin, PinMode.InputPullUp, pValue);
                                        }
                                        else if (Apin.mode == "InputPullDown")
                                        {
                                            controller.OpenPin(inpin, PinMode.InputPullDown, pValue);
                                        }
                                    }

                                    if (!AvailablePin.ContainsKey(inpin))
                                    {
                                        AvailablePin.Add(inpin, Apin);
                                    }

                                }
                                catch (IOException e)
                                {
                                    Log.WriteLine($"Error pin {inpin}: {e.Message}");
                                }
                            }
                            fileyes = true; // Так же есть минимум один активный запрос
                        }
                    }
                }
                catch (Exception err)
                {
                    Log.WriteLine(string.Format(Locale.IsRussian ?
                    "Ошибка: " + err.Message :
                    "Error: " + err.Message, Title));
                }
            }
        }

        /// <summary>
        /// Performs actions after setting the connection.
        /// </summary>
        public override void OnConnectionSet()
        {
        }


        /// <summary>
        /// Performs a communication session.
        /// </summary>
        public override void Session()
        {
            base.Session();

            // Проверка наличия файла конфигурации
            if (!fileyes) //  Если конфигурация не была загружена, выставляем все теги в невалидное состояние и выходим     || !req    
            {
                DeviceData.Invalidate();
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (var pins in AvailablePin)
            {
                DeviceData.Set($"{pins.Value.code}", (double)controller.Read(pins.Key));
            }

            stopwatch.Stop();
            Log.WriteLine(Locale.IsRussian ?
                "Получено за {0} мс" :
                "Received in {0} ms", stopwatch.ElapsedMilliseconds);
            FinishSession();
        }

        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public override void SendCommand(TeleCommand cmd)
        {
            base.SendCommand(cmd);
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (!double.IsNaN(cmd.CmdVal) && cmd.CmdCode != "")
            {
                int key = AvailablePin.FirstOrDefault(x => x.Value.code == cmd.CmdCode).Key;

                if (AvailablePin.ContainsKey(key))
                {
                    int vl;
                    vl = cmd.CmdVal > 0 ? 1 : 0;

                    controller.Write(key, vl);
                }
            }

            stopwatch.Stop();
            Log.WriteLine(Locale.IsRussian ?
                "Получено за {0} мс" :
                "Received in {0} ms", stopwatch.ElapsedMilliseconds);
            FinishCommand();
        }

        /// <summary>
        /// Performs actions when terminating a communication line.
        /// </summary>
        public override void OnCommLineTerminate()
        {
        }
    }
}
