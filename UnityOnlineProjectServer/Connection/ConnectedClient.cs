﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Connection.TickTasking;
using UnityOnlineProjectServer.Content;
using UnityOnlineProjectServer.Content.Map;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    public class ConnectedClient
    {
        public GameField currentField;

        public Socket ClientSocket;
        public DataFrame communicationData;

        public string clientName;

        public Heartbeat heartbeat;
        public NearbyObjPositionReport nearbyObjPositionReport;

        public GameObject playerObject;

        public readonly long id;

        public event ShutdownRequest ShutdownRequestEvent;
        public delegate void ShutdownRequest(long id);


        public ConnectedClient(long id)
        {
            this.id = id;
        }

        public void Initialize(Socket socket)
        {
            communicationData = new DataFrame();

            this.ClientSocket = socket;
            socket.ReceiveBufferSize = DataFrame.BufferSize;
            socket.SendBufferSize = DataFrame.BufferSize;

            heartbeat = new Heartbeat();
            heartbeat.TimeoutEvent += HeartbeatTimeOutEventAction;
            heartbeat.TickEvent += HeartbeatTickEventAction;

            //NearbyObjPositionReport
            nearbyObjPositionReport = new NearbyObjPositionReport();
            nearbyObjPositionReport.TickEvent += ReportNearbyObjPosition;

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

        void ReportNearbyObjPosition(object sender, EventArgs e)
        {
            if (playerObject == null) return;

            foreach (var obj in playerObject.GetNearbyObjects())
            {
                var message = obj.CreateCurrentStatusMessage(MessageType.GameObjectPositionReport);
                SendData(message);
            }
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
                communicationData.buffer,
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

                ProcessDataFrame(bytesRead);

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

        #region Process Received Data Frame

        private void ProcessDataFrame(int bytesRead)
        {
            if (bytesRead > 0)
            {
                for (var i = 0; i < bytesRead; i++)
                {
                    switch (communicationData.buffer[i])
                    {
                        case 0x01:

                            if (!communicationData.SOF)
                            {
                                communicationData.SOF = true;
                            }
                            //Already SOF exist => discard before data
                            else
                            {
                                communicationData.ResetDataFrame();
                            }

                            break;

                        case 0x02:

                            //Completed Message. Process
                            if (communicationData.SOF)
                            {
                                var completeData = communicationData.GetByteData();
                                var receivedMessage = CommunicationUtility.Deserialize(completeData);
                                ProcessMessage(receivedMessage);
                            }
                            //Incomplete Message. Discard
                            communicationData.ResetDataFrame();


                            break;

                        default:

                            if (communicationData.SOF)
                            {
                                communicationData.AddByte(communicationData.buffer[i]);
                            }

                            break;
                    }
                }
            }
        }


        private void ProcessMessage(CommunicationMessage<Dictionary<string, string>> message)
        {
            MessageType messageType;

            try
            {
                messageType = (MessageType)Enum.Parse(typeof(MessageType), message.header.MessageName);
            }
            catch
            {
                Console.WriteLine("MessageType crashed. Cannot process message");
                return;
            }

            switch (messageType)
            {
                case MessageType.HeartBeatRequest:

                    if (message.header.ACK == (int)ACK.ACK) return;

                    //Just Reply
                    SendACKMessage(message);
                    break;

                case MessageType.LoginRequest:

                    clientName = message.body.Any["UserName"];
                    message.header.ACK = (int)ACK.ACK;

                    SendData(message);

                    break;

                case MessageType.TankSpawnRequest:

                    var random = RandomManager.Instance.random;

                    Vector3 position = new Vector3(
                        random.Next(100, 800),
                        20,
                        random.Next(100, 800));

                    Quaternion rotation = Quaternion.CreateFromYawPitchRoll(
                        0,
                        random.Next(-180, 180),
                        0);

                    //Create Object
                    playerObject = currentField.CreateGameObject(GameObject.GameObjectType.Tank, position);
                    playerObject.Rotation = rotation;

                    message.body = new Body<Dictionary<string, string>>()
                    {
                        Any = new Dictionary<string, string>()
                        {
                            ["Type"] = random.Next(0, 4).ToString(),
                            ["Position"] = position.ToString(),
                            ["Quaternion"] = rotation.ToString()
                        }
                    };

                    SendACKMessage(message);

                    break;

                case MessageType.GameObjectPositionReport:

                    playerObject.ApplyCurrentStatusMessage(message);

                    break;
            }
        }
        #endregion

        #region Reply Message

        void SendNACKMessage(CommunicationMessage<Dictionary<string, string>> replyMessage, string reason)
        {
            replyMessage.header.ACK = (int)ACK.NACK;
            replyMessage.header.Reason = reason;
            SendData(replyMessage);
        }

        void SendACKMessage(CommunicationMessage<Dictionary<string, string>> replyMessage)
        {
            replyMessage.header.ACK = (int)ACK.ACK;
            SendData(replyMessage);
        }

        #endregion

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

            playerObject.SendMessageRequestEvent -= SendMessageRequestEventAction;
            playerObject = null;

            communicationData.ResetDataFrame();
            communicationData = null;

            ClientSocket = null;

            ShutdownRequestEvent.Invoke(id);
        }
    }
}
