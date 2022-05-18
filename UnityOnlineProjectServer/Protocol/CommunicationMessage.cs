﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Protocol
{
    public enum MessageType
    {
        Dummy,
        LoginRequest,
        Ping,
        Pong,
        Close,
        PawnSpawnRequest,
        PawnSpawnReport,
        PawnDestroyReport,
        PawnPositionReport,
        PawnActionRequest,
        PlayerChatReport
    }

    public enum ACK
    {
        None = 0,
        ACK,
        NACK
    }

    [Serializable]
    public class CommunicationMessage<T>
    {
        public Header header;
        public Body<T> body;
    }
    [Serializable]
    public class Header
    {
        public int ACK;
        public string Reason;
        public string MessageName;
    }
    [Serializable]
    public class Body<T>
    {
        public T Any;
    }
}
