using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Protocol
{
    public enum CommandType
    {
        ConnectionRequest,
        HeartBeatRequest
    }

    public class CommunicationMessage
    {
        public CommandType MessageType;
        public string Message;
    }
}
