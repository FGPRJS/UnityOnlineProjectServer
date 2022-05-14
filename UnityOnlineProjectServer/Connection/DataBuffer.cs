using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Connection
{
    public class DataBuffer
    {
        public static int BufferSize = 4096;
        public byte[] buffer = new byte[BufferSize];
        public DataFrame frame;
        
        public void Initialize()
        {
            frame = new DataFrame();
        }

        /// <summary>
        /// Check frame info if continuous data frame comes.
        /// if this functions result is FALSE and continuous frame comes, it means data crashed.
        /// </summary>
        /// <returns></returns>
        public bool hasContinuableData()
        {
            if (frame.FIN)
            {
                return true;
            }
            return false;
        }

        public void SetLength(int length)
        {

        }
    }

    public class DataFrame
    {
        public enum DataFrameProcess
        {
            None,
            Crashed,
            FIN_OPCode,
            MASK_PayloadLen,
            PayloadLen16,
            PayloadLen64,
            MaskingKey,
            DATA
        }
        public DataFrameProcess process = DataFrameProcess.None;

        public bool FIN;
        public bool RCV1;
        public bool RCV2;
        public bool RCV3;
        public enum OPCode
        {
            Continuous = 0,
            Text = 1,
            Binary = 2,
            Close = 8,
            Ping = 9,
            Pong = 10,
            None = 999
        }
        public OPCode opcode;

        public bool hasContinuousData;
        
        public bool isMasked;
        public int maskingIndex;
        public byte[] maskingKey;

        public int payloadIndex;
        public int PayloadLength;

        public int dataIndex;
        public byte[] data;

        public DataFrame()
        {
            ResetFrame();
        }

        public void ResetFrame()
        {
            process = DataFrameProcess.None;

            FIN = false;
            RCV1 = false;
            RCV2 = false;
            RCV3 = false;
            opcode = OPCode.None;

            hasContinuousData = false;

            isMasked = false;
            maskingKey = null;
            maskingIndex = 0;

            payloadIndex = 0;
            PayloadLength = 0;

            dataIndex = 0;
            data = null;
    }
    }
}
