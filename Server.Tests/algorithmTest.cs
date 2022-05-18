using System;
using System.Collections.Generic;
using System.Text;
using UnityOnlineProjectServer.Utility;
using Xunit;

namespace Server.Tests
{
    public class algorithmTest
    {
        [Fact]
        public void RFC6455DataLengthCalculate_7byte()
        {
            //0b11111001
            var dataBitArr = BitByte.BytetoBitArray(0xF5);

            //Length Byte(Part of)
            var byteLength = BitByte.PartofBitArraytoByte(dataBitArr, 1);

            Assert.Equal(0x75, byteLength);
        }

        [Fact]
        public void RFC6455DataLengthCalculate_16byte()
        {
            byte[] data = new byte[] { 0x7E, 0x01, 0x10 };

            var byteLength = 0;

            int i = 1;

            for (int payloadIdx = 1; payloadIdx >= 0; payloadIdx--)
            {
                byteLength += data[i] << (8 * payloadIdx);
                i++;
            }

            var result = byteLength;
            Assert.Equal(272, result);
        }

        [Fact]
        public void RFC6455DataLengthCalculate_64byte()
        {
            byte[] data = new byte[] { 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 };

            var byteLength = 0;

            int i = 1;

            for (int payloadIdx = 7; payloadIdx >= 0; payloadIdx--)
            {
                byteLength += data[i] << (8 * payloadIdx);
                i++;
            }

            var result = byteLength;
            Assert.Equal(65536, result);
        }

        [Fact]
        public void ByteXOR()
        {
            byte val1 = 0x11;
            byte val2 = 0x22;

            Assert.Equal(0x33, (byte)(val1 ^ val2));
        }

        [Fact]
        public void BitConverterTest()
        {
            long value = 9999999999999;

            var bytes = BitConverter.GetBytes(value);

            Assert.Equal(4, bytes.Length);
        }
    }
}
