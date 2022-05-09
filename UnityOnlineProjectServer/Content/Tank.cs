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
    public class Tank : GameObject
    {
        public Quaternion TowerRotation;
        public Quaternion CannonRotation;

        public Tank() : base()
        {
            
        }

        ~Tank()
        {
            
        }

        public override CommunicationMessage<Dictionary<string, string>> CreateCurrentStatusMessage()
        {
            var TickMessage = new CommunicationMessage<Dictionary<string, string>>()
            {
                header = new Header()
                {
                    MessageName = MessageType.TankPositionReport.ToString()
                },
                body = new Body<Dictionary<string, string>>()
                {
                    Any = new Dictionary<string, string>()
                    {
                        ["Position"] = Position.ToString(),
                        ["Quaternion"] = Rotation.ToString(),
                        ["TowerQuaternion"] = TowerRotation.ToString(),
                        ["CannonQuaternion"] = CannonRotation.ToString()
                    }
                }
            };

            return TickMessage;
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

                    GameObjectName = message.body.Any["UserName"];
                    message.header.ACK = (int)ACK.ACK;
                    
                    SendMessage(this, message);

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

                    ReceiveCurrentGameObjectStatus(message);

                    break;
            }
        }

        protected override void ReceiveCurrentGameObjectStatus(CommunicationMessage<Dictionary<string, string>> message)
        {
            var rawPositionData = message.body.Any["Position"];
            var readedPositionData = NumericParser.ParseVector(rawPositionData);

            var rawRotationData = message.body.Any["Quaternion"];
            var readedRotationData = NumericParser.ParseQuaternion(rawRotationData);

            var rawTowerRotationData = message.body.Any["TowerQuaternion"];
            var readedTowerRotationData = NumericParser.ParseQuaternion(rawTowerRotationData);

            var rawCannonRotationData = message.body.Any["CannonQuaternion"];
            var readedCannonRotationData = NumericParser.ParseQuaternion(rawCannonRotationData);

            Rotation = readedRotationData;
            Position = readedPositionData;
            TowerRotation = readedTowerRotationData;
            CannonRotation = readedCannonRotationData;
        }

        void SendNACKMessage(CommunicationMessage<Dictionary<string,string>> replyMessage, string reason)
        {
            replyMessage.header.ACK = (int)ACK.NACK;
            replyMessage.header.Reason = reason;
            SendMessage(this, replyMessage);
        }

        void SendACKMessage(CommunicationMessage<Dictionary<string, string>> replyMessage)
        {
            replyMessage.header.ACK = (int)ACK.ACK;
            SendMessage(this, replyMessage);
        }
    }
}
