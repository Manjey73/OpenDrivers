/*
 * Copyright 2021 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : KpModbus
 * Summary  : The class contains utility methods for the Modbus protocol implementation
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2012
 * Modified : 2021
 */

namespace ScadaCommFunc
{
    public static class ByteFunc
    {
        /// <summary>
        /// Разобрать массив, определяющий порядок байт, из строковой записи вида '01234567'.
        /// </summary>
        public static int[]? ParseByteOrder(string byteOrderStr)
        {
            if (string.IsNullOrEmpty(byteOrderStr))
            {
                return null;
            }
            else
            {
                int len = byteOrderStr.Length;
                int[] byteOrder = new int[len];

                for (int i = 0; i < len; i++)
                {
                    byteOrder[i] = int.TryParse(byteOrderStr[i].ToString(), out int n) ? n : 0;
                }

                return byteOrder;
            }
        }

        /// <summary>
        /// Копировать элементы массива с заданным порядоком байт.
        /// </summary>
        public static void ApplyByteOrder(byte[] src, int srcOffset, byte[] dest, int destOffset, int count,
            int[] byteOrder, bool reverse)
        {
            int srcLen = src == null ? 0 : src.Length;
            int endSrcInd = srcOffset + count - 1;
            int ordLen = byteOrder == null ? 0 : byteOrder.Length;

            if (byteOrder == null)
            {
                // копирование данных без учёта порядка байт
                for (int i = 0; i < count; i++)
                {
                    int srcInd = reverse ? endSrcInd - i : srcOffset + i;
                    dest[destOffset++] = 0 <= srcInd && srcInd < srcLen ? src[srcInd] : (byte)0;
                }
            }
            else
            {
                // копирование данных с учётом порядка байт
                for (int i = 0; i < count; i++)
                {
                    int srcInd = i < ordLen ? (reverse ? endSrcInd - byteOrder[i] : srcOffset + byteOrder[i]) : -1;
                    dest[destOffset++] = 0 <= srcInd && srcInd < srcLen ? src[srcInd] : (byte)0;
                }
            }
        }

        /// <summary>
        /// Копировать элементы массива с заданным порядоком байт.
        /// </summary>
        public static void ApplyByteOrder(byte[] src, byte[] dest, int[] byteOrder)
        {
            ApplyByteOrder(src, 0, dest, 0, dest.Length, byteOrder, false);
        }

    }
}
