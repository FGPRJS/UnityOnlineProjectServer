using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOnlineProjectServer.Utility;
using Xunit;

namespace Server.Tests
{
    public class BitByteTest
    {
        /// <summary>
        /// Change 1Byte to 8BitArray. MSB = index 0
        /// </summary>
        [Fact]
        public void BytetoBitArray()
        {
            //0b00111000
            byte value = 0x38;

            var bitArr = BitByte.BytetoBitArray(value);

            Assert.Equal<bool[]>(new bool[]{ false, false, true, true, true, false, false, false }, bitArr);
        }

        [Fact]
        public void BitArrayToByte_JustBitArray()
        {
            bool[] bitArray = new bool[] { true, false, true, false};

            Assert.Equal(10, BitByte.BitArraytoByte(bitArray));
        }

        [Fact]
        public void BitArrayToByte_classBitArray()
        {
            //0b00010010
            BitArray bitArray = new BitArray(new byte[] { 0x12 });

            Assert.Equal(0x12, BitByte.BitArraytoByte(bitArray));
        }

        [Fact]
        public void PartofBitArrayToByte()
        {
            byte value = 0xF3;

            var bitArray = BitByte.BytetoBitArray(value);

            Assert.Equal(3, BitByte.PartofBitArraytoByte(bitArray, 4, 8));
        }
    }
}
