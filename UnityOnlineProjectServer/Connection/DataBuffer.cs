using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityOnlineProjectServer.Protocol;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Connection
{
    public class DataBuffer
    {
        public static int BufferSize = 4096;
        public byte[] buffer = new byte[BufferSize];
        private DataFrame frame;
        
        public void Initialize()
        {
            frame = new DataFrame();
        }

        public List<CommunicationMessage<Dictionary<string, string>>> DecodeFrameRFC6455(int bytesRead)
        {
            var messageList = new List<CommunicationMessage<Dictionary<string, string>>>();

            for (var i = 0; i < bytesRead; i++)
            {
                var data = buffer[i];
                bool[] dataBitArr;

                switch (frame.process)
                {
                    case DataFrame.DataFrameProcess.None:
                    case DataFrame.DataFrameProcess.FIN_OPCode:

                        dataBitArr = BitByte.BytetoBitArray(data);
                        //FINBit = 1
                        if (dataBitArr[0])
                        {
                            //OPCode
                            frame.opcode = (DataFrame.OPCode)BitByte.PartofBitArraytoByte(dataBitArr, 4);
                            if (Enum.IsDefined(typeof(DataFrame.OPCode), frame.opcode))
                            {
                                frame.process = DataFrame.DataFrameProcess.MASK_PayloadLen;
                            }
                            else
                            {
                                frame.ResetFrame();
                            }
                        }
                        //FINBit = 0 -> isContinuous?
                        else
                        {
                            frame.hasContinuousData = true;
                        }

                        break;

                    case DataFrame.DataFrameProcess.MASK_PayloadLen:

                        dataBitArr = BitByte.BytetoBitArray(data);
                        //MASKBit
                        frame.isMasked = dataBitArr[0];
                        if (frame.isMasked)
                        {
                            frame.maskingKey = new byte[4];
                        }


                        //Length Byte(Part of)
                        var byteLength = BitByte.PartofBitArraytoByte(dataBitArr, 1);

                        if (byteLength < 126)
                        {
                            frame.PayloadLength = byteLength;
                            frame.data = new byte[byteLength];
                            frame.process = DataFrame.DataFrameProcess.MaskingKey;
                        }
                        else if (byteLength == 126)
                        {
                            frame.payloadIndex = 1;
                            frame.process = DataFrame.DataFrameProcess.PayloadLen16;
                        }
                        else
                        {
                            frame.payloadIndex = 7;
                            frame.process = DataFrame.DataFrameProcess.PayloadLen64;
                        }

                        break;

                    case DataFrame.DataFrameProcess.PayloadLen16:

                        frame.PayloadLength += buffer[i] << (8 * frame.payloadIndex);
                        frame.payloadIndex--;

                        if (frame.payloadIndex < 0)
                        {
                            frame.data = new byte[frame.PayloadLength];
                            frame.process = DataFrame.DataFrameProcess.MaskingKey;
                        }

                        break;

                    case DataFrame.DataFrameProcess.PayloadLen64:

                        frame.PayloadLength += buffer[i] << (8 * frame.payloadIndex);
                        frame.payloadIndex--;

                        if (frame.payloadIndex < 0)
                        {
                            frame.data = new byte[frame.PayloadLength];
                            frame.process = DataFrame.DataFrameProcess.MaskingKey;
                        }

                        break;

                    case DataFrame.DataFrameProcess.MaskingKey:

                        if (frame.isMasked)
                        {
                            frame.maskingKey[frame.maskingIndex] = buffer[i];
                            frame.maskingIndex++;
                            if (frame.maskingIndex >= frame.maskingKey.Length)
                            {
                                frame.process = DataFrame.DataFrameProcess.DATA;
                            }
                        }
                        else
                        {
                            frame.process = DataFrame.DataFrameProcess.DATA;
                        }

                        //if there is no data
                        if((frame.process == DataFrame.DataFrameProcess.DATA) && (frame.PayloadLength == 0))
                        {
                            messageList.Add(CreateEmptyDataMessage());
                            frame.ResetFrame();
                            frame.process = DataFrame.DataFrameProcess.FIN_OPCode;
                        }

                        break;

                    case DataFrame.DataFrameProcess.DATA:


                        if (frame.isMasked)
                        {
                            byte decodedByte = (byte)(buffer[i] ^ frame.maskingKey[frame.dataIndex % 4]);
                            frame.data[frame.dataIndex] = decodedByte;
                        }
                        else
                        {
                            frame.data[frame.dataIndex] = buffer[i];
                        }

                        frame.dataIndex++;

                        if (frame.dataIndex >= frame.PayloadLength)
                        {
                            if (!frame.hasContinuousData)
                            {

                                CommunicationMessage<Dictionary<string, string>> message = null;

                                if (frame.opcode == DataFrame.OPCode.Text)
                                {
                                    try
                                    {
                                        message = CommunicationUtility.Deserialize(frame.data);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Instance.ErrorLog($"Cannot Parse message. Reason : ${e.Message}");
                                        Logger.Instance.ErrorLog($"Received Message : " + Encoding.UTF8.GetString(frame.data));
                                    }
                                }
                                else if(frame.opcode == DataFrame.OPCode.Close)
                                {
                                    message = new CommunicationMessage<Dictionary<string, string>>()
                                    {
                                        header = new Header()
                                        {
                                            MessageName = MessageType.Close.ToString(),
                                        },
                                        body = new Body<Dictionary<string, string>>()
                                        {
                                            Any = new Dictionary<string, string>()
                                            {
                                                ["Reason"] = Encoding.UTF8.GetString(frame.data)
                                            }
                                        }
                                    };
                                }

                                messageList.Add(message);

                                frame.ResetFrame();

                                break;
                            }

                            //Receive Complete
                            frame.process = DataFrame.DataFrameProcess.FIN_OPCode;
                        }

                        break;
                }
            }

            return messageList;
        }

        private CommunicationMessage<Dictionary<string, string>> CreateEmptyDataMessage()
        {
            CommunicationMessage<Dictionary<string, string>> message = null;

            switch (frame.opcode)
            {
                case DataFrame.OPCode.Ping:

                    message = new CommunicationMessage<Dictionary<string, string>>()
                    {
                        header = new Header()
                        {
                            MessageName = MessageType.Ping.ToString(),
                        }
                    };

                    break;

                case DataFrame.OPCode.Pong:

                    message = new CommunicationMessage<Dictionary<string, string>>()
                    {
                        header = new Header()
                        {
                            MessageName = MessageType.Pong.ToString(),
                        }
                    };

                    break;

            }

            return message;
        }


        public static byte[] EncodeRFC6455(DataFrame.OPCode opcode, byte[] byteData)
        {
            //Create WebSocket Frame
            List<byte> sendBuffer = new List<byte>();
            sendBuffer.Add((byte)(0x80 | (byte)opcode));
            //No Mask
            //Length
            if (byteData.Length <= 0x7D)
            {
                sendBuffer.Add((byte)byteData.Length);
            }
            else if (byteData.Length <= 0x7FFF)
            {
                sendBuffer.Add(0x7E);
                var lengtharr = BitConverter.GetBytes(byteData.Length);
                //Only 2byte
                for (int i = 1; i >= 0 ; i--)
                    sendBuffer.Add(lengtharr[i]);
            }
            else
            {
                sendBuffer.Add(0x7F);
                var lengtharr = BitConverter.GetBytes((long)byteData.Length);
                for (int i = lengtharr.Length - 1; i >= 0; i--)
                    sendBuffer.Add(lengtharr[i]);
            }
            //No Mask Key
            foreach (var data in byteData)
            {
                sendBuffer.Add(data);
            }

            var result = sendBuffer.ToArray();
            return result;
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
