using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Content;
using UnityOnlineProjectServer.Protocol;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Content.GameObject.Implements
{
    public class Tank : Pawn
    {
        public Quaternion TowerRotation;
        public Quaternion CannonRotation;
        public enum TankType
        {
            Red = 0,
            Yellow,
            Green,
            Blue
        }
        public Tank(long id) : base(id)
        {
            isDetector = true;
            sight = 300;
        }

        ~Tank()
        {
            
        }

        public override CommunicationMessage<Dictionary<string, string>> CreateCurrentStatusMessage(MessageType messageType)
        {
            var TickMessage = new CommunicationMessage<Dictionary<string, string>>()
            {
                header = new Header()
                {
                    MessageName = messageType.ToString()
                },
                body = new Body<Dictionary<string, string>>()
                {
                    Any = new Dictionary<string, string>()
                    {
                        ["ID"] = id.ToString(),
                        ["Position"] = Position.ToString(),
                        ["Quaternion"] = Rotation.ToString(),
                        ["TowerQuaternion"] = TowerRotation.ToString(),
                        ["CannonQuaternion"] = CannonRotation.ToString()
                    }
                }
            };

            return TickMessage;
        }


        public override void ApplyCurrentStatusMessage(CommunicationMessage<Dictionary<string, string>> message)
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
    }
}
