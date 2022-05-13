using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Connection
{
    public class DataFrame
    {
        public static int BufferSize = 4096;
        public byte[] buffer = new byte[BufferSize];

        private Queue<byte> _dataQueue = new Queue<byte>();
        public string GetStringData()
        {
            var arr = GetByteData();
            var str = Encoding.ASCII.GetString(arr);
            return str;
        }

        public byte[] GetByteData()
        {
            return _dataQueue.ToArray();
        }
        public void AddByte(byte b)
        {
            _dataQueue.Enqueue(b);
        }
        public void FlushDataQueue()
        {
            _dataQueue.Clear();
        }

        public void ResetDataFrame()
        {
            FlushDataQueue();
        }
    }
}
