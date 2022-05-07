using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Connection
{
    internal class DataFrame
    {
        public static int BufferSize = 4096;
        public byte[] buffer = new byte[BufferSize];

        internal bool SOF = false;
        private Queue<byte> _dataQueue = new Queue<byte>();
        internal string GetStringData()
        {
            var arr = GetByteData();
            var str = Encoding.ASCII.GetString(arr);
            return str;
        }

        internal byte[] GetByteData()
        {
            return _dataQueue.ToArray();
        }
        internal void AddByte(byte b)
        {
            _dataQueue.Enqueue(b);
        }
        internal void FlushDataQueue()
        {
            _dataQueue.Clear();
        }

        internal void ResetDataFrame()
        {
            FlushDataQueue();
            SOF = false;
        }
    }
}
