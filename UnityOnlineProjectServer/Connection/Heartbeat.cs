using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityOnlineProjectProtocol;
using UnityOnlineProjectProtocol.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    internal class Heartbeat
    {
        internal const int heartbeatInterval = 3000;
        internal const int heartbeatTimeout = 18000;

        internal int currentHeartbeatTime = 0;

        internal delegate void HeartbeatTimeOut();
        internal event HeartbeatTimeOut HeartbeatTimeOutEvent;

        internal delegate void HeartbeatTick();
        internal event HeartbeatTick HeartbeatTickEvent;

        internal static CommunicationMessage heartbeatMessage;
        internal static byte[] heartbeatMessageByteData;



        internal Heartbeat()
        {
            if ((heartbeatMessage != null) && (heartbeatMessageByteData != null)) return;

            heartbeatMessage = new CommunicationMessage()
            {
                MessageType = CommandType.HeartBeatRequest
            };

            heartbeatMessageByteData = CommunicationUtility.Serialize(heartbeatMessage);
        }

        internal async void CountHeartbeat(int interval)
        {
            currentHeartbeatTime += interval;

            //Instead of BeginInvoke
            var workTask = Task.Run(() =>
            {
                HeartbeatTickEvent.Invoke();
            });
            await workTask;

            if (currentHeartbeatTime >= heartbeatTimeout)
            {
                currentHeartbeatTime = 0;
                //Instead of BeginInvoke
                workTask = Task.Run(() =>
                {
                    HeartbeatTimeOutEvent.Invoke();
                });
                await workTask;
            }
        }

        internal void ResetHeartbeat()
        {
            currentHeartbeatTime = 0;
        }
    }
}
