using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Connection.TickTasking;
using UnityOnlineProjectServer.Content;
using UnityOnlineProjectServer.Content.Map;
using UnityOnlineProjectServer.Protocol;
using static UnityOnlineProjectServer.Content.GameObject.Implements.Tank;
using static UnityOnlineProjectServer.Content.Pawn;

namespace UnityOnlineProjectServer.Connection
{
    public class ConnectedClient
    {
        public GameField currentField;

        public enum SocketStatus
        {
            Disconnected,
            HandShaking,
            Connected
        }
        public SocketStatus socketStatus;
        
        public TcpClient client;
        public NetworkStream stream;
        public DataFrame communicationData;

        public string clientName;

        public Heartbeat heartbeat;
        public NearbyObjPositionReport nearbyObjPositionReport;

        public Pawn playerObject;

        public readonly long id;

        public event ShutdownRequest ShutdownRequestEvent;
        public delegate void ShutdownRequest(long id);


        public ConnectedClient(long id)
        {
            this.id = id;
        }

        public void Initialize(TcpClient client)
        {
            //Initialize Data
            communicationData = new DataFrame();

            //Initialize Socket
            this.client = client;
            this.client.ReceiveBufferSize = DataFrame.BufferSize;
            this.client.SendBufferSize = DataFrame.BufferSize;

            //Initialize Socket stream
            stream = this.client.GetStream();

            socketStatus = SocketStatus.Disconnected;

            //Initialize Heartbeat
            heartbeat = new Heartbeat();
            heartbeat.TimeoutEvent += HeartbeatTimeOutEventAction;
            heartbeat.TickEvent += HeartbeatTickEventAction;

            //NearbyObjPositionReport
            nearbyObjPositionReport = new NearbyObjPositionReport();
            nearbyObjPositionReport.TickEvent += ReportNearbyObjPosition;

            HandShaking();
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
                var message = obj.CreateCurrentStatusMessage(MessageType.PawnPositionReport);
                SendData(message);
            }
        }

        void SendMessageRequestEventAction(object sender, CommunicationMessage<Dictionary<string, string>> message) 
        {
            SendData(message);
        }

        #endregion

        #region HandShaking

        void HandShaking()
        {
            try
            {
                socketStatus = SocketStatus.HandShaking;

                stream.BeginRead(
                    communicationData.buffer,
                    0,
                    DataFrame.BufferSize,
                    HandShakingCallBack,
                    client);
            }
            catch (NullReferenceException ne)
            {
                Console.WriteLine("ClientSocket Lost");
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket is not available. Shutdown Client.");
                ShutDownRequest();
            }
        }

        private async void HandShakingCallBack(IAsyncResult ar)
        {
            try
            {
                int bytesRead = stream.EndRead(ar);

                byte[] readedData = new byte[bytesRead];

                Array.Copy(communicationData.buffer, readedData, bytesRead);

                string handShakingRequest = Encoding.UTF8.GetString(readedData);

                if (handShakingRequest.Contains("GET"))
                {
                    //HandShake Reply
                    string websocketKey = Regex.Match(handShakingRequest, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                    string websocketKeyReply = websocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    byte[] websocketKeyReplySHA1 = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(websocketKeyReply));
                    string websocketKeyReplySHA1Base64 = Convert.ToBase64String(websocketKeyReplySHA1);

                    byte[] response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Sec-WebSocket-Accept: " + websocketKeyReplySHA1Base64 + "\r\n\r\n");

                    await SendData(response);

                    BeginReceive();
                }
                //Handshaking Crashed.
                else
                {
                    byte[] response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 400 Bad Request\r\n");

                    await SendData(response);

                    Console.WriteLine("Handshaking Failed.");
                    ShutDownRequest();
                }
            }
            catch
            {
                Console.WriteLine("Handshaking Failed.");
                ShutDownRequest();
            }
        }

        #endregion

        void BeginReceive()
        {
            try
            {
                stream.BeginRead(
                    communicationData.buffer,
                    0,
                    DataFrame.BufferSize,
                    DataReceivedCallback,
                    client);
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
                int bytesRead = stream.EndRead(ar);
                Console.WriteLine($"{bytesRead}byte(s) Data Received.");

                if (bytesRead > 0)
                {
                    ProcessDataFrame(bytesRead);

                    heartbeat.ResetTimer();
                    BeginReceive();
                }
                else
                {
                    Console.WriteLine("Client Closed.");
                    ShutDownRequest();
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Network Stream Lost");
                ShutDownRequest();
            }
            catch (IOException)
            {
                Console.WriteLine("Network Stream is not available. Shutdown Client.");
                ShutDownRequest();
            }
        }

        #region Process Received Data Frame

        private void ProcessDataFrame(int bytesRead)
        {

            //for (var i = 0; i < bytesRead; i++)
            //{
            //    switch (communicationData.buffer[i])
            //    {

            //    }

            //    switch (communicationData.buffer[i])
            //    {
            //        case 0x01:

            //            if (!communicationData.SOF)
            //            {
            //                communicationData.SOF = true;
            //            }
            //            //Already SOF exist => discard before data
            //            else
            //            {
            //                communicationData.ResetDataFrame();
            //            }

            //            break;

            //        case 0x02:

            //            //Completed Message. Process
            //            if (communicationData.SOF)
            //            {
            //                var completeData = communicationData.GetByteData();
            //                var receivedMessage = CommunicationUtility.Deserialize(completeData);
            //                ProcessMessage(receivedMessage);
            //            }
            //            //Incomplete Message. Discard
            //            communicationData.ResetDataFrame();


            //            break;

            //        default:

            //            if (communicationData.SOF)
            //            {
            //                communicationData.AddByte(communicationData.buffer[i]);
            //            }

            //            break;
            //    }
            //}
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

                case MessageType.PawnSpawnRequest:

                    Vector3 position = new Vector3(
                        RandomManager.GetRandomWithfloatingPoint(100, 800),
                        20,
                        RandomManager.GetRandomWithfloatingPoint(100, 800));

                    Quaternion rotation = Quaternion.CreateFromYawPitchRoll(
                        0,
                        RandomManager.GetRandomWithfloatingPoint(1, 2),
                        0);
                    
                    //Get Type   
                    var objectType = (PawnType)Enum.Parse(typeof(PawnType), message.body.Any["ObjectType"]);
                    var subobjectType = "";

                    //Create Sub Type
                    switch (objectType)
                    {
                        case PawnType.Tank:

                            var values = Enum.GetValues(typeof(TankType));
                            var subtype = values.GetValue(RandomManager.GetIntegerRandom(0, values.Length));
                            subobjectType = subtype.ToString();

                            break;
                    }

                    //Create Object
                    playerObject = currentField.CreatePawn(objectType, position);
                    playerObject.Rotation = rotation;

                    message.body = new Body<Dictionary<string, string>>()
                    {
                        Any = new Dictionary<string, string>()
                        {
                            ["ID"] = playerObject.id.ToString(),
                            ["ObjectType"] = message.body.Any["ObjectType"],
                            ["ObjectSubType"] = subobjectType,
                            ["Position"] = position.ToString(),
                            ["Quaternion"] = rotation.ToString()
                        }
                    };

                    SendACKMessage(message);

                    break;

                case MessageType.PawnPositionReport:

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

        public async Task SendData(byte[] byteData)
        {
            try
            {
                Console.WriteLine("Send data to client.");

                await stream.WriteAsync(
                    byteData,
                    0,
                    byteData.Length,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot send Data. Reason : " + ex.Message);
            }
        }

        #endregion

        public void ShutDownRequest()
        {
            //Clear Event
            heartbeat.TimeoutEvent -= HeartbeatTimeOutEventAction;
            heartbeat.TickEvent -= HeartbeatTickEventAction;
            heartbeat = null;

            if (playerObject != null)
            {
                //Remove Controlling object
                playerObject.SendMessageRequestEvent -= SendMessageRequestEventAction;

                currentField.RemovePawn(playerObject);
                playerObject = null;
            }

            nearbyObjPositionReport.TickEvent -= ReportNearbyObjPosition;
            nearbyObjPositionReport = null;

            communicationData.ResetDataFrame();
            communicationData = null;

            client = null;

            ShutdownRequestEvent.Invoke(id);
        }
    }
}
