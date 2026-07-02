
namespace ScadaCommFunc
{
    public class CrcFunc
    {

        /// <summary>
        /// Рассчет однобайтовой контрольной суммы (применяется в MBus)
        /// Приборах АО НПФ Логика СПТ941, СПТ941.10/11, СПТ943
        /// с последующим инвертированием байта - применить в коде (byte)(~crc)
        /// Вызов Crc8 с передачей буфера байт в функцию
        /// </summary>
        /// <param name="bval"></param>
        /// <param name="cval"></param>
        /// <returns></returns>

        private static int F_crc_8(int bval, int cval)
        {
            return (bval + cval) % 256;
        }

        public static int Crc8(byte[] buffer, int offset = 0, int count = 0) // Расчет CRC по модулю 256
        {
            if (count == 0) count = buffer.Length;
            int crc = buffer[offset];
            for (int i = offset, last = offset + count; i < last - 1; i++)
            {
                crc = F_crc_8(crc, buffer[i + 1]);
            }
            return crc;
        }

        /// <summary>
        /// Рассчет CRC16/XMODEM - применяется в приборах АО НПФ Логика 
        /// Тепловычислители СПТ961, СПТ961М, СПТ961.1, СПТ961.2, СПТ962, СПТ963
        /// Корректоры расхода газа СПГ761, СПГ761.1, СПГ761.2, СПГ762, СПГ762.1, СПГ762.2, СПГ763, СПГ763.1, СПГ763.2
        /// Сумматоры электрической энергии и мощности СПЕ542
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ushort CRC16_XModem(byte[] msg, int offset = 0, int count = 0) // добавлен offset и count
        {
            const ushort polinom = 0x1021;
            ushort code = 0x0000;
            if (count == 0) count = msg.Length;                                     // добавлен

            for (int i = offset, last = count + offset; i < last; ++i)              // msg.Length убрать ofsett
            {
                code ^= (ushort)(msg[i] << 8);
                for (uint j = 0; j < 8; ++j)
                {
                    if ((code & 0x8000) != 0) code = (ushort)((code << 1) ^ polinom);
                    else code <<= 1;
                }
            }
            return code;
        }

        /// <summary>
        /// Рассчет контрольной суммы
        /// CRC-16/ARC, CRC-16/IBM, CRC-16/DF1(Allen Bradley)
        /// циклический код с полиномом
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ushort CRC16_IBM(byte[] msg)
        {
            const ushort polinom = 0xA001;
            ushort code = 0x0000;
            for (int i = 0, size = msg.Length; i < size; ++i)
            {
                code ^= msg[i];
                for (uint j = 0; j < 8; ++j)
                {
                    if ((code & 0x0001) > 0)
                    {
                        code >>= 1;
                        code ^= polinom;
                    }
                    else code >>= 1;
                }
            }
            return code;
        }

        /// <summary>
        /// Рассчет контрольной суммы CRC16 Modbus, циклический код с полиномом
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ushort CRC16_Modbus(byte[] msg)
        {
            const ushort polinom = 0xa001;
            ushort code = 0xffff;

            for (int i = 0, size = msg.Length; i < size; ++i)
            {
                code ^= (ushort)(msg[i] << 8);

                for (uint j = 0; j < 8; ++j)
                {
                    code >>= 1;
                    if ((code & 0x01) != 0) code ^= polinom;
                }
            }
            return code;
        }


        /// <summary>
        /// Рассчет контрольной суммы 
        /// CRC-16/ARC, CRC-16/IBM, CRC-16/DF1(Allen Bradley)
        /// табличный вариант
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>

        public static ushort CRC16_DF1(byte[] buffer, int length)
        {
            int offset = 0;
            byte crcHi = 0x00;   // high byte of CRC initialized
            byte crcLo = 0x00;   // low byte of CRC initialized
            int index;           // will index into CRC lookup table

            while (length-- > 0) // pass through message buffer
            {
                index = crcLo ^ buffer[offset++]; // calculate the CRC
                crcLo = (byte)(crcHi ^ CRCHiTable[index]);
                crcHi = CRCLoTable[index];
            }
            return (ushort)((crcHi << 8) | crcLo);
        }

        /// <summary>
        /// Рассчет контрольной суммы CRC16 Modbus, табличный вариант
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>

        public static ushort CalcCRC16(byte[] buffer, int length)
        {
            int offset = 0;
            byte crcHi = 0xFF;   // high byte of CRC initialized
            byte crcLo = 0xFF;   // low byte of CRC initialized
            int index;           // will index into CRC lookup table

            while (length-- > 0) // pass through message buffer
            {
                index = crcLo ^ buffer[offset++]; // calculate the CRC
                crcLo = (byte)(crcHi ^ CRCHiTable[index]);
                crcHi = CRCLoTable[index];
            }
            return (ushort)((crcHi << 8) | crcLo);
        }

        private readonly static byte[] CRCHiTable;
        private readonly static byte[] CRCLoTable;

        static CrcFunc()
        {
            CRCHiTable = new byte[] { 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64, 0, 193, 129, 64, 1, 192, 128, 65, 0, 193, 129, 64, 1, 192, 128, 65, 1, 192, 128, 65, 0, 193, 129, 64 };
            CRCLoTable = new byte[] { 0, 192, 193, 1, 195, 3, 2, 194, 198, 6, 7, 199, 5, 197, 196, 4, 204, 12, 13, 205, 15, 207, 206, 14, 10, 202, 203, 11, 201, 9, 8, 200, 216, 24, 25, 217, 27, 219, 218, 26, 30, 222, 223, 31, 221, 29, 28, 220, 20, 212, 213, 21, 215, 23, 22, 214, 210, 18, 19, 211, 17, 209, 208, 16, 240, 48, 49, 241, 51, 243, 242, 50, 54, 246, 247, 55, 245, 53, 52, 244, 60, 252, 253, 61, 255, 63, 62, 254, 250, 58, 59, 251, 57, 249, 248, 56, 40, 232, 233, 41, 235, 43, 42, 234, 238, 46, 47, 239, 45, 237, 236, 44, 228, 36, 37, 229, 39, 231, 230, 38, 34, 226, 227, 35, 225, 33, 32, 224, 160, 96, 97, 161, 99, 163, 162, 98, 102, 166, 167, 103, 165, 101, 100, 164, 108, 172, 173, 109, 175, 111, 110, 174, 170, 106, 107, 171, 105, 169, 168, 104, 120, 184, 185, 121, 187, 123, 122, 186, 190, 126, 127, 191, 125, 189, 188, 124, 180, 116, 117, 181, 119, 183, 182, 118, 114, 178, 179, 115, 177, 113, 112, 176, 80, 144, 145, 81, 147, 83, 82, 146, 150, 86, 87, 151, 85, 149, 148, 84, 156, 92, 93, 157, 95, 159, 158, 94, 90, 154, 155, 91, 153, 89, 88, 152, 136, 72, 73, 137, 75, 139, 138, 74, 78, 142, 143, 79, 141, 77, 76, 140, 68, 132, 133, 69, 135, 71, 70, 134, 130, 66, 67, 131, 65, 129, 128, 64 };
        }



        /// <summary>
        /// Расчет CRC протокола ОВЕН
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static ushort owenCRC16(char[] packet)
        {
            ushort crc = 0;
            for (int i = 0; i < packet.Length; ++i)
            {
                char b = packet[i];
                for (int j = 0; j < 8; ++j, b <<= 1)
                {
                    if (((b ^ (crc >> 8)) & 0x80) > 0)
                    {
                        crc <<= 1;
                        crc ^= 0x8F57;
                    }
                    else
                        crc <<= 1;
                }
            }
            return crc;
        }


        /// <summary>
        /// Рассчет контрольной суммы CRC16 X-25, циклический код с полиномом
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static ushort crc16_x25(byte[] data, int len)
        {
            ushort crc = 0xffff;
            for (int i = 0; i < len; i++)
            {
                crc ^= data[i];
                for (int k = 0; k < 8; k++)
                {
                    //crc = (crc & 1) != 0 ? (crc >> 1) ^ 0x8408 : crc >> 1;
                    if ((crc & 0x01) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0x8408;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }



        /**
            * Use the table check method to generate the CRC verification code according to the data
            * 
            * @param Message verification value, for bytes that need to be verified
            * @return 
       */
        public static ushort CRC_16_X25(byte[] message)
        {
            int crc_reg = 0xFFFF;// The initial value of CRC verification
            for (int i = 0; i < message.Length; i++)
            {
                crc_reg = UIntMoveRight(crc_reg, 8) ^ CRC16X25Table[(crc_reg ^ message[i]) & 0xff];
            }
            var res = ~crc_reg & 0xffff;
            //var reshas = res.ToString("X").ToUpper();
            return (ushort)res;
        }

        private static int[] CRC16X25Table = {
           0X0000, 0X1189, 0X2312, 0X329B, 0X4624, 0X57AD, 0X6536, 0X74BF,
           0X8C48, 0X9DC1, 0XAF5A, 0XBED3, 0XCA6C, 0XDBE5, 0XE97E, 0XF8F7,
           0X1081, 0X0108, 0X3393, 0X221A, 0X56A5, 0X472C, 0X75B7, 0X643E,
           0X9CC9, 0X8D40, 0XBFDB, 0XAE52, 0XDAED, 0XCB64, 0XF9FF, 0XE876,
           0X2102, 0X308B, 0X0210, 0X1399, 0X6726, 0X76AF, 0X4434, 0X55BD,
           0XAD4A, 0XBCC3, 0X8E58, 0X9FD1, 0XEB6E, 0XFAE7, 0XC87C, 0XD9F5,
           0X3183, 0X200A, 0X1291, 0X0318, 0X77A7, 0X662E, 0X54B5, 0X453C,
           0XBDCB, 0XAC42, 0X9ED9, 0X8F50, 0XFBEF, 0XEA66, 0XD8FD, 0XC974,
           0X4204, 0X538D, 0X6116, 0X709F, 0X0420, 0X15A9, 0X2732, 0X36BB,
           0XCE4C, 0XDFC5, 0XED5E, 0XFCD7, 0X8868, 0X99E1, 0XAB7A, 0XBAF3,
           0X5285, 0X430C, 0X7197, 0X601E, 0X14A1, 0X0528, 0X37B3, 0X263A,
           0XDECD, 0XCF44, 0XFDDF, 0XEC56, 0X98E9, 0X8960, 0XBBFB, 0XAA72,
           0X6306, 0X728F, 0X4014, 0X519D, 0X2522, 0X34AB, 0X0630, 0X17B9,
           0XEF4E, 0XFEC7, 0XCC5C, 0XDDD5, 0XA96A, 0XB8E3, 0X8A78, 0X9BF1,
           0X7387, 0X620E, 0X5095, 0X411C, 0X35A3, 0X242A, 0X16B1, 0X0738,
           0XFFCF, 0XEE46, 0XDCDD, 0XCD54, 0XB9EB, 0XA862, 0X9AF9, 0X8B70,
           0X8408, 0X9581, 0XA71A, 0XB693, 0XC22C, 0XD3A5, 0XE13E, 0XF0B7,
           0X0840, 0X19C9, 0X2B52, 0X3ADB, 0X4E64, 0X5FED, 0X6D76, 0X7CFF,
           0X9489, 0X8500, 0XB79B, 0XA612, 0XD2AD, 0XC324, 0XF1BF, 0XE036,
           0X18C1, 0X0948, 0X3BD3, 0X2A5A, 0X5EE5, 0X4F6C, 0X7DF7, 0X6C7E,
           0XA50A, 0XB483, 0X8618, 0X9791, 0XE32E, 0XF2A7, 0XC03C, 0XD1B5,
           0X2942, 0X38CB, 0X0A50, 0X1BD9, 0X6F66, 0X7EEF, 0X4C74, 0X5DFD,
           0XB58B, 0XA402, 0X9699, 0X8710, 0XF3AF, 0XE226, 0XD0BD, 0XC134,
           0X39C3, 0X284A, 0X1AD1, 0X0B58, 0X7FE7, 0X6E6E, 0X5CF5, 0X4D7C,
           0XC60C, 0XD785, 0XE51E, 0XF497, 0X8028, 0X91A1, 0XA33A, 0XB2B3,
           0X4A44, 0X5BCD, 0X6956, 0X78DF, 0X0C60, 0X1DE9, 0X2F72, 0X3EFB,
           0XD68D, 0XC704, 0XF59F, 0XE416, 0X90A9, 0X8120, 0XB3BB, 0XA232,
           0X5AC5, 0X4B4C, 0X79D7, 0X685E, 0X1CE1, 0X0D68, 0X3FF3, 0X2E7A,
           0XE70E, 0XF687, 0XC41C, 0XD595, 0XA12A, 0XB0A3, 0X8238, 0X93B1,
           0X6B46, 0X7ACF, 0X4854, 0X59DD, 0X2D62, 0X3CEB, 0X0E70, 0X1FF9,
           0XF78F, 0XE606, 0XD49D, 0XC514, 0XB1AB, 0XA022, 0X92B9, 0X8330,
           0X7BC7, 0X6A4E, 0X58D5, 0X495C, 0X3DE3, 0X2C6A, 0X1EF1, 0X0F78
        };


        /// <summary>
        /// Non -symbolized right, equivalent to the >>> in JS
        /// </summary>
        /// <param name = "x"> Number of shift </param>
        /// <Param name = "y"> displacement number </param>
        /// <returns></returns>
        public static int UIntMoveRight(int x, int y)
        {
            int mask = 0x7fffffff; //Integer.MAX_VALUE
            for (int i = 0; i < y; i++)
            {
                x >>= 1;
                x &= mask;
            }
            return x;
        }


    }
}
