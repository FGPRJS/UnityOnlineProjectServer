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

        public CommunicationMessage<Dictionary<string, string>> DecodeFrameRFC6455(int bytesRead)
        {
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
                                switch (frame.opcode)
                                {
                                    case DataFrame.OPCode.Ping:

                                        var pingMessage = new CommunicationMessage<Dictionary<string, string>>()
                                        {
                                            header = new Header()
                                            {
                                                MessageName = MessageType.Ping.ToString(),
                                            }
                                        };

                                        frame.ResetFrame();

                                        return pingMessage;

                                    case DataFrame.OPCode.Pong:

                                        var pongMessage = new CommunicationMessage<Dictionary<string, string>>()
                                        {
                                            header = new Header()
                                            {
                                                MessageName = MessageType.Pong.ToString(),
                                            }
                                        };

                                        frame.ResetFrame();

                                        return pongMessage;

                                    case DataFrame.OPCode.Close:

                                        var closeMessage = new CommunicationMessage<Dictionary<string, string>>()
                                        {
                                            header = new Header()
                                            {
                                                MessageName = MessageType.Close.ToString(),
                                            }
                                        };

                                        frame.ResetFrame();

                                        return closeMessage;

                                    default:
                                        frame.process = DataFrame.DataFrameProcess.MASK_PayloadLen;
                                        frame.hasContinuousData = false;
                                        break;
                                }
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
                            //Receive Complete
                            frame.process = DataFrame.DataFrameProcess.FIN_OPCode;

                            if (!frame.hasContinuousData)
                            {
                                CommunicationMessage<Dictionary<string,string>> message = null;

                                try
                                {
                                    message = CommunicationUtility.Deserialize(frame.data);
                                }
                                catch (Exception e)
                                {
                                    Logger.Instance.InfoLog($"Cannot Parse message. Reason : ${e.Message}");
                                    Logger.Instance.InfoLog($"Received Message : " + Encoding.UTF8.GetString(frame.data));
                                }

                                frame.ResetFrame();

                                return message;
                            }
                        }

                        break;
                }

                //Clear Byte
                buffer[i] = 0;
            }

            return null;
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
