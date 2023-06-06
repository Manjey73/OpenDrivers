using Scada.Comm.Channels;
using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Lang;
using Scada.Data.Models;
using Scada.Lang;
using ScadaCommFunc;
using System.Diagnostics;
using System.Globalization;

namespace Scada.Comm.Drivers.DrvPulsar.Logic
{
    internal class DevPulsarLogic : DeviceLogic
    {
        private DevTemplate devTemplate = new DevTemplate();
        private string fileName = "";
        private string filePath = "";
        private bool fileyes = false;   // При отсутствии файла не выполнять опрос
        private List<ActiveCnlList> ActiveCnl = new List<ActiveCnlList>();          // Создание списка Активных сигналов, где ActiveCnl.Cnl - номер сигнала, ActiveCnl.Name - Имя сигнала, 
                                                                                    // ActiveCnl.Fotmat - Тип активной переменной, ActiveCnl.IdxTag индекс сигнала в KPTags, ActiveCnl.IdxValue - Индекс группы,
                                                                                    //  в которую входит сигнал, ActiveCnl.MenuName - Имя меню, которому принадлежит сигнал

        private Dictionary<int, DevTemplate.SndGroup> ActiveSnd = new Dictionary<int, DevTemplate.SndGroup>(); // Ключ номер запроса - занчение весь SndGroup
        private Dictionary<string, object> ActiveCmd = new Dictionary<string, object>(); // Ключ - код тега, значениее объект, либо параметры Val, либо параметры Cmd // TEST TEST TEST
        private class ActiveCnlList
        {
            public int Cnl { get; set; }                // Cnl      - Номер активного канала
            public string Code { get; set; }            // Code     - Код тега активного сигнала
            public string Name { get; set; }            // Name     - Имя активного сигнала
            public string Format { get; set; }          // Format   - Тип переменной = float, double, uint16, uint32, DateTime
            public int IdxValue { get; set; }           // IdxValue - Индекс группы, в которую входит сигнал
            public int IdxTag { get; set; }             // IdxTag   - Индекс тега в представлении KPTags для SetCurData
            public string GroupName { get; set; }        // MenuName - Имя меню, к которому принадлежит параметр
        }

        private static string Error_code(byte error)
        {
            string err = "Неизвестный тип ошибки";
            switch (error)
            {
                case 0x01: err = "Отсутствует запрашиваемый код функции"; break;
                case 0x02: err = "Ошибка в битовой маске запроса"; break;
                case 0x03: err = "Ошибочная длина запроса"; break;
                case 0x04: err = "Отсутствует параметр"; break;
                case 0x05: err = "Запись заблокирована, требуется авторизация"; break;
                case 0x06: err = "Записываемое значение (параметр) находится вне заданного диапазона"; break;
                case 0x07: err = "Отсутствует запрашиваемый тип архива"; break;
                case 0x08: err = "Превышение максимального количества архивных значений за один пакет"; break;
            }
            return err;
        }

        private static readonly NumberFormatInfo NfiDot;
        private static readonly NumberFormatInfo NfiComma;

        string logText;                         // Переменная для вывода лога в Журнал Коммуникатора
        private int readcnt = 0;                // Переменная счетчика принятых байт
        private byte[] buf_out = new byte[1];   // Инициализация буфера для отправки в прибор, начальное значение
        private byte[] buf_in = new byte[1];    // Инициализация выходного буфера
        private byte[] idBCD = new byte[4];     // Буфер для Адреса прибора
        private byte[] byteID = new byte[2];    // Буфер байт для ID запроса
        private int crc = 0;                    // Переменная для контрольной суммы
        private Random rnd = new Random();      // Случайное число для формирования ID запросов

        private string sigCode = "";
        //private int sigN = 1;
        private byte[] byteIDres = new byte[2]; // Буфер для проверки ID запроса при ответе

        private int col;                        // количество байт в переменных ответов в Текущих параметрах в зависимости от типа переменных
        private int mask_ch = 0;                // переменная для параметра MASK_CH
        private int mask_chv = 0;               // переменная для параметра MASK_CH Вес импульса (Регистратор импульсов)
        private int mask_ch_wr = 0;             // переменная для параметра MASK_CH записи данных каналов (Регистратор импульсов)
        private int mask_chv_wr = 0;            // Переменная для параметра MASK_CH записи Веса импульсов (Регистратор импульсов)
        private int res_ch = 0;                 // Количество бит в 1 в маске Текущих параметров для расчета длины ответа
        private int res_chv = 0;                // Количество бит в 1 в маске Веса импульса для расчета длины ответа
        private int maxch;                      // максимальный номер канала Текущих параметров
        private int maxchv;                     // максимальный номер канала Веса импульсов
        private byte[] maskch = new byte[4];    // Инициализация массива для маски считываемых параметров

        private byte sndcode_;
        private bool activeuse = false;         // Переменная наличия активных запросов SndActive должен быть равен true для активации запроса

        private int startCnl = 1;               // Стартовый номер сигнала для Текущих параметров F=0x01, при формировании шаблона можно изменить в параметре SndData соответствующего запроса
        private int startCnlv = 41;             // Стартовый номер сигнала для Веса импульсов F=0x07, при формировании шаблона можно изменить в параметре SndData соответствующего запроса
        private int xValCnt01 = 0;              // Сюда записать номер запроса Текущих параметров для возможности создания маски опрашиваемых параметров
        private int xValCnt07 = 0;              // Сюда записать номер запроса Веса импульсов для возможности создания маски опрашиваемых параметров


        static DevPulsarLogic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevPulsarLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            CanSendCommands = true;
        }

        public override void InitDeviceTags()
        {
            TagGroup tagGroup; // = new TagGroup();
            DeviceTag deviceTag;

            // ------------ Создание тегов на основе созданного Списка Активных переменных  ------------ 

            var categoryCounts =                                            // Считаем количество Меню и количество переменных в Меню в шаблоне
                from p in ActiveCnl
                group p by p.GroupName into g
                select new { NameMenu = g.Key, counts = g.Count() };

            int cnt = 0;
            foreach (var menu in categoryCounts)
            {
                tagGroup = new TagGroup(menu.NameMenu);                                                 // Создание меню Тегов

                var actcnl = ActiveCnl.FindAll(s => s.GroupName == menu.NameMenu).OrderBy(d => d.Cnl);   // Сортировка активных каналов по каждому меню

                foreach (var tags in actcnl)
                {
                    sigCode = ActiveCnl.Find(f => f.Code == tags.Code).Code;

                    deviceTag = tagGroup.AddTag(sigCode, ActiveCnl.Find(f => f.Code == tags.Code).Name);

                    ActiveCnl.Find(s => s.Code == sigCode).IdxTag = cnt;                                 // Заносим номер тега Коммуникатора в Список
                    cnt++;                                                                               // Увеличиваем счетчик тегов
                }
                DeviceTags.AddGroup(tagGroup);
            }

            // Определяем диапазон каналов в группах  Текущие параметры и Вес импульса

            if (xValCnt01 != 0) // Если запрос с кодом 0x01 активен, переменная xValCnt01 содержит номер запроса
            {
                int idx = devTemplate.SndGroups.FindIndex(f => f.Counter == xValCnt01);
                maxch = ActiveCnl.FindLast(d => d.IdxValue == idx).Cnl;     // Максимальный номер канала для Текущих параметров
                res_ch = BitFunc.CountBit32(mask_ch);                       // Определяем количество бит = 1 в маске текущих параметров
                string format = ActiveCnl.Find(d => d.IdxValue == idx).Format;

                if (format == "float" || format == "uint32")
                {
                    col = 4;
                }
                if (format == "double")
                {
                    col = 8;
                }
            }

            if (xValCnt07 != 0) // Если запрос с кодом 0x07 активен, переменная xValCnt07 содержит номер запроса
            {
                int idx = devTemplate.SndGroups.FindIndex(f => f.Counter == xValCnt07);
                maxchv = ActiveCnl.FindLast(d => d.IdxValue == idx).Cnl;    // Максимальный номер канала для Веса импульсов
                res_chv = BitFunc.CountBit32(mask_chv);                     // Определяем количество бит = 1 в маске Веса импульсов
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
                // Определить Номера активных запросов, посылку команд проводить согласно списку активных запросов.

                if (devTemplate.SndGroups.Count != 0) // Определить активные запросы и записать в массив номера запросов для создания тегов по номерам телеграмм
                                                      // Можно упростить до определения индекса
                {
                    for (int snd = 0; snd < devTemplate.SndGroups.Count; snd++)
                    {
                        if (devTemplate.SndGroups[snd].Active)                                                       // Если запрос активен, заносим его номер Cnt в Словарь
                        {
                            if (!ActiveSnd.ContainsKey(devTemplate.SndGroups[snd].Counter))                              // Ключ = SndCnt - Значение = весь запрос SndCnt
                            {
                                ActiveSnd.Add(devTemplate.SndGroups[snd].Counter, devTemplate.SndGroups[snd]);
                            }

                            // TEST TEST TEST Можно упростить, если в Channel записывать 1,2,3 для обоих команд - требуется для создания масок Каналов и Весов каналов
                            byte[] sndcode = ScadaUtils.HexToBytes(devTemplate.SndGroups[snd].Command, true);            // Чтение строки HEX из параметра SndCom
                            sndcode_ = sndcode[0];

                            if (sndcode_ == 0x01)                                                                        // Проверяем какой номер запроса у параметра SndCode - F=0x01 Текущие параметры
                            {
                                xValCnt01 = devTemplate.SndGroups[snd].Counter;                                           // Сохраняем номер запроса (SndCnt)
                                if (devTemplate.SndGroups[snd].userData != "")
                                {
                                    startCnl = Convert.ToInt32(StrToDouble(devTemplate.SndGroups[snd].userData.Trim())); // Сохранить начальный номер сигнала Текущих параметров
                                }
                            }
                            else if (sndcode_ == 0x07)                                                                   // Или F=0x07 (Вес импульса для Регистратора импульсов)
                            {
                                xValCnt07 = devTemplate.SndGroups[snd].Counter;                                           // Сохраняем номер запроса (SndCnt)
                                if (devTemplate.SndGroups[snd].userData != "")
                                {
                                    startCnlv = Convert.ToInt32(StrToDouble(devTemplate.SndGroups[snd].userData.Trim())); // Сохранить начальный номер сигнала Весов импульсов (Регистратор импульсов)
                                }
                            }
                            // TEST TEST TEST Можно упростить, если в Channel записывать 1,2,3 для обоих команд - требуется для создания масок Каналов и Весов каналов

                            activeuse = true;                                                                            // Есть активные запросы
                        }
                    }
                }

                if (devTemplate.CmdGroups.Count != 0) // Определяем наличие активных команд в ветке CmdGroups если они есть и заносим в словарь Индексов команд
                {
                    for (int cmd = 0; cmd < devTemplate.CmdGroups.Count; cmd++)
                    {
                        if (devTemplate.CmdGroups[cmd].Active)
                        {
                            if (!ActiveCmd.ContainsKey(devTemplate.CmdGroups[cmd].Code))
                            {
                                ActiveCmd.Add(devTemplate.CmdGroups[cmd].Code, devTemplate.CmdGroups[cmd]);
                            }
                        }
                    }
                }

                if (activeuse)
                {
                    for (int ac = 0; ac < ActiveSnd.Count; ac++)
                    {
                        var valCnt_ = devTemplate.SndGroups.FindIndex(x => x.Counter == ActiveSnd.ElementAt(ac).Value.Counter);

                        foreach (var val in ActiveSnd.ElementAt(ac).Value.Vals)
                        {
                            if (val.Active)
                            {
                                sigCode = val.Code;        // читаем код тега сигнала переменной

                                if (val.Writable)
                                {
                                    if (!ActiveCmd.ContainsKey(val.Code))
                                    {
                                        ActiveCmd.Add(val.Code, val);
                                    }
                                }

                                int sigN = val.Channel;

                                ActiveCnl.Add(new ActiveCnlList()
                                {
                                    Cnl = sigN,
                                    Code = sigCode,                                             // Номер текущего активного сигнала
                                    Name = val.Name,   // Имя текущего активного сигнала
                                    Format = val.Format, // Тип переменной активного сигнала
                                    IdxValue = valCnt_,   // Индекс группы ответа (ValCnt), в которой находится сигнал // TEST TEST можно обойтись по идее
                                    GroupName = ActiveSnd.ElementAt(ac).Value.GroupName
                                });

                                // Проверяем номер запроса с параметром SndCode = F=0x01 и создаем маску запросов 
                                if (ActiveSnd.ElementAt(ac).Value.Counter == xValCnt01)
                                {   // Заносим в маску номер сигнала - startCnl (1 по умолчанию) бит по расположению.
                                    mask_ch = BitFunc.SetBit(mask_ch, val.Channel - startCnl, val.Active);
                                    maxch = ActiveCnl.FindLast(s => s.IdxValue == ActiveCnl.Find(d => d.Cnl == sigN).IdxValue).Cnl; //  Поиск Максимального номер канала для Текущих параметров
                                }   // SigCnl - startCnl (1 по умолчанию) определяет какой бит 32-х разрядного числа выставить в 1 (единицу)

                                if (ActiveSnd.ElementAt(ac).Value.Counter == xValCnt07)
                                {   // Заносим в маску номер сигнала - startCnlv (41 по умолчанию) бит по расположению.
                                    // Номера сигналов для запроса F=0x07, Вес импульса Регистратора импульсов должны начинаться с 41-ого если не задан в SndData
                                    mask_chv = BitFunc.SetBit(mask_chv, val.Channel - startCnlv, val.Active);
                                }   // SigCnl - startCnlv (41 по умолчанию) определяет какой бит 32-х разрядного числа выставить в 1 (единицу)
                            }
                        }
                    }
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

            for (int i = 0; i < ActiveSnd.Count; i++)
            {
                int sndCnt_ = devTemplate.SndGroups.FindIndex(x => x.Counter == ActiveSnd.ElementAt(i).Value.Counter);  // Выполняем запросы поочередно по индексам из словаря Активных запросов

                byte[] sndcode = ScadaUtils.HexToBytes(devTemplate.SndGroups[sndCnt_].Command, true); // Чтение строки HEX из параметра SndCode
                sndcode_ = sndcode[0];

                // ------------------  Тут вызвать формирование буфера запроса --------------------------------------
                Buf_Out(sndCnt_, sndcode_, null, true);                                             // отправить в функцию Номер и Байт запроса

                if (LastRequestOK)
                {
                    LastRequestOK = false;
                    int tryNum = 0; // Счетчик для корректных ответов

                    // Выполняем опрос если был загружен файл конфигурации
                    while (RequestNeeded(ref tryNum))
                    {
                        Connection.Write(buf_out, 0, buf_out.Length, ProtocolFormat.Hex, out logText);    //послать запрос в порт
                        Log.WriteLine(logText);                                                                        // вывести запрос в Журнал линии связи

                        readcnt = Connection.Read(buf_in, 0, buf_in.Length, PollingOptions.Timeout, ProtocolFormat.Hex, out logText);  //считать значение из порта
                        Log.WriteLine(logText);                                                                                                // вывести запрос в Журнал линии связи

                        // ------------------------------Тут проверка на корректность ответа - ID запроса и CRC -------------------------------------------------------------------
                        var valCnt_ = devTemplate.SndGroups.FindIndex(x => x.Counter == ActiveSnd.ElementAt(i).Value.Counter); // Разбираем ответ поочередно по индексам из Списка Активных запросов

                        if (readcnt == buf_in.Length || readcnt == 11)
                        {
                            crc = CrcFunc.CalcCRC16(buf_in, readcnt);           // Рассчет CRC16 полученного ответа, при совпадении должно вернуть 0 при расчете CRC16(Modbus) и полного буфера вместе с CRC
                            byte fCode = buf_in[4];
                            Array.Copy(buf_in, readcnt - 4, byteIDres, 0, 2);

                            if (!(crc == 0 & fCode != 0 & byteID.SequenceEqual(byteIDres)))                // Проверка CRC, параметра F и ID запроса
                            {
                                if (crc != 0)
                                {
                                    Log.WriteLine(CommPhrases.ResponseCrcError);
                                }
                                else if (fCode == 0)
                                {
                                    string err = Error_code(buf_in[6]);
                                    Log.WriteLine(CommPhrases.ErrorInDataSource + " - " + err);                // При некорректном запросе F будет равен 0x00
                                }
                                else if (!byteID.SequenceEqual(byteIDres))
                                {
                                    Log.WriteLine("ID ответа не совпадает с ID запроса");                      // При несовпадении ID
                                }
                                FinishRequest();
                                invalidData(valCnt_);                                                          // выставить сигналы в невалидное состояние
                            }
                            else
                            {
                                int index_bufin = 6;                                                            // Индекс первой переменной в ответе прибора

                                for (int sig = 0; sig < devTemplate.SndGroups[valCnt_].Vals.Count; sig++)       // Разбор по количеству переменных Vals в ответе
                                {
                                    if (devTemplate.SndGroups[valCnt_].Vals[sig].Active)                        // Если переменная активна, читаем и разбираем ее
                                    {
                                        string sig_type = devTemplate.SndGroups[valCnt_].Vals[sig].Format;      // читаем тип переменной
                                        double mult = devTemplate.SndGroups[valCnt_].Vals[sig].Multiplier;      // читаем множитель (мало ли, вдруг пригодится) :)

                                        //int code = ActiveCnl.Find(s => s.Code == devTemplate.SndGroups[valCnt_].Vals[sig].Code).IdxTag; // Находим в списке Индекс переменной и Указываем индекс Тега

                                        string code = devTemplate.SndGroups[valCnt_].Vals[sig].Code;            // Читаем код переменной

                                        if (sig_type == "float")
                                        {
                                            DeviceData.Set(code, BitConverter.ToSingle(buf_in, index_bufin) * mult, 1);       // Конвертируем буфер байт в переменную float
                                        }
                                        else if (sig_type == "double")
                                        {
                                            DeviceData.Set(code, BitConverter.ToDouble(buf_in, index_bufin) * mult, 1);       // Конвертируем буфер байт в переменную double
                                        }
                                        else if (sig_type == "uint16")
                                        {
                                            DeviceData.Set(code, BitConverter.ToUInt16(buf_in, index_bufin) * mult, 1);       // Конвертируем буфер байт в переменную UInt16
                                        }
                                        else if (sig_type == "uint32")
                                        {
                                            DeviceData.Set(code, BitConverter.ToUInt32(buf_in, index_bufin) * mult, 1);       // Конвертируем буфер байт в переменную UInt32
                                        }
                                        else if (sig_type == "DateTime")                                                    // Определяем системное время и конвертируем в double для Scada
                                        {
                                            int year = Convert.ToInt32(buf_in[index_bufin]) + 2000;     // Читаем из ответа переменные года
                                            int month = Convert.ToInt32(buf_in[index_bufin + 1]);       // месяца
                                            int day = Convert.ToInt32(buf_in[index_bufin + 2]);         // дня
                                            int hour = Convert.ToInt32(buf_in[index_bufin + 3]);        // часа
                                            int minute = Convert.ToInt32(buf_in[index_bufin + 4]);      // минут
                                            int second = Convert.ToInt32(buf_in[index_bufin + 5]);      // секунд
                                            DateTime dateTime = new DateTime(year, month, day, hour, minute, second); //  формируем переменную времени в формате DateTime
                                            DeviceData.Set(code, dateTime.ToOADate(), 1);
                                        }

                                        if (devTemplate.SndGroups[valCnt_].Counter == xValCnt01 || devTemplate.SndGroups[valCnt_].Counter == xValCnt07)
                                        {
                                            if (sig_type == "float" || sig_type == "uint32")
                                            {
                                                index_bufin = index_bufin + 4;           // Увеличиваем индекс переменной для следующего текущего параметра для float
                                            }
                                            else if (sig_type == "double")
                                            {
                                                index_bufin = index_bufin + 8;           // Увеличиваем индекс переменной для следующего текущего параметра для double
                                            }
                                        }
                                    }
                                }
                                Log.WriteLine(CommPhrases.ResponseOK);

                                LastRequestOK = true;
                                FinishRequest();
                            }
                        }
                        else
                        {
                            if (readcnt == 0)
                            {
                                Log.WriteLine(CommPhrases.ResponseError);              // Нет ответа по Timeout - Ошибка связи!
                            }
                            else
                            {
                                Log.WriteLine(Locale.IsRussian ? "Ошибка: некорректная длина ответа!" :
                                    "Error: incorrect response length!");    // Некорректная длина ответа 
                            }
                            FinishRequest();
                            invalidData(valCnt_);                           // выставить сигналы в невалидное состояние
                        }
                        // завершение запроса
                        tryNum++;
                    }
                }
            }

            stopwatch.Stop();
            Log.WriteLine(Locale.IsRussian ?
                "Получено за {0} мс" :
                "Received in {0} ms", stopwatch.ElapsedMilliseconds);
            FinishSession();
        }

        private void invalidData(int cnt) // Расчитать количество невалидных сигналов и определить индекс первого сигнала, выставить сигналы в невалидное состояние
        {
            // Найти индексы начального и конечного номера Тега в Списке, где первое и последнее вхождение номер индекса ответа IdxValue, где произошла ошибка
            var m = ActiveCnl.Find(n => n.IdxValue == cnt).IdxTag;      // Прочитали Номер Тега (IdxTag) первого найденного  в IdxValue списке (стартовый тег)
            var t = ActiveCnl.FindLast(d => d.IdxValue == cnt).IdxTag;  // Прочитали Номер Тега (IdxTag) последней переменной в группе IdxValue (конечный тег)

            // Определяем количество тегов в запросе по формуле (t-m)+1 
            DeviceData.Invalidate(m, (t - m) + 1);
        }

        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public override void SendCommand(TeleCommand cmd)
        {
            base.SendCommand(cmd);
            Stopwatch stopwatch = Stopwatch.StartNew();

            LastRequestOK = false;

            bool WriteOk = false;               // Идентификатор успешной записи
            mask_ch_wr = 0;                     // переменная для параметра MASK_CH записи данных каналов (Регистратор импульсов)
            mask_chv_wr = 0;                    // Переменная для параметра MASK_CH записи Веса импульсов (Регистратор импульсов)

            byte cmdCom = 0x00;                // переменная для байта запроса CmdCom - параметр F протокола (номера для записи)

            byte[] byteData = new byte[] { };     // Буфер для значения переменной
            double cmdVal = cmd.CmdVal;
            int cmdNum; // = cmd.CmdNum;

            byte[] cmdcom = new byte[] { };
            string cmdtype;

            object cmdcnl;
            if (ActiveCmd.ContainsKey(cmd.CmdCode))
            {
                cmdcnl = ActiveCmd[cmd.CmdCode];

                try
                {
                    var valobject = (DevTemplate.SndGroup.Val)cmdcnl;

                    cmdNum = valobject.Channel;
                    cmdtype = valobject.Format;
                    try
                    {
                        cmdcom = ScadaUtils.HexToBytes(valobject.Command, true, true);
                    }
                    catch (FormatException e)
                    {
                        Log.WriteLine($"CmdCom {e.Message}"); // TEST проверить, зависнет или нет ?
                    }
                    Log.WriteLine($"Переменная из Val");
                }
                catch
                {
                    var cmdobject = (DevTemplate.CmdGroup)cmdcnl;
                    Log.WriteLine($"Переменная из Cmd");

                    cmdNum = cmdobject.Channel;
                    cmdtype = cmdobject.Format;
                    try
                    {
                        cmdcom = ScadaUtils.HexToBytes(cmdobject.Command, true, true);
                    }
                    catch (FormatException e)
                    {
                        Log.WriteLine($"CmdCom {e.Message}"); // TEST проверить, зависнет или нет ?
                    }
                }

                if (!double.IsNaN(cmd.CmdVal) && cmd.CmdCode != "")
                {
                    cmdCom = cmdcom[0];

                    // Определив диапазон проверяем к какому из них относятся Текущие параметры и Веса импульса для составления маски

                    if ((cmdNum >= startCnl && cmdNum <= maxch) || (cmdNum >= startCnlv && cmdNum <= maxchv))
                    {
                        if (cmdNum >= startCnl && cmdNum <= maxch && !(cmdNum >= startCnlv && cmdNum <= maxchv))
                        {
                            mask_ch_wr = BitFunc.SetBit(mask_ch_wr, cmdNum - startCnl, true);       // Если каналы относятся к Текущим данным, то формируем маску для  записи маски текущих данных
                        }
                        else
                        {
                            mask_chv_wr = BitFunc.SetBit(mask_chv_wr, cmdNum - startCnlv, true);    // Иначе для записи маски Весов импульсов
                        }
                    }
                    if (cmdtype == "uint16")
                    {
                        //Array.Resize(ref byteData, 2);
                        byteData = BitConverter.GetBytes(Convert.ToUInt16(cmdVal));
                    }
                    else if (cmdtype == "float")
                    {
                        //Array.Resize(ref byteData, 4);
                        byteData = BitConverter.GetBytes(Convert.ToSingle(cmdVal));
                    }
                    else if (cmdtype == "double")
                    {
                        byteData = BitConverter.GetBytes(cmdVal);
                    }
                    else if (cmdtype == "DateTime")
                    {
                        //Array.Resize(ref byteData, 6);
                        DateTime dateTime = DateTime.FromOADate(cmdVal);

                        //byteData[0] = Convert.ToByte(dateTime.Year - 2000);
                        //byteData[1] = Convert.ToByte(dateTime.Month);
                        //byteData[2] = Convert.ToByte(dateTime.Day);
                        //byteData[3] = Convert.ToByte(dateTime.Hour);
                        //byteData[4] = Convert.ToByte(dateTime.Minute);
                        //byteData[5] = Convert.ToByte(dateTime.Second);


                        byteData = new byte[] { Convert.ToByte(dateTime.Year - 2000), Convert.ToByte(dateTime.Month), Convert.ToByte(dateTime.Day), Convert.ToByte(dateTime.Hour), Convert.ToByte(dateTime.Minute), Convert.ToByte(dateTime.Second) };
                    }

                    if (cmdCom == 0x0B) Array.Resize(ref byteData, 8);                                  // Увеличить размер буфера до 8 байт записываемого параметра F=0x0B PARAM_VAL_NEW

                    Buf_Out(cmdNum, cmdCom, byteData, false);                                           // отправить в функцию Номер индекса команды управления и Байт запроса // TEST TEST TEST cmdCnl

                    Connection.Write(buf_out, 0, buf_out.Length, ProtocolFormat.Hex, out logText);      //послать запрос в порт
                    Log.WriteLine(logText);                                                             // вывести запрос в Журнал линии связи 

                    readcnt = Connection.Read(buf_in, 0, buf_in.Length, PollingOptions.Timeout, ProtocolFormat.Hex, out logText);  //считать значение из порта
                    Log.WriteLine(logText);                                                                                        // вывести запрос в Журнал линии связи

                    // Проверка выполнения команды прибором - определяется по ответу прибора на запись команды

                    if (readcnt == buf_in.Length || readcnt == 11)
                    {
                        crc = CrcFunc.CalcCRC16(buf_in, readcnt);           // Рассчет CRC16 полученного ответа, при совпадении должно вернуть 0 при расчете CRC16(Modbus) и полного буфера вместе с CRC
                        byte fCode = buf_in[4];                             // Чтение кода команды F
                        Array.Copy(buf_in, readcnt - 4, byteIDres, 0, 2);

                        if (!(crc == 0 & fCode != 0 & byteID.SequenceEqual(byteIDres)))                    // Проверка CRC, параметра F и ID запроса
                        {
                            if (crc != 0)
                            {
                                Log.WriteLine(CommPhrases.ResponseCrcError);
                            }
                            else if (fCode == 0)
                            {
                                string err = Error_code(buf_in[6]);
                                Log.WriteLine(CommPhrases.WriteDataError + " - " + err);                // При некорректном запросе F будет равен 0x00
                            }
                            else if (!byteID.SequenceEqual(byteIDres))
                            {
                                Log.WriteLine("ID ответа не совпадает с ID запроса");                     // При несовпадении ID
                            }
                        }
                        else
                        {
                            if (fCode == 0x03 || fCode == 0x08)
                            {
                                byte[] maskchRes = new byte[4];
                                Array.Copy(buf_in, 6, maskchRes, 0, 4);
                                if (maskch.SequenceEqual(maskchRes)) WriteOk = true;
                            }
                            if (fCode == 0x05)
                            {
                                if (buf_in[6] != 0) WriteOk = true;
                            }
                            if (fCode == 0x0B)
                            {
                                ushort Result_WR = BitConverter.ToUInt16(buf_in, 6);
                                if (Result_WR == 0) WriteOk = true;
                            }

                            if (WriteOk)
                            {
                                LastRequestOK = true;
                                string nameCnl = ActiveCnl.Find(c => c.Cnl == cmdNum).Name;
                                Log.WriteLine($"{CommPhrases.ResponseOK} - Запись команды {nameCnl} выполнена");
                            }
                            else
                            {
                                Log.WriteLine(CommPhrases.WriteDataError);
                            }
                        }
                    }
                    else
                    {
                        if (readcnt == 0)
                        {
                            Log.WriteLine(CommPhrases.ResponseError);              // Нет ответа по Timeout - Ошибка связи!
                        }
                        else
                        {
                            Log.WriteLine(Locale.IsRussian ? "Ошибка: некорректная длина ответа!" :
                                    "Error: incorrect response length!");
                        }
                    }
                }
                else
                {
                    Log.WriteLine($"Ошибка: {cmd.CmdCode} неиствестный код команды"); // CommPhrases.InvalidCommand
                }
            }
            else
            {
                Log.WriteLine($"{CommPhrases.InvalidCommand} {cmd.CmdCode}"); // $"Ошибка: {cmd.CmdCode} неиствестный код команды"
            }
            FinishRequest(); // TEST
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

        public static double StrToDouble(string s)
        {
            try
            {
                return ParseDouble(s);
            }
            catch
            {
                return double.NaN;
            }
        }

        public static double ParseDouble(string s)
        {
            return double.Parse(s, s.Contains(".") ? NfiDot : NfiComma);
        }

        // --------------------------------------------- Формирование буфера для команд чтения и команд записи
        private void Buf_Out(int Num, byte Fcode, byte[] bData, bool read) // формирование буфера отправки в порт Num = Номер индекса запроса или команды  
        {                                                                   // Fcode = параметр команды SndCode или CmdCode, read = true - чтение, выполняются запросы Snd или read = false, выполняются команды
            if (read)
            {                                                               // Тут собраны команды чтения
                if (Fcode == 0x01 || Fcode == 0x07)                         // Если код равен F=0x01 - Текущие параметры или F=0x07 - Вес омпульсов
                {
                    Array.Resize(ref buf_out, 14);                          // Меняем размер буфера для запроса Текущих параметров и Веса импульсов
                    if (Fcode == 0x01)
                    {
                        maskch = BitConverter.GetBytes(mask_ch);            // запись битовой маски Текущих параметров в массив байт
                        Array.Resize(ref buf_in, col * res_ch + 10);        // длина ответа 4 * n каналов (или 8 * n каналов) + 10 байт
                    }
                    else if (Fcode == 0x07)
                    {
                        maskch = BitConverter.GetBytes(mask_chv);           // запись битовой маски Веса импульсов в массив байт
                        Array.Resize(ref buf_in, 4 * res_chv + 10);         // длина ответа 4 * n каналов + 10 байт
                    }

                    Array.Copy(maskch, 0, buf_out, 6, maskch.Length);       // Копирование маски в буфер запроса
                }
                else if (Fcode == 0x04)                                     // Если код равен F=0x04 - Системное время
                {
                    Array.Resize(ref buf_out, 10);                          // Меняем размер буфера для запроса Системного времмени
                    Array.Resize(ref buf_in, 16);                           // длина ответа 16 байт
                }
                else if (Fcode == 0x0A)                                     // Если код равен F=0x0A - Параметры прибора
                {
                    Array.Resize(ref buf_out, 12);                          // Меняем размер буфера для запроса Параметров прибора
                    byte[] snddata = ScadaUtils.HexToBytes(devTemplate.SndGroups[Num].userData, true);       // Чтение строки HEX из параметра SndData
                    buf_out[6] = snddata[0];                                                                // требуется 1 байт, код параметра
                    buf_out[7] = 0x00;                                                                      // второй байт будет со значением 0
                    Array.Resize(ref buf_in, 18);                           // длина ответа 18 байт
                }
            }
            else                                                            // Тут собраны команды записи
            {
                if (Fcode == 0x03 || Fcode == 0x08)                                                                     // Если код равен F=0x03 – код функции записи текущих показаний
                {                                                                                                       // Или F=0x08 - Вес импульса для Регистраторов импульса
                    Array.Resize(ref buf_out, 0x0E + bData.Length);                                                     // Меняем размер буфера для запроса Текущих параметров
                    maskch = Fcode == 0x03 ? BitConverter.GetBytes(mask_ch_wr) : BitConverter.GetBytes(mask_chv_wr);    // запись битовой маски редактируемого канала в массив байт
                    Array.Copy(maskch, 0, buf_out, 6, maskch.Length);                                                   // Копирование маски в буфер запроса
                    Array.Copy(bData, 0, buf_out, 10, bData.Length);                                                    // Копируем значение cmdVal в буфер запроса
                    Array.Resize(ref buf_in, 14);                                                                       // длина ответа 14 байт
                }
                else if (Fcode == 0x05)                                                                                 // Запись времени в прибор
                {
                    Array.Resize(ref buf_out, 10 + bData.Length);
                    Array.Copy(bData, 0, buf_out, 6, bData.Length);
                    Array.Resize(ref buf_in, 14);                                                                       // длина ответа 14 байт
                }
                else if (Fcode == 0x0B)                                                                                 // Запись параметров в прибор
                {
                    Array.Resize(ref buf_out, 12 + bData.Length);
                    Array.Copy(bData, 0, buf_out, 8, bData.Length);
                    byte[] cmddata = ScadaUtils.HexToBytes(devTemplate.CmdGroups[Num].userData, true);       // Чтение строки HEX из параметра SndData
                    buf_out[6] = cmddata[0];                                                                // требуется 1 байт, код параметра
                    buf_out[7] = 0x00;                                                                      // второй байт будет со значением 0
                    Array.Resize(ref buf_in, 12);                                                           // длина ответа 12 байт
                }

            }

            buf_out[4] = Fcode;                                             // Копируем в буфер код запроса F
            buf_out[5] = Convert.ToByte(buf_out.Length);                    // Запись длины массива запроса - параметр L
            idBCD = BitConverter.GetBytes(ConvFunc.DecToBCD(NumAddress));   // Преобразование адреса в BCD формат
            ConvFunc.Reverse_array(idBCD, false);                           // Переворот буфера старшим байтом вперед
            Array.Copy(idBCD, 0, buf_out, 0, idBCD.Length);                 // Копирование адреса в буфер запроса

            byteID = BitConverter.GetBytes((ushort)rnd.Next(1, 65535));     // Сформировать случайный ID запроса
            buf_out[buf_out.Length - 4] = byteID[0];
            buf_out[buf_out.Length - 3] = byteID[1];

            crc = CrcFunc.CalcCRC16(buf_out, buf_out.Length - 2);       // Рассчет контрольной суммы CRC16
            buf_out[buf_out.Length - 2] = (byte)(crc % 256);            // Запись младшего байта контрольной суммы в буфер
            buf_out[buf_out.Length - 1] = (byte)(crc / 256);            // Запись старшего байта контрольной суммы в буфер
        }

    }
}
