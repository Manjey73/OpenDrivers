namespace ScadaCommFunc
{
    public static class BitFunc
    {
        #region  ROL 32 bit
        /// <summary>
        /// цикличиский сдвиг битов влево до 32-х бит, на входе переменная UInt32
        /// </summary>
        /// <param name="number"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        /// 
        public static UInt32 ROL(UInt32 number, int shift)
        {
            shift %= 31;
            return (number << shift) | (number >> (32 - shift));
        }
        #endregion  ROL 32 bit

        // Циклический сдвиг влево («по кругу») для byte
        // Cyclic left shift ("in a circle") for byte
        #region ROL Byte
        public static byte ROLByte(byte number, int shift)
        {
            shift %= 7;
            return (byte)((number << shift) | (number >> (8 - shift)));
        }
        #endregion ROL Byte


        /// <summary>
        /// цикличиский сдвиг битов вправо до 32-х бит, на входе переменная UInt32
        /// </summary>
        /// <param name="number"></param>
        /// <param name="shift"></param>
        /// <returns></returns>

        public static UInt32 ROR(UInt32 number, int shift)
        {
            shift %= 31;
            return (number >> shift) | (number << (32 - shift));
        }

        // Циклический сдвиг вправо («по кругу») для byte
        // Cyclic right shift ("in a circle") for byte
        #region ROR Byte
        public static byte RORByte(byte number, int shift)
        {
            shift %= 7;
            return (byte)((number >> shift) | (number << (8 - shift)));
        }
        #endregion ROR Byte

        /// <summary>
        /// Получить бит из числа int
        /// </summary>
        /// <param name="val"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// 
        public static int GetBit(int val, int n)
        {
            int intVal = val;
            return (intVal >> n) & 1;
        }

        /// <summary>
        /// вставить бит в число int
        /// </summary>
        /// <param name="n"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        public static int SetBit(int n, int index, bool value)
        {
            return value ? n | (1 << index) : n & ~(1 << index);
        }

        /// <summary>
        /// Расчитать количество бит в 32-х битном числе
        /// Calculate the number of bits in a 32-bit number
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        /// 
        public static int CountBit32(int n)
        {
            n = ((n >> 1) & 0x55555555) + (n & 0x55555555);
            n = ((n >> 2) & 0x33333333) + (n & 0x33333333);
            n = ((n >> 4) & 0x0F0F0F0F) + (n & 0x0F0F0F0F);
            n = ((n >> 8) & 0x00FF00FF) + (n & 0x00FF00FF);
            n = ((n >> 16) & 0x0000FFFF) + (n & 0x0000FFFF);
            return n;
        }

    }
}
