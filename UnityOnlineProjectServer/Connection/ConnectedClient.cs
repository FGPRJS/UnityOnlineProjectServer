using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    internal class ConnectedClient
    {
        internal Socket ClientSocket;
        internal DataFrame Frame;

        internal Heartbeat heartbeat;
        
        internal Player player;
        internal PositionReport positionReport;

        public readonly long id;

        internal event ShutdownRequest ShutdownRequestEvent;
        internal delegate void ShutdownRequest(long id);


        internal ConnectedClient(long id)
        {
            this.id = id;
        }

        internal void Initialize(Socket socket)
        {
            Frame = new DataFrame();

            this.ClientSocket = socket;
            socket.ReceiveBufferSize = DataFrame.BufferSize;
            socket.SendBufferSize = DataFrame.BufferSize;

            heartbeat = new Heartbeat();
            heartbeat.TimeoutEvent += HeartbeatTimeOutEventAction;
            heartbeat.TickEvent += HeartbeatTickEventAction;

            //Player
            player = new Player();
            player.SendMessageRequestEvent += SendMessageRequestEventAction;

            //Position Report
            positionReport = new PositionReport();
            positionReport.TickEvent += PositionReportTickEventAction;

            BeginReceive();
        }

        private void PositionReportTickEventAction()
        {
            var message = new CommunicationMessage<Dictionary<string, string>>()
            {
                header = new Header()
                {
                    MessageName = MessageType.TankPositionReport.ToString()
                },
                body = new Body<Dictionary<string, string>>()
                {
                    Any = new Dictionary<string, string>()
                    {
                        ["Position"] = player.playerData.Position.ToString(),
                        ["Quaternion"] = player.playerData.Rotation.ToString(),
                        ["TowerQuaternion"] = player.playerData.TowerRotation.ToString(),
                        ["CannonQuaternion"] = player.playerData.CannonRotation.ToString()
                    }
                }
            };
            SendData(message);
        }

        #region Event Action

        void HeartbeatTimeOutEventAction()
        {
            ShutDownRequest();
        }

        void HeartbeatTickEventAction()
        {
            SendData(Heartbeat.heartbeatMessageByteData);
        }

        void SendMessageRequestEventAction(CommunicationMessage<Dictionary<string, string>> message) 
        {
            SendData(message);
        }

        #endregion

        void BeginReceive()
        {
            try
            {
                ClientSocket?.BeginReceive(
                Frame.buffer,
                0,
                DataFrame.BufferSize,
                SocketFlags.None,
                DataReceivedCallback,
                ClientSocket);
            }
            catch(NullReferenceException ne)
            {
                Console.WriteLine("ClientSocket Lost");
            }
            catch(SocketException se)
            {
                Console.WriteLine("Socket is not available. Shutdown Client.");
                ShutDownRequest();
            }
        }

        private void DataReceivedCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = ClientSocket.EndReceive(ar);

                if(bytesRead > 0)
                {
                    for (var i = 0; i < bytesRead; i++)
                    {
                        switch (Frame.buffer[i])
                        {
                            case 0x01:

                                if (!Frame.SOF)
                                {
                                    Frame.SOF = true;
                                }
                                //Already SOF exist => discard before data
                                else
                                {
                                    Frame.ResetDataFrame();
                                }

                                break;

                            case 0x02:

                                if (Frame.SOF)
                                {
                                    var completeData = Frame.GetByteData();
                                    var receivedMessage = CommunicationUtility.Deserialize(completeData);
                                    player.ProcessMessage(receivedMessage);
                                }
                                //Incomplete Message. Discard
                                Frame.ResetDataFrame();


                                break;

                            default:

                                if (Frame.SOF)
                                {
                                    Frame.AddByte(Frame.buffer[i]);
                                }

                                break;
                        }
                    }
                }

                heartbeat.ResetTimer();
                BeginReceive();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("ClientSocket Lost");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("ClientSocket is Disposed");
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket is not available. Shutdown Client.");
                ShutDownRequest();
            }
        }

        #region Send Data
        internal void SendData(CommunicationMessage<Dictionary<string,string>> message)
        {
            try
            {
                var byteData = CommunicationUtility.Serialize(message);

                SendData(byteData);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot send Data. Reason : " + ex.Message);
            }
        }

        internal void SendData(byte[] byteData)
        {
            try
            {
                Console.WriteLine("Send data to client.");

                var sendData = new byte[byteData.Length + 2];
                //Set SOF
                sendData[0] = 0x01;
                Array.Copy(byteData, 0, sendData, 1, byteData.Length);
                //Set EOF
                sendData[^1] = 0x02;

                ClientSocket?.BeginSend(
                    sendData,
                    0,
                    sendData.Length,
                    SocketFlags.None,
                    SendCallback,
                    ClientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot send Data. Reason : " + ex.Message);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send Failed. Reason : " + e.Message);
            }
        }
        #endregion

        internal void ShutDownRequest()
        {
            //Clear Event
            heartbeat.TimeoutEvent -= HeartbeatTimeOutEventAction;
            heartbeat.TickEvent -= HeartbeatTickEventAction;
            heartbeat = null;

            player.SendMessageRequestEvent -= SendMessageRequestEventAction;
            player = null;

            positionReport.TickEvent -= PositionReportTickEventAction;
            positionReport = null;

            Frame.ResetDataFrame();
            Frame = null;

            ClientSocket = null;

            ShutdownRequestEvent.Invoke(id);
        }
    }
}
