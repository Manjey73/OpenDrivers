using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scada.Comm.Channels;
using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Lang;
using Scada.Data.Models;
using Scada.Lang;
using ScadaCommFunc;
using Scada.Data.Const;
using System.Diagnostics;

namespace Scada.Comm.Drivers.DrvDanfossECL.Logic
{
    internal class DevDanfossECLLogic : DeviceLogic
    {
        private readonly DanfossECLOptions options;          // the drv options
        private DevTemplate devTemplate = new DevTemplate();
        private string fileName = "";
        private string filePath = "";
        private bool fileyes = false;   // При отсутствии файла не выполнять опрос
        private int inBufLen = 5;       // Тут пока 5 байт на ответ
        private readonly byte[] inBuf;  // the input buffer
        private int read_cnt;
        private const float No_sensor = 192.0F;
        private const float Sensor_Shortcircuit = -64.0F;
        private class ActiveSignal // Класс для списка активных команд
        {
            public DevTemplate.CmdGroup actComm { get; set; }
            public byte[] req { get; set; } // массив для сохранения запроса после подготовки CRC
        }

        private Dictionary<string, ActiveSignal> activReq = new Dictionary<string, ActiveSignal>(); // Словарь активных запросов, где в качестве ключа используется Код активного Тега


        public override void InitDeviceTags()
        {
            if (activReq.Count > 0)
            {
                TagGroup tagGroup = new TagGroup(devTemplate.Name); // Создание меню Тегов последовательно по списку Сигналов

                foreach (var listReq in activReq)
                {
                    DeviceTag deviceTag = tagGroup.AddTag(listReq.Key, listReq.Value.actComm.Name); // Ключ словаря является кодом тега, Значение = Имя тега

                    // Тут можем изменять Format тега для отображения в логе, число, дата, строка и так далее, пример ниже для строки
                    //if (listReq.Value.actComm.Format == "string") // Нет такого слова
                    //{
                    //    deviceTag.Format = TagFormat.String;
                    //}
                }

                DeviceTags.AddGroup(tagGroup); // Инициализация всех тегов // Теперь не нужно так как на автомате?
            }
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
                    fileyes = true;
                }
                catch (Exception err)
                {
                    Log.WriteLine(string.Format(Locale.IsRussian ?
                    "Ошибка: " + err.Message :
                    "Error: " + err.Message, Title));
                }
            }

            if (devTemplate != null)
            {
                if (devTemplate.CmdGroups.Count > 0)
                {
                    foreach (var act in devTemplate.CmdGroups)
                    {
                        if (act.Active) // Если тег активен, заносим его в словарь, подготовив данные для запроса
                        {
                            ActiveSignal actSig = new ActiveSignal();
                            actSig.actComm = act; // Целиком весь набор CmdGroup класса DevTemplate кидаем в activeCommand (может и не требуется и можно ограничиться байтовым буфером)


                            bool read = ConvFunc.IsHex(actSig.actComm.Read);
                            bool write = ConvFunc.IsHex(actSig.actComm.Write);
                            bool par = ConvFunc.IsHex(actSig.actComm.Parameter);

                            bool isok = read & write & par;

                            if (isok)
                            {
                                byte[] req = new byte[5];

                                req[0] = ConvFunc.StringToHex(actSig.actComm.Read)[0];    // Байт чтения, проверки на количество символов в строке нет, подразумевается два символа, так как параметр однобайтовый
                                req[1] = ConvFunc.StringToHex(actSig.actComm.Parameter)[0];     // Аналогично, тут байт параметра для чтения

                                req[4] = xorCS(req);

                                actSig.req = req; // Заносим буфер с расчитанной CS в наш класс для сохранения в словаре

                                if (!activReq.ContainsKey(act.Code))
                                {
                                    activReq.Add(act.Code, actSig);
                                }
                            }
                            else
                            {
                                Log.WriteLine($"Один из параметров не является hex числом {actSig.actComm.Name}"); // Выводим в лог информацию об имени, если у нас есть ошибка в HEX значении и не добавляем его в словарь
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// расчет контрольной суммы по XOR
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        private byte xorCS(byte[] buf) //  не уверен что правильно, требуется проверка. 
        {
            int xorcs = buf[0];
            for (int i = 1; i < buf.Length - 1; i++)
            {
                xorcs = xorcs ^ buf[i];
            }
            return (byte)xorcs;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevDanfossECLLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            options = new DanfossECLOptions(deviceConfig.PollingOptions.CustomOptions);
            //UserPwd = string.IsNullOrEmpty(options.UserPwd) ? "111111" : options.UserPwd;
            //AdminPwd = string.IsNullOrEmpty(options.AdminPwd) ? "222222" : options.AdminPwd;
            //Level = string.IsNullOrEmpty(options.Level) ? "1" : options.Level;
            //inBuf = new byte[20];
            //inBufLen = 0;
            inBuf = new byte[inBufLen];


            CanSendCommands = true;
            ConnectionRequired = true;
        }


        /// <summary>
        /// Performs actions after setting the connection.
        /// </summary>
        public override void OnConnectionSet()
        {
            //if (Connection != null)
            //    Connection.NewLine = "\x0D";
        }

        /// <summary>
        /// Performs a communication session.
        /// </summary>
        public override void Session()
        {
            base.Session();
            Stopwatch stopwatch = Stopwatch.StartNew();

            LastRequestOK = true;

            if (!fileyes)        // Если конфигурация не была загружена, выставляем все теги в невалидное состояние и выходим         
            {
                DeviceData.Invalidate();
                return;
            }

            foreach (var request in activReq)
            {
                Request(request.Value.req); // Проходим по списку активных запросов и отправляем готовые массивы запросов


                if (LastRequestOK)
                {
                    // Тут вытаскивать из буфера значения с проверкой типа данных и так далее
                    double rezult = 0;

                    if (request.Value.actComm.Format.ToLower() == "temp") // TEST List<string>
                    {
                        byte[] resultreversed = { inBuf[3], inBuf[2] }; // Сразу берем из буфера
                        short temp16bit = BitConverter.ToInt16(resultreversed, 0);
                        rezult = (float)temp16bit/128; // Температуру сразу делим на 128, для float оставляем умножение или деление

                        if (rezult == No_sensor) // Выводим в События если нет сенсора
                        {
                            DeviceEvent sensor = new DeviceEvent();
                            sensor.Descr = "No sensor present (n/a)!";
                            sensor.Text = "No sensor present (n/a)!";
                            sensor.Ack = false;
                            sensor.TextFormat = EventTextFormat.CustomText;
                            sensor.Timestamp = DateTime.UtcNow;
                            sensor.CnlStat = 23;
                            sensor.CnlVal = rezult;
                            rezult = double.NaN;
                        }
                        else if (rezult == Sensor_Shortcircuit) // Выводим в События если сенсор в коротком замыкании
                        {
                            DeviceEvent sensor = new DeviceEvent();
                            sensor.Descr = "Short circuit in sensor!";
                            sensor.Text = "Short circuit in sensor!";
                            sensor.Ack = false;
                            sensor.TextFormat = EventTextFormat.CustomText;
                            sensor.Timestamp = DateTime.UtcNow;
                            sensor.CnlStat = 23;
                            sensor.CnlVal = rezult;
                            rezult = double.NaN;
                        }
                    }
                    else if (request.Value.actComm.Format.ToLower() == "byte")
                    {
                        rezult = inBuf[2];
                    }
                    else if (request.Value.actComm.Format.ToLower() == "sbyte")
                    {
                        rezult = (sbyte)inBuf[2]; // Фактически это SignedByte по протоколу.
                    }
                    else if(request.Value.actComm.Format.ToLower() == "int16" || request.Value.actComm.Format.ToLower() == "float")
                    {
                        byte[] resultreversed = { inBuf[3], inBuf[2] }; // Сразу берем из буфера
                        short temp16bit = BitConverter.ToInt16(resultreversed, 0);
                        rezult = temp16bit; // Тут применяем требуемое умножение или деление либо Mul = 0.1 либо Div = 10 и так далее
                        // Можно в том числе получить и параметры температуры, установив Div = 128 или Mul = 0.0078125 без появления Событий (см. выше параметр temp)
                    }

                    if (request.Value.actComm.Mul != "" || request.Value.actComm.Div != "") // Если заполнено одно из полей Mul - умножение или Div - деление
                    {                                                                       // проверяем и выполняем действие, заолнять обязательно только одно из полей.
                        if (request.Value.actComm.Mul != "")                                // хотя по коду можно выполнить сперва умножение, а потом и деление :)
                            rezult = rezult * SToDouble(request.Value.actComm.Mul); // Применяем умножение
                        if (request.Value.actComm.Div != "")
                            rezult = rezult / SToDouble(request.Value.actComm.Div); // Применяем деление
                    }
                    //else
                    //{
                    //    rezult = rezult * 1;  // это по идее можно вообще убрать.
                    //}
                    DeviceData.Set(request.Key, rezult); // Пока значение в теге просто какой-то rezult :)
                }
                else
                {
                    DeviceData.Invalidate(request.Key);
                }
            }


            stopwatch.Stop();
            Log.WriteLine(Locale.IsRussian ?
                "Получено за {0} мс" :
                "Received in {0} ms", stopwatch.ElapsedMilliseconds);

            FinishSession();
        }

        private void Request(byte[] request)
        {
            int tryNum = 0;
            LastRequestOK = false;
            while (RequestNeeded(ref tryNum)) // Использует количество попыток запроса при ошибке из настроек Опроса Устройства
            {
                string logText;
                WriteRequest(request, out logText);
                Log.WriteLine(logText);

                ReadAnswer(inBuf, 5, out logText);
                Log.WriteLine(logText);

                checkCS();

                FinishRequest();
                //Thread.Sleep(PollingOptions.Delay); // Спим указанную задержку, в настройках задаем 400 мс
                tryNum++;
            }
        }

        /// <summary>
        /// Считать ответ из последовательного порта
        /// </summary>
        private void ReadAnswer(byte[] buffer, int count, out string logText)
        {
            read_cnt = Connection.Read(buffer, 0, count, PollingOptions.Timeout, ProtocolFormat.Hex, out logText);
        }

        private void WriteRequest(byte[] request, out string logText)
        {
            Connection.Write(request, 0, request.Length, ProtocolFormat.Hex, out logText);
        }

        private void checkCS()
        {
            //string logText = null;
            if (inBuf[4] == xorCS(inBuf))
            {
                LastRequestOK = true;
                Log.WriteLine(CommPhrases.ResponseOK);
            }
            else
            {
                LastRequestOK = false;
                Log.WriteLine(CommPhrases.ResponseCsError);
            }
        }


        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public override void SendCommand(TeleCommand cmd) // Команды никак не разрабатывались, нужен прибор и тестировать те или иные варианты, на данный момент это просто набросок
        {
            base.SendCommand(cmd);

            string cmdCode = cmd.CmdCode;

            if (activReq.ContainsKey(cmdCode)) // Собственно проверяем есть ли  у нас переменная с таким кодом
            {
                if (!double.IsNaN(cmd.CmdVal))
                {
                    // Тут проверяем по типу отправки значения
                    // И делаем тут логику записи значений

                    string cmdFormat = activReq[cmdCode].actComm.Format; // TEST List<string>

                    if (cmdFormat == "float")
                    {

                    }
                }
                else if (cmd.CmdData != null && cmd.CmdData.Length != 0)
                {
                    // Тут проверяем если у нас отправляется строка
                    // аналогично, разбирая строку или HEX строку если в таком виде делаем команду
                }
            }
            FinishCommand();
        }

        private double SToDouble(string s) // Позволяет в строке вводить дробные числа с ,(запятой) или .(точкой)
        {
            double result = 1;
            if (!double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out result))
            {
                if (!double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out result))
                {
                    return 1;
                }
            }
            return result;
        }
    }
}
