using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Content;
using UnityOnlineProjectServer.Protocol;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Connection
{
    internal class Player
    {
        public TankData playerData;
        public static Dictionary<string, Player> PlayerMap = new Dictionary<string, Player>();

        internal PositionReport positionReport;

        public delegate void SendMessageRequest(CommunicationMessage<Dictionary<string, string>> message);
        public event SendMessageRequest SendMessageRequestEvent;

        public Player()
        {
            //Position Report
            positionReport = new PositionReport();
            positionReport.TickEvent += PositionReportTickEventAction;
        }

        ~Player()
        {
            positionReport.TickEvent -= PositionReportTickEventAction;
        }

        private void PositionReportTickEventAction()
        {
            if (playerData == null) return;

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
                        ["Position"] = playerData.Position.ToString(),
                        ["Quaternion"] = playerData.Rotation.ToString(),
                        ["TowerQuaternion"] = playerData.TowerRotation.ToString(),
                        ["CannonQuaternion"] = playerData.CannonRotation.ToString()
                    }
                }
            };
            SendMessageRequestEvent.Invoke(message);
        }


        public void ProcessMessage(CommunicationMessage<Dictionary<string, string>> message)
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

                    //Duplicate Name
                    if (PlayerMap.ContainsKey(message.header.MessageName))
                    {
                        SendNACKMessage(message, "Already exist user that has same name.");
                        return;
                    }

                    playerData = new TankData(message.body.Any["UserName"]);
                    message.header.ACK = (int)ACK.ACK;
                    
                    SendMessageRequestEvent.Invoke(message);

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

                case MessageType.TankPositionReport:

                    var rawPositionData = message.body.Any["Position"];
                    var readedPositionData = NumericParser.ParseVector(rawPositionData);

                    var rawRotationData = message.body.Any["Quaternion"];
                    var readedRotationData = NumericParser.ParseQuaternion(rawRotationData);

                    var rawTowerRotationData = message.body.Any["TowerQuaternion"];
                    var readedTowerRotationData = NumericParser.ParseQuaternion(rawTowerRotationData);

                    var rawCannonRotationData = message.body.Any["CannonQuaternion"];
                    var readedCannonRotationData = NumericParser.ParseQuaternion(rawCannonRotationData);

                    playerData.Rotation = readedRotationData;
                    playerData.Position = readedPositionData;
                    playerData.TowerRotation = readedTowerRotationData;
                    playerData.CannonRotation = readedCannonRotationData;

                    break;
            }
        }

        void SendNACKMessage(CommunicationMessage<Dictionary<string,string>> replyMessage, string reason)
        {
            replyMessage.header.ACK = (int)ACK.NACK;
            replyMessage.header.Reason = reason;
            SendMessageRequestEvent?.Invoke(replyMessage);
        }

        void SendACKMessage(CommunicationMessage<Dictionary<string, string>> replyMessage)
        {
            replyMessage.header.ACK = (int)ACK.ACK;
            SendMessageRequestEvent?.Invoke(replyMessage);
        }
    }
}
