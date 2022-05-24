using System;
using System.Collections.Generic;
using System.Text;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Content.GameObject.Implements
{
    public class Dummy : Pawn
    {
        public Dummy(long id) : base(id)
        {
           
        }

        public override void ApplyCurrentMovingStatusMessage(CommunicationMessage<Dictionary<string, string>> message)
        {
            throw new NotImplementedException();
        }

        public override void ApplyCurrentPositionStatusMessage(CommunicationMessage<Dictionary<string, string>> message)
        {
            return;
        }

        public override CommunicationMessage<Dictionary<string, string>> CreateCurrentMovingStatusMessage(MessageType messageType)
        {
            throw new NotImplementedException();
        }

        public override CommunicationMessage<Dictionary<string, string>> CreateCurrentPositionMessage(MessageType messageType)
        {
            return null;
        }

        public override CommunicationMessage<Dictionary<string, string>> CreateObjectInfoMessage(MessageType messageType)
        {
            throw null;
        }
    }
}
