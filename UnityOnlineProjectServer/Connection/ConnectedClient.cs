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
    public class ConnectedClient
    {
        public Socket ClientSocket;
        public DataFrame Frame;

        public Heartbeat heartbeat;
        
        public Tank player;

        public readonly long id;

        public event ShutdownRequest ShutdownRequestEvent;
        public delegate void ShutdownRequest(long id);


        public ConnectedClient(long id)
        {
            this.id = id;
        }

        public void Initialize(Socket socket)
        {
            Frame = new DataFrame();

            this.ClientSocket = socket;
            socket.ReceiveBufferSize = DataFrame.BufferSize;
            socket.SendBufferSize = DataFrame.BufferSize;

            heartbeat = new Heartbeat();
            heartbeat.TimeoutEvent += HeartbeatTimeOutEventAction;
            heartbeat.TickEvent += HeartbeatTickEventAction;

            //Player
            player = new Tank();
            player.SendMessageRequestEvent += SendMessageRequestEventAction;

            BeginReceive();
        }

        

        #region Event Action

        void HeartbeatTimeOutEventAction(object sender, EventArgs arg)
        {
            ShutDownRequest();
        }

        void HeartbeatTickEventAction(object sender, EventArgs arg)
        {
            SendData(Heartbeat.heartbeatMessageByteData);
        }

        void SendMessageRequestEventAction(object sender, CommunicationMessage<Dictionary<string, string>> message) 
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
        public void SendData(CommunicationMessage<Dictionary<string,string>> message)
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

        public void SendData(byte[] byteData)
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

        public void ShutDownRequest()
        {
            //Clear Event
            heartbeat.TimeoutEvent -= HeartbeatTimeOutEventAction;
            heartbeat.TickEvent -= HeartbeatTickEventAction;
            heartbeat = null;

            player.SendMessageRequestEvent -= SendMessageRequestEventAction;
            player = null;

            Frame.ResetDataFrame();
            Frame = null;

            ClientSocket = null;

            ShutdownRequestEvent.Invoke(id);
        }
    }
}
