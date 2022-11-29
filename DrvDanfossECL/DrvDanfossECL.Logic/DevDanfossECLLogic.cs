using Scada.Comm.Channels;
using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Lang;
using Scada.Data.Entities;
using Scada.Data.Models;
using Scada.Lang;
using ScadaCommFunc;
using System;
using System.Diagnostics;

namespace Scada.Comm.Drivers.DrvDanfossECL.Logic
{
    internal class DevDanfossECLLogic : DeviceLogic
    {
        private DevTemplate devTemplate = new DevTemplate();
        private string fileName = "";
        private string filePath = "";
        private bool fileyes = false;   // При отсутствии файла не выполнять опрос
        private byte[] inBuf; // = new byte[5];  // the input buffer
        private int read_cnt;
        private const float No_sensor = 192.0F;
        private const float Sensor_Shortcircuit = -64.0F;
        private ushort uAddr = 0x0000; // Адрес параметра
        private ushort addrUshort = 0x0000; // Создание адреса для составления команды

        private bool high; // Переменная, указывающая какой байт адреса используется true - Address(high), false - Address(low)
        private bool writeEvenRam;
        private bool writeOnlyRam;
        private List<KeyValuePair<string, ActiveSignal>> listActive;

        private class ActiveSignal // Класс для списка активных команд
        {
            public DevTemplate.Parameters actComm { get; set; }
            public byte[] req { get; set; } // массив для сохранения запроса после подготовки CRC
            public ushort Address { get; set; } // Сохранить Адресс параметра в uint чтобы дальше не выполнять проверок
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
                bool chk = string.IsNullOrEmpty(devTemplate.WriteEvenRAM) ? writeEvenRam = true : bool.TryParse(devTemplate.WriteEvenRAM, out writeEvenRam);
                chk = string.IsNullOrEmpty(devTemplate.WriteOnlyRAM) ? writeOnlyRam = false : bool.TryParse(devTemplate.WriteOnlyRAM, out writeOnlyRam);

                //Log.WriteLine($"WriteEvenRAM = {writeEvenRam} and {devTemplate.WriteEvenRAM}"); // TEST

                if (devTemplate.Parameter.Count > 0)
                {
                    foreach (var act in devTemplate.Parameter)
                    {
                        if (act.Active) // Если тег активен, заносим его в словарь, подготовив данные для запроса
                        {
                            bool isok = false;

                            ActiveSignal actSig = new ActiveSignal();
                            actSig.actComm = act; // Целиком весь набор Parameter класса DevTemplate кидаем в activeCommand (может и не требуется и можно ограничиться байтовым буфером)

                            // Проверить длину Адреса: = 3, то добавить впереди 0 и проверить на HEX
                            string addr = actSig.actComm.Address;
                            if (actSig.actComm.Address.Length == 3)
                            {
                                addr = $"0{actSig.actComm.Address}";
                                bool par = ConvFunc.IsHex(addr);

                                if (par)
                                {
                                    byte[] bAddr = ScadaUtils.HexToBytes(addr);
                                    Array.Reverse(bAddr);
                                    uAddr = BitConverter.ToUInt16(bAddr, 0);

                                    // Проверка адреса на запись, независимо от выставленного параметра Write
                                    // 16-bit value read-only (E30 - E41, E46 - E4D)
                                    if (uAddr >= 0x0E30 && uAddr <= 0x0E4D)
                                    {
                                        actSig.actComm.Write = false;
                                    }

                                    // Тут определить требуемую команду по адресу 0xCх и создать адрес включая команду
                                    addrUshort = (ushort)(uAddr | 0xC000);
                                    isok = true;
                                }
                            }
                            else if (actSig.actComm.Address.Length == 2) // Если два символа, то это адрес EEPROM - чтение командой Read EEPROM (16bit) = 80
                            {
                                addr = $"00{actSig.actComm.Address}";
                                bool par = ConvFunc.IsHex(addr);

                                if (par)
                                {
                                    byte[] bAddr = ScadaUtils.HexToBytes(addr);
                                    Array.Reverse(bAddr);
                                    uAddr = BitConverter.ToUInt16(bAddr, 0);

                                    // Тут определить требуемую команду по адресу 0xCх и создать адрес включая команду
                                    addrUshort = (ushort)(uAddr | 0x8000);
                                    isok = true;
                                }
                            }

                            if (isok)
                            {
                                byte[] req = new byte[5];

                                req[0] = (byte)(addrUshort / 256);    // Байт Команды с частью адреса
                                req[1] = (byte)(addrUshort % 256);    // Младший айт Адреса

                                req[4] = xorCS(req);

                                actSig.req = req; // Заносим буфер с расчитанной CS в наш класс для сохранения в словаре

                                actSig.Address = uAddr;

                                if (!activReq.ContainsKey(act.Code))
                                {
                                    activReq.Add(act.Code, actSig);
                                }
                            }
                            else
                            {
                                Log.WriteLine($"Параметр не является hex числом {actSig.actComm.Name}"); // Выводим в лог информацию об имени, если у нас есть ошибка в HEX значении и не добавляем его в словарь
                            }
                        }
                    }

                    // Кинуть в List и перебирать уже его ???
                    listActive = activReq.ToList();
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
            CanSendCommands = true;
            ConnectionRequired = true;
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
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (!fileyes)        // Если конфигурация не была загружена, выставляем все теги в невалидное состояние и выходим         
            {
                DeviceData.Invalidate();
                return;
            }

            for(int i = 0; i < listActive.Count; i++)
            {
                bool readLow = false;

                // Если запрос больше первого, Адрес предыдущего меньше на 1, предыдущий адрес четный и оба параметра sbyte
                if (i > 0 && listActive[i - 1].Value.Address == listActive[i].Value.Address - 1 &&
                    listActive[i - 1].Value.Address % 2 == 0 && listActive[i - 1].Value.actComm.Format == "sbyte" && 
                    listActive[i].Value.actComm.Format == "sbyte" )
                {
                    readLow = true; // Разрешаем чтение младшего байта не выполняя сам запрос
                }
                else
                {
                    // Если нет, то выполняем новый запрос
                    Request(listActive[i].Value.req);
                }

                if (LastRequestOK)
                {
                    // Тут вытаскивать из буфера значения с проверкой типа данных и так далее
                    double rezult = 0;

                    if (listActive[i].Value.actComm.Format.ToLower() == "temp") // TEST List<string>
                    {
                        byte[] resultreversed = { inBuf[3], inBuf[2] }; // Сразу берем из буфера
                        short temp16bit = BitConverter.ToInt16(resultreversed, 0);
                        rezult = (float)temp16bit / 128; // Температуру сразу делим на 128, для float оставляем умножение или деление

                        if (rezult == No_sensor || rezult == Sensor_Shortcircuit) // Выводим --- если нет сенсора
                        {
                            rezult = double.NaN;
                        }
                    }
                    else if (listActive[i].Value.actComm.Format.ToLower() == "sbyte")
                    {
                        if (!readLow)
                            rezult = (sbyte)inBuf[2]; // Фактически это SignedByte по протоколу.
                        else
                            rezult = (sbyte)inBuf[3];
                    }
                    else if (listActive[i].Value.actComm.Format.ToLower() == "int16")
                    {
                        byte[] resultreversed = { inBuf[3], inBuf[2] }; // Сразу берем из буфера
                        short temp16bit = BitConverter.ToInt16(resultreversed, 0);
                        rezult = temp16bit; // Тут применяем требуемое умножение Multiplier например для деления на 10 = 0.1
                        // Можно в том числе получить и параметры температуры, Multiplier = 0.0078125 без появления Событий (см. выше параметр temp)
                    }

                    // Если заполнено поле Multiplier, выполняем действие над результатом
                    if (!string.IsNullOrEmpty(listActive[i].Value.actComm.Multiplier)) rezult = rezult * SToDouble(listActive[i].Value.actComm.Multiplier);

                    DeviceData.Set(listActive[i].Key, rezult);
                }
                else
                {
                    DeviceData.Invalidate(listActive[i].Key);
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

                inBuf = new byte[5]; // TEST
                ReadAnswer(inBuf, 5, out logText);
                Log.WriteLine(logText);

                checkRead();

                FinishRequest();
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

        private void checkRead()
        {
            if (read_cnt == 5)
            {
                if (inBuf[4] == xorCS(inBuf))
                {
                    if (inBuf[0] == 0x02 || inBuf[0] == 0x01)
                    {
                        LastRequestOK = true;
                        Log.WriteLine(CommPhrases.ResponseOK);
                    }
                    else
                    {
                        Log.WriteLine($"Error: {ErrorCode(inBuf[0])}");
                        //Thread.Sleep(PollingOptions.Delay);
                    }
                }
                else
                {
                    Log.WriteLine(CommPhrases.ResponseCsError);
                    //Thread.Sleep(PollingOptions.Delay);
                }
            }
            else
            {
                Log.WriteLine(CommPhrases.ResponseError);
                //Thread.Sleep(PollingOptions.Delay);
            }
        }


        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public override void SendCommand(TeleCommand cmd) // Команды никак не разрабатывались, нужен прибор и тестировать те или иные варианты, на данный момент это просто набросок
        {
            base.SendCommand(cmd);
            string cmdCode = cmd.CmdCode;

            if (!string.IsNullOrEmpty(activReq[cmd.CmdCode].actComm.min_val) && !string.IsNullOrEmpty(activReq[cmd.CmdCode].actComm.max_val))
            {
                // Проверка на минимальные и максимальные значения
                double minVal = SToDouble(activReq[cmd.CmdCode].actComm.min_val);
                double maxVal = SToDouble(activReq[cmd.CmdCode].actComm.max_val);

                if (cmd.CmdVal < minVal || cmd.CmdVal > maxVal)
                {
                    string error = Locale.IsRussian ?
                        $"Ошибка: Значение команды находится вне диапазона ({(int)minVal}...{(int)maxVal})" :
                        $"Error: The value of the command is out of range ({(int)minVal}...{(int)maxVal})";

                    DeviceEvent MinMax = new DeviceEvent();
                    MinMax.DeviceTag = DeviceTags[cmd.CmdCode];
                    MinMax.CnlVal = cmd.CmdVal;
                    MinMax.Descr = "Значение команды вне диапазона";
                    MinMax.Text = $"{error}";
                    MinMax.Ack = false;
                    MinMax.TextFormat = EventTextFormat.CustomText;
                    MinMax.Timestamp = DateTime.UtcNow;
                    MinMax.CnlStat = 23;
                    MinMax.CnlVal = cmd.CmdVal;

                    DeviceData.EnqueueEvent(MinMax);

                    Log.WriteLine(error);
                    LastRequestOK = false;
                    FinishCommand();
                    return;
                }
            }

            if (activReq.ContainsKey(cmdCode) && activReq[cmdCode].actComm.Write) // Собственно проверяем есть ли  у нас переменная с таким кодом и позволяет ли она запись
            {
                if (!double.IsNaN(cmd.CmdVal))
                {
                    string cmdFormat = activReq[cmdCode].actComm.Format;
                    uAddr = activReq[cmdCode].Address; // Чтение адреса параметра

                    // Определить EEPROM адрес и high он или low
                    ushort eepromAddr = GetEEPROMadr(uAddr, out high);

                    if (cmdFormat == "int16")
                    {
                        // Проверка адреса на запись 16-bit value
                        // Тут определить требуемую команду записи для 16 бит переменных 0xDх и создать адрес включая команду
                        if (uAddr > 0x00FF) // Предположительно RAM адреса, доступные для записи, если в пределах 0x00FF то соответственно ROM адреса // 0x0E72 && uAddr <= 0x0E78
                        {
                            addrUshort = (ushort)(uAddr | 0xD000);
                            WriteInt(cmd.CmdVal);

                            if (!writeOnlyRam)
                            {
                                addrUshort = (ushort)(eepromAddr | 0x9000); // Запись Int в EEPROM
                                WriteInt(cmd.CmdVal);
                            }
                        }
                        else if (uAddr <= 0x00FF) // Если адрес ниже, то это ROM
                        {
                            addrUshort = (ushort)(uAddr | 0x9000);
                            WriteInt(cmd.CmdVal);
                        }
                    }
                    else if (cmdFormat == "sbyte") 
                    {
                        if (high)
                        {
                            addrUshort = (ushort)(uAddr | 0xF000); // Запись в RAM старшего байта
                            WriteByte(cmd.CmdVal);

                            if (!writeOnlyRam)
                            {
                                addrUshort = (ushort)(eepromAddr | 0xB000); // Запись в EEPROM старшего байта
                                WriteByte(cmd.CmdVal);
                            }
                        }
                        else
                        {
                            addrUshort = (ushort)((uAddr | 0xE000) - Convert.ToInt32(writeEvenRam)); // Запись в RAM младшего байта 0x01
                            WriteByte(cmd.CmdVal);

                            if (!writeOnlyRam)
                            {
                                addrUshort = (ushort)(eepromAddr | 0xA000); // Запись в EEPROM младшего байта
                                WriteByte(cmd.CmdVal);
                            }
                        }
                    }
                }
                else if (cmd.CmdData != null && cmd.CmdData.Length != 0)
                {
                    // Тут проверяем если у нас отправляется строка
                    // аналогично, разбирая строку или HEX строку если в таком виде делаем команду
                }
            }
            else
            {
                LastRequestOK = false;
                string error = Locale.IsRussian ?
                    "Ошибка: Переменная только для чтения или не существует в списке активных" :
                    "Error: The variable Read-Only or does not exist in the active list";

                DeviceEvent ReadOnly = new DeviceEvent();
                ReadOnly.DeviceTag = DeviceTags[cmd.CmdCode];
                ReadOnly.CnlVal = cmd.CmdVal;
                ReadOnly.Descr = "The Read-Only variable";
                ReadOnly.Text = $"{error} {cmd.CmdCode}";
                ReadOnly.Ack = false;
                ReadOnly.TextFormat = EventTextFormat.CustomText;
                ReadOnly.Timestamp = DateTime.UtcNow;
                ReadOnly.CnlStat = 23;
                ReadOnly.CnlVal = cmd.CmdVal;

                DeviceData.EnqueueEvent(ReadOnly);
                Log.WriteLine(error);
            }
            FinishCommand();
        }

        private void WriteInt(double CmdVal)
        {
            byte[] req = new byte[5];

            req[0] = (byte)(addrUshort / 256);    // Байт Команды с частью адреса
            req[1] = (byte)(addrUshort % 256);    // Младший айт Адреса

            short cmdVal = Convert.ToInt16(CmdVal);
            byte[] bCmdVal = BitConverter.GetBytes(cmdVal);
            req[2] = bCmdVal[1];
            req[3] = bCmdVal[0];
            req[4] = xorCS(req);

            Request(req);

            //Log.WriteLine($"Write  {ScadaUtils.BytesToHex(req)} Addr {addrUshort} Значение {cmdVal}");
        }

        private void WriteByte(double CmdVal)
        {
            byte[] req = new byte[5];

            req[0] = (byte)(addrUshort / 256);    // Байт Команды с частью адреса
            req[1] = (byte)(addrUshort % 256);    // Младший айт Адреса

            sbyte cmdVal = Convert.ToSByte(CmdVal);

            if (high)
            {
                req[2] = (byte)cmdVal;
                req[3] = 0x00;
            }
            else
            {
                req[2] = 0x00;
                req[3] = (byte)cmdVal;
            }

            req[4] = xorCS(req);
            Request(req);
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

        /// <summary>
        /// Получение адреса EEPROM параметра
        /// </summary>
        /// <param name="ramadr"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        private byte GetEEPROMadr(ushort ramadr, out bool high)
        {
            byte eadr;
            if (ramadr % 2 == 0)
            {
                // четное число? определяем high 
                high = true;
                eadr = (byte)((ramadr / 2) - 0x707);
            }
            else
            {
                // нечетное число? определяем low 
                high = false;
                eadr = (byte)(((ramadr - 1) / 2) - 0x707);
            }
            return eadr;
        }

        public string ErrorCode(byte errCode)
        {
            string errorCode = "";
            switch (errCode)
            {
                case 0x00: errorCode = "Unknown value"; break;
                case 0x02: errorCode = "Answer from a request"; break;
                case 0x10: errorCode = "Invalid value for date or clock"; break;
                case 0x20: errorCode = "Non existing circuit is chosen"; break;
                case 0x30: errorCode = "Non existing Mode is chosen"; break;
                case 0x40: errorCode = "Non existing Port is chosen"; break;
                case 0xF0: errorCode = "Communication error"; break;
            }
            return errorCode;
        }

    }
}
