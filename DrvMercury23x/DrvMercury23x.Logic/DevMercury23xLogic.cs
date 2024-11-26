// Copyright (c) Andrey Burakhin. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Scada.Comm.Channels;
using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Lang;
using Scada.Data.Models;
using Scada.Lang;
using ScadaCommFunc;
using System.Diagnostics;
using Scada.Data.Const;


namespace Scada.Comm.Drivers.DrvMercury23x.Logic
{
    internal class DevMercury23xLogic : DeviceLogic
    {
        private readonly Mercury23xOptions options;         // the tester options
        private readonly string UserPwd;                    // Пароль 1-го уровня доступа
        private readonly string AdminPwd;                   // Пароль 2-го уровня доступа

        private DevTemplate devTemplate = new DevTemplate();

        private int read_cnt = 0;
        private string fileName = "";
        private string filePath = "";
        private bool fileyes = false;   // При отсутствии файла не выполнять опрос
        private bool CommSucc = true;
        private byte[] inBuf = new byte[300];
        private int mask_g1 = 0;    // Входная переменная для выбора тегов
        private int mgn_znac;
        private int energy;

        private bool ActiveProfile = false;
        private bool firstAProfile = true;

        // TEST TEST TEST
        private bool settLoaded;   // загрузка настроек выполнена
                                   //private DateTime hrBegDT;  // дата и час начала часовых архивов
                                   //private DateTime dayBegDT; // дата и час начала суточных архивов
        private DateTime hrReqDT;  // дата и час запроса часовых архивов
                                   //private DateTime dayReqDT; // дата запроса суточных архивов
                                   // TEST TEST TEST



        private Requests requests = new Requests();
        private void InitRequests()
        {
            requests.testCnlReq = Protocol.TestCnlReq(NumAddress);


            if (Level == "1" || Level.ToLower() == "user")
            {
                requests.openCnlReq = Protocol.OpenCnlReq(NumAddress, "1", UserPwd);
            }
            else if ( Level == "2" || Level.ToLower() == "admin")
            {
                requests.openCnlReq = Protocol.OpenCnlReq(NumAddress, "2", AdminPwd);
            }
            else
            {
                requests.openCnlReq = Protocol.OpenCnlReq(NumAddress, "1", UserPwd);
            }

            requests.openAdmReq = Protocol.OpenCnlReq(NumAddress, "2", AdminPwd);
            requests.closeCnlReq = Protocol.WriteComReq(NumAddress, 0x02);
            requests.readTimeReq = Protocol.WriteCompReq(NumAddress, 0x04, 0x00);
            requests.lastSyncReq = Protocol.WriteCompReq(NumAddress, 0x04, 0x02, new byte[] { 0xFF }); // Чтение последней записи журнала синхронизации времени
            requests.kuiReq = Protocol.KuiReq(NumAddress);
            requests.infoReq = Protocol.InfoReq(NumAddress);
            requests.curTimeReq = Protocol.CurTimeReq(NumAddress);
            requests.wordStatReq = Protocol.WriteCompReq(NumAddress, 0x04, 0x14, new byte[] { 0xFF }); // Чтение последней записи журнала кода словосостояния прибора

            // Формирование запроса фиксации данных в зависимости от параметра multicast
            if (devTemplate.multicast)
            {
                requests.fixDataReq = Protocol.FixDataReq(0xFE);
            }
            else
            {
                requests.fixDataReq = Protocol.FixDataReq(NumAddress);
            }
        }

        public static long Ticks() // возвращает время в миллисекундах
        {
            DateTime now = DateTime.Now;
            long time = now.Ticks / 10000;
            return time;
        }

        public class MyDevice                   // переменные для устройств на линии связи
        {
            public SaveParam saveParam = new SaveParam();
            public bool testcnl = false;        // фиксация команды тестирования канала
            public bool opencnl = false;        // фиксация команды авторизации и открытия канала
            public bool Kui = false;            // фиксация команды чтения коэффициентов трансформации
            public int ku = 1;                  // Коэффициент трансформации напряжения 
            public int ki = 1;                  // Коэффициент трансформации тока
            public int[] parkui = new int[13];  // массив для данных коэффициентов
            public long tik = 0;                // таймер времени открытого канала
            public int firstKp = 0;             // КП, являющийся первым КП в опросе, последующие не записывают номер КП, значение остается = 0
            public bool firstFix = false;       // Переменная для фиксации адреса первого КП
            public DateTime dt;                 // Время команды фиксации для сравнения 

            // Данные счетчика
            public bool readInfo = false;       // блокировка чтения информации счетчика
            public int serial = 0;              // Серийный номер счетчика
            public int Aconst = 500;            // Постоянная счетчика, по умолчанию 500 имп/квт*ч
            public DateTime made;               // Дата изготовления
            public DateTime srezDt;             // Дата чтения архива
            public DateTime LastSyncDt;         // Время последней синхронизации в приборе

            public override string ToString()
            {
                string outprops = string.Concat("SN_", serial.ToString(), " Изготовлен ", made.ToString("dd.MM.yyyy"), " Время архива ", srezDt.ToString());
                return outprops;
            }
        }

        protected virtual string address
        {
            get
            {
                return devTemplate.Name + "_" + Convert.ToString(NumAddress);
            }
        }

        private MyDevice devaddr = new MyDevice();
        private MyDevice GetMyDevice()
        {
            if (!LineContext.SharedData.ContainsKey(address))
            {
                LineContext.SharedData.Add(address, devaddr);
            }
            else
            {
                devaddr = LineContext.SharedData[address] as MyDevice;
            }
            return devaddr;
        }




        public static string ToLogString(byte errcode)
        {
            string logs = "";
            switch (errcode)
            {
                case 0x01: logs = Locale.IsRussian ? "Недопустимая команда или параметр" : "Invalid command or parameter"; break;
                case 0x02: logs = Locale.IsRussian ? "Внутренняя ошибка счетчика" : "Internal error of the device"; break;
                case 0x03: logs = Locale.IsRussian ? "Недостаточен уровень доступа" : "Insufficient access level"; break;
                case 0x04: logs = Locale.IsRussian ? "Внутренние часы корректировались" : "The internal clock was adjusted"; break;
                case 0x05: logs = Locale.IsRussian ? "Не открыт канал связи" : "The communication channel is not open"; break;
            }
            return logs;
        }

        public static int ConstA(byte aconst)
        {
            aconst = (byte)(aconst & 0x0f);
            // Постоянная счетчика в имп/квт*ч  - 2.3.16 - Чтение варианта исполнения 2-й байт 0-3 биты
            int consta = 500;
            switch (aconst)
            {
                case 0x00: consta = 5000; break;
                case 0x01: consta = 25000; break;
                case 0x02: consta = 1250; break;
                case 0x03: consta = 500; break;
                case 0x04: consta = 1000; break;
                case 0x05: consta = 250; break;
            }
            return consta;
        }


        // Новый целочисленный массив с данными
        public int[] nmass_int(int[] mass_in, uint mask)
        {
            int c = 0;
            int b = 0;
            int[] par_num = new int[1];

            while (mask != 0)
            {
                if ((mask & 1) != 0)
                {
                    Array.Resize(ref par_num, c + 1); //изменить размер массива
                    par_num[c] = mass_in[b];
                    c++;
                }
                mask = mask >> 1;
                b++;
            }
            return par_num;
        }

        public bool chan_err(byte code) // Проверка изменения кода ошибки в ответе прибора и выставление флага для генерации события
        {
            bool q = false;
            if (code_err != code) q = !q;
            return q;
        }

        private int[] bwri = new int[13];       // BWRI для запроса параметром 14h
        private int[] bwrc = new int[13];       // Разрешающая способность регистров хранения 
        private int[] b_length = new int[13];   // количество байт в ответе счетчика
        private int[] parb = new int[13];       // количество байт в параметре ответа (4 или 3)
        private int[] parc = new int[13];       // количество параметров в ответе (4, 3 или 1)

        // Массив значений параметров BWRI счетчика + 'энергии от сброса параметр 14h
        // Команды BWRI для запроса 14h:
        // 0x00 - Мощность P по сумме фаз, фазе 1, фазе 2, фазе 3   (Вт)
        // 0x04 - Мощность Q по сумме фаз, фазе 1, фазе 2, фазе 3   (вар)
        // 0x08 - Мощность S по сумме фаз, фазе 1, фазе 2, фазе 3   (ВА)
        // 0x10 - Напряжение по фазе 1, фазе 2, фазе 3              (В)
        // 0x30 - Косинус ф по сумме фаз, фазе 1, фазе 2, фазе3
        // 0x20 - Ток по фазе 1, фазе 2, фазе 3                     (А)
        // 0x40 - Частота сети
        // 0x50 - Угол м-ду ф. 1 и 2, 1 и 3, 2 и 3                  (градусы)

        // F0,-,F4 - Зафиксированная энергия от сброса
        private int[] bwri_14 = new int[] { 0x00, 0x04, 0x08, 0x30, 0x10, 0x20, 0x50, 0x40, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4 }; // BWRI для запроса параметром 14h
        private int[] bwrc_14 = new int[] { 100, 100, 100, 1000, 100, 1000, 100, 100, 1000, 1000, 1000, 1000, 1000 }; // Разрешающая способность регистров хранения 
        private int[] b_length_14 = new int[] { 19, 19, 19, 15, 12, 12, 12, 6, 19, 19, 19, 19, 19 };   // количество байт в ответе счетчика
        private int[] parb_14 = new int[] { 4, 4, 4, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4 };    // количество байт в параметре ответа (4 или 3)
        private int[] parc_14 = new int[] { 4, 4, 4, 4, 3, 3, 3, 1, 4, 4, 4, 4, 4 };    // количество параметров в ответе (4, 3 или 1)

        // Массив значений параметров BWRI счетчика + 'энергии от сброса для чтения параметром 16h
        // Команды BWRI для запроса 16h:
        // 0x00 - Мощность P по сумме фаз, фазе 1, фазе 2, фазе 3   (Вт)
        // 0x04 - Мощность Q по сумме фаз, фазе 1, фазе 2, фазе 3   (вар)
        // 0x08 - Мощность S по сумме фаз, фазе 1, фазе 2, фазе 3   (ВА)
        // 0x11 - Напряжение по фазе 1, фазе 2, фазе 3              (В)
        // 0x30 - Косинус ф по сумме фаз, фазе 1, фазе 2, фазе3
        // 0x21 - Ток по фазе 1, фазе 2, фазе 3                     (А)
        // 0x40 - Частота сети
        // 0x51 - Угол м-ду ф. 1 и 2, 1 и 3, 2 и 3                  (градусы)

        private int[] bwri_16 = { 0x00, 0x04, 0x08, 0x30, 0x11, 0x21, 0x51, 0x40, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4 }; // BWRI для запроса параметром 16h
        private int[] bwrc_16 = { 100, 100, 100, 1000, 100, 1000, 100, 100, 1000, 1000, 1000, 1000, 1000 }; // Разрешающая способность регистров хранения 
        private int[] b_length_16 = { 15, 15, 15, 15, 12, 12, 12, 6, 19, 19, 19, 19, 19 };   // количество байт в ответе счетчика
        private int[] parb_16 = { 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4 };    // количество байт в параметре ответа (4 или 3)
        private int[] parc_16 = { 4, 4, 4, 4, 3, 3, 3, 1, 4, 4, 4, 4, 4 };    // количество параметров в ответе (4, 3 или 1)

        // Массив значений энергии по фазам прямого направления и чтения энергии от сброса функцией 0x05 при чтении счетчика параметром 16h кода функции 0x08
        private int[] massenergy = new int[] { 0, 1, 2, 3, 4 };

        private bool newmass = false;
        private bool newenergy = false;

        private int tagIndex = 0;
        private int[] nbwri = new int[1];
        private int[] nbwrc = new int[1];
        private int[] nb_length = new int[1];
        private int[] nparb = new int[1];
        private int[] nparc = new int[1];
        private int[] nenergy = new int[1];
        private bool readstatus = false;    // Читать статус счетчика
        private byte code_err = 0x0;
        private byte code = 0x0;
        private int Napr = 1;               // Направление Активной или Реактивной мощности 1 = прямое (бит направления = 0), при обратном значение = -1
        private readonly int fixTime = 30;           // по умолчанию разница времени фиксации 30 секунд
        private readonly int saveTime = 60;          // Период сохранения данных БД, по умолчанию 60 сек
        private bool timeSync = false;
        private uint readPQSUI = 0;
        private uint energyL = 0;
        private int halfArch = 2;               // По умолчанию номер архивного среза
        private int srezPeriod = 30;            // Период среза средних мощностей, по умолчанию 30 минут
        private byte[] wordStat = new byte[8];  // массив байт для словосостояния прибора

        private byte CMD;           // Command code (Код команды)
        private byte PAR;           // Parametr code (Код параметра)

        private string readparam, Level; //входная строка для параметра команды 0x08, объявление переменной для пароля и уровня доступа

        public class Profile
        {
            public string profileName;
            public string code;
            public double range;
            public int offset;
        }
        private List<Profile> profile = new List<Profile>();

        Dictionary<string, CnlPrototypeFactory.ActiveChannel> channels = new Dictionary<string, CnlPrototypeFactory.ActiveChannel>();

        // Выполняем действия при добавлении Линии связи - Чтение шаблона, создание списка Тегов
        public override void InitDeviceTags() // OnAddedToCommLine()
        {

            if (mgn_znac != 0)
            {
                bool mgn_P = BitFunc.GetBit(mask_g1, 0) > 0; // Параметр Bit группы "Мощность P" должен быть равен 0 
                if (mgn_P)
                    tagcreate("Мгновенные значения:", 0); // idgr

                bool mgn_Q = BitFunc.GetBit(mask_g1, 1) > 0; // Параметр Bit группы "Мощность Q" должен быть равен 1
                if (mgn_Q)
                    tagcreate("Мгновенные значения:", 1);

                bool mgn_S = BitFunc.GetBit(mask_g1, 2) > 0; // Параметр Bit группы "Мощность S" должен быть равен 2
                if (mgn_S)
                    tagcreate("Мгновенные значения:", 2);

                bool mgn_cos = BitFunc.GetBit(mask_g1, 3) > 0; // Параметр Bit группы "COSф" должен быть равен 3
                if (mgn_cos)
                    tagcreate("Мгновенные значения:", 3);

                bool mgn_U = BitFunc.GetBit(mask_g1, 4) > 0; // Параметр Bit группы "Напряжение" должен быть равен 4
                if (mgn_U)
                    tagcreate("Мгновенные значения:", 4);

                bool mgn_I = BitFunc.GetBit(mask_g1, 5) > 0; // Параметр Bit группы "Ток" должен быть равен 5
                if (mgn_I)
                    tagcreate("Мгновенные значения:", 5);

                bool mgn_FU = BitFunc.GetBit(mask_g1, 6) > 0; // Параметр Bit группы "Угол м-ду ф." должен быть равен 6
                if (mgn_FU)
                    tagcreate("Мгновенные значения:", 6);

                bool mgn_F = BitFunc.GetBit(mask_g1, 7) > 0; // Параметр Bit группы "Частота" должен быть равен 7
                if (mgn_F)
                    tagcreate("Мгновенные значения:", 7);
            }

            if (energy != 0)
            {
                bool en_summ = BitFunc.GetBit(mask_g1, 8) > 0; // Параметр Bit группы "Сумма" должен быть равен 8
                if (en_summ)
                    tagcreate("Энергия от сброса:", 8);

                bool en_tar1 = BitFunc.GetBit(mask_g1, 9) > 0; // Параметр Bit группы "Тариф 1" должен быть равен 9
                if (en_tar1)
                    tagcreate("Энергия от сброса:", 9);

                bool en_tar2 = BitFunc.GetBit(mask_g1, 10) > 0; // Параметр Bit группы "Тариф 2" должен быть равен 10
                if (en_tar2)
                    tagcreate("Энергия от сброса:", 10);

                bool en_tar3 = BitFunc.GetBit(mask_g1, 11) > 0; // Параметр Bit группы "Тариф 3" должен быть равен 11
                if (en_tar3)
                    tagcreate("Энергия от сброса:", 11);

                bool en_tar4 = BitFunc.GetBit(mask_g1, 12) > 0; // Параметр Bit группы "Тариф 4" должен быть равен 12
                if (en_tar4)
                    tagcreate("Энергия от сброса:", 12);

                bool enL_summ = BitFunc.GetBit(mask_g1, 13) > 0; // Параметр Bit группы "Сумма А+" должен быть равен 13
                if (enL_summ)
                    tagcreate("Энергия от сброса:", 13);

                bool enL_tar1 = BitFunc.GetBit(mask_g1, 14) > 0; // Параметр Bit группы "Тариф 1 А+" должен быть равен 14
                if (enL_tar1)
                    tagcreate("Энергия от сброса:", 14);

                bool enL_tar2 = BitFunc.GetBit(mask_g1, 15) > 0; // Параметр Bit группы "Тариф 2 А+" должен быть равен 15
                if (enL_tar2)
                    tagcreate("Энергия от сброса:", 15);

                bool enL_tar3 = BitFunc.GetBit(mask_g1, 16) > 0; // Параметр Bit группы "Тариф 3 А+" должен быть равен 16
                if (enL_tar3)
                    tagcreate("Энергия от сброса:", 16);

                bool enL_tar4 = BitFunc.GetBit(mask_g1, 17) > 0; // Параметр Bit группы "Тариф 4 А+" должен быть равен 17
                if (enL_tar4)
                    tagcreate("Энергия от сброса:", 17);
            }

            if (profile.Count > 0) // Нужен индекс группы профилей ?
            {
                //tagGroup = new TagGroup("Профили мощностей");
                for (int p = 0; p < profile.Count; p++)
                {
                    channels.Add(profile[p].code,
                        new CnlPrototypeFactory.ActiveChannel()
                        {
                            GroupName = "Профили мощностей",
                            Name = profile[p].profileName,
                            Code = profile[p].code,
                            Mode = 1,
                            CnlType = CnlTypeID.Input,
                        });
                }
            }

            if (readstatus)
            {
                channels.Add("error",
                    new CnlPrototypeFactory.ActiveChannel()
                    {
                        GroupName = "Статус:",
                        Name = "Код ошибки:",
                        Code = "error",
                        CnlType = CnlTypeID.Input,
                    });
                channels.Add("kI",
                    new CnlPrototypeFactory.ActiveChannel()
                    {
                        GroupName = "Статус:",
                        Name = "коэфф. трансф. тока:",
                        Code = "kI",
                        CnlType = CnlTypeID.Input,
                    });
                channels.Add("kU",
                    new CnlPrototypeFactory.ActiveChannel()
                    {
                        GroupName = "Статус:",
                        Name = "коэфф. трансф. напряжения:",
                        Code = "kU",
                        CnlType = CnlTypeID.Input,
                    });

                channels.Add("wordSt",
                    new CnlPrototypeFactory.ActiveChannel()
                    {
                        GroupName = "Статус:",
                        Name = "Слово состояния:",
                        Code = "wordSt",
                        CnlType = CnlTypeID.Input,
                        format = "hex",
                    });
            }

            if (devTemplate.CmdGroups.Count > 0)
            {
                for (int c = 0; c < devTemplate.CmdGroups.Count; c++)
                {
                    int errorCnt = 0;
                    byte[] cData = null;
                    string paras = null;
                    int lendata = 0;
                    string format = "";

                    if (devTemplate.CmdGroups[c].Active)
                    {
                        int cnt_ = devTemplate.CmdGroups[c].inCnt;
                        int mode_ = 1;
                        var md = int.TryParse(devTemplate.CmdGroups[c].Mode, out mode_);

                        try
                        {
                            byte[] cmd_ = ScadaUtils.HexToBytes(devTemplate.CmdGroups[c].Cmd, true);
                            CMD = cmd_[0];
                        }
                        catch
                        {
                            Log.WriteLine(string.Format(Locale.IsRussian ?
                                "Ошибка задания кода команды. Строка команды не является Hex или пуста. Индекс CmdGroup = {0}" :
                                "Error setting command сode. The command line is not Hex or empty. Index CmdGroup = {0}", c));
                            errorCnt++;
                        }

                        try
                        {
                            paras = string.IsNullOrEmpty(devTemplate.CmdGroups[c].Par) ? null : devTemplate.CmdGroups[c].Par;
                            if (paras != null)
                            {
                                byte[] par_ = ScadaUtils.HexToBytes(devTemplate.CmdGroups[c].Par, true);
                                PAR = par_[0];
                            }
                        }
                        catch
                        {
                            Log.WriteLine(string.Format(Locale.IsRussian ?
                                "Ошибка задания параметра команды. Строка параметров не является Hex. Индекс CmdGroup = {0}" :
                                "Error setting the command parameter. The parameter string is not Hex. Index CmdGroup = {0}", c));
                            errorCnt++;
                        }

                        try
                        {
                            string datas = string.IsNullOrEmpty(devTemplate.CmdGroups[c].Data) ? null : devTemplate.CmdGroups[c].Data;
                            if (datas != null)
                            {
                                byte[] data_ = ScadaUtils.HexToBytes(datas, true);
                                Array.Resize(ref cData, data_.Length);
                                Array.Copy(data_, cData, data_.Length);
                                lendata = data_.Length;
                                format = "string";
                            }
                        }
                        catch
                        {
                            Log.WriteLine(string.Format(Locale.IsRussian ?
                                "Ошибка: строка данных не является Hex. Индекс CmdGroup = {0}" :
                                "Error: the data string is not Hex. Index CmdGroup = {0}", c));
                            errorCnt++;
                        }

                        byte[] comm;
                        if (paras != null)
                        {
                            comm = Protocol.WriteCompReq(NumAddress, CMD, PAR, cData);
                        }
                        else
                        {
                            comm = Protocol.WriteComReq(NumAddress, CMD, cData);
                        }

                        if (errorCnt == 0)
                        {
                            channels.Add(devTemplate.CmdGroups[c].Code,
                                new CnlPrototypeFactory.ActiveChannel()
                                {
                                    GroupName = "Команды",
                                    Name = devTemplate.CmdGroups[c].Name,
                                    Code = devTemplate.CmdGroups[c].Code,
                                    setCommand = comm,
                                    scCnt = cnt_,
                                    datalen = lendata,
                                    CnlType = CnlTypeID.Output,
                                    format = format,
                                    Mode = mode_,
                                });
                        }
                    }
                }
            }

            foreach (CnlPrototypeGroup group in CnlPrototypeFactory.GetCnlPrototypeGroups(channels)) // 
            {
                DeviceTags.AddGroup(group.ToTagGroup());
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
                readstatus = devTemplate.readStatus;
                readparam = string.IsNullOrEmpty(devTemplate.readparam) ? "14h" : devTemplate.readparam;

                timeSync = devTemplate.SyncTime;
                halfArch = devTemplate.halfArchStat == 0 ? halfArch : devTemplate.halfArchStat;

                if (devTemplate.SndGroups.Count != 0) // Определить активные запросы объектов и записать в список индексы запросов для создания тегов
                {
                    for (int sg = 0; sg < devTemplate.SndGroups.Count; sg++)
                    {
                        if (devTemplate.SndGroups[sg].Active)
                            mask_g1 = BitFunc.SetBit(mask_g1, devTemplate.SndGroups[sg].Bit, true);
                    }
                }

                if (devTemplate.ProfileGroups.Count != 0) //Определить наличие запросов профилей
                {
                    for (int i = 0; i < devTemplate.ProfileGroups.Count; i++)
                    {
                        if (devTemplate.ProfileGroups[i].Active)
                        {
                            for (int k = 0; k < devTemplate.ProfileGroups[i].value.Count; k++)
                            {
                                if (devTemplate.ProfileGroups[i].value[k].active)
                                {
                                    profile.Add(new Profile
                                    {
                                        profileName = devTemplate.ProfileGroups[i].value[k].name,
                                        code = devTemplate.ProfileGroups[i].value[k].code,
                                        range = SToDouble(devTemplate.ProfileGroups[i].value[k].multiplier),
                                        offset = k * 2
                                    });
                                }
                            }
                            ActiveProfile = true;
                        }
                    }
                }

                if (readparam == "14h")
                {
                    Array.Copy(bwri_14, bwri, 13);
                    Array.Copy(bwrc_14, bwrc, 13);
                    Array.Copy(b_length_14, b_length, 13);
                    Array.Copy(parb_14, parb, 13);
                    Array.Copy(parc_14, parc, 13);
                }

                if (readparam == "16h")
                {
                    Array.Copy(bwri_16, bwri, 13);
                    Array.Copy(bwrc_16, bwrc, 13);
                    Array.Copy(b_length_16, b_length, 13);
                    Array.Copy(parb_16, parb, 13);
                    Array.Copy(parc_16, parc, 13);
                }


                readPQSUI = Convert.ToUInt32(mask_g1 & 0x1FFF); // проверка маски на необходимость чтения мгновенных значений

                mgn_znac = mask_g1 & 0xFF; // отсечь мгновенные значения для организации отображения тегов
                energy = mask_g1 & 0x3FF00; // Отсечь параметры энергии для организации отображения тегов

                uint par14h = Convert.ToUInt32(mask_g1 & 0x1FFF); // отсечь количество параметров для команды 08h и параметра 14h

                energyL = Convert.ToUInt32(mask_g1 & 0x3E000); // Проверка наличия опроса значений энергии прямого направления
                energyL = BitFunc.ROR(energyL, 13);

                if (!newmass)
                {
                    nbwri = nmass_int(bwri, par14h); // Создание новых массивов для команды 08h параметр 14h для чтения согласно битовой маске.
                    nbwrc = nmass_int(bwrc, par14h);
                    nb_length = nmass_int(b_length, par14h);
                    nparb = nmass_int(parb, par14h);
                    nparc = nmass_int(parc, par14h);
                    newmass = true;
                }
                if (!newenergy)
                {
                    nenergy = nmass_int(massenergy, energyL); // Создание нового массива значений энергии по фазам от сброса согласно битовой маске
                    newenergy = true;
                }
                InitRequests();
            }

            GetMyDevice();
            MyDevice prop = (MyDevice)LineContext.SharedData[address];

            // Чтение параметров счетчика из файла по переменной readinfo
            if (!prop.readInfo)
            {
                hrReqDT = DateTime.MinValue;

                LoadSettings();

                if (settLoaded)
                {
                    bool t1 = DateTime.TryParse(prop.saveParam.madeDt, out prop.made);
                    bool t2 = int.TryParse(prop.saveParam.serial, out prop.serial);
                    bool t3 = int.TryParse(prop.saveParam.constA, out prop.Aconst);

                    // Если все переменные считаны и корректны то запрос чтения параметров выполняться не будет
                    prop.readInfo = t1 && t2 && t3;

                    bool t4 = DateTime.TryParse(prop.saveParam.arcDt, out prop.srezDt);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevMercury23xLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            options = new Mercury23xOptions(deviceConfig.PollingOptions.CustomOptions);
            UserPwd = string.IsNullOrEmpty(options.UserPwd) ? "111111" : options.UserPwd;
            AdminPwd = string.IsNullOrEmpty(options.AdminPwd) ? "222222" : options.AdminPwd;
            Level = string.IsNullOrEmpty(options.Level) ? "1" : options.Level;

            CanSendCommands = true;
            ConnectionRequired = true;
        }


        #region Session

        /// <summary>
        /// Performs a communication session.
        /// </summary>
        public override void Session()
        {
            base.Session();
            tagIndex = 0;

            if (!fileyes)        // Если конфигурация не была загружена, выставляем все теги в невалидное состояние и выходим         
            {
                DeviceData.Invalidate();
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            CommSucc = true;
            MyDevice prop = (MyDevice)LineContext.SharedData[address];

            // код работает один раз при запуске линии
            if (devTemplate.multicast && !prop.firstFix)
            {
                // Если параметр firstFix = true, то первый КП на линии посылает команду фиксации данных по широковещательному
                // адресу 0xFE в линию, иначе каждый КП посылает команду своему прибору при параметре multicast = false
                prop.firstKp = DeviceNum; // в качестве стартового используем номер КП, остальные будут = 0
                for (int x = 0; x < LineContext.SharedData.Count; x++)
                {
                    try
                    {
                        var Val = (MyDevice)LineContext.SharedData.ElementAt(x).Value;
                        Val.firstFix = true;
                    }
                    catch { }
                }
            }

            if (!prop.testcnl)
            {
                Request(requests.testCnlReq, 4);

                if (LastRequestOK)
                {
                    prop.testcnl = true;
                }
                else
                {
                    // Если тест канала не прошел, больше не опрашиваем
                    //CommSucc = false;
                    DeviceData.Invalidate();
                    return;
                }
            }

            // Открытие канала с уровнем доступа согласно введенного пароля.
            if (prop.testcnl)
            {
                long t2 = Ticks();
                if ((t2 > (prop.tik + 240000)) || !prop.opencnl)
                {
                    // Запрос на открытие канала
                    Request(requests.openCnlReq, 4);

                    if (LastRequestOK) prop.opencnl = true;
                    prop.tik = t2;
                }
            }

            // Определить начало нового дня, возможна синхронизация времени или нет
            if (prop.opencnl && timeSync && DateTime.Today > prop.LastSyncDt)
            {
                DateTime nowDt = DateTime.Now; // запрос времени с секундами, минутами, часами
                bool needSync = false;

                // Чтение времени последней синхронизации счетчика
                Request(requests.lastSyncReq, 16);

                if (LastRequestOK)
                {
                    // DateTime lastSyncDt = DateTime.MinValue;
                    DateTime lastSyncDt = new DateTime(2000 + (int)ConvFunc.BcdToDec(new byte[] { inBuf[6] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[5] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[4] }));
                    prop.LastSyncDt = lastSyncDt;

                    if (DateTime.Today > lastSyncDt)
                    {
                        needSync = true;
                    }
                }

                if (needSync)
                {
                    Request(requests.readTimeReq, 11);
                    if (LastRequestOK)
                    {
                        DateTime readDt = new DateTime(2000 + (int)ConvFunc.BcdToDec(new byte[] { inBuf[7] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[6] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[5] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[3] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[2] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[1] }));

                        // Если время счетчика перешло на новый день
                        if (readDt > DateTime.Today)
                        {
                            if (nowDt.Subtract(readDt).TotalSeconds > Math.Abs(20))
                            {
                                int second;
                                int minutes;
                                int hours;
                                // Часы в счетчике отстают или спешат больше 4-х минут
                                if (nowDt.Subtract(readDt).TotalMinutes > Math.Abs(4))
                                {
                                    if (nowDt.Subtract(readDt).TotalMinutes > 0)
                                    {
                                        readDt = readDt.AddMinutes(3);
                                    }
                                    else
                                    {
                                        readDt = readDt.AddMinutes(-3);
                                    }
                                    second = readDt.Second;
                                    minutes = readDt.Minute;
                                    hours = readDt.Hour;
                                }
                                else
                                {
                                    second = nowDt.Second;
                                    minutes = nowDt.Minute;
                                    hours = nowDt.Hour;
                                }

                                byte sec = (byte)ConvFunc.DecToBCD(second);
                                byte min = (byte)ConvFunc.DecToBCD(minutes);
                                byte hour = (byte)ConvFunc.DecToBCD(hours);
                                Request(Protocol.WriteCompReq(NumAddress, 0x03, 0x0D, new byte[] { sec, min, hour }), 4, string.Format(Locale.IsRussian ? "Команда синхронизация времени" : "Time synchronization command"));
                                if (LastRequestOK)
                                {
                                    Log.WriteLine(string.Format(Locale.IsRussian ?
                                        "OK: Команда Выполнена успешно" :
                                        "OK: Command completed successfully"));
                                }
                                else
                                {
                                    Log.WriteLine(string.Format(Locale.IsRussian ?
                                        "Ошибка: Команда не выполнена" :
                                        "Error: Command not executed"));
                                }
                            }
                            else
                            {
                                // Если расхождение меньше 20 сек
                                prop.LastSyncDt = nowDt;
                            }
                        }
                    }
                }
            }

            // Запрос информации счетчика - Серийный номер, дата выпуска, версия ПО, вариант исполнения
            if (devTemplate.info && prop.opencnl && !prop.readInfo)
            {
                Request(requests.infoReq, 19);

                if (LastRequestOK)
                {
                    int snNum = 0;
                    int multip = 1;

                    for (int d = 4; d > 0; d--)
                    {
                        int ch = Convert.ToInt32(inBuf[d]);
                        snNum = (ch * multip) + snNum;
                        multip = ch > 99 ? multip *= 1000 : multip *= 100;
                    }
                    prop.serial = snNum;                                           // Сохраняем серийный номер
                    prop.made = new DateTime(2000 + inBuf[7], inBuf[6], inBuf[5]); // Сохраняем дату изготовления
                    prop.Aconst = ConstA(inBuf[12]);                               // Сохраняем постоянну счетчика имп/квт*ч

                    prop.saveParam.serial = snNum.ToString();
                    prop.saveParam.madeDt = prop.made.ToString("dd.MM.yyyy");
                    prop.saveParam.constA = prop.Aconst.ToString();

                    SaveSettings();
                }
                else
                {
                    // Данные со счетчика не считаны, чтение профилей средней мощности невозможно (заделка на будущее)
                }
                prop.readInfo = true;
            }

            // Запрос коэффициентов трансформации напряжения и тока
            if (prop.opencnl && !prop.Kui)
            {
                Request(requests.kuiReq, 7);

                if (LastRequestOK)
                {
                    prop.Kui = true;
                    prop.ku = Convert.ToInt32(BitFunc.ROR(BitConverter.ToUInt16(inBuf, 1), 8));
                    prop.ki = Convert.ToInt32(BitFunc.ROR(BitConverter.ToUInt16(inBuf, 3), 8));
                    prop.parkui = new int[] { prop.ki, prop.ki, prop.ki, 1, prop.ku, prop.ki, 1, 1, prop.ki, prop.ki, prop.ki, prop.ki, prop.ki };
                }
            }

            // При закрытом канале связи не опрашивать счетчик и выйти, выставив все в невалидное состояние
            if (!prop.opencnl)
            {
                DeviceData.Invalidate();
            }
            else
            {
                // ------------Получить мгновенные значения P,Q,S,U,I вариант 2
                if (readPQSUI != 0)
                {
                    if (readparam == "14h") // При чтении параметром 16h не нужна фиксация данных
                    {
                        if (devTemplate.multicast)
                        {
                            DateTime datetime = DateTime.Now;

                            if (prop.firstKp == DeviceNum)
                            {
                                // запись для всех КП времени фиксации и посылка команды фиксации данных
                                for (int x = 0; x < LineContext.SharedData.Count; x++)
                                {
                                    try
                                    {
                                        var Val = (MyDevice)LineContext.SharedData.ElementAt(x).Value;
                                        Val.dt = datetime;
                                    }
                                    catch { }
                                }
                                Write(requests.fixDataReq);
                                Thread.Sleep(PollingOptions.Delay);
                            }
                            else
                            {
                                // Тут сравнение времени фиксации и при необходимости отправка команды фиксации данных
                                if (datetime.Subtract(prop.dt).TotalSeconds > fixTime)
                                {
                                    Write(requests.fixDataReq);
                                    Thread.Sleep(PollingOptions.Delay);
                                }
                            }
                        }
                        else // используем команду фиксации по адресу счетчика
                        {
                            Request(requests.fixDataReq, 4);
                        }
                    }

                    // --------- формирование запросов P,Q,S,U,I и энергия от сброса при параметре 14h или 16h
                    for (int f = 0; f < nbwri.Length; f++)
                    {
                        int bwrim = nbwri[f] & 0xf0;

                        requests.dataReq = Protocol.DataReq(NumAddress, readparam, nbwri[f]);

                        Request(requests.dataReq, nb_length[f]);
                        int quantity = channels[DeviceTags[tagIndex].Code].quantity; // Количество активных каналов в текущем запросе, проверенных на этапе создания каналов

                        if (LastRequestOK)
                        {
                            int idxP = 0;
                            int znx = 1;
                            for (int zn = 0; zn < nparc[f]; zn++) // nparc[f] количество параметров в ответе для разных запросов.
                            {

                                if (channels[DeviceTags[tagIndex].Code].idxPar == idxP)
                                {
                                    byte[] zn_temp = new byte[4];
                                    uint znac_temp; // = 0; было

                                    if (nparb[f] == 4)
                                    {
                                        Array.Copy(inBuf, znx, zn_temp, 0, nparb[f]);                       // Копирование количества байт nparb[f] во временный буфер
                                        znac_temp = BitConverter.ToUInt32(zn_temp, 0);

                                        znac_temp = BitFunc.ROR(znac_temp, 16);
                                        if (nbwri[f] == 0x00 && (znac_temp & 0x80000000) >= 1) Napr = -1;   // определение направления Активной   мощности
                                        if (nbwri[f] == 0x04 && (znac_temp & 0x40000000) >= 1) Napr = -1;   // определение направления Реактивной мощности
                                        if (bwrim != 0xf0) znac_temp = znac_temp & 0x3fffffff;              // наложение маски для удаления направления для получения значения
                                    }
                                    else
                                    {
                                        Array.Copy(inBuf, znx, zn_temp, 0, 1);
                                        Array.Copy(inBuf, znx + 1, zn_temp, 2, 2);
                                        znac_temp = BitConverter.ToUInt32(zn_temp, 0);

                                        znac_temp = BitFunc.ROR(znac_temp, 16);
                                        if (nbwri[f] == 0x00 && (znac_temp & 0x800000) >= 1) Napr = -1;   // определение направления Активной   мощности
                                        if (nbwri[f] == 0x04 && (znac_temp & 0x400000) >= 1) Napr = -1;   // определение направления Реактивной мощности
                                        if (bwrim != 0xf0) znac_temp = znac_temp & 0x3fffff;              // наложение маски для удаления направления для получения значения
                                        if (nbwri[f] == 0x30) znac_temp = znac_temp & 0x3ff;              // наложение маски на 3-х байтовую переменную косинуса
                                    }

                                    if (znac_temp == 0xffffffff && nparb[f] == 4)
                                    {
                                        //DeviceData.Invalidate(tag, 1);
                                        DeviceData.Invalidate(DeviceTags[tagIndex].Code, 1);
                                    }
                                    else
                                    {
                                        //получение значения с учетом разрещшающей способности
                                        double znac = Convert.ToDouble(znac_temp) / nbwrc[f] * prop.parkui[f];
                                        // Значение умножается на множитель, если его нет, то он равен 1

                                        //DeviceData.Set(tag, znac * Napr * SToDouble(channels[DeviceTags[tag].Code].range), 1); 

                                        DeviceData.Set(DeviceTags[tagIndex].Code, znac * Napr * SToDouble(channels[DeviceTags[tagIndex].Code].range), 1); // TEST

                                        Napr = 1;
                                    }
                                    tagIndex++;
                                }
                                idxP++; // увеличиваем индекс тега в группе
                                znx = znx + nparb[f];
                            }
                        }
                        else
                        {
                            //prop.opencnl = false; // Надо ли ?
                            //DeviceData.Invalidate(tag, quantity);

                            DeviceData.Invalidate(DeviceTags[tagIndex].Code, quantity); // TEST

                            tagIndex = tagIndex + quantity;
                        }
                    }
                }

                //------------Получить пофазные значения накопленной энергии прямого направления  код запросв 0x05, параметр 0x60
                if (energyL != 0)
                {
                    for (int f = 0; f < nenergy.Length; f++)
                    {
                        requests.energyPReq = Protocol.EnergyPReq(NumAddress, nenergy[f]);
                        Request(requests.energyPReq, 15);
                        int quantity = channels[DeviceTags[tagIndex].Code].quantity; // Количество активных каналов в текущем запросе, проверенных на этапе создания каналов

                        // Тут проверка ответа на корректность и разбор значений
                        if (LastRequestOK)
                        {
                            int idxP = 0;
                            int znx = 1;
                            for (int z = 0; z < 3; z++)
                            {
                                if (channels[DeviceTags[tagIndex].Code].idxPar == idxP)
                                {
                                    uint znac_temp = BitConverter.ToUInt32(inBuf, znx);
                                    znac_temp = BitFunc.ROR(znac_temp, 16);
                                    double znac = Convert.ToDouble(znac_temp) / 1000 * prop.ki * prop.ku; // TEST добавлен ku

                                    DeviceData.Set(DeviceTags[tagIndex], znac, 1); // TEST вместо tag
                                    tagIndex++;
                                }
                                idxP++;
                                znx = znx + 4;
                            }
                        }
                        else
                        {
                            DeviceData.Invalidate(DeviceTags[tagIndex].Code, quantity); // 3   TEST вместо tag
                            tagIndex = tagIndex + quantity;
                        }
                    }
                }

                //Чтение профилей мощностей - Вид энергии 0 (A +, A -, R +, R -)
                if (ActiveProfile)
                {
                    DateTime dt = DateTime.Now;
                    if (dt.Subtract(prop.srezDt) > TimeSpan.FromMinutes(srezPeriod) || firstAProfile)
                    {
                        if (profile.Count > 0)
                        {
                            Request(Protocol.WriteCompReq(NumAddress, 0x08, 0x13), 12); // Чтение последней записи среза мощностей
                            if (LastRequestOK && ToLogString(code) == "")
                            {
                                // -------Определить дату последней записи профиля средних мощностей ---------
                                dt = new DateTime(2000 + (int)ConvFunc.BcdToDec(new byte[] { inBuf[8] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[7] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[6] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[4] }), (int)ConvFunc.BcdToDec(new byte[] { inBuf[5] }), 0);

                                srezPeriod = inBuf[9]; // Проверка периода среза и запись значения в переменную, по умолчанию 30 минут

                                // Последняя ячейка памяти записи средней мощности
                                byte[] NumCell = new byte[2];
                                Array.Copy(inBuf, 1, NumCell, 0, 2);
                                Array.Reverse(NumCell);
                                // Адрес последней ячейки памяти
                                int ramstart = BitConverter.ToUInt16(NumCell, 0) * 16;

                                // Пока только Вид энергии 0 (A+, A-, R+, R-)
                                requests.readRomReq = Protocol.ReadRomReq(NumAddress, 0, 3, ramstart, 15); // прочитать последнюю запись, 15 байт
                                Request(requests.readRomReq, 18); // Чтение ROM

                                if (LastRequestOK)
                                {
                                    DateTime nowDt = DateTime.Now;
                                    // Обработка профилей мощности
                                    int znx = 8;
                                    // Определение статуса среза средниих мощностей false = Архивный (Полный срез), true = Неполный срез, номер задается
                                    // параметром шаблона halfArchStat, необходимо предварительно создать Номер и цвет в Проект - Вспомогательные таблицы - Статусы каналов 
                                    bool halfSrez = (inBuf[1] & 0x02) > 0;
                                    int cnlStat = halfSrez ? halfArch : 2;


                                    // TEST TEST 

                                    //Log.WriteLine($" {nowDt.Subtract(dt) > TimeSpan.FromMinutes(0)}    {nowDt.Subtract(dt) < TimeSpan.FromMinutes(srezPeriod)}");

                                    // TEST TEST 


                                    //if (nowDt.Subtract(dt) > TimeSpan.FromMinutes(0) && (nowDt.Subtract(dt) < TimeSpan.FromMinutes(srezPeriod)))
                                    //{
                                    double second = saveTime != 0 ? nowDt.Subtract(dt).TotalSeconds : 0;
                                    DateTime writeDt = dt;
                                    double tme = 0;
                                    string archMask = string.IsNullOrEmpty(devTemplate.ArchMask) ? "" : devTemplate.ArchMask;

                                    bool dataset = false;
                                    if (archMask == "")
                                    {
                                        // Колхоз
                                        do
                                        {
                                            // Формируем новый архивный срез по количеству параметров прибора
                                            DateTime sliceDT = TimeZoneInfo.ConvertTimeToUtc(writeDt.AddSeconds(tme));
                                            DeviceSlice slice = new DeviceSlice(sliceDT, profile.Count, profile.Count);

                                            for (int pf = 0; pf < profile.Count; pf++)
                                            {
                                                // считаем среднее мощности согласно формуле раздела 2.4
                                                double prof = (double)BitConverter.ToUInt16(inBuf, znx + profile[pf].offset) * (60 / inBuf[7]) / (2 * prop.Aconst);

                                                slice.DeviceTags[pf] = DeviceTags[profile[pf].code];
                                                slice.CnlData[pf] = new CnlData(prof * profile[pf].range, cnlStat);

                                                // Запись в текущие промежутки последней записи если время saveTime не равно 0, тогда запись в точку времени среза
                                                if (!dataset) DeviceData.Set(DeviceTags[profile[pf].code], prof * profile[pf].range, cnlStat); // Надо по коду тега ????? DeviceData.Set(DeviceTags[profile[pf].code].DataIndex, prof * profile[pf].range, cnlStat)
                                            }
                                            dataset = true; // Отправляем DeviceData.Set один раз
                                            slice.Descr = "Запись ср. мощностей " + nowDt.ToString();
                                            DeviceData.EnqueueSlice(slice);

                                            tme = tme + saveTime;
                                        }
                                        while (tme < second);
                                    }
                                    else
                                    {
                                        // Формируем новый архивный срез по количеству параметров прибора
                                        DateTime sliceDT = TimeZoneInfo.ConvertTimeToUtc(writeDt);
                                        DeviceSlice slice = new DeviceSlice(sliceDT, profile.Count, profile.Count);
                                        slice.ArchiveMask = int.Parse(devTemplate.ArchMask); // Пишем в архивы по маске ?????

                                        for (int pf = 0; pf < profile.Count; pf++)
                                        {
                                            // считаем среднее мощности согласно формуле раздела 2.4
                                            double prof = (double)BitConverter.ToUInt16(inBuf, znx + profile[pf].offset) * (60 / inBuf[7]) / (2 * prop.Aconst);
                                            slice.DeviceTags[pf] = DeviceTags[profile[pf].code];
                                            slice.CnlData[pf] = new CnlData(prof * profile[pf].range, cnlStat);

                                            DeviceData.Set(DeviceTags[profile[pf].code], prof * profile[pf].range, cnlStat); // TEST Текущие данные профилей средних мощностей DeviceTags[profile[pf].code].DataIndex
                                        }

                                        slice.Descr = "Запись ср. мощностей " + nowDt.ToString();
                                        DeviceData.EnqueueSlice(slice);
                                    }

                                    firstAProfile = false;
                                    //Сохранить время последнего считанного архива в список сохраняемых параметров
                                    prop.srezDt = dt;
                                    prop.saveParam.arcDt = prop.srezDt.ToString(); // prop.srezDt.ToString()
                                    SaveSettings();
                                    //}
                                }
                                //else
                                //{
                                //    if (inBuf[1] == 0x05) prop.opencnl = false;
                                //}
                            }
                            else
                            {
                                string message = string.Format(Locale.IsRussian ?
                                            "Ошибка: {0}" :
                                            "Error: {0}", ToLogString(code));
                                Log.WriteLine(message);

                                if (inBuf[1] == 0x05)
                                {
                                    prop.opencnl = false;
                                }
                            }
                        }
                    }
                }

                // У статуса и коэффициентов фиксированные номера сигналов
                // Статус передаем всегда на случай потери связи
                // при потери связи переменная открытия канала сбрасывается
                // Чтение последней записи журнала кода состояния прибора
                if (readstatus) DeviceData.Set("error", code, 1);

                if (readstatus && prop.opencnl)
                {
                    DeviceData.Set("kI", prop.ki, 1);
                    DeviceData.Set("kU", prop.ku, 1);

                    Request(requests.wordStatReq, 16);
                    if (LastRequestOK)
                    {
                        Array.Copy(inBuf, 7, wordStat, 4, 2);
                        Array.Copy(inBuf, 9, wordStat, 2, 2);
                        Array.Copy(inBuf, 11, wordStat, 0, 2);
                        DeviceData.Set("wordSt", BitConverter.ToUInt64(wordStat, 0), 1);
                    }
                    else
                    {
                        DeviceData.Invalidate("wordSt", 1);
                    }
                }

                bool change = chan_err(code);

                if (readstatus && change)
                {
                    // генерация события
                    DeviceEvent status = new DeviceEvent(DeviceTags["error"]);

                    int even = 21;               // Зеленый цвет события
                    status.Descr = "Связь восстановлена"; // в логе Коммуникатора
                    status.Text = "Связь восстановлена";

                    if (code != 0)
                    {
                        even = 23;    // Красный цвет события
                        status.Descr = "Потеря связи";  // в логе Коммуникатора
                        status.Text = "Потеря связи";
                    }

                    status.Ack = false;
                    status.TextFormat = EventTextFormat.CustomText; // EventTextFormat.CustomText
                    status.Timestamp = DateTime.UtcNow;
                    status.CnlStat = even;
                    status.CnlVal = code;

                    DeviceData.EnqueueEvent(status);
                    code_err = code;
                }
            }

            LineContext.SharedData[address] = prop; // записать данные в общие свойства

            stopwatch.Stop();
            Log.WriteLine(Locale.IsRussian ?
                "Получено за {0} мс" :
                "Received in {0} ms", stopwatch.ElapsedMilliseconds);

            //FinishRequest(); // А нужен ли он тут ?????
            FinishSession();
        }
        #endregion Session


        #region SendCommand
        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public override void SendCommand(TeleCommand cmd)
        {
            base.SendCommand(cmd);

            LastRequestOK = false;
            MyDevice prop = (MyDevice)LineContext.SharedData[address]; // получить данные из общих свойств

            string cmdCode = cmd.CmdCode;

            if (channels.ContainsKey(cmdCode)) // cmdNum
            {
                // Определить уровень команды, пользовательский или администратора
                // Если основной уровень 1 - пользователь, а команда с уровнем 2 - Админ, 
                // закрываем канал, открываем с уровнем администратора
                if (Level != "2" && channels[cmdCode].Mode == 2 || Level.ToLower() != "admin" && channels[cmdCode].Mode == 2) // cmdNum
                {
                    Request(requests.closeCnlReq, 4); // закрытие канала
                    if (LastRequestOK)
                    {
                        // Изменить атрибут открытого канала на false
                        prop.opencnl = false;

                        Request(requests.openAdmReq, 4);
                        if (LastRequestOK)
                        {
                            // Выполнить команду - по сути изменить параметр что канал открыт
                            prop.opencnl = true;
                        }
                    }
                }

                if (prop.opencnl) // Если канал открыт, выполняем команду
                {
                    if (!double.IsNaN(cmd.CmdVal))
                    {
                        Request(channels[cmdCode].setCommand, channels[cmdCode].scCnt, string.Format(Locale.IsRussian ? "Отправка команды {0}" : "Sending a command {0}", channels[cmdCode].Name));

                        if (LastRequestOK && ToLogString(code) == "")
                        {
                            Log.WriteLine(string.Format(Locale.IsRussian ?
                                "OK Команда {0} Выполнена успешно" :
                                "OK Command {0} completed successfully", channels[cmdCode].Name));
                        }
                        else
                        {
                            Log.WriteLine(string.Format(Locale.IsRussian ?
                                "Ошибка Команда {0} не выполнена" :
                                "Error Command {0} not executed", channels[cmdCode].Name));
                        }
                    }
                    if (cmd.CmdData != null && cmd.CmdData.Length != 0)
                    {
                        byte[] bindata = new byte[cmd.CmdData.Length];
                        bindata = cmd.CmdData;

                        DeviceEvent status = new DeviceEvent(DeviceTags["error"]);
                        int even = 21; // Зеленый цвет события GOOD

                        if (bindata.Length == channels[cmdCode].datalen)
                        {
                            // Индекс начала блока данных в запросе
                            int startbindat = channels[cmdCode].setCommand.Length - bindata.Length - 2;
                            byte[] DataBin = new byte[channels[cmdCode].setCommand.Length];
                            Array.Copy(channels[cmdCode].setCommand, 0, DataBin, 0, startbindat);
                            Array.Copy(cmd.CmdData, 0, DataBin, startbindat, cmd.CmdData.Length);
                            ushort res = CrcFunc.CalcCRC16(DataBin, DataBin.Length - 2);
                            DataBin[DataBin.Length - 2] = (byte)(res % 256);                // добавить контрольную сумму к буферу посылки
                            DataBin[DataBin.Length - 1] = (byte)(res / 256);

                            Request(DataBin, channels[cmdCode].scCnt, string.Format(Locale.IsRussian ? "Отправка команды {0}" : "Sending a command {0}", channels[cmdCode].Name));

                            if (LastRequestOK && ToLogString(code) == "")
                            {
                                string message = string.Format(Locale.IsRussian ?
                                    "OK Команда {0} выполнена успешно" :
                                    "OK Command {0} completed successfully", channels[cmdCode].Name);
                                Log.WriteLine(message);

                                status.Descr = message; // в логе Коммуникатора
                                status.Text = message;
                            }
                            else
                            {
                                string message = string.Format(Locale.IsRussian ?
                                    "Ошибка Команда {0} не выполнена, {1}" :
                                    "Error Command {0} not executed, {1}", channels[cmdCode].Name, ToLogString(code));
                                Log.WriteLine(message);
                                even = 23;    // Красный цвет события
                                status.Descr = message;  // в логе Коммуникатора
                                status.Text = message;
                                LastRequestOK = false; // При ошибке на команду для расчета ????????? TEST TEST
                            }
                        }
                        else
                        {
                            string message = string.Format(Locale.IsRussian ?
                                "Ошибка длина блока данных должен быть равна {0}" :
                                "Error the length of the data block must be equal to = {0}", channels[cmdCode].datalen);
                            Log.WriteLine(message);
                            even = 23;    // Красный цвет события
                            status.Descr = message;  // в логе Коммуникатора
                            status.Text = message;
                        }

                        status.Ack = false;
                        status.TextFormat = EventTextFormat.CustomText; // EventTextFormat.CustomText
                        status.Timestamp = DateTime.UtcNow;
                        status.CnlStat = even;
                        status.CnlVal = code;

                        DeviceData.EnqueueEvent(status);
                    }
                }
            }

            if (Level != "2" && channels[cmdCode].Mode == 2 || Level.ToLower() != "admin" && channels[cmdCode].Mode == 2)
            {
                // После выполнения команды Администратором закрываем канал
                // в следующей сессии канал откроется с заданным уровнем доступа
                Request(requests.closeCnlReq, 4); // закрытие канала
                if (LastRequestOK)
                {
                    prop.opencnl = false;
                    LineContext.SharedData[address] = prop;
                }
            }
            FinishCommand();
        }

        #endregion SendCommand

        private class Requests
        {
            public byte[] testCnlReq;   // запрос тестирования канала
            public byte[] openCnlReq;   // запрос на открытие канала
            public byte[] openAdmReq;   // Запрос открытия канала с Административным паролем
            public byte[] closeCnlReq;  // Запрос на закрытие канала
            public byte[] readTimeReq;  // Запрос чтения времени
            public byte[] lastSyncReq;  // Запрос времени последней синхронизации
            public byte[] fixDataReq;   // запрос на фиксацию данных
            public byte[] dataReq;      // запрос чтения данных
            public byte[] energyPReq;   // запрос на чтение пофазной энергии А+ (0x05 0x60 тариф)
            public byte[] kuiReq;       // запрос значений трансформации тока и напряжения
            public byte[] infoReq;      // Запрос информации счетчика в ускоренном режиме (0x08 0x01) - 16 байт
            public byte[] readRomReq;   // Запрос на чтение информации по физическим адресам памяти (0x06)
            public byte[] curTimeReq;   // Зпапрос текущего времени 2.1 Запросы на чтение массивов времен (код 0x04 параметр 0x00)
            public byte[] wordStatReq;  // Запрос словосостояния счетчика

            public Requests()
            {
            }
        }

        private void tagcreate(string nameMenu, int bit)
        {
            int idgr = devTemplate.SndGroups.FindIndex(f => f.Bit == bit); // Ищем индекс с соответствующим битом группы
            // Перенести сюда количество активных параметров и вместо переменной bit сделать переменную quantity для группы ?
            var actQuant = from c in devTemplate.SndGroups[idgr].value where c.active == true select c;
            int quantity = actQuant.Count();

            for (int i = 0; i < devTemplate.SndGroups[idgr].value.Count; i++)
            {
                if (devTemplate.SndGroups[idgr].value[i].active) // Сохраняем только активные сигналы
                {
                    channels.Add(devTemplate.SndGroups[idgr].value[i].code,
                        new CnlPrototypeFactory.ActiveChannel()
                        {
                            GroupName = nameMenu,
                            Name = $"{devTemplate.SndGroups[idgr].Name} {devTemplate.SndGroups[idgr].value[i].name}", // Полное имя параметра Имя группы + Имя тега
                            Code = devTemplate.SndGroups[idgr].value[i].code,
                            Mode = 1,
                            CnlType = CnlTypeID.Input,
                            range = string.IsNullOrEmpty(devTemplate.SndGroups[idgr].value[i].multiplier) ? "1" : devTemplate.SndGroups[idgr].value[i].multiplier,
                            idxPar = i,
                            quantity = quantity // В каждой переменной группы указываем quantity хотя достаточно для первой
                        });
                }
            }
        }

        private double SToDouble(string s)
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
        /// Загрузить из файла даты последнего архива и время запроса архивов
        /// </summary>
        private void LoadSettings()
        {
            MyDevice prop = (MyDevice)LineContext.SharedData[address];
            string fname = $"{AppDirs.StorageDir}Mercury23x_L{LineContext.CommLineNum:D3}_A{NumAddress:D3}.xml";

            try
            {
                prop.saveParam = FileFunc.LoadXml(typeof(SaveParam), fname) as SaveParam;
                if (prop.saveParam != null) settLoaded = true;
            }
            catch (Exception err)
            {
                Log.WriteLine(string.Format(Locale.IsRussian ?
                "Не найден файл настроек: " + err.Message :
                "No settings file found: " + err.Message));
            }
        }

        /// <summary>
        /// Выполнить действия при завершении работы линии связи
        /// </summary>
        public override void OnCommLineTerminate()
        {
            // если все срезы переданы и загрузка настроек выполнена, сохранить время запроса архивов в файле
            if (profile.Count > 0)
                SaveSettings();
        }

        /// <summary>
        /// Сохранить в файле время запроса последнего архивов
        /// </summary>
        private void SaveSettings()
        {

            MyDevice prop = (MyDevice)LineContext.SharedData[address];
            // Номер линии связи, Number - Номер Устройства, Address - Адрес прибора
            string fname = $"{AppDirs.StorageDir}Mercury23x_L{LineContext.CommLineNum:D3}_A{NumAddress:D3}.xml";
            try
            {
                FileFunc.SaveXml(prop.saveParam, fname);
            }
            catch (Exception err)
            {
                Log.WriteLine(string.Format(Locale.IsRussian ?
                "Ошибка при сохранении настроек в файле: " + err.Message :
                "Error when saving settings in a file: " + err.Message));
            }
        }

        private void Request(byte[] request, int Cnt, string descr = null)
        {
            int tryNum = 0;
            if (CommSucc) // обработчик ошибок, который не связан с ошибками CRC
            {
                LastRequestOK = false;
                while (RequestNeeded(ref tryNum))
                {
                    if (descr != null) Log.WriteLine(descr); // Если есть описание, выводим в лог
                    string logText;
                    WriteRequest(request, out logText);
                    Log.WriteLine(logText);

                    ReadAnswer(inBuf, Cnt, out logText);
                    Log.WriteLine(logText);

                    checkCRC(Cnt);

                    if (!LastRequestOK)
                    {
                        Thread.Sleep(PollingOptions.Delay);
                    }
                    FinishRequest();
                    tryNum++;
                }
            }
        }

        /// <summary>
        /// Считать ответ из последовательного порта
        /// </summary>
        private void ReadAnswer(byte[] buffer, int count, out string logText)
        {
            read_cnt = Connection.Read(buffer, 0, count, PollingOptions.Timeout, ProtocolFormat.Hex, out logText);
        }


        /// <summary>
        /// Отправить запрос и сделать запись в журнал
        /// </summary>
        private void Write(byte[] request)
        {
            string logText;
            WriteRequest(request, out logText);
            Log.WriteLine(logText);
        }

        private void WriteRequest(byte[] request, out string logText)
        {
            Connection.Write(request, 0, request.Length, ProtocolFormat.Hex, out logText);
        }

        private void checkCRC(int count)
        {
            if (read_cnt >= 4)
            {
                ushort crc = CrcFunc.CalcCRC16(inBuf, read_cnt);
                if (crc == 0)
                {
                    if (read_cnt > 4)
                    {
                        code = 0x00;
                        Log.WriteLine(CommPhrases.ResponseOK);
                    }
                    else if (read_cnt == 4)
                    {
                        if (read_cnt == count && inBuf[1] != 0x00)
                        {
                            code = inBuf[1];
                        }
                        else
                        {
                            code = inBuf[1];
                            Log.WriteLine(CommPhrases.ResponseOK);
                        }
                    }
                    LastRequestOK = true;
                    CommSucc = true;
                }
                else Log.WriteLine(CommPhrases.ResponseCrcError);
            }
            else
            {
                Log.WriteLine(CommPhrases.ResponseError);
            }
        }

    }
}
