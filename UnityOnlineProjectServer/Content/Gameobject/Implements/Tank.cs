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

        Vector3 _moveDirection;
        float _moveDelta;
        Vector3 _rotationVector;
        float _rotationDelta;
        Vector3 _towerRotationVector;
        float _towerRotationDelta;
        Vector3 _cannonRotationVector;
        float _cannonRotationDelta;

        public enum TankType
        {
            Red = 0,
            Yellow,
            Green,
            Blue
        }
        public TankType subType;
        public Tank(long id) : base(id)
        {
        }

        ~Tank()
        {
            
        }

        public override CommunicationMessage<Dictionary<string, string>> CreateObjectInfoMessage(MessageType messageType)
        {
            var message = new CommunicationMessage<Dictionary<string, string>>()
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
                        ["PawnName"] = PawnName,
                        ["ObjectType"] = PawnType.Tank.ToString(),
                        ["ObjectSubType"] = subType.ToString(),
                        ["Position"] = Position.ToString(),
                        ["Quaternion"] = Rotation.ToString()
                    }
                }
            };

            return message;
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

        public override CommunicationMessage<Dictionary<string, string>> CreateCurrentMovingStatusMessage(MessageType messageType)
        {
            var message = new CommunicationMessage<Dictionary<string, string>>()
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
                        ["MoveDirection"] = _moveDirection.ToString(),
                        ["MoveDelta"] = _moveDelta.ToString(),
                        ["RotationVector"] = _rotationVector.ToString(),
                        ["RotationDelta"] = _rotationDelta.ToString(),
                        ["TowerRotationVector"] = _towerRotationVector.ToString(),
                        ["TowerRotationDelta"] = _towerRotationDelta.ToString(),
                        ["CannonRotationVector"] = _cannonRotationVector.ToString(),
                        ["CannonRotationDelta"] = _cannonRotationDelta.ToString(),
                    }
                }
            };

            return message;
        }

        public override void ApplyCurrentMovingStatusMessage(CommunicationMessage<Dictionary<string, string>> message)
        {
            var rawMoveDirection = message.body.Any["MoveDirection"];
            var moveDirection = NumericParser.ParseVector(rawMoveDirection);
            var rawmoveDelta = message.body.Any["MoveDelta"];
            var moveDelta = float.Parse(rawmoveDelta);
            var rawRotationVector = message.body.Any["RotationVector"];
            var rotationVector = NumericParser.ParseVector(rawRotationVector);
            var rawRotationDelta = message.body.Any["RotationDelta"];
            var rotationDelta = float.Parse(rawRotationDelta);
            var rawTowerRotationVector = message.body.Any["TowerRotationVector"];
            var towerRotationVector = NumericParser.ParseVector(rawTowerRotationVector);
            var rawTowerRotationDelta = message.body.Any["TowerRotationDelta"];
            var towerRotationDelta = float.Parse(rawTowerRotationDelta);
            var rawCannonRotationVector = message.body.Any["CannonRotationVector"];
            var cannonRotationVector = NumericParser.ParseVector(rawCannonRotationVector);
            var rawCannonRotationDelta = message.body.Any["CannonRotationDelta"];
            var cannonRotationDelta = float.Parse(rawCannonRotationDelta);

            _moveDirection = moveDirection;
            _moveDelta = moveDelta;
            _rotationVector = rotationVector;
            _rotationDelta = rotationDelta;
            _towerRotationVector = towerRotationVector;
            _towerRotationDelta = towerRotationDelta;
            _cannonRotationVector = cannonRotationVector;
            _cannonRotationDelta = cannonRotationDelta;
        }
    }
}
