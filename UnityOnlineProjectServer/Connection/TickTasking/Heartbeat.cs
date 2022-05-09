using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    public class Heartbeat : TickTask
    {
        public static CommunicationMessage<Dictionary<string, string>> heartbeatMessage;
        public static byte[] heartbeatMessageByteData;


        public Heartbeat()
        {
            _interval = 5000;
            _maxCount = 4;
            _hasTimeout = true;

            if ((heartbeatMessage != null) && (heartbeatMessageByteData != null)) return;

            heartbeatMessage = new CommunicationMessage<Dictionary<string, string>>()
            {
                header = new Header()
                {
                    MessageName = MessageType.HeartBeatRequest.ToString()
                }
            };

            heartbeatMessageByteData = CommunicationUtility.Serialize(heartbeatMessage);
        }
    }
}
