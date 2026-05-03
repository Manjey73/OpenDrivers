using Scada.Comm.Channels;
using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Lang;
using Scada.Lang;
using ScadaCommFunc;
using System.Diagnostics;

namespace Scada.Comm.Drivers.DrvBetarV12.Logic
{
    internal class DevBetarV12Logic : DeviceLogic
    {
        #region Variable
        private byte[] inBuf = new byte[19];  // the input buffer
        private int read_cnt;
        private byte[] reqAddr;
        #endregion Variable

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevBetarV12Logic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            CanSendCommands = true;
            ConnectionRequired = true;
        }

        public override void InitDeviceTags()
        {
            TagGroup tagGroup = new TagGroup($"Счетчик {NumAddress.ToString()}"); // Создание меню Тегов последовательно по списку Сигналов

            DeviceTag deviceTag = tagGroup.AddTag("directflow", "Пок. прямого потока");
            deviceTag = tagGroup.AddTag("reverseflow", "Пок. обратного потока");
            deviceTag = tagGroup.AddTag("magnetictime", "Время магн. воздействия");
            deviceTag = tagGroup.AddTag("servicebyte", "Служебный байт");

            DeviceTags.AddGroup(tagGroup); // Инициализация всех тегов // Теперь не нужно так как на автомате?
        }


        /// <summary>
        /// Выполнить действия при запуске линии связи
        /// </summary>
        public override void OnCommLineStart()
        {

            reqAddr = ReadReq(NumAddress); 
            //Log.WriteLine($"NumAddress {NumAddress}"); // TEST TEST
        }


        /// <summary>
        /// Performs a communication session.
        /// </summary>
        public override void Session()
        {
            base.Session();
            Stopwatch stopwatch = Stopwatch.StartNew();

            Request(reqAddr);

            if (LastRequestOK)
            {
                DeviceData.Set("directflow", GetValue(5));
                DeviceData.Set("reverseflow", GetValue(9));
                DeviceData.Set("magnetictime", GetMagneticTime(13)/3600); // Время в секундах /60 = минуты, /60 = часы
                DeviceData.Set("servicebyte", GetService(17)); // 00 - нет магнитного воздействия, 01 - наличие магнитного воздействия
            }
            else
            {
                DeviceData.Invalidate();
            }

            stopwatch.Stop();
            Log.WriteLine(Locale.IsRussian ?
                "Получено за {0} мс" :
                "Received in {0} ms", stopwatch.ElapsedMilliseconds);

            FinishSession();
        }

        private double GetValue(int offset)
        {
            byte[] val = new byte[4];
            Array.Copy(inBuf, offset, val, 0, 4);
            Array.Reverse(val);
            double zDec = ((double)ConvFunc.BcdToDec(val)) / 1000;

            //Log.WriteLine($"Значение Dec = {zDec}"); // TEST TEST

            return zDec;
        }

        private double GetMagneticTime(int offset)
        {
            double zDec = BitConverter.ToUInt32(inBuf, offset);

            //Log.WriteLine($"Значение Magnetic = {zDec}"); // TEST TEST
            return zDec;
        }

        private double GetService(int offset)
        {
            double zDec = inBuf[offset];

            //Log.WriteLine($"Значение Dec = {zDec}");  // TEST TEST
            return zDec;
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

                ReadAnswer(inBuf, 19, out logText);
                Log.WriteLine(logText);

                // TEST
                //read_cnt = 19;
                //inBuf = [0x5A, 0x00, 0xBC, 0x61, 0x4E, 0x91, 0x45, 0x08, 0x00, 0x67, 0x23, 0x02, 0x00, 0x28, 0x23, 0x00, 0x00, 0x00, 0x20];
                //Log.WriteLine(BuildReadLogText(inBuf, 0, 19, ProtocolFormat.Hex));
                // TEST

                checkRead();

                FinishRequest();
                tryNum++;
            }
        }

        private void WriteRequest(byte[] request, out string logText)
        {
            Connection.Write(request, 0, request.Length, ProtocolFormat.Hex, out logText);
        }

        /// <summary>
        /// Считать ответ из последовательного порта
        /// </summary>
        private void ReadAnswer(byte[] buffer, int count, out string logText)
        {
            read_cnt = Connection.Read(buffer, 0, count, PollingOptions.Timeout, ProtocolFormat.Hex, out logText);
        }

        private void checkRead()
        {
            if (read_cnt == 19)
            {
                if (inBuf[18] == CrcFunc.Crc8(inBuf, 1, inBuf.Length-2))
                {
                        LastRequestOK = true;
                        Log.WriteLine(CommPhrases.ResponseOK);
                }
                else
                {
                    Log.WriteLine(CommPhrases.ResponseCsError);
                }
            }
            else
            {
                Log.WriteLine(CommPhrases.ResponseError);
            }
        }

        private byte[] ReadReq(int devAddr)
        {
            byte[] readCnl = new byte[7];
            readCnl[0] = 0xCD;
            byte[] numMass = BitConverter.GetBytes(devAddr);
            Array.Reverse(numMass);
            Array.Copy(numMass, 0, readCnl, 1, numMass.Length);
            readCnl[readCnl.Length - 2] = 0x71;
            int res = CrcFunc.Crc8(readCnl, 1, readCnl.Length - 2);                //получить контрольную сумму
            readCnl[readCnl.Length - 1] = (byte)res;
            return readCnl;
        }

        /// <summary>
        /// Builds a log text about reading data.
        /// </summary>
        protected static string BuildReadLogText(byte[] buffer, int offset, int readCnt, ProtocolFormat format)
        {
            return $"{CommPhrases.ReceiveNotation} ({readCnt}): " +
                ScadaUtils.BytesToString(buffer, offset, readCnt, format == ProtocolFormat.Hex);
        }

    }
}
