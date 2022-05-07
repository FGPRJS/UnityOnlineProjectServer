using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    internal class Heartbeat
    {
        private const int HeartbeatInterval = 5000;
        private const int MaxHeartbeatTimeoutCount = 3;

        private int _currentHeartbeatTime = 0;
        private int _currentHeartbeatTimeoutCount = 0;

        internal delegate void HeartbeatTimeOut();
        internal event HeartbeatTimeOut HeartbeatTimeOutEvent;

        internal delegate void HeartbeatTick();
        internal event HeartbeatTick HeartbeatTickEvent;

        internal static CommunicationMessage<Dictionary<string, string>> heartbeatMessage;
        internal static byte[] heartbeatMessageByteData;



        internal Heartbeat()
        {
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

        internal void CountHeartbeat(int interval)
        {
            _currentHeartbeatTime += interval;

            if(_currentHeartbeatTime >= HeartbeatInterval)
            {
                _currentHeartbeatTime = 0;
                HeartbeatTickEvent?.Invoke();
                _currentHeartbeatTimeoutCount++;
            }

            if (_currentHeartbeatTimeoutCount >= MaxHeartbeatTimeoutCount)
            {
                _currentHeartbeatTimeoutCount = 0;
                HeartbeatTimeOutEvent?.Invoke();
            }
        }

        internal void ResetHeartbeat()
        {
            _currentHeartbeatTime = 0;
            _currentHeartbeatTimeoutCount = 0;
        }
    }
}
