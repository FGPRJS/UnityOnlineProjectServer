using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Utility
{
    public class BitByte
    {
        public static bool[] BytetoBitArray(byte value)
        {
            bool[] result = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                var buffer = value;

                var mask = (byte)(1 << (7 - i));
                buffer &= mask;
                if (buffer > 0) result[i] = true;
            }

            return result;
        }

        public static byte BitArraytoByte(bool[] bitArray)
        {
            byte result = 0;

            int firstIndex = bitArray.Length - 1;

            for (int i = 0; i < bitArray.Length; i++)
            {
                var shifted = Convert.ToByte(bitArray[i]) << (firstIndex - i);
                result += Convert.ToByte(shifted);
            }

            return result;
        }

        public static byte BitArraytoByte(BitArray bitArray)
        {
            byte result = 0;

            for (int i = 0; i < bitArray.Length; i++)
            {
                var shifted = Convert.ToByte(bitArray[i]) << i;
                result += Convert.ToByte(shifted);
            }

            return result;
        }

        public static byte PartofBitArraytoByte(bool[] bitArray, int start, int end = 8)
        {
            byte result = 0;

            for (int i = start; i < end; i++)
            {
                var shifted = Convert.ToByte(bitArray[i]) << (7 - i);
                result += Convert.ToByte(shifted);
            }

            return result;
        }
    }
}
