using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Content;
using UnityOnlineProjectServer.Content.GameObject.Implements;
using UnityOnlineProjectServer.Content.TickTasking;
using UnityOnlineProjectServer.Protocol;
using static UnityOnlineProjectServer.Content.GameObject.Implements.Tank;
using static UnityOnlineProjectServer.Content.Pawn;

namespace UnityOnlineProjectServer.Connection
{
    public class ConnectedClient
    {
        public enum SocketStatus
        {
            Disconnected,
            HandShaking,
            Connected
        }
        public SocketStatus socketStatus;
        
        public TcpClient socket;
        public NetworkStream stream;
        public DataBuffer receivedData;

        public string clientName;

        public Heartbeat heartbeat;

        public Pawn PlayerObject;

        public long id;
        public EventHandler PlayerObjectAssignedEvent;
        public EventHandler HandshakeCompleteEvent;
        public EventHandler<CommunicationMessage<Dictionary<string,string>>> ChatEvent;
        public EventHandler<long> ShutdownRequestEvent;


        public void Initialize(TcpClient client)
        {
            //Initialize Data
            receivedData = new DataBuffer();
            receivedData.Initialize();

            //Initialize Socket
            socket = client;
            socket.ReceiveBufferSize = DataBuffer.BufferSize;
            socket.SendBufferSize = DataBuffer.BufferSize;

            //Initialize Socket stream
            stream = socket.GetStream();

            socketStatus = SocketStatus.Disconnected;

            //Initialize Heartbeat
            heartbeat = new Heartbeat();
            heartbeat.TimeoutEvent += HeartbeatTimeOutEventAction;
            heartbeat.TickEvent += HeartbeatTickEventAction;

            HandShaking();
        }


        #region Event Action

        void HeartbeatTimeOutEventAction(object sender, EventArgs arg)
        {
            ShutDownRequest();
        }

        void HeartbeatTickEventAction(object sender, EventArgs arg)
        {
            SendPing();
        }

        

        #endregion

        #region HandShaking

        void HandShaking()
        {
            try
            {
                socketStatus = SocketStatus.HandShaking;
                Console.WriteLine("Start Handshaking");

                stream.BeginRead(
                    receivedData.buffer,
                    0,
                    DataBuffer.BufferSize,
                    HandShakingCallBack,
                    socket);
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

                Array.Copy(receivedData.buffer, readedData, bytesRead);

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

                    await stream.WriteAsync(
                        response,
                        0,
                        response.Length,
                        CancellationToken.None);

                    socketStatus = SocketStatus.Connected;
                    Console.WriteLine("Hanshaking Complete");

                    BeginReceive();
                }
                //Handshaking Crashed.
                else
                {
                    socketStatus = SocketStatus.Disconnected;

                    byte[] response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 400 Bad Request\r\n");

                    SendTextData(response);

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
                stream?.BeginRead(
                    receivedData?.buffer,
                    0,
                    DataBuffer.BufferSize,
                    DataReceivedCallback,
                    socket);
            }
            catch(Exception ne)
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
                    var receivedMessage = receivedData.DecodeFrameRFC6455(bytesRead);
                    Console.WriteLine("Receive : " + JsonConvert.SerializeObject(receivedMessage));
                    ProcessMessage(receivedMessage);

                    BeginReceive();
                }
                else
                {
                    Console.WriteLine("Client Closed.");
                    ShutDownRequest();
                }

                heartbeat?.ResetTimer();
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

        private void ProcessMessage(CommunicationMessage<Dictionary<string, string>> message)
        {
            MessageType messageType;

            if (message == null) return;

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
                case MessageType.Ping:

                    SendPong();

                    break;

                case MessageType.Pong:

                    //do nothing

                    break;

                case MessageType.Close:

                    ShutDownRequest();

                    break;

                case MessageType.LoginRequest:

                    clientName = message.body.Any["UserName"];
                    message.header.ACK = (int)ACK.ACK;

                    HandshakeCompleteEvent?.Invoke(this, EventArgs.Empty);
                    SendACKMessage(message);

                    break;

                case MessageType.PlayerChatReport:

                    ChatEvent.Invoke(this, message);

                    break;

                case MessageType.GameObjectSpawnRequest:

                    Vector3 position = new Vector3(
                        RandomManager.GetRandomWithfloatingPoint(790, 800),
                        20,
                        RandomManager.GetRandomWithfloatingPoint(790, 800));

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

                            //Create Object
                            var newTank = new Tank(id);
                            newTank.subType = (TankType)subtype;
                            newTank.PawnName = clientName;
                            newTank.Position = position;
                            newTank.Rotation = rotation;

                            PlayerObject = newTank;

                            break;
                    }

                    message.body = new Body<Dictionary<string, string>>()
                    {
                        Any = new Dictionary<string, string>()
                        {
                            ["ID"] = PlayerObject.id.ToString(),
                            ["PawnName"] = clientName,
                            ["ObjectType"] = message.body.Any["ObjectType"],
                            ["ObjectSubType"] = subobjectType,
                            ["Position"] = position.ToString(),
                            ["Quaternion"] = rotation.ToString()
                        }
                    };

                    SendACKMessage(message);

                    PlayerObjectAssignedEvent?.Invoke(this, EventArgs.Empty);

                    break;

                case MessageType.TankPositionReport:

                    PlayerObject.ApplyCurrentStatusMessage(message);

                    break;

                case MessageType.TankMovingReport:

                    PlayerObject.ApplyCurrentMovingStatusMessage(message);

                    break;
            }
        }
        #endregion

        #region Reply Message

        void SendNACKMessage(CommunicationMessage<Dictionary<string, string>> replyMessage, string reason)
        {
            replyMessage.header.ACK = (int)ACK.NACK;
            replyMessage.header.Reason = reason;
            SendTextData(replyMessage);
        }

        void SendACKMessage(CommunicationMessage<Dictionary<string, string>> replyMessage)
        {
            replyMessage.header.ACK = (int)ACK.ACK;
            SendTextData(replyMessage);
        }

        #endregion

        #region Send Data
        public void SendTextData(CommunicationMessage<Dictionary<string,string>> message)
        {
            try
            {
                var byteData = CommunicationUtility.Serialize(message);
                Console.WriteLine("Send : " + JsonConvert.SerializeObject(message));
                SendTextData(byteData);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot send Data. Reason : " + ex.Message);
            }
        }

        public void SendTextData(byte[] byteData)
        {
            try
            {
                Console.WriteLine($"Send {byteData.Length} byte(s) ByteData to client.");

                var result = DataBuffer.EncodeRFC6455(DataFrame.OPCode.Text, byteData);

                stream.WriteAsync(
                    result,
                    0,
                    result.Length,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot send Data. Reason : " + ex.Message);
            }
        }

        public async Task SendPing()
        {
            try
            {
                Console.WriteLine("Send PING");

                var result = DataBuffer.EncodeRFC6455(DataFrame.OPCode.Ping, new byte[0]);

                await stream.WriteAsync(
                    result,
                    0,
                    result.Length,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot send PING. Reason : " + ex.Message);
            }
        }

        public async Task SendPong()
        {
            try
            {
                Console.WriteLine("Send PONG");

                var result = DataBuffer.EncodeRFC6455(DataFrame.OPCode.Pong, new byte[0]);

                await stream.WriteAsync(
                    result,
                    0,
                    result.Length,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot send PONG. Reason : " + ex.Message);
            }
        }

        #endregion

        public void ShutDownRequest()
        {
            //Clear Event
            if(heartbeat != null)
            {
                heartbeat.TimeoutEvent -= HeartbeatTimeOutEventAction;
                heartbeat.TickEvent -= HeartbeatTickEventAction;
                heartbeat = null;
            }

            receivedData = null;

            socket = null;

            ShutdownRequestEvent?.Invoke(this, id);
        }
    }
}
