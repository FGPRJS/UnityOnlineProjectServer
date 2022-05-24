using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using UnityOnlineProjectServer.Protocol;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Content
{
    public abstract class Pawn
    {
        public enum PawnType
        {
            Dummy,
            Tank
        }

        public long id;

        public string PawnName;

        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation;
        public DateTime RecentPositionReceivedTime;

        public DateTime RecentMovingReceivedTime;

        public Pawn(long id)
        {
            //ID
            this.id = id;
        }

        public abstract CommunicationMessage<Dictionary<string, string>> CreateObjectInfoMessage(MessageType messageType);

        public abstract CommunicationMessage<Dictionary<string, string>> CreateCurrentStatusMessage(MessageType messageType);

        public abstract CommunicationMessage<Dictionary<string, string>> CreateCurrentMovingStatusMessage(MessageType messageType);

        public abstract void ApplyCurrentMovingStatusMessage(CommunicationMessage<Dictionary<string, string>> message);
        public abstract void ApplyCurrentStatusMessage(CommunicationMessage<Dictionary<string, string>> message);
    }
}
